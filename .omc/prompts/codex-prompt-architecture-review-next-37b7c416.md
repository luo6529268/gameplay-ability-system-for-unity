---
provider: "codex"
agent_role: "architect"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs"
  - "Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs"
  - "Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs"
  - "Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs"
  - "Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs"
  - "Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs"
  - "Temp/NTSD_ProductionEntityStress.dispersed-air-role-render-sort-detail-20260725.json"
  - "Temp/NTSD_ProductionEntityStress.dispersed-role-render-subphase-detail-20260725.json"
timestamp: "2026-07-25T12:47:49.356Z"
---

--- File: Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs ---
﻿using System;
using System.Collections.Generic;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Spatial;

namespace NTSD.Animation
{
    /// <summary>
    /// 遍历当前战斗世界全部运行时实体的场景查询器。
    /// </summary>
    public class BruteForceSceneQuery : ILF2SceneQuery
    {
        private const int HitCandidateMax = 20;
        private const int CandidateDistanceUnset = 1000;
        private const int RectMin = -1000000000;
        private const int RectMax = 1000000000;

        private readonly SimulationWorld _world;
        private readonly CollisionBroadphaseBackend _collisionBroadphase;
        private readonly List<SceneQueryHit> _tmpHitResult = new List<SceneQueryHit>(16);
        private readonly List<LF2Entity> _tmpAllObjects = new List<LF2Entity>(32);
        private readonly List<SceneQueryHit> _emptyCandidateHits = new List<SceneQueryHit>(0);
        private readonly Dictionary<LF2Entity, List<SceneQueryHit>> _candidateCache =
            new Dictionary<LF2Entity, List<SceneQueryHit>>();
        private readonly LooseQuadtreeBroadphase _shadowBroadphase = new LooseQuadtreeBroadphase();
        private readonly List<RoleAwareShadowParticipant> _roleShadowParticipants =
            new List<RoleAwareShadowParticipant>(128);
        private readonly List<RoleAwareShadowItrEntry> _roleShadowItrEntries =
            new List<RoleAwareShadowItrEntry>(128);
        private readonly List<SpatialBroadphaseEntry> _shadowEntries = new List<SpatialBroadphaseEntry>(128);
        private readonly List<int> _shadowQueryIndices = new List<int>(64);
        private readonly List<long> _shadowBrutePairs = new List<long>(256);
        private readonly List<long> _shadowTreePairs = new List<long>(256);
        private readonly List<long> _shadowAcceptedPairs = new List<long>(64);
        private readonly Dictionary<int, int> _shadowSlotToOrdinal = new Dictionary<int, int>();
        private readonly SpatialBroadphaseDiagnostics _shadowDiagnostics = new SpatialBroadphaseDiagnostics();
        private readonly RoleAwareCollisionShadowDiagnostics _roleShadowDiagnostics =
            new RoleAwareCollisionShadowDiagnostics();
        private readonly LooseQuadtreeBroadphase _roleFormalBroadphase =
            new LooseQuadtreeBroadphase();
        private readonly List<RoleAwareFormalParticipant> _roleFormalParticipants =
            new List<RoleAwareFormalParticipant>(128);
        private readonly List<SpatialBroadphaseEntry> _roleFormalSpatialBodyEntries =
            new List<SpatialBroadphaseEntry>(128);
        private readonly List<RoleAwareFormalBodyEntry> _roleFormalBodyEntries =
            new List<RoleAwareFormalBodyEntry>(128);
        private readonly List<RoleAwareFormalItrEntry> _roleFormalItrEntries =
            new List<RoleAwareFormalItrEntry>(128);
        private readonly List<int> _roleFormalQueryBodyIndices = new List<int>(64);
        private readonly LooseQuadtreeBroadphase _formalBroadphase = new LooseQuadtreeBroadphase();
        private readonly List<LF2Entity> _formalParticipants = new List<LF2Entity>(128);
        private readonly List<RuntimeEntityHandle> _formalParticipantHandles =
            new List<RuntimeEntityHandle>(128);
        private readonly List<IncrementalSpatialEntry> _formalIncrementalEntries =
            new List<IncrementalSpatialEntry>(128);
        private readonly List<int> _formalFallbackOrdinals = new List<int>(32);
        private readonly List<RuntimeEntityHandle> _formalQueryHandles =
            new List<RuntimeEntityHandle>(64);
        private readonly List<long> _formalPairKeys = new List<long>(256);
        private readonly List<long> _formalAuthorityPairKeys = new List<long>(256);
        private readonly Dictionary<int, int> _formalSlotToOrdinal = new Dictionary<int, int>();
        private readonly HashSet<int> _formalSeenSlots = new HashSet<int>();
        private readonly LooseQuadtreeBroadphase _immediateBroadphase = new LooseQuadtreeBroadphase();
        private readonly List<SpatialBroadphaseEntry> _immediateEntries =
            new List<SpatialBroadphaseEntry>(128);
        private readonly List<int> _immediateQueryIndices = new List<int>(64);
        private readonly List<int> _immediateFallbackIndices = new List<int>(32);
        private readonly List<int> _immediateCandidateIndices = new List<int>(96);
        private readonly List<LF2Entity> _immediateTargets = new List<LF2Entity>(96);
        private bool _consumeCandidateCache;
        private int _formalFallbackParticipantCount;
        private bool _formalCollectionAborted;
        private int _lastFormalPairCount;
        private int _lastRoleAwareBodyEntryCount;
        private int _lastRoleAwareItrQueryCount;
        public bool ShadowBroadphaseDiagnosticsEnabled { get; set; }
        public CollisionFormalCollectorMode FormalCollectorMode { get; set; }
        internal SpatialBroadphaseDiagnostics ShadowBroadphaseDiagnostics => _shadowDiagnostics;
        public RoleAwareCollisionShadowDiagnostics RoleAwareShadowDiagnostics =>
            _roleShadowDiagnostics;
        internal CollisionBroadphaseBackend CollisionBroadphase => _collisionBroadphase;
        internal int FormalFallbackParticipantCount => _formalFallbackParticipantCount;
        public bool FormalCollectionAborted => _formalCollectionAborted;
        internal SpatialSynchronizeResult FormalSpatialSynchronizeResult { get; private set; }
        internal LooseQuadtreeBroadphase FormalBroadphaseForSelfCheck => _formalBroadphase;
#if UNITY_INCLUDE_TESTS
        public bool ThrowDuringRoleAwareShadowForSelfCheck { get; set; }
        public int ThrowAfterRoleAwareFormalPairCountForSelfCheck { get; set; } = -1;
#endif
        public CollisionFormalCollectorMode LastFormalCollectorModeForDiagnostics { get; private set; }
        public int LastFormalPairCountForDiagnostics => _lastFormalPairCount;
        public int LastFormalFallbackParticipantCountForDiagnostics =>
            _formalFallbackParticipantCount;
        public bool LastFormalCollectionAbortedForDiagnostics => _formalCollectionAborted;
        public SpatialSynchronizeResult LastFormalSynchronizeResultForDiagnostics =>
            FormalSpatialSynchronizeResult;
        public int LastRoleAwareBodyEntryCountForDiagnostics => _lastRoleAwareBodyEntryCount;
        public int LastRoleAwareItrQueryCountForDiagnostics => _lastRoleAwareItrQueryCount;
        public BruteForceSceneQuery(
            SimulationWorld world,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            _world = world;
            _collisionBroadphase = collisionBroadphase;
        }

        internal void ResetFormalSpatialBroadphase()
        {
            _formalBroadphase.ResetIncremental();
            _immediateBroadphase.ResetIncremental();
            FormalSpatialSynchronizeResult = default;
        }

        private bool TryGetImmediateSpatialTargets(
            in SpatialAabbXZ queryBounds,
            out List<LF2Entity> targets)
        {
            targets = _immediateTargets;
            targets.Clear();
            _world.GetAllEntities(_tmpAllObjects);
            if (_collisionBroadphase != CollisionBroadphaseBackend.LooseQuadtree ||
                !queryBounds.IsValid)
            {
                return false;
            }

            _immediateEntries.Clear();
            _immediateFallbackIndices.Clear();
            for (int index = 0; index < _tmpAllObjects.Count; index++)
            {
                LF2Entity entity = _tmpAllObjects[index];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;
                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                if (runtimeSlot < 0 || runtimeSlot >= _world.MaxRuntimeSlotsForServices ||
                    !ReferenceEquals(_world.FindEntityByRuntimeSlotForQuery(runtimeSlot), entity))
                {
                    return false;
                }

                if (TryBuildCollisionBroadphaseAabb(
                        entity,
                        entity.GetCollisionFrameData(),
                        out SpatialAabbXZ bounds))
                {
                    _immediateEntries.Add(new SpatialBroadphaseEntry(runtimeSlot, index, bounds));
                }
                else
                {
                    _immediateFallbackIndices.Add(index);
                }
            }

            BattleStageRuntimeState stage = _world.Runtime?.Stage;
            int stageWidth = stage?.StageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            var preferredRoot = new SpatialAabbXZ(
                0,
                zMin,
                stageWidth > 0 ? stageWidth : 1,
                zMax > zMin ? zMax : zMin + 1);
            try
            {
                _immediateBroadphase.Rebuild(_immediateEntries, preferredRoot);
                _immediateBroadphase.Query(queryBounds, _immediateQueryIndices);
            }
            catch
            {
                targets.Clear();
                return false;
            }

            _immediateCandidateIndices.Clear();
            for (int i = 0; i < _immediateQueryIndices.Count; i++)
            {
                int inputIndex = _immediateQueryIndices[i];
                if (inputIndex < 0 || inputIndex >= _tmpAllObjects.Count)
                {
                    targets.Clear();
                    return false;
                }
                _immediateCandidateIndices.Add(inputIndex);
            }
            _immediateCandidateIndices.AddRange(_immediateFallbackIndices);
            _immediateCandidateIndices.Sort();
            int previous = -1;
            for (int i = 0; i < _immediateCandidateIndices.Count; i++)
            {
                int index = _immediateCandidateIndices[i];
                if (index == previous)
                    continue;
                if (index < 0 || index >= _tmpAllObjects.Count)
                {
                    targets.Clear();
                    return false;
                }
                previous = index;
                targets.Add(_tmpAllObjects[index]);
            }
            return true;
        }

        public List<SceneQueryHit> QueryBodyHits(in PhysicsState.BattleVolume vol, LF2Entity exclude)
        {
            _tmpHitResult.Clear();

            if (_consumeCandidateCache && exclude != null &&
                _candidateCache.TryGetValue(exclude, out var cachedCandidates))
            {
                int candidateCount = exclude.Runtime?.HitCandidateCount ?? cachedCandidates.Count;
                if (candidateCount < 0)
                    candidateCount = 0;
                if (candidateCount > cachedCandidates.Count)
                    candidateCount = cachedCandidates.Count;

                for (int i = 0; i < candidateCount; i++)
                {
                    SceneQueryHit hit = cachedCandidates[i];
                    LF2Entity target = hit.ResolveCurrentTarget(_world);
                    if (target == null || target == exclude)
                        continue;
                    if (IsPendingFlushDestroy(target))
                        continue;
                    if (IsPureTransitionSmoke(target))
                        continue;

                    LF2FrameData targetFrame = target.GetCollisionFrameData();
                    if (target.PS == null || targetFrame == null)
                        continue;
                    if (!HasAnyReleaseBody(targetFrame))
                        continue;

                    if (!HitsTarget(vol, target, targetFrame, out int bodyX))
                        continue;

                    _tmpHitResult.Add(new SceneQueryHit(target, bodyX, hit.ItrIndex, hit.RuntimeItr));
                }

                return _tmpHitResult;
            }

            List<LF2Entity> spatialTargets = null;
            bool spatial = TryBuildImmediateVolumeAabb(vol, out SpatialAabbXZ volumeBounds) &&
                           TryGetImmediateSpatialTargets(volumeBounds, out spatialTargets);
            if (!spatial)
                _world.GetAllEntities(_tmpAllObjects);
            List<LF2Entity> source = spatial ? spatialTargets : _tmpAllObjects;

            for (int i = 0; i < source.Count; i++)
            {
                LF2Entity target = source[i];
                if (target == exclude) continue;
                if (IsPendingFlushDestroy(target)) continue;
                if (IsPureTransitionSmoke(target)) continue;
                LF2FrameData targetFrame = target.GetCollisionFrameData();
                if (target.PS == null || targetFrame == null) continue;
                if (!HasAnyReleaseBody(targetFrame)) continue;

                float spriteWidthPx = target.GetSpriteWidthPxForCollision();
                if (spriteWidthPx <= 0f)
                {
                    continue;
                }

                for (int b = 0; b < targetFrame.bodies.Count; b++)
                {
                    BodyBox body = targetFrame.bodies[b];
                    if (!IsReleaseBody(body))
                        continue;

                    if (!TryBuildBodyBattleVolume(target, targetFrame, body, out PhysicsState.BattleVolume bodyVolume))
                        continue;
                    bool intersects = CollisionUtil.Intersect(vol, bodyVolume);
                    if (intersects)
                    {
                        int bodyX = body.x;
                        _tmpHitResult.Add(new SceneQueryHit(target, bodyX));
                        break;
                    }
                }
            }

            return _tmpHitResult;
        }

        public List<SceneQueryHit> QueryBodyHits(LF2Entity attacker, LF2FrameData attackerFrame, InteractionArea itr)
        {
            if (_consumeCandidateCache)
            {
                _tmpHitResult.Clear();
                if (attacker != null &&
                    itr != null &&
                    _candidateCache.TryGetValue(attacker, out var cached))
                {
                    int itrIndex = ResolveItrIndex(attackerFrame, itr);
                    LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
                    int candidateCount = attacker.Runtime?.HitCandidateCount ?? cached.Count;
                    if (candidateCount < 0)
                        candidateCount = 0;
                    if (candidateCount > cached.Count)
                        candidateCount = cached.Count;

                    for (int i = 0; i < candidateCount; i++)
                    {
                        SceneQueryHit hit = cached[i];
                        LF2Entity target = hit.ResolveCurrentTarget(_world);
                        if (target == null || IsPendingFlushDestroy(target))
                            continue;
                        if (hit.ItrIndex != itrIndex)
                            continue;

                        // 中文注释：
                        // step6 记录的是“哪一个 pair / 哪一个 itrIndex 命中过”。
                        // 到 step7/step9 真正消费时，C++ 会继续读取对象当前这一拍的正式运行时字段；
                        // 不能把 step6 当时缓存下来的 runtime itr 原样复用到本拍后续逻辑。
                        // 否则 type=3 在同一 tick 里被命中换帧后，仍可能继续拿旧帧的大 itr 出手。
                        InteractionArea runtimeItr = ResolveRuntimeItrForPair(
                            attacker,
                            target,
                            attackerCollisionFrame,
                            itr,
                            out bool zeroAttackerHpOnConsume,
                            out bool releaseHeavyHeldTargetOnConsume);
                        if (runtimeItr == null)
                            continue;

                        // 中文注释：
                        // step6 只记录“这一对对象 + 这一条 itrIndex 曾经命中过”。
                        // 到 step7/step9 真正消费时，必须再按当前拍的正式关系字段和 runtime itr
                        // 做一次硬过滤，避免 type=3 在同 tick 内换队伍/换帧后，
                        // 旧 candidate 仍沿着缓存链继续漏进后面的命中消费。
                        if (IsReleaseConsumerPairBlocked(attacker, target))
                            continue;
                        if (!RuntimeConsumeItrAllowed(attacker, runtimeItr, target))
                            continue;

                        _tmpHitResult.Add(new SceneQueryHit(
                            target,
                            hit.BodyX,
                            hit.ItrIndex,
                            runtimeItr,
                            zeroAttackerHpOnConsume,
                            releaseHeavyHeldTargetOnConsume));
                    }
                }

                return _tmpHitResult;
            }

            return QueryBodyHitsImmediate(attacker, attackerFrame, itr);
        }

        public List<SceneQueryHit> QueryBodyHits(
            LF2Entity attacker,
            LF2FrameData attackerFrame,
            InteractionArea itr,
            in PhysicsState.BattleVolume volume)
        {
            _tmpHitResult.Clear();
            if (attacker == null || attacker.PS == null || attackerFrame == null || itr == null)
                return _tmpHitResult;
            if (IsPendingFlushDestroy(attacker))
                return _tmpHitResult;
            if (GetAuthoredCurrentFrame(attacker) == null)
                return _tmpHitResult;

            LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
            if (attackerCollisionFrame?.itrs == null || attackerCollisionFrame.itrs.Count == 0)
                return _tmpHitResult;

            List<LF2Entity> spatialTargets = null;
            bool spatial = TryBuildImmediateVolumeAabb(volume, out SpatialAabbXZ volumeBounds) &&
                           TryGetImmediateSpatialTargets(volumeBounds, out spatialTargets);
            if (!spatial)
                _world.GetAllEntities(_tmpAllObjects);
            List<LF2Entity> source = spatial ? spatialTargets : _tmpAllObjects;

            for (int i = 0; i < source.Count; i++)
            {
                LF2Entity target = source[i];
                if (target == attacker || target == null || target.PS == null)
                    continue;
                if (IsPendingFlushDestroy(target))
                    continue;

                LF2FrameData targetCurrentFrame = GetAuthoredCurrentFrame(target);
                LF2FrameData targetCollisionFrame = target.GetCollisionFrameData();
                if (targetCurrentFrame == null)
                    continue;
                if (!HasAnyReleaseBody(targetCurrentFrame))
                    continue;
                if (!HasAnyReleaseBody(targetCollisionFrame))
                    continue;
                if (!ImmediateQueryPairAllowed(attacker, target))
                    continue;
                InteractionArea runtimeItr = ResolveRuntimeItrForPair(
                    attacker,
                    target,
                    attackerCollisionFrame,
                    itr,
                    out _,
                    out _);
                if (runtimeItr == null)
                    continue;
                if (!ItrAllowed(attacker, attackerFrame, runtimeItr, target, targetCurrentFrame))
                    continue;
                if (!HitsTarget(volume, target, targetCollisionFrame, out int bodyX))
                    continue;
                if (!CandidateAccepts(attacker, attackerFrame, runtimeItr, target, targetCurrentFrame, bodyX))
                    continue;

                _tmpHitResult.Add(new SceneQueryHit(target, bodyX));
            }

            return _tmpHitResult;
        }

        /// <summary>
        /// C++ release step6：在 step7/step9 之前统一收集碰撞候选。
        /// 后续两个碰撞循环只消费这里的快照，不能让 step8 或同 tick 新生对象立即参与。
        /// </summary>
        public void CollectCollisionCandidates()
        {
            _candidateCache.Clear();
            _consumeCandidateCache = false;
            _formalFallbackParticipantCount = 0;
            _formalCollectionAborted = false;
            _lastFormalPairCount = 0;
            _lastRoleAwareBodyEntryCount = 0;
            _lastRoleAwareItrQueryCount = 0;
            FormalSpatialSynchronizeResult = default;
            int currentTick = _world?.CurrentTickIndex ?? 0;

            _world.GetAllEntities(_tmpAllObjects);
            ResetCandidateCollectionState();

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity attacker = _tmpAllObjects[i];
                if (attacker == null || attacker.PS == null || IsPendingFlushDestroy(attacker))
                    continue;
                if (IsCollisionCandidateSuppressed(attacker, currentTick))
                    continue;
                if (GetAuthoredCurrentFrame(attacker) == null)
                    continue;

                LF2FrameData attackerFrame = attacker.GetCollisionFrameData();
                if (attackerFrame?.itrs == null || attackerFrame.itrs.Count == 0)
                    continue;

                _candidateCache[attacker] = new List<SceneQueryHit>(16);
            }

            if (ShadowBroadphaseDiagnosticsEnabled)
            {
                try
                {
                    BuildShadowBroadphase(currentTick);
                }
                catch (Exception)
                {
                    AbortRoleAwareShadow();
                }
            }

            CollisionFormalCollectorMode collectorMode = ResolveFormalCollectorMode();
            LastFormalCollectorModeForDiagnostics = collectorMode;
            if (collectorMode != CollisionFormalCollectorMode.ForceBruteForce)
            {
                uint rngStateBeforeFormal = _world.Rng.State;
                ulong rngCallsBeforeFormal = _world.Rng.CallCount;
                bool formalSucceeded = true;
                if (formalSucceeded)
                {
                    try
                    {
                        formalSucceeded =
                            collectorMode == CollisionFormalCollectorMode.ForceRoleAware
                                ? TryCollectCollisionCandidatesRoleAware(currentTick)
                                : TryCollectCollisionCandidatesLoose(currentTick);
                    }
                    catch (Exception)
                    {
                        formalSucceeded = false;
                    }
                }

                if (!formalSucceeded)
                {
                    _formalCollectionAborted = true;
                    _world.Rng.RestoreState(rngStateBeforeFormal, rngCallsBeforeFormal);
                    ResetCandidateCollectionState();
                    CollectCollisionCandidatesBruteForce(currentTick);
                }
            }
            else
            {
                CollectCollisionCandidatesBruteForce(currentTick);
            }

            if (ShadowBroadphaseDiagnosticsEnabled && !_roleShadowDiagnostics.CollectionAborted)
            {
                try
                {
                    CompareShadowBroadphaseResults();
                }
                catch (Exception)
                {
                    AbortRoleAwareShadow();
                }
            }

            _consumeCandidateCache = true;
        }

        private CollisionFormalCollectorMode ResolveFormalCollectorMode()
        {
            if (FormalCollectorMode != CollisionFormalCollectorMode.Configured)
                return FormalCollectorMode;

            return _collisionBroadphase == CollisionBroadphaseBackend.LooseQuadtree
                ? CollisionFormalCollectorMode.ForceLegacyUnionAabb
                : CollisionFormalCollectorMode.ForceBruteForce;
        }

        private void ResetCandidateCollectionState()
        {
            foreach (KeyValuePair<LF2Entity, List<SceneQueryHit>> pair in _candidateCache)
                pair.Value?.Clear();

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity entity = _tmpAllObjects[i];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;

                entity.ClearHitCandidateCarriers();
                entity.Runtime.HitCandidateCount = 0;
                entity.Runtime.HitCandidateNearestDistance = CandidateDistanceUnset;
                entity.Runtime.HitCandidateKind1Distance = CandidateDistanceUnset;
                entity.Runtime.HitCandidateExtraDistance = CandidateDistanceUnset;
            }
        }

        private void CollectCollisionCandidatesBruteForce(int currentTick)
        {
            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity a = _tmpAllObjects[i];
                if (a == null || a.PS == null || IsPendingFlushDestroy(a))
                    continue;
                if (IsCollisionCandidateSuppressed(a, currentTick))
                    continue;

                for (int j = i + 1; j < _tmpAllObjects.Count; j++)
                {
                    LF2Entity b = _tmpAllObjects[j];
                    if (b == null || b.PS == null || IsPendingFlushDestroy(b))
                        continue;
                    if (IsCollisionCandidateSuppressed(b, currentTick))
                        continue;

                    CollectCandidatesForPair(a, b);
                    CollectCandidatesForPair(b, a);
                }
            }
        }

        private bool TryCollectCollisionCandidatesLoose(int currentTick)
        {
            _formalParticipants.Clear();
            _formalParticipantHandles.Clear();
            _formalIncrementalEntries.Clear();
            _formalFallbackOrdinals.Clear();
            _formalPairKeys.Clear();
            _formalAuthorityPairKeys.Clear();
            _formalSlotToOrdinal.Clear();
            _formalSeenSlots.Clear();

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity entity = _tmpAllObjects[i];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;
                if (IsCollisionCandidateSuppressed(entity, currentTick))
                    continue;

                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                if (runtimeSlot < 0 ||
                    runtimeSlot >= _world.MaxRuntimeSlotsForServices ||
                    !ReferenceEquals(_world.FindEntityByRuntimeSlotForQuery(runtimeSlot), entity) ||
                    !_formalSeenSlots.Add(runtimeSlot) ||
                    !_world.TryGetCurrentRuntimeHandle(runtimeSlot, entity, out RuntimeEntityHandle handle))
                {
                    return AbortFormalSpatialIndex();
                }
                _formalSlotToOrdinal.Add(runtimeSlot, _formalParticipants.Count);
                _formalParticipants.Add(entity);
                _formalParticipantHandles.Add(handle);
            }

            int participantCount = _formalParticipants.Count;

            for (int authorityOrdinal = 0; authorityOrdinal < participantCount; authorityOrdinal++)
            {
                LF2Entity entity = _formalParticipants[authorityOrdinal];
                if (!TryBuildCollisionBroadphaseAabb(
                        entity,
                        entity.GetCollisionFrameData(),
                        out SpatialAabbXZ bounds))
                {
                    _formalFallbackOrdinals.Add(authorityOrdinal);
                    continue;
                }

                _formalIncrementalEntries.Add(new IncrementalSpatialEntry(
                    _formalParticipantHandles[authorityOrdinal],
                    bounds));
            }

            if (_formalIncrementalEntries.Count + _formalFallbackOrdinals.Count != participantCount)
                return AbortFormalSpatialIndex();

            _formalFallbackParticipantCount = _formalFallbackOrdinals.Count;

            BattleStageRuntimeState stage = _world?.Runtime?.Stage;
            int stageWidth = stage?.StageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            SpatialAabbXZ preferredRoot = new SpatialAabbXZ(
                0,
                zMin,
                stageWidth > 0 ? stageWidth : 1,
                zMax > zMin ? zMax : zMin + 1);

            try
            {
                FormalSpatialSynchronizeResult = _formalBroadphase.Synchronize(
                    _formalIncrementalEntries,
                    preferredRoot);
                if (!FormalSpatialSynchronizeResult.Succeeded ||
                    FormalSpatialSynchronizeResult.IndexedCount != _formalIncrementalEntries.Count)
                {
                    return AbortFormalSpatialIndex();
                }

                if (participantCount < 2)
                    return true;

                for (int entryIndex = 0; entryIndex < _formalIncrementalEntries.Count; entryIndex++)
                {
                    IncrementalSpatialEntry entry = _formalIncrementalEntries[entryIndex];
                    _formalBroadphase.QueryHandles(entry.Bounds, _formalQueryHandles);
                    for (int resultIndex = 0; resultIndex < _formalQueryHandles.Count; resultIndex++)
                    {
                        RuntimeEntityHandle otherHandle = _formalQueryHandles[resultIndex];
                        if (otherHandle == entry.Handle)
                            continue;

                        if (!_world.TryResolveRuntimeHandle(otherHandle, out LF2Entity otherEntity) ||
                            otherEntity?.Runtime == null ||
                            otherEntity.Runtime.SlotIndex != otherHandle.Slot)
                        {
                            return AbortFormalSpatialIndex();
                        }

                        int otherRuntimeSlot = otherHandle.Slot;
                        if (!_formalSlotToOrdinal.TryGetValue(otherRuntimeSlot, out int mappedOrdinal) ||
                            mappedOrdinal < 0 || mappedOrdinal >= participantCount ||
                            !ReferenceEquals(_formalParticipants[mappedOrdinal], otherEntity) ||
                            _formalParticipantHandles[mappedOrdinal] != otherHandle)
                        {
                            return AbortFormalSpatialIndex();
                        }
                        AddRuntimeSlotPair(entry.Handle.Slot, otherRuntimeSlot);
                    }
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }

            for (int fallbackIndex = 0; fallbackIndex < _formalFallbackOrdinals.Count; fallbackIndex++)
            {
                int fallbackOrdinal = _formalFallbackOrdinals[fallbackIndex];
                for (int authorityOrdinal = 0; authorityOrdinal < participantCount; authorityOrdinal++)
                {
                    if (authorityOrdinal != fallbackOrdinal)
                    {
                        AddRuntimeSlotPair(
                            _formalParticipants[fallbackOrdinal].Runtime.SlotIndex,
                            _formalParticipants[authorityOrdinal].Runtime.SlotIndex);
                    }
                }
            }

            SortAndDeduplicate(_formalPairKeys);
            for (int pairIndex = 0; pairIndex < _formalPairKeys.Count; pairIndex++)
            {
                long pairKey = _formalPairKeys[pairIndex];
                int firstSlot = (int)(pairKey >> 32);
                int secondSlot = (int)(pairKey & 0xffffffffL);
                if (!_formalSlotToOrdinal.TryGetValue(firstSlot, out int firstOrdinal) ||
                    !_formalSlotToOrdinal.TryGetValue(secondSlot, out int secondOrdinal) ||
                    firstOrdinal == secondOrdinal)
                {
                    return AbortFormalSpatialIndex();
                }

                uint minOrdinal = (uint)Math.Min(firstOrdinal, secondOrdinal);
                uint maxOrdinal = (uint)Math.Max(firstOrdinal, secondOrdinal);
                _formalAuthorityPairKeys.Add(((long)minOrdinal << 32) | maxOrdinal);
            }

            SortAndDeduplicate(_formalAuthorityPairKeys);
            try
            {
                for (int pairIndex = 0; pairIndex < _formalAuthorityPairKeys.Count; pairIndex++)
                {
                    long pairKey = _formalAuthorityPairKeys[pairIndex];
                    int firstOrdinal = (int)(pairKey >> 32);
                    int secondOrdinal = (int)(pairKey & 0xffffffffL);
                    if (firstOrdinal < 0 || secondOrdinal <= firstOrdinal ||
                        secondOrdinal >= participantCount)
                    {
                        return AbortFormalSpatialIndex();
                    }

                    LF2Entity first = _formalParticipants[firstOrdinal];
                    LF2Entity second = _formalParticipants[secondOrdinal];
                    CollectCandidatesForPair(first, second);
                    CollectCandidatesForPair(second, first);
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }

            _lastFormalPairCount = _formalAuthorityPairKeys.Count;
            return true;
        }

        private bool TryCollectCollisionCandidatesRoleAware(int currentTick)
        {
            _roleFormalParticipants.Clear();
            _roleFormalSpatialBodyEntries.Clear();
            _roleFormalBodyEntries.Clear();
            _roleFormalItrEntries.Clear();
            _roleFormalQueryBodyIndices.Clear();
            _formalAuthorityPairKeys.Clear();
            _formalSeenSlots.Clear();

            for (int authorityOrdinal = 0;
                 authorityOrdinal < _tmpAllObjects.Count;
                 authorityOrdinal++)
            {
                LF2Entity entity = _tmpAllObjects[authorityOrdinal];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;
                if (IsCollisionCandidateSuppressed(entity, currentTick))
                    continue;

                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                if (runtimeSlot < 0 ||
                    runtimeSlot >= _world.MaxRuntimeSlotsForServices ||
                    !ReferenceEquals(_world.FindEntityByRuntimeSlotForQuery(runtimeSlot), entity) ||
                    !_formalSeenSlots.Add(runtimeSlot) ||
                    !_world.TryGetCurrentRuntimeHandle(
                        runtimeSlot,
                        entity,
                        out RuntimeEntityHandle handle))
                {
                    return AbortFormalSpatialIndex();
                }

                LF2FrameData collisionFrame = entity.GetCollisionFrameData();
                var participant = new RoleAwareFormalParticipant(
                    entity,
                    collisionFrame,
                    handle);
                int participantOrdinal = _roleFormalParticipants.Count;
                _roleFormalParticipants.Add(participant);

                if (collisionFrame?.bodies != null)
                {
                    for (int bodyIndex = 0;
                         bodyIndex < collisionFrame.bodies.Count;
                         bodyIndex++)
                    {
                        BodyBox body = collisionFrame.bodies[bodyIndex];
                        if (!IsReleaseBody(body))
                            continue;

                        participant.HasBody = true;
                        if (TryBuildFormalBodyAabb(
                                entity,
                                collisionFrame,
                                body,
                                out SpatialAabbXZ bodyBounds))
                        {
                            int formalBodyIndex = _roleFormalBodyEntries.Count;
                            _roleFormalBodyEntries.Add(new RoleAwareFormalBodyEntry(
                                participantOrdinal,
                                handle));
                            _roleFormalSpatialBodyEntries.Add(new SpatialBroadphaseEntry(
                                runtimeSlot,
                                formalBodyIndex,
                                bodyBounds));
                        }
                        else
                        {
                            participant.HasFallbackBody = true;
                        }
                    }
                }

                if (collisionFrame?.itrs != null)
                {
                    for (int itrIndex = 0;
                         itrIndex < collisionFrame.itrs.Count;
                         itrIndex++)
                    {
                        InteractionArea itr = collisionFrame.itrs[itrIndex];
                        if (!IsReleaseItrGeometry(itr))
                            continue;

                        participant.HasAttackItr = true;
                        if (TryBuildImmediateItrAabb(
                                entity,
                                collisionFrame,
                                itr,
                                out SpatialAabbXZ itrBounds))
                        {
                            _roleFormalItrEntries.Add(new RoleAwareFormalItrEntry(
                                participantOrdinal,
                                handle,
                                itrBounds));
                        }
                        else
                        {
                            participant.HasFallbackAttackItr = true;
                        }
                    }
                }
            }

            int participantCount = _roleFormalParticipants.Count;
            BattleStageRuntimeState stage = _world?.Runtime?.Stage;
            int stageWidth = stage?.StageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            var preferredRoot = new SpatialAabbXZ(
                0,
                zMin,
                stageWidth > 0 ? stageWidth : 1,
                zMax > zMin ? zMax : zMin + 1);

            try
            {
                _roleFormalBroadphase.Rebuild(
                    _roleFormalSpatialBodyEntries,
                    preferredRoot);

                for (int itrEntryIndex = 0;
                     itrEntryIndex < _roleFormalItrEntries.Count;
                     itrEntryIndex++)
                {
                    RoleAwareFormalItrEntry itrEntry =
                        _roleFormalItrEntries[itrEntryIndex];
                    if (!TryValidateRoleAwareParticipant(
                            itrEntry.ParticipantOrdinal,
                            itrEntry.Handle,
                            out _))
                    {
                        return AbortFormalSpatialIndex();
                    }

                    _roleFormalBroadphase.Query(
                        itrEntry.Bounds,
                        _roleFormalQueryBodyIndices);
                    for (int resultIndex = 0;
                         resultIndex < _roleFormalQueryBodyIndices.Count;
                         resultIndex++)
                    {
                        int bodyEntryIndex = _roleFormalQueryBodyIndices[resultIndex];
                        if (bodyEntryIndex < 0 ||
                            bodyEntryIndex >= _roleFormalBodyEntries.Count)
                        {
                            return AbortFormalSpatialIndex();
                        }

                        RoleAwareFormalBodyEntry bodyEntry =
                            _roleFormalBodyEntries[bodyEntryIndex];
                        if (!TryValidateRoleAwareParticipant(
                                bodyEntry.ParticipantOrdinal,
                                bodyEntry.Handle,
                                out _))
                        {
                            return AbortFormalSpatialIndex();
                        }

                        AddAuthorityOrdinalPair(
                            itrEntry.ParticipantOrdinal,
                            bodyEntry.ParticipantOrdinal);
                    }
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }

            for (int attackerOrdinal = 0;
                 attackerOrdinal < participantCount;
                 attackerOrdinal++)
            {
                RoleAwareFormalParticipant attacker =
                    _roleFormalParticipants[attackerOrdinal];
                if (!attacker.HasAttackItr)
                    continue;

                for (int targetOrdinal = 0;
                     targetOrdinal < participantCount;
                     targetOrdinal++)
                {
                    if (attackerOrdinal == targetOrdinal)
                        continue;

                    RoleAwareFormalParticipant target =
                        _roleFormalParticipants[targetOrdinal];
                    if (!target.HasBody ||
                        (!attacker.HasFallbackAttackItr && !target.HasFallbackBody))
                    {
                        continue;
                    }

                    AddAuthorityOrdinalPair(attackerOrdinal, targetOrdinal);
                }
            }

            SortAndDeduplicate(_formalAuthorityPairKeys);
            _formalFallbackParticipantCount = CountRoleAwareFormalFallbackParticipants();
            _lastFormalPairCount = _formalAuthorityPairKeys.Count;
            _lastRoleAwareBodyEntryCount = _roleFormalBodyEntries.Count;
            _lastRoleAwareItrQueryCount = _roleFormalItrEntries.Count;
            FormalSpatialSynchronizeResult = new SpatialSynchronizeResult(
                true,
                true,
                _roleFormalBodyEntries.Count,
                0,
                0,
                0,
                _roleFormalBodyEntries.Count);

            try
            {
                for (int pairIndex = 0;
                     pairIndex < _formalAuthorityPairKeys.Count;
                     pairIndex++)
                {
                    long pairKey = _formalAuthorityPairKeys[pairIndex];
                    int firstOrdinal = (int)(pairKey >> 32);
                    int secondOrdinal = (int)(pairKey & 0xffffffffL);
                    if (firstOrdinal < 0 ||
                        secondOrdinal <= firstOrdinal ||
                        secondOrdinal >= participantCount)
                    {
                        return AbortFormalSpatialIndex();
                    }

                    if (!TryValidateRoleAwareParticipant(
                            firstOrdinal,
                            _roleFormalParticipants[firstOrdinal].Handle,
                            out LF2Entity first) ||
                        !TryValidateRoleAwareParticipant(
                            secondOrdinal,
                            _roleFormalParticipants[secondOrdinal].Handle,
                            out LF2Entity second))
                    {
                        return AbortFormalSpatialIndex();
                    }

                    CollectCandidatesForPair(first, second);
                    CollectCandidatesForPair(second, first);
#if UNITY_INCLUDE_TESTS
                    if (ThrowAfterRoleAwareFormalPairCountForSelfCheck >= 0 &&
                        pairIndex + 1 >= ThrowAfterRoleAwareFormalPairCountForSelfCheck)
                    {
                        throw new InvalidOperationException(
                            "Forced role-aware formal collector self-check failure.");
                    }
#endif
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }

            return true;
        }

        private bool TryValidateRoleAwareParticipant(
            int participantOrdinal,
            RuntimeEntityHandle expectedHandle,
            out LF2Entity entity)
        {
            entity = null;
            if (participantOrdinal < 0 ||
                participantOrdinal >= _roleFormalParticipants.Count)
            {
                return false;
            }

            RoleAwareFormalParticipant participant =
                _roleFormalParticipants[participantOrdinal];
            if (participant.Handle != expectedHandle ||
                !_world.TryResolveRuntimeHandle(expectedHandle, out entity) ||
                !ReferenceEquals(entity, participant.Entity) ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != expectedHandle.Slot)
            {
                entity = null;
                return false;
            }

            return true;
        }

        private int CountRoleAwareFormalFallbackParticipants()
        {
            int count = 0;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (participant.HasFallbackAttackItr || participant.HasFallbackBody)
                    count++;
            }

            return count;
        }

        private void AddAuthorityOrdinalPair(int firstOrdinal, int secondOrdinal)
        {
            if (firstOrdinal == secondOrdinal ||
                firstOrdinal < 0 ||
                secondOrdinal < 0)
            {
                return;
            }

            uint min = (uint)Math.Min(firstOrdinal, secondOrdinal);
            uint max = (uint)Math.Max(firstOrdinal, secondOrdinal);
            _formalAuthorityPairKeys.Add(((long)min << 32) | max);
        }

        private bool AbortFormalSpatialIndex()
        {
            _formalBroadphase.ResetIncremental();
            return false;
        }

        private void AddRuntimeSlotPair(int firstSlot, int secondSlot)
        {
            if (firstSlot == secondSlot || firstSlot < 0 || secondSlot < 0)
                return;

            uint min = (uint)Math.Min(firstSlot, secondSlot);
            uint max = (uint)Math.Max(firstSlot, secondSlot);
            _formalPairKeys.Add(((long)min << 32) | max);
        }

        private void BuildShadowBroadphase(int currentTick)
        {
            _roleShadowDiagnostics.Begin();
            _shadowDiagnostics.Begin(0);
#if UNITY_INCLUDE_TESTS
            if (ThrowDuringRoleAwareShadowForSelfCheck)
                throw new InvalidOperationException("Forced role-aware shadow self-check failure.");
#endif
            _roleShadowParticipants.Clear();
            _roleShadowItrEntries.Clear();
            _shadowEntries.Clear();
            _shadowQueryIndices.Clear();
            _shadowBrutePairs.Clear();
            _shadowTreePairs.Clear();
            _shadowAcceptedPairs.Clear();
            _shadowSlotToOrdinal.Clear();

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity entity = _tmpAllObjects[i];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;
                if (IsCollisionCandidateSuppressed(entity, currentTick))
                    continue;

                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                if (runtimeSlot < 0 ||
                    runtimeSlot >= _world.MaxRuntimeSlotsForServices ||
                    !ReferenceEquals(_world.FindEntityByRuntimeSlotForQuery(runtimeSlot), entity) ||
                    _shadowSlotToOrdinal.ContainsKey(runtimeSlot))
                {
                    throw new InvalidOperationException("Invalid role-aware shadow participant.");
                }

                var participant = new RoleAwareShadowParticipant(
                    entity,
                    entity.GetCollisionFrameData(),
                    runtimeSlot);
                int participantOrdinal = _roleShadowParticipants.Count;
                _shadowSlotToOrdinal.Add(runtimeSlot, participantOrdinal);
                _roleShadowParticipants.Add(participant);

                LF2FrameData frame = participant.Frame;
                if (frame?.bodies != null)
                {
                    for (int bodyIndex = 0; bodyIndex < frame.bodies.Count; bodyIndex++)
                    {
                        BodyBox body = frame.bodies[bodyIndex];
                        if (!IsReleaseBody(body))
                            continue;

                        participant.HasBody = true;
                        _roleShadowDiagnostics.BodyCount++;
                        if (TryBuildShadowBodyAabb(entity, frame, body, out SpatialAabbXZ bodyBounds))
                        {
                            _shadowEntries.Add(new SpatialBroadphaseEntry(
                                runtimeSlot,
                                _shadowEntries.Count,
                                bodyBounds));
                        }
                        else
                        {
                            participant.HasFallbackBody = true;
                            _roleShadowDiagnostics.FallbackBodyCount++;
                        }
                    }
                }

                if (frame?.itrs != null)
                {
                    for (int itrIndex = 0; itrIndex < frame.itrs.Count; itrIndex++)
                    {
                        InteractionArea itr = frame.itrs[itrIndex];
                        if (!IsReleaseItrGeometry(itr))
                            continue;

                        participant.HasAttackItr = true;
                        _roleShadowDiagnostics.AttackItrCount++;
                        if (TryBuildImmediateItrAabb(entity, frame, itr, out SpatialAabbXZ itrBounds))
                        {
                            _roleShadowItrEntries.Add(new RoleAwareShadowItrEntry(
                                participantOrdinal,
                                itrBounds));
                        }
                        else
                        {
                            participant.HasFallbackAttackItr = true;
                            _roleShadowDiagnostics.FallbackAttackItrCount++;
                        }
                    }
                }
            }

            BattleStageRuntimeState stage = _world?.Runtime?.Stage;
            int stageWidth = stage?.StageWidthPx ?? 800;
            int zMin = stage?.ZMin ?? 180;
            int zMax = stage?.ZMax ?? 350;
            var preferredRoot = new SpatialAabbXZ(
                0,
                zMin,
                stageWidth > 0 ? stageWidth : 1,
                zMax > zMin ? zMax : zMin + 1);
            _shadowBroadphase.Rebuild(_shadowEntries, preferredRoot);

            for (int firstOrdinal = 0;
                 firstOrdinal < _roleShadowParticipants.Count;
                 firstOrdinal++)
            {
                for (int secondOrdinal = firstOrdinal + 1;
                     secondOrdinal < _roleShadowParticipants.Count;
                     secondOrdinal++)
                {
                    if ((ShadowDirectionMayOverlap(firstOrdinal, secondOrdinal) ||
                         ShadowDirectionMayOverlap(secondOrdinal, firstOrdinal)) &&
                        TryBuildPairKey(
                            _roleShadowParticipants[firstOrdinal].RuntimeSlot,
                            _roleShadowParticipants[secondOrdinal].RuntimeSlot,
                            out long pairKey))
                    {
                        _shadowBrutePairs.Add(pairKey);
                    }
                }
            }

            for (int itrEntryIndex = 0;
                 itrEntryIndex < _roleShadowItrEntries.Count;
                 itrEntryIndex++)
            {
                RoleAwareShadowItrEntry itrEntry = _roleShadowItrEntries[itrEntryIndex];
                RoleAwareShadowParticipant attacker =
                    _roleShadowParticipants[itrEntry.ParticipantOrdinal];
                _shadowBroadphase.Query(itrEntry.Bounds, _shadowQueryIndices);
                for (int resultIndex = 0; resultIndex < _shadowQueryIndices.Count; resultIndex++)
                {
                    int bodyEntryIndex = _shadowQueryIndices[resultIndex];
                    if (bodyEntryIndex < 0 || bodyEntryIndex >= _shadowEntries.Count)
                        throw new InvalidOperationException("Invalid role-aware body query result.");
                    SpatialBroadphaseEntry bodyEntry = _shadowEntries[bodyEntryIndex];
                    if (TryBuildPairKey(
                            attacker.RuntimeSlot,
                            bodyEntry.RuntimeSlot,
                            out long pairKey))
                    {
                        _shadowTreePairs.Add(pairKey);
                    }
                }
            }

            for (int attackerOrdinal = 0;
                 attackerOrdinal < _roleShadowParticipants.Count;
                 attackerOrdinal++)
            {
                RoleAwareShadowParticipant attacker =
                    _roleShadowParticipants[attackerOrdinal];
                if (!attacker.HasAttackItr)
                    continue;

                for (int targetOrdinal = 0;
                     targetOrdinal < _roleShadowParticipants.Count;
                     targetOrdinal++)
                {
                    if (attackerOrdinal == targetOrdinal)
                        continue;
                    RoleAwareShadowParticipant target =
                        _roleShadowParticipants[targetOrdinal];
                    if (!target.HasBody ||
                        (!attacker.HasFallbackAttackItr && !target.HasFallbackBody))
                    {
                        continue;
                    }

                    if (TryBuildPairKey(
                            attacker.RuntimeSlot,
                            target.RuntimeSlot,
                            out long pairKey))
                    {
                        _shadowTreePairs.Add(pairKey);
                    }
                }
            }

            SortAndDeduplicate(_shadowBrutePairs);
            SortAndDeduplicate(_shadowTreePairs);
            _roleShadowDiagnostics.ParticipantCount = _roleShadowParticipants.Count;
            _roleShadowDiagnostics.IndexedBodyCount = _shadowEntries.Count;
            _roleShadowDiagnostics.QueriedAttackItrCount = _roleShadowItrEntries.Count;
            _shadowDiagnostics.IndexedCount = _roleShadowParticipants.Count;
            _shadowDiagnostics.FallbackCount =
                CountRoleAwareFallbackParticipants();
        }

        private void CompareShadowBroadphaseResults()
        {
            _shadowAcceptedPairs.Clear();
            foreach (KeyValuePair<LF2Entity, List<SceneQueryHit>> pair in _candidateCache)
            {
                int attackerSlot = pair.Key?.Runtime?.SlotIndex ?? -1;
                List<SceneQueryHit> hits = pair.Value;
                int count = pair.Key?.Runtime?.HitCandidateCount ?? hits?.Count ?? 0;
                if (hits == null)
                    continue;
                if (count > hits.Count)
                    count = hits.Count;

                for (int i = 0; i < count; i++)
                {
                    if (TryBuildPairKey(attackerSlot, hits[i].TargetSlot, out long pairKey))
                        _shadowAcceptedPairs.Add(pairKey);
                }
            }

            SortAndDeduplicate(_shadowAcceptedPairs);
            _shadowDiagnostics.BrutePairCount = _shadowBrutePairs.Count;
            _shadowDiagnostics.QuadtreePairCount = _shadowTreePairs.Count;
            _shadowDiagnostics.AcceptedPairCount = _shadowAcceptedPairs.Count;
            _roleShadowDiagnostics.BrutePairCount = _shadowBrutePairs.Count;
            _roleShadowDiagnostics.QuadtreePairCount = _shadowTreePairs.Count;
            _roleShadowDiagnostics.AcceptedPairCount = _shadowAcceptedPairs.Count;

            int bruteIndex = 0;
            int treeIndex = 0;
            while (bruteIndex < _shadowBrutePairs.Count || treeIndex < _shadowTreePairs.Count)
            {
                if (treeIndex >= _shadowTreePairs.Count ||
                    (bruteIndex < _shadowBrutePairs.Count &&
                     _shadowBrutePairs[bruteIndex] < _shadowTreePairs[treeIndex]))
                {
                    _shadowDiagnostics.MismatchCount++;
                    _roleShadowDiagnostics.MismatchCount++;
                    if (_shadowDiagnostics.FirstMissingPair < 0)
                        _shadowDiagnostics.FirstMissingPair = _shadowBrutePairs[bruteIndex];
                    _roleShadowDiagnostics.RecordFirstDifference(
                        _shadowBrutePairs[bruteIndex],
                        RoleAwareCollisionShadowDifference.MissingFromQuadtree);
                    bruteIndex++;
                }
                else if (bruteIndex >= _shadowBrutePairs.Count ||
                         _shadowTreePairs[treeIndex] < _shadowBrutePairs[bruteIndex])
                {
                    _shadowDiagnostics.MismatchCount++;
                    _roleShadowDiagnostics.MismatchCount++;
                    if (_shadowDiagnostics.FirstExtraPair < 0)
                        _shadowDiagnostics.FirstExtraPair = _shadowTreePairs[treeIndex];
                    _roleShadowDiagnostics.RecordFirstDifference(
                        _shadowTreePairs[treeIndex],
                        RoleAwareCollisionShadowDifference.ExtraInQuadtree);
                    treeIndex++;
                }
                else
                {
                    bruteIndex++;
                    treeIndex++;
                }
            }

            for (int i = 0; i < _shadowAcceptedPairs.Count; i++)
            {
                long pairKey = _shadowAcceptedPairs[i];
                if (_shadowTreePairs.BinarySearch(pairKey) >= 0)
                    continue;

                _shadowDiagnostics.MismatchCount++;
                _roleShadowDiagnostics.MismatchCount++;
                if (_shadowDiagnostics.FirstAcceptedPairMissingFromTree < 0)
                    _shadowDiagnostics.FirstAcceptedPairMissingFromTree = pairKey;
                _roleShadowDiagnostics.RecordFirstDifference(
                    pairKey,
                    RoleAwareCollisionShadowDifference.AcceptedPairMissingFromQuadtree);
            }
        }

        private bool ShadowDirectionMayOverlap(int attackerOrdinal, int targetOrdinal)
        {
            RoleAwareShadowParticipant attacker = _roleShadowParticipants[attackerOrdinal];
            RoleAwareShadowParticipant target = _roleShadowParticipants[targetOrdinal];
            if (!attacker.HasAttackItr || !target.HasBody)
                return false;
            if (attacker.HasFallbackAttackItr || target.HasFallbackBody)
                return true;

            for (int itrIndex = 0; itrIndex < _roleShadowItrEntries.Count; itrIndex++)
            {
                RoleAwareShadowItrEntry itr = _roleShadowItrEntries[itrIndex];
                if (itr.ParticipantOrdinal != attackerOrdinal)
                    continue;

                for (int bodyIndex = 0; bodyIndex < _shadowEntries.Count; bodyIndex++)
                {
                    SpatialBroadphaseEntry body = _shadowEntries[bodyIndex];
                    if (body.RuntimeSlot == target.RuntimeSlot &&
                        itr.Bounds.Overlaps(body.Bounds))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int CountRoleAwareFallbackParticipants()
        {
            int count = 0;
            for (int i = 0; i < _roleShadowParticipants.Count; i++)
            {
                RoleAwareShadowParticipant participant = _roleShadowParticipants[i];
                if (participant.HasFallbackBody || participant.HasFallbackAttackItr)
                    count++;
            }
            return count;
        }

        private void AbortRoleAwareShadow()
        {
            _roleShadowParticipants.Clear();
            _roleShadowItrEntries.Clear();
            _shadowEntries.Clear();
            _shadowQueryIndices.Clear();
            _shadowBrutePairs.Clear();
            _shadowTreePairs.Clear();
            _shadowAcceptedPairs.Clear();
            _shadowSlotToOrdinal.Clear();
            _roleShadowDiagnostics.Abort();
            ResetLegacyShadowDiagnosticsAfterAbort();
        }

        private void ResetLegacyShadowDiagnosticsAfterAbort()
        {
            _shadowDiagnostics.IndexedCount = 0;
            _shadowDiagnostics.FallbackCount = 0;
            _shadowDiagnostics.BrutePairCount = 0;
            _shadowDiagnostics.QuadtreePairCount = 0;
            _shadowDiagnostics.AcceptedPairCount = 0;
            _shadowDiagnostics.MismatchCount = 0;
            _shadowDiagnostics.FirstMissingPair = -1;
            _shadowDiagnostics.FirstExtraPair = -1;
            _shadowDiagnostics.FirstAcceptedPairMissingFromTree = -1;
        }

        private static bool TryBuildPairKey(int firstSlot, int secondSlot, out long pairKey)
        {
            pairKey = -1;
            if (firstSlot < 0 || secondSlot < 0 || firstSlot == secondSlot)
                return false;

            uint min = (uint)(firstSlot < secondSlot ? firstSlot : secondSlot);
            uint max = (uint)(firstSlot < secondSlot ? secondSlot : firstSlot);
            pairKey = ((long)min << 32) | max;
            return true;
        }

        private static void SortAndDeduplicate(List<long> values)
        {
            if (values.Count < 2)
                return;

            values.Sort();
            int write = 1;
            long previous = values[0];
            for (int read = 1; read < values.Count; read++)
            {
                long value = values[read];
                if (value == previous)
                    continue;
                values[write++] = value;
                previous = value;
            }

            if (write < values.Count)
                values.RemoveRange(write, values.Count - write);
        }

        public bool TryGetCollisionCandidateSequence(LF2Entity attacker, out List<SceneQueryHit> candidates)
        {
            // C++ release step6 会先固定本 tick 的碰撞候选快照。
            // step7/step9 只允许消费这份快照，不能对“本帧后续才出现的对象”
            // 回退到即时 QueryBodyHits，否则会让 step8 或同帧新增对象提前参与碰撞。
            if (_consumeCandidateCache)
            {
                if (attacker != null && _candidateCache.TryGetValue(attacker, out candidates))
                    return true;

                candidates = _emptyCandidateHits;
                return true;
            }

            candidates = null;
            return false;
        }

        public void EndCollisionCandidateConsumption()
        {
            _consumeCandidateCache = false;
            _candidateCache.Clear();
        }

        private List<SceneQueryHit> QueryBodyHitsImmediate(LF2Entity attacker, LF2FrameData attackerFrame, InteractionArea itr)
        {
            _tmpHitResult.Clear();
            if (attacker == null || attacker.PS == null || attackerFrame == null || itr == null)
                return _tmpHitResult;
            if (IsPendingFlushDestroy(attacker))
                return _tmpHitResult;
            if (GetAuthoredCurrentFrame(attacker) == null)
                return _tmpHitResult;

            LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
            if (attackerCollisionFrame?.itrs == null || attackerCollisionFrame.itrs.Count == 0)
                return _tmpHitResult;

            List<LF2Entity> spatialTargets = null;
            bool spatial = TryBuildImmediateItrAabb(
                               attacker,
                               attackerCollisionFrame,
                               itr,
                               out SpatialAabbXZ itrBounds) &&
                           TryGetImmediateSpatialTargets(itrBounds, out spatialTargets);
            if (!spatial)
                _world.GetAllEntities(_tmpAllObjects);
            List<LF2Entity> source = spatial ? spatialTargets : _tmpAllObjects;

            for (int i = 0; i < source.Count; i++)
            {
                LF2Entity target = source[i];
                if (target == attacker || target == null || target.PS == null) continue;
                if (IsPendingFlushDestroy(target)) continue;

                LF2FrameData targetCurrentFrame = GetAuthoredCurrentFrame(target);
                LF2FrameData targetCollisionFrame = target.GetCollisionFrameData();
                if (targetCurrentFrame == null) continue;
                if (!HasAnyReleaseBody(targetCurrentFrame)) continue;
                if (!HasAnyReleaseBody(targetCollisionFrame)) continue;
                if (!ImmediateQueryPairAllowed(attacker, target))
                    continue;
                if (!ItrAllowed(attacker, attackerFrame, itr, target, targetCurrentFrame))
                    continue;
                if (!HitsTarget(attacker, attackerCollisionFrame, itr, target, targetCollisionFrame, out int bodyX))
                    continue;
                if (!CandidateAccepts(attacker, attackerFrame, itr, target, targetCurrentFrame, bodyX))
                    continue;

                _tmpHitResult.Add(new SceneQueryHit(target, bodyX));
            }

            return _tmpHitResult;
        }

        private static bool IsCollisionCandidateSuppressed(LF2Entity entity, int currentTick)
        {
            if (entity?.Runtime == null)
                return false;

            int untilTick = entity.Runtime.SuppressCollisionCandidateUntilTick;
            return untilTick > 0 && currentTick < untilTick;
        }

        private void CollectCandidatesForPair(LF2Entity attacker, LF2Entity target)
        {
            if (attacker == null || target == null || attacker == target)
                return;

            if (!CandidateCollectionPairAllowed(attacker, target))
                return;

            LF2FrameData attackerCurrentFrame = GetAuthoredCurrentFrame(attacker);
            if (attackerCurrentFrame == null)
                return;
            LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
            if (attackerCurrentFrame?.itrs == null || attackerCurrentFrame.itrs.Count == 0)
                return;

            if (!_candidateCache.TryGetValue(attacker, out var dst))
                return;

            LF2FrameData targetCurrentFrame = GetAuthoredCurrentFrame(target);
            LF2FrameData targetCollisionFrame = target.GetCollisionFrameData();
            if (targetCurrentFrame == null || !HasAnyReleaseBody(targetCurrentFrame))
            {
                return;
            }

            if (!PassesReleaseCoarsePrefilter(
                    attacker,
                    attackerCurrentFrame,
                    attackerCollisionFrame,
                    target,
                    targetCurrentFrame,
                    targetCollisionFrame))
            {
                return;
            }

            for (int itrIndex = 0; itrIndex < attackerCollisionFrame.itrs.Count; itrIndex++)
            {
                InteractionArea itr = attackerCollisionFrame.itrs[itrIndex];
                if (itr == null) continue;
                // C++ release step6 collect 对 kind=5 的过滤与几何检测都基于原始 itr，
                // 不能提前套消费侧的 runtime 替换结果。
                if (!ItrAllowed(attacker, attackerCurrentFrame, itr, target, targetCurrentFrame))
                    continue;
                if (HitsTarget(attacker, attackerCollisionFrame, itr, target, targetCollisionFrame, out int bodyX) &&
                    CandidateAccepts(attacker, attackerCurrentFrame, itr, target, targetCurrentFrame, bodyX))
                {
                    TryRecordReleaseCandidate(attacker, target, itr, targetCurrentFrame, bodyX, itrIndex, dst);
                }
            }
        }

        private void TryRecordReleaseCandidate(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            LF2FrameData targetFrame,
            int bodyX,
            int itrIndex,
            List<SceneQueryHit> dst)
        {
            if (attacker == null || target == null || itr == null || targetFrame == null || dst == null)
                return;

            int rejectFlag = ResolveReleaseRejectFlag(attacker, itr, target, targetFrame, bodyX);
            if (rejectFlag == 2)
                return;

            if (TryRecordNearestPathCandidate(
                    attacker,
                    target,
                    itr,
                    targetFrame,
                    bodyX,
                    itrIndex,
                    rejectFlag,
                    dst))
                return;

            int candidateCount = attacker.Runtime?.HitCandidateCount ?? dst.Count;
            if (candidateCount >= HitCandidateMax)
                return;

            rejectFlag = ApplyPrev2GroundRejectFlag(attacker, target, itr, targetFrame, rejectFlag);
            if (rejectFlag == 2)
                return;

            if (!AcceptReleaseSelectFlagCandidate(attacker, target, itr, targetFrame, rejectFlag))
                return;

            SceneQueryHit candidate = new SceneQueryHit(target, bodyX, itrIndex, itr);
            if (candidateCount < dst.Count)
                dst[candidateCount] = candidate;
            else
                dst.Add(candidate);

            attacker.Runtime.HitCandidateCount = candidateCount + 1;
        }

        private bool TryRecordNearestPathCandidate(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            LF2FrameData targetFrame,
            int bodyX,
            int itrIndex,
            int rejectFlag,
            List<SceneQueryHit> dst)
        {
            if (attacker == null || target == null || itr == null || targetFrame == null || dst == null)
                return false;

            if (rejectFlag != 0)
                return false;

            if (!IsReleaseNearestCandidatePath(itr))
                return false;

            int targetPrev2State = GetAuthoredPrev2Frame(target)?.state
                                   ?? targetFrame.state;
            if (targetPrev2State == LF2States.WeaponOnGround)
            {
                bool attackerSpecialOk = GetCurrentDataObjectType(attacker) > 0 && attacker.Runtime.LinkState >= 0;
                if (!attackerSpecialOk)
                    return false;
            }

            int distance = ResolveNearestCandidateDistance(attacker, target);
            int bestDistance = attacker.Runtime?.HitCandidateNearestDistance ?? CandidateDistanceUnset;
            if (bestDistance != CandidateDistanceUnset)
            {
                bool replace = distance < bestDistance;
                if (!replace && distance == bestDistance)
                    replace = (attacker.Match?.Rng?.NextInt(0, 2) ?? 0) == 0;
                if (!replace)
                    return true;
            }

            attacker.Runtime.HitCandidateNearestDistance = distance;
            // C++ release 在 nearest path 接管并替换更近目标时，
            // 会把 cand0 写成当前目标并把 candidate_count 直接重置为 1。
            dst.Clear();
            dst.Add(new SceneQueryHit(target, bodyX, itrIndex, itr));
            attacker.Runtime.HitCandidateCount = 1;
            return true;
        }

        internal static InteractionArea ResolveRuntimeItrForPair(
            LF2Entity attacker,
            LF2Entity target,
            LF2FrameData attackerFrame,
            InteractionArea sourceItr,
            out bool zeroAttackerHpOnConsume,
            out bool releaseHeavyHeldTargetOnConsume)
        {
            zeroAttackerHpOnConsume = false;
            releaseHeavyHeldTargetOnConsume = false;
            if (attacker == null || target == null || sourceItr == null)
                return sourceItr;

            InteractionArea itr = sourceItr;
            bool copied = false;

            if (sourceItr.kind == 5 && attacker.Runtime.LinkState < 0)
            {
                int holderSlot = ResolveKind5HolderRuntimeSlot(attacker);
                if (holderSlot >= 0)
                {
                    LF2Entity holder = attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
                    LF2FrameData holderFrame = holder?.GetCollisionFrameData();
                    if (holder != null &&
                        holderFrame != null &&
                        holder.Runtime.TargetSlotIndex == (attacker.Runtime?.SlotIndex ?? -1))
                    {
                        // C++ release collision.cpp 0x42CA6E~0x42CA7F:
                        // kind=5 替换读取的是 holder 碰撞帧（prev2）的 wpoint.attacking，
                        // 不是 hit_j，也不是 1-based 索引。
                        int attackingItrIndex =
                            holderFrame.wpoints != null && holderFrame.wpoints.Count > 0
                                ? holderFrame.wpoints[0].attacking
                                : 0;
                        int targetSlot = target.Runtime?.SlotIndex ?? -1;
                        if (attackingItrIndex > 0 &&
                            holderSlot != targetSlot &&
                            holderFrame.itrs != null &&
                            attackingItrIndex < holderFrame.itrs.Count)
                        {
                            itr = sourceItr.ShallowCopy();
                            InteractionArea holderItr = holderFrame.itrs[attackingItrIndex];
                            // C++ release collision.cpp 这里保留原 kind=5 的几何框，
                            // 只把伤害相关字段替成 holder itr，并把 kind 强制改成 0。
                            // 这一步发生在消费侧；step6 candidate collect 不能拿这份替换后的
                            // itr 去绕过 kind=5 自己的过滤链。
                            itr.kind = 0;
                            itr.dvx = holderItr.dvx;
                            itr.dvy = holderItr.dvy;
                            itr.fall = holderItr.fall;
                            itr.bdefend = holderItr.bdefend;
                            itr.injury = holderItr.injury;
                            itr.arest = holderItr.arest;
                            itr.vrest = holderItr.vrest;
                            itr.effect = holderItr.effect;
                            itr.attacking = holderItr.attacking;
                            itr.catchingact = holderItr.catchingact;
                            itr.catchingact2 = holderItr.catchingact2;
                            itr.caughtact = holderItr.caughtact;
                            itr.caughtact2 = holderItr.caughtact2;
                            itr.respond = holderItr.respond;
                            itr.pickingact = holderItr.pickingact;
                            itr.pickedact = holderItr.pickedact;
                            itr.throwvx = holderItr.throwvx;
                            itr.throwvy = holderItr.throwvy;
                            itr.zwidth = holderItr.zwidth;
                            itr.throwvz = holderItr.throwvz;
                            itr.throwinjury = holderItr.throwinjury;
                            copied = true;
                        }
                    }
                }
            }

            if (itr.kind == 4 && attacker.WeaponCount > 0)
            {
                if (!copied)
                {
                    itr = sourceItr.ShallowCopy();
                    copied = true;
                }

                itr.kind = 0;
                bool facingRight = attacker.Dirh() > 0;
                double vx = attacker.Runtime?.Vx ?? 0.0;
                if ((vx > 0f && !facingRight) || (vx < 0f && facingRight))
                    itr.dvx = -itr.dvx;
            }

            if (target.Runtime != null && target.Runtime.LinkState == 2 && itr.kind == 0)
            {
                int heldTargetSlot = target.Runtime.TargetSlotIndex;
                if (heldTargetSlot >= 0)
                {
                    LF2Entity heldTarget = target.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
                    if (heldTarget != null &&
                        heldTarget.Runtime?.HolderStableId == (target.Runtime?.SlotIndex ?? -1) &&
                        heldTarget.Runtime.LinkState == -2)
                    {
                        releaseHeavyHeldTargetOnConsume = true;
                    }
                }
            }

            if (GetCurrentDataObjectType(target) == (int)LF2ObjectType.HeavyWeapon)
            {
                if (!copied)
                {
                    itr = sourceItr.ShallowCopy();
                    copied = true;
                }

                itr.dvx /= 2;
                itr.dvy /= 2;
            }

            if (itr.kind == 9)
            {
                if (GetCurrentDataObjectType(target) == (int)LF2ObjectType.Character)
                {
                    if (!copied)
                    {
                        itr = sourceItr.ShallowCopy();
                        copied = true;
                    }

                    itr.kind = 0;
                    zeroAttackerHpOnConsume = true;
                }
                else
                {
                    int targetState = target.Frame?.D?.state ?? 0;
                    if (targetState == 1002 || targetState == 2000)
                    {
                        if (!copied)
                        {
                            itr = sourceItr.ShallowCopy();
                            copied = true;
                        }

                        itr.kind = 0;
                    }
                }
            }

            return itr;
        }

        private bool AcceptReleaseSelectFlagCandidate(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            LF2FrameData targetFrame,
            int rejectFlag)
        {
            if (attacker == null || target == null || itr == null || targetFrame == null)
                return false;

            int kind = itr.kind;
            int selectFlag = rejectFlag;

            if (rejectFlag == 0 && kind == 1 && !AcceptReleaseKind1Nearest(attacker, target))
                selectFlag = 2;

            if (kind == 4 && attacker.WeaponCount != 0 && selectFlag != 2)
                selectFlag = 1;

            if (kind != 1 && kind != 2 && kind != 7 && selectFlag != 2)
                selectFlag = 1;

            switch (kind)
            {
                case 1:
                    if (selectFlag != 2 && AcceptReleaseKind1TowardVictim(attacker, target, targetFrame))
                        selectFlag = 1;
                    break;
                case 2:
                    if (selectFlag != 2 && AcceptReleaseKind2Candidate(attacker, targetFrame.state))
                        selectFlag = 1;
                    break;
                case 7:
                    if (selectFlag != 2 && AcceptReleaseKind7Candidate(attacker, targetFrame.state))
                        selectFlag = 1;
                    break;
            }

            return selectFlag == 1;
        }

        private bool PassesReleaseCoarsePrefilter(
            LF2Entity attacker,
            LF2FrameData attackerCurrentFrame,
            LF2FrameData attackerCollisionFrame,
            LF2Entity target,
            LF2FrameData targetCurrentFrame,
            LF2FrameData targetCollisionFrame)
        {
            if (attacker == null || target == null)
                return false;
            if (attackerCurrentFrame?.itrs == null || attackerCurrentFrame.itrs.Count == 0)
                return false;
            if (attackerCollisionFrame?.itrs == null || attackerCollisionFrame.itrs.Count == 0)
                return false;
            if (!HasAnyReleaseBody(targetCollisionFrame))
                return false;
            if (!TryUnionBodyRect(targetCollisionFrame, out LocalRect bodyUnion, out bool fullHeight))
                return false;

            WorldRect bodyWorld = LocalRectWorldRect(target, targetCollisionFrame, bodyUnion, fullHeight);
            if (TryUnionItrRect(attackerCollisionFrame, out LocalRect itrUnion))
            {
                WorldRect itrWorld = LocalRectWorldRect(attacker, attackerCollisionFrame, itrUnion, fullHeight: false);
                if (Overlap(itrWorld, bodyWorld))
                    return true;
            }

            // kind=5 uses its own pair filters. Keep it out of the ordinary union so its
            // authored probe rectangle cannot inflate unrelated coarse bounds, but still
            // allow a frame containing only kind=5 to reach the per-itr candidate pass.
            int targetType = GetCurrentDataObjectType(target);
            int targetState = targetCurrentFrame?.state ?? 0;
            for (int i = 0; i < attackerCollisionFrame.itrs.Count; i++)
            {
                InteractionArea itr = attackerCollisionFrame.itrs[i];
                if (itr == null || itr.kind != 5 || !IsReleaseItrGeometry(itr))
                    continue;
                if (!Kind5Allowed(attacker, target, targetState, targetType))
                    continue;

                WorldRect kind5World = ItrWorldRect(attacker, attackerCollisionFrame, itr);
                if (Overlap(kind5World, bodyWorld))
                    return true;
            }

            return false;
        }

        private static bool IsReleaseNearestCandidatePath(InteractionArea itr)
        {
            return itr != null && itr.vrest == 0 && itr.kind != 1 && itr.kind != 2 && itr.kind != 7;
        }

        internal static int ResolveNearestCandidateDistance(LF2Entity attacker, LF2Entity target)
        {
            if (attacker?.PS == null || target?.PS == null)
                return int.MaxValue;

            int targetX = target.Runtime != null ? target.Runtime.XInt : (int)target.PS.x;
            int holderSlot = ResolveReleaseNegativeLinkHolderSlot(attacker);
            if (attacker.Runtime.LinkState < 0 && holderSlot >= 0)
            {
                if (holderSlot == (target.Runtime?.SlotIndex ?? -1))
                    return 2000;

                LF2Entity holder = attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
                if (holder?.PS != null)
                    return UnityEngine.Mathf.Abs((holder.Runtime != null ? holder.Runtime.XInt : (int)holder.PS.x) - targetX);
            }

            return UnityEngine.Mathf.Abs((attacker.Runtime != null ? attacker.Runtime.XInt : (int)attacker.PS.x) - targetX);
        }

        private bool AcceptReleaseKind1Nearest(LF2Entity attacker, LF2Entity target)
        {
            if (attacker?.PS == null || target?.PS == null)
                return false;

            int attackerX = attacker.Runtime != null ? attacker.Runtime.XInt : (int)attacker.PS.x;
            int targetX = target.Runtime != null ? target.Runtime.XInt : (int)target.PS.x;
            int distance = UnityEngine.Mathf.Abs(attackerX - targetX);
            int bestDistance = target.Runtime?.HitCandidateKind1Distance ?? CandidateDistanceUnset;
            if (bestDistance != CandidateDistanceUnset)
            {
                bool replace = distance < bestDistance;
                if (!replace && distance == bestDistance)
                    replace = (attacker.Match?.Rng?.NextInt(0, 2) ?? 0) == 0;
                if (!replace)
                    return false;
            }

            target.Runtime.HitCandidateKind1Distance = distance;
            return true;
        }

        private bool AcceptReleaseKind1TowardVictim(LF2Entity attacker, LF2Entity target, LF2FrameData targetFrame)
        {
            if (attacker?.PS == null || target?.PS == null || targetFrame == null)
                return false;

            bool towardVictim = false;
            bool right = IsRightPressed(attacker);
            bool left = IsLeftPressed(attacker);
            int attackerX = attacker.Runtime != null ? attacker.Runtime.XInt : (int)attacker.PS.x;
            int targetX = target.Runtime != null ? target.Runtime.XInt : (int)target.PS.x;
            if (right && attackerX < targetX) towardVictim = true;
            if (left && attackerX >= targetX) towardVictim = true;

            return towardVictim && targetFrame.state == LF2States.Injured2;
        }

        private static bool AcceptReleaseKind2Candidate(LF2Entity attacker, int targetState)
        {
            if (attacker == null)
                return false;

            bool jumpFresh = IsJumpFresh(attacker);
            if (attacker.Runtime.LinkState == 0 && jumpFresh && targetState == LF2States.WeaponOnGround)
                return true;
            if (jumpFresh && targetState == LF2States.HeavyWeaponOnGround)
                return true;
            return false;
        }

        private static bool AcceptReleaseKind7Candidate(LF2Entity attacker, int targetState)
        {
            return attacker != null &&
                   IsJumpFresh(attacker) &&
                   targetState == LF2States.WeaponOnGround;
        }

        private static bool IsJumpFresh(LF2Entity entity)
        {
            if (entity is not LF2Character character)
                return false;

            return character.Runtime.KeyJump != 0 && character.Runtime.PrevJump == 0;
        }

        private static bool IsLeftPressed(LF2Entity entity)
        {
            if (entity is LF2Character character)
                return character.Runtime.KeyLeft != 0;
            return false;
        }

        private static bool IsRightPressed(LF2Entity entity)
        {
            if (entity is LF2Character character)
                return character.Runtime.KeyRight != 0;
            return false;
        }

        internal static bool IsReleaseConsumerPairBlocked(LF2Entity attacker, LF2Entity target)
        {
            // 中文注释：
            // 这里先只保留 C++ consume 路径里明确可确认的“被抓目标”拦截。
            // same-team / zero-team child / direct-owner 这些关系，
            // 正式版当前可见实现并不是通过一条全局 pair gate 统一拦掉，
            // 而是分散在 kind-group / kind=5 / nearest-path / link 语义里。
            // Unity 之前额外叠了一层全局闸门，会把本该进入正式筛选链的 pair
            // 提前挡掉，导致 type=3/opoint 行为继续偏离 C++。
            return TargetBeingCaughtPairBlocked(attacker, target);
        }

        internal static bool RuntimeConsumeItrAllowed(LF2Entity attacker, InteractionArea itr, LF2Entity target)
        {
            if (attacker == null || itr == null || target == null)
                return false;
            if (IsReleaseConsumerPairBlocked(attacker, target))
                return false;

            LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
            LF2FrameData targetCurrentFrame = GetAuthoredCurrentFrame(target);
            if (attackerCollisionFrame == null || targetCurrentFrame == null)
                return false;

            return ItrAllowed(attacker, attackerCollisionFrame, itr, target, targetCurrentFrame);
        }

        private static bool CandidateCollectionPairAllowed(LF2Entity attacker, LF2Entity target)
        {
            if (attacker == null || target == null)
                return false;
            if (IsPendingFlushDestroy(attacker) || IsPendingFlushDestroy(target))
                return false;
            if (IsPureTransitionSmoke(attacker) || IsPureTransitionSmoke(target))
                return false;
            if (attacker.AttackExempt > 0)
                return false;

            int attackerVrestKey = attacker.Runtime?.SlotIndex ?? -1;
            if (attackerVrestKey >= 0 && target.ItrRest != null && target.ItrRest.HasVrest(attackerVrestKey))
                return false;
            if (IsBlockedReleasePair(attacker, target))
                return false;

            return true;
        }

        private static bool IsBlockedReleasePair(LF2Entity attacker, LF2Entity target)
        {
            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            if (attackerOid != 200 &&
                attackerOid != 203 &&
                attackerOid != 205 &&
                attackerOid != 206 &&
                attackerOid != 207 &&
                attackerOid != 215 &&
                attackerOid != 216)
            {
                return false;
            }

            LF2FrameData targetFrame = target?.Frame?.D;
            return LF2Entity.ResolveCurrentDataObjectId(target) == 9 &&
                   (target.Frame?.N ?? -1) == 301 &&
                   targetFrame != null &&
                   targetFrame.hit_a == 999 &&
                   targetFrame.hit_d == 999 &&
                   targetFrame.hit_j == 999 &&
                   attacker.RelationTeam == target.RelationTeam &&
                   attacker.RelationTeam != 0;
        }

        private static bool ImmediateQueryPairAllowed(LF2Entity attacker, LF2Entity target)
        {
            if (attacker == null || target == null)
                return false;
            if (IsPendingFlushDestroy(attacker) || IsPendingFlushDestroy(target))
                return false;
            if (TargetBeingCaughtPairBlocked(attacker, target))
                return false;
            return CandidateCollectionPairAllowed(attacker, target);
        }

        private static bool TargetBeingCaughtPairBlocked(LF2Entity attacker, LF2Entity target)
        {
            if (attacker == null || target == null)
                return false;

            LF2FrameData targetPrev2Frame = GetAuthoredPrev2Frame(target);
            CatchPoint targetCpoint = targetPrev2Frame?.cpoint;
            if (targetCpoint == null || targetCpoint.kind != 2)
                return false;

            int catcherSlot = target.CatcherSlotIndex;
            if (catcherSlot < 0)
                return false;

            LF2Entity catcher = target.Match?.FindEntityByRuntimeSlotForQuery(catcherSlot);
            if (catcher == null || catcher.CaughtSlotIndex != (attacker.Runtime?.SlotIndex ?? -1))
                return false;

            CatchPoint catcherPrev2Cpoint = GetAuthoredPrev2Frame(catcher)?.cpoint;
            return catcherPrev2Cpoint == null || catcherPrev2Cpoint.hurtable == 0;
        }

        private static bool IsPureTransitionSmoke(LF2Entity entity)
        {
            if (entity == null || LF2Entity.ResolveCurrentDataObjectId(entity) != 999)
                return false;

            if (entity.Frame != null && (entity.Frame.N < 0 || entity.Frame.N >= LF2FrameCache.MaxFrameIdExclusive))
                return true;

            if (entity.Runtime != null &&
                entity.Runtime.SpawnSemantic == (int)LF2Tasks.ReleaseSpawnSemantic.TransitionEffect)
            {
                return true;
            }

            LF2FrameData frame = entity.GetCollisionFrameData();
            if (frame == null)
                return false;

            // broken_weapon.dat 在 release 路径里只有 frame 399 带 bdy，
            // 其余 frame 都是碎片 / 烟雾展示路径。这里按数据语义过滤，
            // 避免 60~167 这类纯展示帧漏进碰撞链。
            // 但 120~138 冰碎片 authored chain 是 next=101，不应再被 “pic==999”
            // 粗暴归为纯烟雾，否则会把非 transition 的 999 物体误过滤掉。
            if (frame.bodies == null || frame.bodies.Count == 0)
                return frame.state == 3005 || (frame.pic == 999 && frame.next == 1000);

            return frame.state == 3005 || (frame.pic == 999 && frame.next == 1000);
        }

        private static bool IsPendingFlushDestroy(LF2Entity entity)
        {
            return entity?.Runtime != null && entity.Runtime.PendingFlushDestroy;
        }

        private static LF2FrameData GetAuthoredCurrentFrame(LF2Entity entity)
        {
            if (entity?.Frame == null || entity.FrameCache?.HasFrame(entity.Frame.N) != true)
                return null;

            return entity.Frame.D;
        }

        private static LF2FrameData GetAuthoredPrev2Frame(LF2Entity entity)
        {
            if (entity?.Frame == null || entity.FrameCache?.HasFrame(entity.Frame.Prev2) != true)
                return null;

            return entity.Frame.Prev2D;
        }

        private static int GetCurrentDataObjectType(LF2Entity entity)
        {
            return LF2Entity.ResolveCurrentDataObjectType(entity);
        }

        private static bool ItrAllowed(
            LF2Entity attacker,
            LF2FrameData attackerFrame,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetFrame)
        {
            if (attacker == null || itr == null || target == null || targetFrame == null)
                return false;

            int kind = itr.kind;
            if (!IsReleaseItrGeometry(itr))
                return false;
            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            int targetOid = LF2Entity.ResolveCurrentDataObjectId(target);
            int targetType = GetCurrentDataObjectType(target);
            int targetState = targetFrame?.state ?? 0;
            LF2FrameData attackerCurrentFrame = GetAuthoredCurrentFrame(attacker) ?? attackerFrame;
            LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData() ?? attackerFrame;

            if (IsBlockedReleaseOidInteraction(attackerOid, targetOid, kind))
                return false;
            if ((kind == 3 || kind == 8) && targetType != (int)LF2ObjectType.Character)
                return false;
            // C++ collect path uses the active frame for the kind=8 lead-in gate,
            // the collision (prev2) frame for same-team state checks, and prev for
            // the kind=0 effect filters.
            if (kind == 8 && DeferState3005Kind8LeadIn(attacker, attackerCurrentFrame))
                return false;
            if (target.HitStun != 0 && kind != 8 && kind != 14)
                return false;

            int attackerState = attackerCollisionFrame?.state ?? 0;

            if (RunsKindGroupFilters(kind, targetType))
            {
                bool skipGroup = targetState == LF2States.Frozen || targetState == LF2States.BeingCaught;
                if (!skipGroup)
                {
                    if (targetOid == 0xD4)
                    {
                        if (attackerOid != 0xD4)
                            skipGroup = true;
                        else if ((target.Frame?.N ?? 0) % 10 == 5 && (attacker.Frame?.N ?? 0) % 10 == 0)
                            skipGroup = true;
                    }

                    if (!skipGroup && HasSameNonZeroRelationTeam(attacker, target) && kind != 8)
                    {
                        // 中文注释：
                        // 这里严格按当前 C++ release 的 collision_collect/collision 正式筛选链：
                        // 1. 只有 same-team(unk_364 相同且非 0，kind!=8) 时，才会进入这一段；
                        // 2. attacker.state==18 且 effect!=21/22 不是“额外拦截”，而是 same-team 的放行例外；
                        // 3. 若不满足这个放行例外，则：
                        //    - attacker 是角色且 target 是 type=3，朝向不同 -> 跳过；
                        //    - target.obj_type 不属于 {1,2,4,6} -> 跳过；
                        //
                        // Unity 之前把这段误写成了若干无条件 return false，
                        // 会把 C++ 正式版允许保留的 pair 直接挡掉，也会把真正的 same-team 过滤
                        // 和 weapon/type3 例外关系打乱，进而放大 205 / opoint / 烟雾链漂移。
                        bool sameTeamPassThrough =
                            attackerState == LF2States.Burning &&
                            itr.effect != 21 &&
                            itr.effect != 22;

                        if (!sameTeamPassThrough)
                        {
                            if (GetCurrentDataObjectType(attacker) == (int)LF2ObjectType.Character &&
                                targetType == (int)LF2ObjectType.SpecialAttack &&
                                !SameFacing(attacker, target))
                            {
                                return false;
                            }

                            if (!IsReleaseWeaponType(targetType))
                                return false;
                        }
                    }
                }
            }

            if (kind == 5 && !Kind5Allowed(attacker, target, targetState, targetType))
                return false;

            if (kind == 0 && !Kind0EffectAllowed(attacker, itr, target))
                return false;

            return true;
        }

        private static bool IsBlockedReleaseOidInteraction(int attackerOid, int targetOid, int kind)
        {
            if (kind == 9 || targetOid != 209)
                return false;

            return attackerOid == 200 ||
                   attackerOid == 203 ||
                   attackerOid == 205 ||
                   attackerOid == 206 ||
                   attackerOid == 207 ||
                   attackerOid == 215 ||
                   attackerOid == 216;
        }

        private static bool DeferState3005Kind8LeadIn(LF2Entity attacker, LF2FrameData activeFrame)
        {
            if (attacker == null || activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
                return false;
            if (activeFrame.hit_Fa > 0 || HasOpoint(activeFrame))
                return true;
            if (activeFrame.next <= 0 || activeFrame.next == activeFrame.frameId)
                return false;

            LF2FrameData nextFrame = attacker.GetFrameDataById(activeFrame.next);
            return nextFrame != null && (nextFrame.hit_Fa > 0 || HasOpoint(nextFrame));
        }

        private static bool HasOpoint(LF2FrameData frame)
        {
            return frame != null &&
                   ((frame.opoints != null && frame.opoints.Count > 0) || frame.opoint.HasValue);
        }

        private static bool RunsKindGroupFilters(int kind, int targetType)
        {
            return kind < 4 ||
                   kind == 6 ||
                   (kind == 9 && targetType == (int)LF2ObjectType.Character) ||
                   kind == 10 ||
                   kind == 11 ||
                   kind == 15 ||
                   kind == 16;
        }

        private static bool Kind5Allowed(LF2Entity attacker, LF2Entity target, int targetState, int targetType)
        {
            int holderSlot = ResolveKind5HolderRuntimeSlot(attacker);
            LF2Entity holder = holderSlot >= 0
                ? attacker?.Match?.FindEntityByRuntimeSlotForQuery(holderSlot)
                : null;
            int holderTeam = holder != null ? holder.RelationTeam : 0;
            if (holderTeam == 0 || holderTeam != target.RelationTeam)
                return true;
            if (targetState == LF2States.Frozen || IsReleaseWeaponType(targetType))
                return true;
            if (LF2Entity.ResolveCurrentDataObjectId(target) != 0xD4)
                return false;
            if (LF2Entity.ResolveCurrentDataObjectId(attacker) != 0xD4)
                return true;
            return (target.Frame?.N ?? 0) % 10 == 5 && (attacker.Frame?.N ?? 0) % 10 == 0;
        }

        private static bool Kind0EffectAllowed(
            LF2Entity attacker,
            InteractionArea itr,
            LF2Entity target)
        {
            int attackerPrevState = attacker.FrameCache?.GetFrameDataById(attacker.Frame?.Prev ?? 0)?.state ?? 0;
            int targetPrevState = target.FrameCache?.GetFrameDataById(target.Frame?.Prev ?? 0)?.state ?? 0;
            int targetType = GetCurrentDataObjectType(target);

            if (itr.effect == 4 && targetType == (int)LF2ObjectType.Character)
                return false;
            if (itr.effect == 20 &&
                (targetType != (int)LF2ObjectType.Character ||
                 targetPrevState == LF2States.Burning ||
                 targetPrevState == LF2States.FirenSpecific))
                return false;
            if (itr.effect == 21 &&
                (targetPrevState == LF2States.Burning || targetPrevState == LF2States.FirenSpecific))
                return false;
            if (itr.effect == 30 && target.Frame != null && target.Frame.N >= 200 && target.Frame.N <= 202)
                return false;
            if (itr.effect == 2 && attackerPrevState == LF2States.FirenSpecific && targetPrevState == LF2States.Burning)
                return false;

            return true;
        }

        private static int ResolveKind5HolderRuntimeSlot(LF2Entity attacker)
        {
            if (attacker == null)
                return -1;

            return attacker.ResolveReleaseNeutralHolderSlotOrImplicitZero();
        }

        private static bool HasSameNonZeroRelationTeam(LF2Entity a, LF2Entity b)
        {
            return a != null && b != null && a.RelationTeam == b.RelationTeam && a.RelationTeam != 0;
        }

        private static bool SameFacing(LF2Entity a, LF2Entity b)
        {
            return a?.PS != null && b?.PS != null && a.PS.dir == b.PS.dir;
        }

        private static bool IsReleaseWeaponType(int type)
        {
            return type == (int)LF2ObjectType.LightWeapon ||
                   type == (int)LF2ObjectType.HeavyWeapon ||
                   type == (int)LF2ObjectType.ThrowWeapon ||
                   type == (int)LF2ObjectType.Drink;
        }

        private bool HitsTarget(
            LF2Entity attacker,
            LF2FrameData attackerCollisionFrame,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetCollisionFrame,
            out int bodyX)
        {
            bodyX = 0;
            if (attacker == null || attacker.PS == null || attackerCollisionFrame == null || itr == null)
                return false;
            if (target == null || target.PS == null || targetCollisionFrame == null)
                return false;
            if (!HasAnyReleaseBody(targetCollisionFrame))
                return false;
            if (!IsReleaseItrGeometry(itr))
                return false;

            int attackerZ = CollisionZInt(attacker, attackerCollisionFrame);
            int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
            int zDelta = CollisionZInt(target, targetCollisionFrame) - attackerZ;
            if (zDelta >= zHalf || zDelta <= -zHalf)
                return false;

            WorldRect itrRect = ItrWorldRect(attacker, attackerCollisionFrame, itr);
            for (int b = 0; b < targetCollisionFrame.bodies.Count; b++)
            {
                BodyBox body = targetCollisionFrame.bodies[b];
                if (!IsReleaseBody(body)) continue;

                WorldRect bodyRect = BodyWorldRect(target, targetCollisionFrame, body, collectSemantics: true);
                if (!Overlap(itrRect, bodyRect)) continue;

                bodyX = body.x;
                return true;
            }

            return false;
        }

        private static bool HitsTarget(
            in PhysicsState.BattleVolume itrVolume,
            LF2Entity target,
            LF2FrameData targetCollisionFrame,
            out int bodyX)
        {
            bodyX = 0;
            if (target == null || target.PS == null || targetCollisionFrame == null)
                return false;
            if (!HasAnyReleaseBody(targetCollisionFrame))
                return false;

            for (int b = 0; b < targetCollisionFrame.bodies.Count; b++)
            {
                BodyBox body = targetCollisionFrame.bodies[b];
                if (!IsReleaseBody(body))
                    continue;

                if (!TryBuildBodyBattleVolume(target, targetCollisionFrame, body, out PhysicsState.BattleVolume bodyVolume))
                    continue;
                if (!CollisionUtil.Intersect(itrVolume, bodyVolume))
                    continue;

                bodyX = body.x;
                return true;
            }

            return false;
        }

        private static bool CandidateAccepts(
            LF2Entity attacker,
            LF2FrameData attackerFrame,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetFrame,
            int bodyX)
        {
            if (attacker == null || attackerFrame == null || itr == null || target == null || targetFrame == null)
                return false;

            // C++ release 的 collect 分层里，这一段之前已经走过：
            // 1. pair gate: attack_exempt / vrest / 特例 oid 过滤
            // 2. itr gate: kind-specific / hit_stop / same-team / effect filters
            // 3. hit test: itr 与 bdy 的真实相交
            // record path 本身不再额外做一轮 vrest/hit_stop 复判。
            _ = bodyX;
            return true;
        }

        private static int ResolveReleaseNegativeLinkHolderSlot(LF2Entity attacker)
        {
            if (attacker == null)
                return -1;

            return attacker.ResolveReleaseNegativeLinkHolderSlotOrImplicitZero();
        }

        private int ResolveReleaseRejectFlag(
            LF2Entity attacker,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetFrame,
            int bodyX)
        {
            if (attacker == null || itr == null || target == null || targetFrame == null)
                return 2;

            int kind = itr.kind;
            int targetState = targetFrame.state;

            // C++ sub419f80_record_candidate:
            // victim.state==12 && itr.fall<=40 && kind!=10/11 -> reject_flag=2
            if (targetState == LF2States.Falling &&
                itr.fall <= 40 &&
                kind != 10 &&
                kind != 11)
            {
                return 2;
            }

            if (bodyX >= 1000 &&
                IsReleaseNearestCandidatePath(itr) &&
                !NearestBodyCandidateAllowed(attacker))
                return 2;

            return 0;
        }

        private static int ApplyPrev2GroundRejectFlag(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            LF2FrameData targetFrame,
            int rejectFlag)
        {
            if (rejectFlag != 0 || attacker == null || target == null || itr == null || targetFrame == null)
                return rejectFlag;

            int targetPrev2State = GetAuthoredPrev2Frame(target)?.state
                                   ?? targetFrame.state;
            if (targetPrev2State != LF2States.WeaponOnGround)
                return rejectFlag;

            bool attackerSpecialOk = GetCurrentDataObjectType(attacker) > 0 && attacker.Runtime.LinkState >= 0;
            if (!attackerSpecialOk && itr.kind != 2 && itr.kind != 7 && itr.kind != 10)
                return 2;

            return rejectFlag;
        }

        private static bool NearestBodyCandidateAllowed(LF2Entity attacker)
        {
            if (attacker == null)
                return false;

            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            int attackerType = GetCurrentDataObjectType(attacker);
            if ((attackerType == (int)LF2ObjectType.Character || attackerOid == 201 || attackerOid == 202) &&
                attacker.RelationTeam != 5)
            {
                return true;
            }

            int holderSlot = ResolveReleaseNegativeLinkHolderSlot(attacker);
            if (attacker.Runtime.LinkState < 0 && holderSlot >= 0)
            {
                LF2Entity holder = attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
                if (holder != null &&
                    GetCurrentDataObjectType(holder) == (int)LF2ObjectType.Character &&
                    holder.RelationTeam != 5)
                {
                    return true;
                }
            }

            return false;
        }

        private static bool TryUnionItrRect(LF2FrameData frame, out LocalRect rect)
        {
            rect = default;
            if (frame?.itrs == null || frame.itrs.Count == 0)
                return false;

            bool found = false;
            long minX = 0;
            long minY = 0;
            long maxX = 0;
            long maxY = 0;
            for (int i = 0; i < frame.itrs.Count; i++)
            {
                InteractionArea itr = frame.itrs[i];
                if (!IsReleaseItrForUnion(itr))
                    continue;

                long x1 = itr.x;
                long y1 = itr.y;
                long x2 = x1 + itr.w;
                long y2 = y1 + itr.h;
                if (!found)
                {
                    found = true;
                    minX = x1;
                    minY = y1;
                    maxX = x2;
                    maxY = y2;
                }
                else
                {
                    if (x1 < minX) minX = x1;
                    if (y1 < minY) minY = y1;
                    if (x2 > maxX) maxX = x2;
                    if (y2 > maxY) maxY = y2;
                }
            }

            if (!found)
                return false;

            rect = new LocalRect(
                ClampRect(minX),
                ClampRect(minY),
                ClampRect(maxX - minX),
                ClampRect(maxY - minY));
            return true;
        }

        private static bool TryUnionBodyRect(LF2FrameData frame, out LocalRect rect, out bool fullHeight)
        {
            rect = default;
            fullHeight = false;
            if (frame?.bodies == null || frame.bodies.Count == 0)
                return false;

            bool found = false;
            long minX = 0;
            long minY = 0;
            long maxX = 0;
            long maxY = 0;
            for (int i = 0; i < frame.bodies.Count; i++)
            {
                BodyBox body = frame.bodies[i];
                if (!IsReleaseBody(body))
                    continue;

                fullHeight |= BodyIsReleaseFullHeight(body);
                long x1 = body.x;
                long y1 = body.y;
                long x2 = x1 + body.w;
                long y2 = y1 + body.h;
                if (!found)
                {
                    found = true;
                    minX = x1;
                    minY = y1;
                    maxX = x2;
                    maxY = y2;
                }
                else
                {
                    if (x1 < minX) minX = x1;
                    if (y1 < minY) minY = y1;
                    if (x2 > maxX) maxX = x2;
                    if (y2 > maxY) maxY = y2;
                }
            }

            if (!found)
                return false;

            rect = fullHeight
                ? new LocalRect(ClampRect(minX), int.MinValue, ClampRect(maxX - minX), 999)
                : new LocalRect(
                    ClampRect(minX),
                    ClampRect(minY),
                    ClampRect(maxX - minX),
                    ClampRect(maxY - minY));
            return true;
        }

        private static int ResolveItrIndex(LF2FrameData frame, InteractionArea itr)
        {
            if (frame?.itrs == null || itr == null)
                return -1;

            for (int i = 0; i < frame.itrs.Count; i++)
            {
                if (ReferenceEquals(frame.itrs[i], itr))
                    return i;
            }

            return -1;
        }

        private static int CollisionZInt(LF2Entity entity, LF2FrameData frame)
        {
            if (entity == null)
                return 0;
            return entity.GetCollisionZInt(frame);
        }

        private static WorldRect ItrWorldRect(LF2Entity entity, LF2FrameData frame, InteractionArea itr)
        {
            if (itr != null && itr.y == int.MinValue)
                return ItrWorldRectExeRaw(entity, frame, itr);

            return LocalRectWorldRect(
                entity,
                frame,
                new LocalRect(itr.x, itr.y, itr.w, itr.h),
                fullHeight: false);
        }

        private static WorldRect BodyWorldRect(LF2Entity entity, LF2FrameData frame, BodyBox body, bool collectSemantics)
        {
            return LocalRectWorldRect(
                entity,
                frame,
                new LocalRect(body.x, body.y, body.w, body.h),
                fullHeight: collectSemantics && BodyIsReleaseFullHeight(body));
        }

        internal static bool TryBuildCollisionBroadphaseAabb(
            LF2Entity entity,
            LF2FrameData collisionFrame,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (entity == null || entity.PS == null || collisionFrame == null)
                return false;

            bool found = false;
            int minX = 0;
            int maxX = 0;
            int minZ = 0;
            int maxZ = 0;
            int collisionZ = CollisionZInt(entity, collisionFrame);

            if (collisionFrame.bodies != null)
            {
                for (int i = 0; i < collisionFrame.bodies.Count; i++)
                {
                    BodyBox body = collisionFrame.bodies[i];
                    if (!IsReleaseBody(body))
                        continue;

                    WorldRect rect = BodyWorldRect(entity, collisionFrame, body, collectSemantics: true);
                    AddBroadphaseRange(
                        rect.X1,
                        rect.X2,
                        collisionZ,
                        ClampRect((long)collisionZ + 1),
                        ref found,
                        ref minX,
                        ref maxX,
                        ref minZ,
                        ref maxZ);
                }
            }

            if (collisionFrame.itrs != null)
            {
                for (int i = 0; i < collisionFrame.itrs.Count; i++)
                {
                    InteractionArea itr = collisionFrame.itrs[i];
                    if (!IsReleaseItrGeometry(itr))
                        continue;

                    WorldRect rect = ItrWorldRect(entity, collisionFrame, itr);
                    int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
                    AddBroadphaseRange(
                        rect.X1,
                        rect.X2,
                        ClampRect((long)collisionZ - zHalf),
                        ClampRect((long)collisionZ + zHalf),
                        ref found,
                        ref minX,
                        ref maxX,
                        ref minZ,
                        ref maxZ);
                }
            }

            if (!found)
                return false;

            bounds = new SpatialAabbXZ(minX, minZ, maxX, maxZ);
            return bounds.IsValid;
        }

        private static bool TryBuildImmediateItrAabb(
            LF2Entity attacker,
            LF2FrameData frame,
            InteractionArea itr,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (attacker == null || frame == null || itr == null || !IsReleaseItrGeometry(itr))
                return false;
            WorldRect rect = ItrWorldRect(attacker, frame, itr);
            int collisionZ = CollisionZInt(attacker, frame);
            int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
            bounds = new SpatialAabbXZ(
                Math.Min(rect.X1, rect.X2),
                ClampRect((long)collisionZ - zHalf),
                Math.Max(rect.X1, rect.X2),
                ClampRect((long)collisionZ + zHalf));
            return bounds.IsValid;
        }

        private static bool TryBuildShadowBodyAabb(
            LF2Entity target,
            LF2FrameData frame,
            BodyBox body,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (target == null || frame == null || !IsReleaseBody(body))
                return false;

            WorldRect rect = BodyWorldRect(target, frame, body, collectSemantics: true);
            int collisionZ = CollisionZInt(target, frame);
            int zHalf = BodyIsReleaseFullHeight(body) ? 9999 : 15;
            bounds = new SpatialAabbXZ(
                Math.Min(rect.X1, rect.X2),
                ClampRect((long)collisionZ - zHalf),
                Math.Max(rect.X1, rect.X2),
                ClampRect((long)collisionZ + zHalf));
            return bounds.IsValid;
        }

        private static bool TryBuildFormalBodyAabb(
            LF2Entity target,
            LF2FrameData frame,
            BodyBox body,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (target == null || frame == null || !IsReleaseBody(body))
                return false;

            WorldRect rect = BodyWorldRect(target, frame, body, collectSemantics: true);
            int collisionZ = CollisionZInt(target, frame);
            bounds = new SpatialAabbXZ(
                Math.Min(rect.X1, rect.X2),
                collisionZ,
                Math.Max(rect.X1, rect.X2),
                ClampRect((long)collisionZ + 1));
            return bounds.IsValid;
        }

        private static bool TryBuildImmediateVolumeAabb(
            in PhysicsState.BattleVolume volume,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            double x1 = volume.x + volume.vx;
            double x2 = x1 + volume.w;
            double z1 = volume.z - volume.zwidth;
            double z2 = volume.z + volume.zwidth;
            if (double.IsNaN(x1) || double.IsNaN(x2) ||
                double.IsNaN(z1) || double.IsNaN(z2) ||
                double.IsInfinity(x1) || double.IsInfinity(x2) ||
                double.IsInfinity(z1) || double.IsInfinity(z2))
            {
                return false;
            }
            int minX = ClampRect((long)Math.Floor(Math.Min(x1, x2)) - 1L);
            int maxX = ClampRect((long)Math.Ceiling(Math.Max(x1, x2)) + 1L);
            int minZ = ClampRect((long)Math.Floor(Math.Min(z1, z2)) - 1L);
            int maxZ = ClampRect((long)Math.Ceiling(Math.Max(z1, z2)) + 1L);
            bounds = new SpatialAabbXZ(minX, minZ, maxX, maxZ);
            return bounds.IsValid;
        }

        private static void AddBroadphaseRange(
            int x1,
            int x2,
            int z1,
            int z2,
            ref bool found,
            ref int minX,
            ref int maxX,
            ref int minZ,
            ref int maxZ)
        {
            SpatialAabbXZ range = SpatialAabbXZ.Normalize(x1, z1, x2, z2);
            if (!range.IsValid)
                return;

            if (!found)
            {
                found = true;
                minX = range.MinX;
                maxX = range.MaxX;
                minZ = range.MinZ;
                maxZ = range.MaxZ;
                return;
            }

            if (range.MinX < minX) minX = range.MinX;
            if (range.MaxX > maxX) maxX = range.MaxX;
            if (range.MinZ < minZ) minZ = range.MinZ;
            if (range.MaxZ > maxZ) maxZ = range.MaxZ;
        }

        internal static bool TryBuildBodyBattleVolume(
            LF2Entity entity,
            LF2FrameData frame,
            BodyBox body,
            out PhysicsState.BattleVolume volume)
        {
            volume = default;
            if (entity == null || frame == null || !IsReleaseBody(body))
                return false;

            WorldRect rect = BodyWorldRect(entity, frame, body, collectSemantics: true);
            int centerZ = CollisionZInt(entity, frame);
            int zHalf = BodyIsReleaseFullHeight(body) ? 9999 : 15;
            volume = new PhysicsState.BattleVolume(
                rect.X1,
                rect.Y1,
                centerZ,
                0f,
                0f,
                rect.X2 - rect.X1,
                rect.Y2 - rect.Y1,
                zHalf);
            return true;
        }

        private static WorldRect LocalRectWorldRect(LF2Entity entity, LF2FrameData frame, LocalRect rect, bool fullHeight)
        {
            int x = entity.Runtime != null ? entity.Runtime.XInt : (int)entity.PS.x;
            int y = entity.Runtime != null ? entity.Runtime.YInt : (int)entity.PS.y;
            bool facingLeft = entity.PS.dir == "left";

            int x1;
            int x2;
            if (!facingLeft)
            {
                x1 = ClampRect((long)x - frame.centerx + rect.X);
                x2 = ClampRect((long)x1 + rect.W);
            }
            else
            {
                x2 = ClampRect((long)x + frame.centerx - rect.X);
                x1 = ClampRect((long)x2 - rect.W);
            }

            if (fullHeight)
                return new WorldRect(x1, RectMin, x2, RectMax);

            VerticalWorldRect(y, frame.centery, rect.Y, rect.H, intMinFullHeight: false, out int y1, out int y2);
            return new WorldRect(x1, y1, x2, y2);
        }

        private static WorldRect ItrWorldRectExeRaw(LF2Entity entity, LF2FrameData frame, InteractionArea itr)
        {
            int x = entity.Runtime != null ? entity.Runtime.XInt : (int)entity.PS.x;
            int y = entity.Runtime != null ? entity.Runtime.YInt : (int)entity.PS.y;
            bool facingLeft = entity.PS.dir == "left";

            int x1;
            int x2;
            if (!facingLeft)
            {
                x1 = ExeI32Add3(x, -frame.centerx, itr.x);
                x2 = unchecked(x1 + itr.w);
            }
            else
            {
                x2 = ExeI32Add3(x, frame.centerx, -itr.x);
                x1 = unchecked(x2 - itr.w);
            }

            int y1 = ExeI32Add3(y, -frame.centery, itr.y);
            int y2 = unchecked(y1 + itr.h);
            return new WorldRect(x1, y1, x2, y2);
        }

        private static bool BodyIsReleaseFullHeight(BodyBox body)
        {
            return body != null && body.y == int.MinValue && body.x < -100 && body.w >= 900;
        }

        private static bool HasAnyReleaseBody(LF2FrameData frame)
        {
            if (frame?.bodies == null || frame.bodies.Count == 0)
                return false;

            for (int i = 0; i < frame.bodies.Count; i++)
            {
                if (IsReleaseBody(frame.bodies[i]))
                    return true;
            }

            return false;
        }

        private static bool IsReleaseBody(BodyBox body)
        {
            return body != null;
        }

        private static bool IsReleaseItrForUnion(InteractionArea itr)
        {
            if (itr == null)
                return false;

            // kind=5 is tested separately against the current pair in the coarse pass.
            if (itr.kind == 5)
                return false;

            return IsReleaseItrGeometry(itr);
        }

        internal static bool IsReleaseItrGeometry(InteractionArea itr)
        {
            return itr != null;
        }

        private static void VerticalWorldRect(
            int baseY,
            int centerY,
            int localY,
            int localH,
            bool intMinFullHeight,
            out int y1,
            out int y2)
        {
            if (intMinFullHeight && localY == int.MinValue)
            {
                y1 = RectMin;
                y2 = RectMax;
                return;
            }

            long top = (long)baseY - centerY + localY;
            y1 = ClampRect(top);
            y2 = ClampRect(top + localH);
        }

        private static int ClampRect(long value)
        {
            if (value < RectMin) return RectMin;
            if (value > RectMax) return RectMax;
            return (int)value;
        }

        private static int ExeI32Add3(int a, int b, int c)
        {
            return unchecked(a + b + c);
        }

        private static bool Overlap(in WorldRect a, in WorldRect b)
        {
            return a.X1 < b.X2 && a.X2 > b.X1 && a.Y1 < b.Y2 && a.Y2 > b.Y1;
        }
    }

    public enum RoleAwareCollisionShadowDifference
    {
        None = 0,
        MissingFromQuadtree = 1,
        ExtraInQuadtree = 2,
        AcceptedPairMissingFromQuadtree = 3,
    }

    public enum CollisionFormalCollectorMode
    {
        Configured = 0,
        ForceBruteForce = 1,
        ForceLegacyUnionAabb = 2,
        ForceRoleAware = 3,
    }

    public sealed class RoleAwareCollisionShadowDiagnostics
    {
        public int RebuildCount { get; private set; }
        public int ParticipantCount { get; internal set; }
        public int BodyCount { get; internal set; }
        public int IndexedBodyCount { get; internal set; }
        public int FallbackBodyCount { get; internal set; }
        public int AttackItrCount { get; internal set; }
        public int QueriedAttackItrCount { get; internal set; }
        public int FallbackAttackItrCount { get; internal set; }
        public int BrutePairCount { get; internal set; }
        public int QuadtreePairCount { get; internal set; }
        public int AcceptedPairCount { get; internal set; }
        public int MismatchCount { get; internal set; }
        public long FirstDifferencePair { get; private set; } = -1;
        public RoleAwareCollisionShadowDifference FirstDifference { get; private set; }
        public bool CollectionAborted { get; private set; }

        internal void Begin()
        {
            RebuildCount++;
            ResetCurrent();
        }

        internal void Abort()
        {
            ResetCurrent();
            CollectionAborted = true;
        }

        internal void RecordFirstDifference(
            long pair,
            RoleAwareCollisionShadowDifference difference)
        {
            if (FirstDifferencePair >= 0)
                return;
            FirstDifferencePair = pair;
            FirstDifference = difference;
        }

        private void ResetCurrent()
        {
            ParticipantCount = 0;
            BodyCount = 0;
            IndexedBodyCount = 0;
            FallbackBodyCount = 0;
            AttackItrCount = 0;
            QueriedAttackItrCount = 0;
            FallbackAttackItrCount = 0;
            BrutePairCount = 0;
            QuadtreePairCount = 0;
            AcceptedPairCount = 0;
            MismatchCount = 0;
            FirstDifferencePair = -1;
            FirstDifference = RoleAwareCollisionShadowDifference.None;
            CollectionAborted = false;
        }
    }

    internal sealed class RoleAwareShadowParticipant
    {
        public RoleAwareShadowParticipant(
            LF2Entity entity,
            LF2FrameData frame,
            int runtimeSlot)
        {
            Entity = entity;
            Frame = frame;
            RuntimeSlot = runtimeSlot;
        }

        public LF2Entity Entity { get; }
        public LF2FrameData Frame { get; }
        public int RuntimeSlot { get; }
        public bool HasBody { get; set; }
        public bool HasFallbackBody { get; set; }
        public bool HasAttackItr { get; set; }
        public bool HasFallbackAttackItr { get; set; }
    }

    internal readonly struct RoleAwareShadowItrEntry
    {
        public RoleAwareShadowItrEntry(int participantOrdinal, in SpatialAabbXZ bounds)
        {
            ParticipantOrdinal = participantOrdinal;
            Bounds = bounds;
        }

        public int ParticipantOrdinal { get; }
        public SpatialAabbXZ Bounds { get; }
    }

    internal sealed class RoleAwareFormalParticipant
    {
        public RoleAwareFormalParticipant(
            LF2Entity entity,
            LF2FrameData frame,
            RuntimeEntityHandle handle)
        {
            Entity = entity;
            Frame = frame;
            Handle = handle;
        }

        public LF2Entity Entity { get; }
        public LF2FrameData Frame { get; }
        public RuntimeEntityHandle Handle { get; }
        public bool HasBody { get; set; }
        public bool HasFallbackBody { get; set; }
        public bool HasAttackItr { get; set; }
        public bool HasFallbackAttackItr { get; set; }
    }

    internal readonly struct RoleAwareFormalBodyEntry
    {
        public RoleAwareFormalBodyEntry(
            int participantOrdinal,
            RuntimeEntityHandle handle)
        {
            ParticipantOrdinal = participantOrdinal;
            Handle = handle;
        }

        public int ParticipantOrdinal { get; }
        public RuntimeEntityHandle Handle { get; }
    }

    internal readonly struct RoleAwareFormalItrEntry
    {
        public RoleAwareFormalItrEntry(
            int participantOrdinal,
            RuntimeEntityHandle handle,
            in SpatialAabbXZ bounds)
        {
            ParticipantOrdinal = participantOrdinal;
            Handle = handle;
            Bounds = bounds;
        }

        public int ParticipantOrdinal { get; }
        public RuntimeEntityHandle Handle { get; }
        public SpatialAabbXZ Bounds { get; }
    }

    /// <summary>
    /// 纯碰撞工具函数，不持有战斗逻辑状态。
    /// </summary>
    public static class CollisionUtil
    {
        public static bool Intersect(in PhysicsState.BattleVolume a, in PhysicsState.BattleVolume b)
        {
            float aLeft = a.x + a.vx;
            float aTop = a.y + a.vy;
            float aRight = aLeft + a.w;
            float aBottom = aTop + a.h;

            float bLeft = b.x + b.vx;
            float bTop = b.y + b.vy;
            float bRight = bLeft + b.w;
            float bBottom = bTop + b.h;

            if (aBottom < bTop) return false;
            if (aTop > bBottom) return false;
            if (aRight < bLeft) return false;
            if (aLeft > bRight) return false;

            float aZMin = a.z - a.zwidth;
            float aZMax = a.z + a.zwidth;
            float bZMin = b.z - b.zwidth;
            float bZMax = b.z + b.zwidth;

            if (aZMax < bZMin) return false;
            if (aZMin > bZMax) return false;

            return true;
        }
    }

    internal readonly struct WorldRect
    {
        public readonly int X1;
        public readonly int Y1;
        public readonly int X2;
        public readonly int Y2;

        public WorldRect(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }
    }

    internal readonly struct LocalRect
    {
        public readonly int X;
        public readonly int Y;
        public readonly int W;
        public readonly int H;

        public LocalRect(int x, int y, int w, int h)
        {
            X = x;
            Y = y;
            W = w;
            H = h;
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/Presentation/BattlePresentationShadowBuild.cs ---
using System;
using System.Collections.Generic;
using System.Threading;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using Unity.Profiling;
using UnityEngine;

namespace NTSD.Simulation.Presentation
{
    public enum BattleRenderCommandType : byte
    {
        Shadow = 0,
        Entity = 1,
        OverlayGlyph = 2,
        HitRecord = 3,
    }

    public enum BattlePresentationDifferenceKind : byte
    {
        None = 0,
        ExpectedMissing = 1,
        UnexpectedLegacy = 2,
        Category = 3,
        Identity = 4,
        Visual = 5,
        Position = 6,
        Size = 7,
        Flip = 8,
        SortOrder = 9,
        Color = 10,
        RenderState = 11,
        ResourceKey = 12,
    }

    public enum BattleOverlayParityState : byte
    {
        None = 0,
        AuthorityExpectedButLegacyMissing = 1,
    }

    public enum BattlePresentationParityStatus : byte
    {
        None = 0,
        PendingLegacyFrame = 1,
        Complete = 2,
        IncompleteLegacyFrame = 3,
    }

    public enum BattleSpriteMaterialSemantic : byte
    {
        Unsupported = 0,
        PremultipliedSpriteAlpha = 1,
    }

    public readonly struct BattleSpriteRenderState
    {
        public BattleSpriteRenderState(
            Color32 color,
            bool flipX,
            bool flipY,
            SpriteMaskInteraction maskInteraction,
            BattleSpriteMaterialSemantic materialSemantic)
        {
            Color = color;
            FlipX = flipX;
            FlipY = flipY;
            MaskInteraction = maskInteraction;
            MaterialSemantic = materialSemantic;
        }

        public Color32 Color { get; }
        public bool FlipX { get; }
        public bool FlipY { get; }
        public SpriteMaskInteraction MaskInteraction { get; }
        public BattleSpriteMaterialSemantic MaterialSemantic { get; }
        public bool IsSupported =>
            MaskInteraction == SpriteMaskInteraction.None &&
            MaterialSemantic == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;

        public static BattleSpriteRenderState Default(bool flipX = false)
        {
            return new BattleSpriteRenderState(
                new Color32(255, 255, 255, 255),
                flipX,
                false,
                SpriteMaskInteraction.None,
                BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha);
        }
    }

    public static class BattleSpriteMaterialContract
    {
        public const string BuiltInSpriteShaderName = "Sprites/Default";
        public const string CentralTextureShaderName = "NTSD/BattleCentralTransparent";
        public const string CentralArrayShaderName = "NTSD/BattleCentralTransparentArray";
        public const string AlphaContractTag = "NTSDAlphaContract";
        public const string PremultipliedAlphaContract = "PremultipliedSpriteAlpha";

        private static readonly int ColorId = Shader.PropertyToID("_Color");

        public static BattleSpriteMaterialSemantic Classify(Material material)
        {
            if (material == null || material.shader == null)
                return BattleSpriteMaterialSemantic.Unsupported;

            string shaderName = material.shader.name;
            if (shaderName != BuiltInSpriteShaderName &&
                shaderName != CentralTextureShaderName &&
                shaderName != CentralArrayShaderName)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (shaderName != BuiltInSpriteShaderName &&
                material.GetTag(AlphaContractTag, false, string.Empty) != PremultipliedAlphaContract)
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            if (!material.HasProperty(ColorId) || !IsWhite(material.GetColor(ColorId)) ||
                material.IsKeywordEnabled("PIXELSNAP_ON"))
            {
                return BattleSpriteMaterialSemantic.Unsupported;
            }

            return BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        public static bool IsDeclaredCentralMaterial(Material material, bool textureArray)
        {
            if (material == null || material.shader == null)
                return false;
            string expectedShader = textureArray
                ? CentralArrayShaderName
                : CentralTextureShaderName;
            return material.shader.name == expectedShader &&
                   Classify(material) == BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha;
        }

        private static bool IsWhite(Color color)
        {
            const float epsilon = 0.000001f;
            return Mathf.Abs(color.r - 1f) <= epsilon &&
                   Mathf.Abs(color.g - 1f) <= epsilon &&
                   Mathf.Abs(color.b - 1f) <= epsilon &&
                   Mathf.Abs(color.a - 1f) <= epsilon;
        }
    }

    public readonly struct BattleSpriteValueDescriptor
    {
        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized)
            : this(
                requiresSprite,
                hasSprite,
                spriteInstanceId,
                textureInstanceId,
                materialInstanceId,
                pixelRect,
                pivotNormalized,
                false,
                default(BattleSpriteKey))
        {
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            bool hasLogicalResourceKey,
            BattleSpriteKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = hasLogicalResourceKey;
            LogicalResourceKey = BattleVisualResourceKey.FromEntity(logicalResourceKey);
        }

        public BattleSpriteValueDescriptor(
            bool requiresSprite,
            bool hasSprite,
            int spriteInstanceId,
            int textureInstanceId,
            int materialInstanceId,
            Rect pixelRect,
            Vector2 pivotNormalized,
            BattleVisualResourceKey logicalResourceKey)
        {
            RequiresSprite = requiresSprite;
            HasSprite = hasSprite;
            SpriteInstanceId = spriteInstanceId;
            TextureInstanceId = textureInstanceId;
            MaterialInstanceId = materialInstanceId;
            PixelRect = pixelRect;
            PivotNormalized = pivotNormalized;
            HasLogicalResourceKey = true;
            LogicalResourceKey = logicalResourceKey;
        }

        public bool RequiresSprite { get; }
        public bool HasSprite { get; }
        public int SpriteInstanceId { get; }
        public int TextureInstanceId { get; }
        public int MaterialInstanceId { get; }
        public Rect PixelRect { get; }
        public Vector2 PivotNormalized { get; }
        public bool HasLogicalResourceKey { get; }
        public BattleVisualResourceKey LogicalResourceKey { get; }
    }

    public readonly struct BattlePresentationHitRecordSnapshot
    {
        public BattlePresentationHitRecordSnapshot(int age, int anchorX, int anchorZ)
        {
            Age = age;
            AnchorX = anchorX;
            AnchorZ = anchorZ;
        }

        public int Age { get; }
        public int AnchorX { get; }
        public int AnchorZ { get; }
    }

    public readonly struct BattleHitRecordOwnerSnapshot
    {
        public BattleHitRecordOwnerSnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            float renderOffsetX,
            int cameraX,
            int hitRecordStart,
            int hitRecordCount)
        {
            Handle = handle;
            StableId = stableId;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
    }

    public sealed class BattleHitRecordPresentationCycle
    {
        private BattleHitRecordOwnerSnapshot[] owners = new BattleHitRecordOwnerSnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords =
            new BattlePresentationHitRecordSnapshot[16];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int CycleId { get; private set; }
        public int TickIndex { get; private set; }
        public int OwnerCount { get; private set; }
        public int HitRecordCount { get; private set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; private set; } =
            BattleCommonVisualCatalog.Empty;
        public bool HasValidSparkPublication => CommonVisualCatalog.IsSparkValid;

        public BattleHitRecordOwnerSnapshot GetOwner(int index)
        {
            if ((uint)index >= (uint)OwnerCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return owners[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        internal void Reset(
            int cycleId,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog)
        {
            ReleasePublicationBinding();
            CycleId = cycleId;
            TickIndex = tickIndex;
            OwnerCount = 0;
            HitRecordCount = 0;
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
        }

        internal void AddOwner(in BattleHitRecordOwnerSnapshot owner)
        {
            EnsureCapacity(ref owners, OwnerCount + 1);
            owners[OwnerCount++] = owner;
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureCapacity(ref hitRecords, HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattleHitRecordPresentationCycle previousCycle)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousCycle != null &&
                previousCycle.bindingManager == manager &&
                ReferenceEquals(previousCycle.boundCatalog, nextCatalog))
            {
                bindingManager = previousCycle.bindingManager;
                boundCatalog = previousCycle.boundCatalog;
                previousCycle.bindingManager = null;
                previousCycle.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;
            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct BattlePresentationEntitySnapshot
    {
        public BattlePresentationEntitySnapshot(
            RuntimeEntityHandle handle,
            int stableId,
            int objectId,
            int currentDatObjectId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int presentationBaseOrder,
            int hitStop,
            bool hasCurrentFrame,
            int state,
            int linkState,
            int hp2Orig,
            int relationTeam,
            int currentDatObjType,
            int xInt,
            int yInt,
            float displayZ,
            float renderOffsetX,
            int cameraX,
            int frameDelay,
            float centerX,
            float centerY,
            float pixelWidth,
            float pixelHeight,
            Vector2 heldVisualAttachmentOffsetPixels,
            Rect normalizedUv,
            Vector2 pivot,
            bool flipX,
            bool hasCatalogKey,
            BattleSpriteValueDescriptor spriteDescriptor,
            int hitRecordStart,
            int hitRecordCount,
            bool entityVisible = true,
            bool shadowVisible = true,
            Vector2 localOffsetPixels = default(Vector2),
            int frameId = -1)
        {
            Handle = handle;
            StableId = stableId;
            ObjectId = objectId;
            CurrentDatObjectId = currentDatObjectId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            PresentationBaseOrder = presentationBaseOrder;
            HitStop = hitStop;
            HasCurrentFrame = hasCurrentFrame;
            State = state;
            LinkState = linkState;
            HP2Orig = hp2Orig;
            RelationTeam = relationTeam;
            CurrentDatObjType = currentDatObjType;
            XInt = xInt;
            YInt = yInt;
            DisplayZ = displayZ;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            FrameDelay = frameDelay;
            CenterX = centerX;
            CenterY = centerY;
            PixelWidth = pixelWidth;
            PixelHeight = pixelHeight;
            HeldVisualAttachmentOffsetPixels = heldVisualAttachmentOffsetPixels;
            NormalizedUv = normalizedUv;
            Pivot = pivot;
            FlipX = flipX;
            HasCatalogKey = hasCatalogKey;
            SpriteDescriptor = spriteDescriptor;
            HitRecordStart = hitRecordStart;
            HitRecordCount = hitRecordCount;
            EntityVisible = entityVisible;
            ShadowVisible = shadowVisible;
            LocalOffsetPixels = localOffsetPixels;
            FrameId = frameId;
        }

        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int ObjectId { get; }
        public int CurrentDatObjectId { get; }
        public int VisualDataId => CurrentDatObjectId;
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int PresentationBaseOrder { get; }
        public int HitStop { get; }
        public bool HasCurrentFrame { get; }
        public int State { get; }
        public int LinkState { get; }
        public int HP2Orig { get; }
        public int RelationTeam { get; }
        public int CurrentDatObjType { get; }
        public int XInt { get; }
        public int YInt { get; }
        public float DisplayZ { get; }
        public float RenderOffsetX { get; }
        public int CameraX { get; }
        public int FrameDelay { get; }
        public float CenterX { get; }
        public float CenterY { get; }
        public float PixelWidth { get; }
        public float PixelHeight { get; }
        public Vector2 HeldVisualAttachmentOffsetPixels { get; }
        public Rect NormalizedUv { get; }
        public Vector2 Pivot { get; }
        public bool FlipX { get; }
        public bool HasCatalogKey { get; }
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
        public int HitRecordStart { get; }
        public int HitRecordCount { get; }
        public bool EntityVisible { get; }
        public bool ShadowVisible { get; }
        public Vector2 LocalOffsetPixels { get; }
        public int FrameId { get; }
    }

    public readonly struct BattleRenderCommand
    {
        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                zInt,
                runtimeSlot,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                pivot,
                normalizedUv,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        public BattleRenderCommand(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int zInt,
            int runtimeSlot,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            Vector2 pivot,
            Rect normalizedUv,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            ZInt = zInt;
            RuntimeSlot = runtimeSlot;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            Pivot = pivot;
            NormalizedUv = normalizedUv;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int ZInt { get; }
        public int RuntimeSlot { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public Vector2 Pivot { get; }
        public Rect NormalizedUv { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
    }

    public sealed class BattlePresentationFrame
    {
        private static readonly ProfilerMarker FrozenFrameCopyMarker =
            new ProfilerMarker("NTSD.BattlePresentation.FrozenFrameCopy");
        private BattlePresentationEntitySnapshot[] entities = new BattlePresentationEntitySnapshot[16];
        private BattlePresentationHitRecordSnapshot[] hitRecords = new BattlePresentationHitRecordSnapshot[16];
        private BattleRenderCommand[] commands = new BattleRenderCommand[64];
        private readonly char[,] slotLabelChars = new char[10, 12];
        private readonly int[] slotLabelState = new int[10];
        private CharacterAnimtorManager bindingManager;
        private BattleSpriteCatalog boundCatalog = BattleSpriteCatalog.Empty;

        public int TickIndex { get; internal set; }
        public int EntityCount { get; internal set; }
        public int HitRecordCount { get; internal set; }
        public int CommandCount { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        public BattleCommonVisualCatalog CommonVisualCatalog { get; internal set; } =
            BattleCommonVisualCatalog.Empty;
        public BattleCommonVisualBinding CommonShadowBinding { get; internal set; }
        public string CommonShadowDiagnostic { get; internal set; } = string.Empty;
        public int EntityCapacity => entities.Length;
        public int HitRecordCapacity => hitRecords.Length;
        public int CommandCapacity => commands.Length;
        internal char[,] SlotLabelChars => slotLabelChars;
        internal int[] SlotLabelState => slotLabelState;
        public BattleSpriteCatalog BoundCatalogForAcceptance => boundCatalog;
        internal BattleSpriteCatalog BoundCatalog => boundCatalog;

        public BattlePresentationEntitySnapshot GetEntity(int index)
        {
            if ((uint)index >= (uint)EntityCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return entities[index];
        }

        public BattlePresentationHitRecordSnapshot GetHitRecord(int index)
        {
            if ((uint)index >= (uint)HitRecordCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return hitRecords[index];
        }

        public BattleRenderCommand GetCommand(int index)
        {
            if ((uint)index >= (uint)CommandCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return commands[index];
        }

        internal void CopyFrom(
            BattlePresentationFrame source,
            BattleTickDetailPhaseDiagnostics detailDiagnostics = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (ReferenceEquals(this, source))
                return;

            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy);
            try
            {
                using (FrozenFrameCopyMarker.Auto())
                {
                    ReleasePublicationBinding();
                    EnsureEntityCapacity(source.EntityCount);
                    EnsureHitRecordCapacity(source.HitRecordCount);
                    EnsureCommandCapacity(source.CommandCount);
                    Array.Copy(source.entities, entities, source.EntityCount);
                    Array.Copy(source.hitRecords, hitRecords, source.HitRecordCount);
                    Array.Copy(source.commands, commands, source.CommandCount);
                    Array.Copy(source.slotLabelChars, slotLabelChars, source.slotLabelChars.Length);
                    Array.Copy(source.slotLabelState, slotLabelState, source.slotLabelState.Length);

                    TickIndex = source.TickIndex;
                    EntityCount = source.EntityCount;
                    HitRecordCount = source.HitRecordCount;
                    CommandCount = source.CommandCount;
                    OverlayUnsupportedCount = source.OverlayUnsupportedCount;
                    CommonVisualCatalog = source.CommonVisualCatalog;
                    CommonShadowBinding = source.CommonShadowBinding;
                    CommonShadowDiagnostic = source.CommonShadowDiagnostic;
                    // Submission catalog binding owns resource lifetime for frozen copies.
                    boundCatalog = source.boundCatalog;
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderPrepareFrameFrozenFrameCopy);
            }
        }

        internal void Reset(
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog = null)
        {
            ReleasePublicationBinding();
            TickIndex = tickIndex;
            EntityCount = 0;
            HitRecordCount = 0;
            CommandCount = 0;
            OverlayUnsupportedCount = 0;
            Array.Clear(slotLabelChars, 0, slotLabelChars.Length);
            Array.Clear(slotLabelState, 0, slotLabelState.Length);
            CommonVisualCatalog = commonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
            CommonShadowBinding = commonVisualCatalog?.Shadow;
            CommonShadowDiagnostic = commonVisualCatalog?.Diagnostic ??
                                     BattleCommonVisualCatalog.Empty.Diagnostic;
        }

        internal void RetainPublicationBinding(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog,
            BattlePresentationFrame previousFrame)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (manager == null || ReferenceEquals(nextCatalog, BattleSpriteCatalog.Empty))
                return;

            if (previousFrame != null &&
                previousFrame.bindingManager == manager &&
                ReferenceEquals(previousFrame.boundCatalog, nextCatalog))
            {
                bindingManager = previousFrame.bindingManager;
                boundCatalog = previousFrame.boundCatalog;
                previousFrame.bindingManager = null;
                previousFrame.boundCatalog = BattleSpriteCatalog.Empty;
                return;
            }

            manager.RegisterRendererCatalogBinding(nextCatalog);
            bindingManager = manager;
            boundCatalog = nextCatalog;
        }

        internal void ReleasePublicationBinding()
        {
            CharacterAnimtorManager manager = bindingManager;
            BattleSpriteCatalog catalog = boundCatalog;
            bindingManager = null;
            boundCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        internal void EnsureEntityCapacity(int required) => EnsureCapacity(ref entities, required);
        internal void EnsureHitRecordCapacity(int required) => EnsureCapacity(ref hitRecords, required);
        internal void EnsureCommandCapacity(int required) => EnsureCapacity(ref commands, required);

        internal void AddEntity(in BattlePresentationEntitySnapshot entity)
        {
            EnsureEntityCapacity(EntityCount + 1);
            entities[EntityCount++] = entity;
        }

        internal void AddHitRecord(in BattlePresentationHitRecordSnapshot hitRecord)
        {
            EnsureHitRecordCapacity(HitRecordCount + 1);
            hitRecords[HitRecordCount++] = hitRecord;
        }

        internal void AddCommand(in BattleRenderCommand command)
        {
            EnsureCommandCapacity(CommandCount + 1);
            commands[CommandCount++] = command;
        }

        private static void EnsureCapacity<T>(ref T[] array, int required)
        {
            if (required <= array.Length)
                return;

            int next = array.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref array, next);
        }
    }

    public readonly struct LegacyPresentationProbe
    {
        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            bool flipX,
            BattleSpriteValueDescriptor spriteDescriptor)
            : this(
                type,
                handle,
                stableId,
                visualDataId,
                effectivePic,
                sortOrder,
                sortingLayerId,
                localSequence,
                position,
                size,
                BattleSpriteRenderState.Default(flipX),
                spriteDescriptor)
        {
        }

        public LegacyPresentationProbe(
            BattleRenderCommandType type,
            RuntimeEntityHandle handle,
            int stableId,
            int visualDataId,
            int effectivePic,
            int sortOrder,
            int sortingLayerId,
            int localSequence,
            Vector3 position,
            Vector2 size,
            BattleSpriteRenderState renderState,
            BattleSpriteValueDescriptor spriteDescriptor)
        {
            Type = type;
            Handle = handle;
            StableId = stableId;
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
            SortOrder = sortOrder;
            SortingLayerId = sortingLayerId;
            LocalSequence = localSequence;
            Position = position;
            Size = size;
            RenderState = renderState;
            SpriteDescriptor = spriteDescriptor;
        }

        public BattleRenderCommandType Type { get; }
        public RuntimeEntityHandle Handle { get; }
        public int StableId { get; }
        public int VisualDataId { get; }
        public int EffectivePic { get; }
        public int SortOrder { get; }
        public int SortingLayerId { get; }
        public int LocalSequence { get; }
        public Vector3 Position { get; }
        public Vector2 Size { get; }
        public BattleSpriteRenderState RenderState { get; }
        public Color32 Color => RenderState.Color;
        public bool FlipX => RenderState.FlipX;
        public bool FlipY => RenderState.FlipY;
        public BattleSpriteValueDescriptor SpriteDescriptor { get; }
    }

    public sealed class BattlePresentationParityDiagnostics
    {
        public BattlePresentationParityStatus Status { get; internal set; }
        public int TickIndex { get; internal set; }
        public int ExpectedCount { get; internal set; }
        public int ActualCount { get; internal set; }
        public int DifferenceCount { get; internal set; }
        public int FirstDifferenceIndex { get; internal set; } = -1;
        public BattlePresentationDifferenceKind FirstDifferenceKind { get; internal set; }
        public BattleOverlayParityState OverlayState { get; internal set; }
        public int OverlayUnsupportedCount { get; internal set; }
        public int IncompleteLegacyFrameCount { get; internal set; }
        public int FirstIncompleteLegacyTick { get; internal set; } = -1;
        public int LastIncompleteLegacyTick { get; internal set; } = -1;
        public int CompletedLegacyFrameCount { get; internal set; }
        public bool HasFirstExpectedCommand { get; internal set; }
        public BattleRenderCommand FirstExpectedCommand { get; internal set; }
        public bool HasFirstActualProbe { get; internal set; }
        public LegacyPresentationProbe FirstActualProbe { get; internal set; }
    }

    public sealed class BattlePresentationCoordinator
    {
        private static readonly Comparison<LF2Entity> EntityOrderComparison = CompareEntityOrder;
        private static readonly int ObjectSortingLayerId = SortingLayer.NameToID("Object");
        private static readonly ProfilerMarker SortEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.SortEntities");
        private static readonly ProfilerMarker CaptureHitRecordsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureHitRecords");
        private static readonly ProfilerMarker CaptureEntitiesMarker =
            new ProfilerMarker("NTSD.BattlePresentation.CaptureEntities");
        private static readonly ProfilerMarker BuildCommandsMarker =
            new ProfilerMarker("NTSD.BattlePresentation.BuildCommands");
        private readonly BattlePresentationFrame frameA = new BattlePresentationFrame();
        private readonly BattlePresentationFrame frameB = new BattlePresentationFrame();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleA =
            new BattleHitRecordPresentationCycle();
        private readonly BattleHitRecordPresentationCycle hitRecordCycleB =
            new BattleHitRecordPresentationCycle();
        private readonly List<LF2Entity> entityScratch = new List<LF2Entity>(128);
        private readonly BattleEntityOverlayGlyph[] overlayGlyphScratch =
            new BattleEntityOverlayGlyph[32];
        private LegacyPresentationProbe[] legacyProbes = new LegacyPresentationProbe[64];
        private BattlePresentationFrame publishedFrame;
        private BattleHitRecordPresentationCycle publishedHitRecordCycle;
        private BattlePresentationBackendMode mode;
        private int nextHitRecordCycleId;
        private int finalizedHitRecordCycleId;
        private int legacyProbeCount;
        private int probeSequence;
        private bool awaitingLegacyCompletion;

        public BattlePresentationCoordinator()
        {
            mode = BattlePresentationBackendMode.LegacyOnly;
            Diagnostics = new BattlePresentationParityDiagnostics();
        }

        public BattlePresentationBackendMode Mode => mode;
        public BattlePresentationFrame PublishedFrame => Volatile.Read(ref publishedFrame);
        public BattleHitRecordPresentationCycle PublishedHitRecordCycle =>
            Volatile.Read(ref publishedHitRecordCycle);
        public BattlePresentationParityDiagnostics Diagnostics { get; }
        public bool IsCapturingLegacyProbes => awaitingLegacyCompletion;
        internal int LastHitRecordOwnerLookupCount { get; private set; }

        public void SetMode(BattlePresentationBackendMode value)
        {
            BattlePresentationBackendResolver.ValidateAvailable(value);
            mode = value;
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                if (awaitingLegacyCompletion)
                    RecordIncompleteLegacyFrame();
                awaitingLegacyCompletion = false;
                legacyProbeCount = 0;
            }
        }

        public void BeginFrame(SimulationWorld world, int tickIndex)
        {
            if (world == null)
                return;

            entityScratch.Clear();
            try
            {
                BattleTickDetailPhaseDiagnostics detailDiagnostics =
                    world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameSortEntities);
                try
                {
                    using (SortEntitiesMarker.Auto())
                    {
                        world.GetPresentationEntitiesNoAlloc(entityScratch);
                        entityScratch.Sort(EntityOrderComparison);
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderBeginFrameSortEntities);
                }

                CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
                BattleCommonVisualCatalog commonVisualCatalog =
                    manager?.CommonVisualCatalog ?? BattleCommonVisualCatalog.Empty;
                BattleHitRecordPresentationCycle previousCycle = PublishedHitRecordCycle;
                BattleHitRecordPresentationCycle writeCycle =
                    ReferenceEquals(previousCycle, hitRecordCycleA)
                        ? hitRecordCycleB
                        : hitRecordCycleA;
                int cycleId = nextHitRecordCycleId == int.MaxValue ? 1 : nextHitRecordCycleId + 1;
                nextHitRecordCycleId = cycleId;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords);
                try
                {
                    using (CaptureHitRecordsMarker.Auto())
                    {
                        CaptureHitRecordCycle(
                            world,
                            entityScratch,
                            tickIndex,
                            cycleId,
                            commonVisualCatalog,
                            writeCycle);
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderBeginFrameCaptureHitRecords);
                }
                if (writeCycle.HitRecordCount > 0 && commonVisualCatalog.IsSparkValid)
                {
                    writeCycle.RetainPublicationBinding(
                        manager,
                        manager?.SpriteCatalog,
                        previousCycle);
                }
                Interlocked.Exchange(ref publishedHitRecordCycle, writeCycle);
                previousCycle?.ReleasePublicationBinding();

                // Legacy overlays consume the same immutable command snapshot, but the
                // central renderer still refuses to build or submit geometry in this mode.
                if (mode == BattlePresentationBackendMode.LegacyOnly)
                {
                    CaptureBuildAndPublishFrame(
                        world,
                        entityScratch,
                        tickIndex,
                        commonVisualCatalog,
                        writeCycle,
                        manager);
                    return;
                }

                if (mode == BattlePresentationBackendMode.CentralOnly)
                {
                    CaptureBuildAndPublishFrame(
                        world,
                        entityScratch,
                        tickIndex,
                        commonVisualCatalog,
                        writeCycle,
                        manager);
                    awaitingLegacyCompletion = false;
                    legacyProbeCount = 0;
                    return;
                }

                if (mode != BattlePresentationBackendMode.CentralShadowBuild)
                    return;

                if (awaitingLegacyCompletion)
                    RecordIncompleteLegacyFrame();

                CaptureBuildAndPublishFrame(
                    world,
                    entityScratch,
                    tickIndex,
                    commonVisualCatalog,
                    writeCycle,
                    manager);
                legacyProbeCount = 0;
                probeSequence = 0;
                awaitingLegacyCompletion = true;
                Diagnostics.Status = BattlePresentationParityStatus.PendingLegacyFrame;
                Diagnostics.TickIndex = tickIndex;
            }
            finally
            {
                entityScratch.Clear();
            }
        }

        public bool FinalizePublishedHitRecordCycle(SimulationWorld world)
        {
            BattleHitRecordPresentationCycle cycle = PublishedHitRecordCycle;
            if (world == null || cycle == null || cycle.CycleId == finalizedHitRecordCycleId)
                return false;

            finalizedHitRecordCycleId = cycle.CycleId;
            if (!cycle.HasValidSparkPublication)
                return false;

            bool changed = false;
            try
            {
                for (int ownerIndex = 0; ownerIndex < cycle.OwnerCount; ownerIndex++)
                {
                    BattleHitRecordOwnerSnapshot owner = cycle.GetOwner(ownerIndex);
                    if (!world.TryResolveRuntimeHandle(owner.Handle, out LF2Entity entity) ||
                        entity == null || entity.HitRecordCount != owner.HitRecordCount)
                    {
                        continue;
                    }

                    bool sampleMatches = true;
                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (entity.GetHitRecordAge(hitIndex) != hit.Age)
                        {
                            sampleMatches = false;
                            break;
                        }
                    }
                    if (!sampleMatches)
                        continue;

                    for (int hitIndex = 0; hitIndex < owner.HitRecordCount; hitIndex++)
                    {
                        BattlePresentationHitRecordSnapshot hit = cycle.GetHitRecord(
                            owner.HitRecordStart + hitIndex);
                        if (BattleCommonVisualCatalog.TryResolveSparkAge(hit.Age, out _))
                        {
                            entity.AdvanceHitRecordFromPresentation(hitIndex, hit.Age);
                            changed = true;
                        }
                        else if (hitIndex == owner.HitRecordCount - 1)
                        {
                            changed |= entity.RemoveHitRecordTailFromPresentation(
                                hitIndex,
                                owner.HitRecordCount,
                                hit.Age);
                        }
                    }
                }
            }
            finally
            {
                cycle.ReleasePublicationBinding();
            }

            return changed;
        }

        public void ReleaseResources()
        {
            frameA.ReleasePublicationBinding();
            frameB.ReleasePublicationBinding();
            hitRecordCycleA.ReleasePublicationBinding();
            hitRecordCycleB.ReleasePublicationBinding();
        }

        public void Reset()
        {
            ReleaseResources();
            Interlocked.Exchange(ref publishedFrame, null);
            Interlocked.Exchange(ref publishedHitRecordCycle, null);
            entityScratch.Clear();
            nextHitRecordCycleId = 0;
            finalizedHitRecordCycleId = 0;
            legacyProbeCount = 0;
            probeSequence = 0;
            awaitingLegacyCompletion = false;
            LastHitRecordOwnerLookupCount = 0;
            Diagnostics.Status = BattlePresentationParityStatus.None;
            Diagnostics.TickIndex = 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = 0;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            Diagnostics.OverlayUnsupportedCount = 0;
            Diagnostics.IncompleteLegacyFrameCount = 0;
            Diagnostics.FirstIncompleteLegacyTick = -1;
            Diagnostics.LastIncompleteLegacyTick = -1;
            Diagnostics.CompletedLegacyFrameCount = 0;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
        }

        public void CompleteLegacyFrame()
        {
            if (!awaitingLegacyCompletion)
                return;

            awaitingLegacyCompletion = false;
            ComparePublishedFrameToLegacyProbes();
            Diagnostics.Status = BattlePresentationParityStatus.Complete;
            Diagnostics.CompletedLegacyFrameCount++;
        }

        private void RecordIncompleteLegacyFrame()
        {
            int incompleteTick = PublishedFrame?.TickIndex ?? Diagnostics.TickIndex;
            Diagnostics.Status = BattlePresentationParityStatus.IncompleteLegacyFrame;
            Diagnostics.IncompleteLegacyFrameCount++;
            if (Diagnostics.FirstIncompleteLegacyTick < 0)
                Diagnostics.FirstIncompleteLegacyTick = incompleteTick;
            Diagnostics.LastIncompleteLegacyTick = incompleteTick;
            awaitingLegacyCompletion = false;
            legacyProbeCount = 0;
        }

        internal void RecordLegacyProbe(in LegacyPresentationProbe probe)
        {
            if (!awaitingLegacyCompletion)
                return;

            EnsureLegacyProbeCapacity(legacyProbeCount + 1);
            legacyProbes[legacyProbeCount++] = new LegacyPresentationProbe(
                probe.Type,
                probe.Handle,
                probe.StableId,
                probe.VisualDataId,
                probe.EffectivePic,
                probe.SortOrder,
                probe.SortingLayerId,
                probeSequence++,
                probe.Position,
                probe.Size,
                probe.RenderState,
                probe.SpriteDescriptor);
        }

        internal void RecordLegacyHitRecordProbe(
            in BattleHitRecordOwnerSnapshot owner,
            SpriteRenderer renderer,
            int hitRecordIndex,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                BattleSpriteMaterialContract.Classify(material));
            bool matchesPublishedBinding = binding?.MatchesSprite(sprite) == true;
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.HitRecord,
                owner.Handle,
                owner.StableId,
                -1,
                -1,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                hitRecordIndex,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void RecordLegacyOverlayProbe(
            in BattleRenderCommand command,
            SpriteRenderer renderer,
            BattleCommonVisualBinding binding)
        {
            if (!awaitingLegacyCompletion || renderer == null || !renderer.enabled)
                return;

            Sprite sprite = renderer.sprite;
            Rect rect = sprite != null ? sprite.rect : Rect.zero;
            Vector2 pivot = sprite != null && rect.width > 0f && rect.height > 0f
                ? new Vector2(sprite.pivot.x / rect.width, sprite.pivot.y / rect.height)
                : Vector2.zero;
            Texture2D texture = sprite != null ? sprite.texture : null;
            Material material = renderer.sharedMaterial;
            var renderState = new BattleSpriteRenderState(
                renderer.color,
                renderer.flipX,
                renderer.flipY,
                renderer.maskInteraction,
                BattleSpriteMaterialContract.Classify(material));
            bool matchesPublishedBinding = binding != null &&
                                         binding.Key == command.SpriteDescriptor.LogicalResourceKey &&
                                         binding.MatchesSprite(sprite);
            BattleSpriteValueDescriptor descriptor = matchesPublishedBinding
                ? new BattleSpriteValueDescriptor(
                    true,
                    true,
                    sprite.GetInstanceID(),
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot,
                    binding.Key)
                : new BattleSpriteValueDescriptor(
                    true,
                    sprite != null,
                    sprite != null ? sprite.GetInstanceID() : 0,
                    texture != null ? texture.GetInstanceID() : 0,
                    material != null ? material.GetInstanceID() : 0,
                    rect,
                    pivot);
            RecordLegacyProbe(new LegacyPresentationProbe(
                BattleRenderCommandType.OverlayGlyph,
                command.Handle,
                command.StableId,
                command.VisualDataId,
                command.EffectivePic,
                renderer.sortingOrder,
                renderer.sortingLayerID,
                command.LocalSequence,
                renderer.transform.position,
                sprite != null ? sprite.rect.size : Vector2.zero,
                renderState,
                descriptor));
        }

        internal void ResetLegacyProbesForSelfCheck()
        {
            if (!awaitingLegacyCompletion)
                return;
            legacyProbeCount = 0;
            probeSequence = 0;
        }

        private void CaptureHitRecordCycle(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            int cycleId,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle cycle)
        {
            cycle.Reset(cycleId, tickIndex, commonVisualCatalog);
            for (int index = 0; index < sortedEntities.Count; index++)
            {
                LF2Entity entity = sortedEntities[index];
                NTSDEntityRuntime runtime = entity?.Runtime;
                int slot = runtime?.SlotIndex ?? -1;
                if (entity == null || runtime == null || slot < 0 ||
                    runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                    tickIndex < runtime.FirstPresentationTick ||
                    !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                {
                    continue;
                }

                int sampledCount = Math.Min(entity.HitRecordCount, LF2Entity.MaxHitRecordSlots);
                if (sampledCount <= 0)
                    continue;
                int hitRecordStart = cycle.HitRecordCount;
                for (int hitIndex = 0; hitIndex < sampledCount; hitIndex++)
                {
                    cycle.AddHitRecord(new BattlePresentationHitRecordSnapshot(
                        entity.GetHitRecordAge(hitIndex),
                        entity.GetHitRecordX(hitIndex),
                        entity.GetHitRecordZ(hitIndex)));
                }
                cycle.AddOwner(new BattleHitRecordOwnerSnapshot(
                    handle,
                    runtime.StableId,
                    runtime.ZInt,
                    slot,
                    entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
                    entity.GetRenderOffsetX(),
                    world.ReleaseCameraX,
                    hitRecordStart,
                    sampledCount));
            }
        }

        private void CaptureBuildAndPublishFrame(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            CharacterAnimtorManager manager)
        {
            BattlePresentationFrame previousFrame = PublishedFrame;
            BattlePresentationFrame writeFrame = ReferenceEquals(previousFrame, frameA) ? frameB : frameA;
            CaptureAndBuild(
                world,
                sortedEntities,
                tickIndex,
                commonVisualCatalog,
                hitRecordCycle,
                writeFrame);
            if (RequiresPublicationBinding(writeFrame))
            {
                writeFrame.RetainPublicationBinding(
                    manager,
                    manager?.SpriteCatalog,
                    previousFrame);
            }

            Interlocked.Exchange(ref publishedFrame, writeFrame);
            previousFrame?.ReleasePublicationBinding();
        }

        private static bool RequiresPublicationBinding(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int commandIndex = 0; commandIndex < frame.CommandCount; commandIndex++)
            {
                BattleSpriteValueDescriptor descriptor = frame.GetCommand(commandIndex).SpriteDescriptor;
                if (descriptor.HasLogicalResourceKey && descriptor.HasSprite)
                    return true;
            }

            return false;
        }

        private void CaptureAndBuild(
            SimulationWorld world,
            List<LF2Entity> sortedEntities,
            int tickIndex,
            BattleCommonVisualCatalog commonVisualCatalog,
            BattleHitRecordPresentationCycle hitRecordCycle,
            BattlePresentationFrame frame)
        {
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            try
            {
                using (CaptureEntitiesMarker.Auto())
                {
                    frame.Reset(tickIndex, commonVisualCatalog);
                    Array.Copy(
                        world.Runtime.SlotLabels.BattleSlotLabels,
                        frame.SlotLabelChars,
                        frame.SlotLabelChars.Length);
                    Array.Copy(
                        world.Runtime.SlotLabels.BattleSlotLabelState,
                        frame.SlotLabelState,
                        frame.SlotLabelState.Length);
                    frame.EnsureEntityCapacity(sortedEntities.Count);
                    int hitRecordOwnerCursor = 0;
                    LastHitRecordOwnerLookupCount = 0;

                    for (int i = 0; i < sortedEntities.Count; i++)
                    {
                        LF2Entity entity = sortedEntities[i];
                        NTSDEntityRuntime runtime = entity?.Runtime;
                        int slot = runtime?.SlotIndex ?? -1;
                        if (entity == null || runtime == null || slot < 0 ||
                            runtime.OidMergeDormant || runtime.PendingFlushDestroy ||
                            tickIndex < runtime.FirstPresentationTick ||
                            !world.TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle handle))
                        {
                            continue;
                        }

                        LF2FrameData currentFrame = entity.Frame?.D;
                        int visualDataId = LF2Entity.ResolveCurrentDataObjectId(entity);
                        int effectivePic = entity.GetRenderPicIndex();
                        bool hasCatalogKey = entity.TryResolveCurrentSpriteEntry(out BattleSpriteEntry entry);
                        Sprite catalogSprite = entry?.LegacySprite;
                        Texture2D catalogTexture = entry?.SharedTexture;
                        var spriteDescriptor = new BattleSpriteValueDescriptor(
                            hasCatalogKey,
                            catalogSprite != null,
                            catalogSprite != null ? catalogSprite.GetInstanceID() : 0,
                            catalogTexture != null ? catalogTexture.GetInstanceID() : 0,
                            0,
                            entry?.PixelRect ?? Rect.zero,
                            entry?.Pivot ?? Vector2.zero,
                            hasCatalogKey,
                            hasCatalogKey ? entry.Key : default);
                        int hitRecordStart = frame.HitRecordCount;
                        int sourceHitRecordCount = 0;
                        BattleHitRecordOwnerSnapshot hitRecordOwner = default;
                        if (hitRecordOwnerCursor < hitRecordCycle.OwnerCount)
                        {
                            LastHitRecordOwnerLookupCount++;
                            BattleHitRecordOwnerSnapshot candidate =
                                hitRecordCycle.GetOwner(hitRecordOwnerCursor);
                            if (candidate.Handle.Equals(handle))
                            {
                                hitRecordOwner = candidate;
                                sourceHitRecordCount = candidate.HitRecordCount;
                                hitRecordOwnerCursor++;
                            }
                        }
                        frame.EnsureHitRecordCapacity(frame.HitRecordCount + sourceHitRecordCount);
                        for (int hitIndex = 0; hitIndex < sourceHitRecordCount; hitIndex++)
                        {
                            frame.AddHitRecord(hitRecordCycle.GetHitRecord(
                                hitRecordOwner.HitRecordStart + hitIndex));
                        }

                        int holderSlot = runtime.HolderStableId;
                        LF2Entity holder = world.FindEntityByRuntimeSlotForQuery(holderSlot);
                        Vector2 heldVisualAttachmentOffsetPixels =
                            LF2ObjectRenderer.ResolveHeldVisualAttachmentOffsetPixels(
                                runtime,
                                currentFrame,
                                holder,
                                NTSDRenderSpace.BattleVisualScale);
                        LF2Sprite entitySprite = entity.Sprite;
                        bool entityVisible = entitySprite?.EntityVisible ?? true;
                        bool shadowVisible = entitySprite?.ShadowVisible ?? true;
                        Vector2 localOffsetPixels = entitySprite?.LocalOffsetPixels ?? Vector2.zero;

                        frame.AddEntity(new BattlePresentationEntitySnapshot(
                            handle,
                            runtime.StableId,
                            entity.ObjectId,
                            visualDataId,
                            effectivePic,
                            runtime.ZInt,
                            slot,
                            entity.GetRenderSortingOrder() - SimulationWorld.PresentationEntitySubOrder,
                            runtime.HitStop,
                            currentFrame != null,
                            currentFrame?.state ?? -1,
                            runtime.LinkState,
                            runtime.HP2Orig,
                            runtime.RelationTeam,
                            entity.GetCurrentDataObjectTypeForSimulation(),
                            entity.GetRuntimeXInt(),
                            entity.GetRuntimeYInt(),
                            entity.GetDisplayZ(),
                            entity.GetRenderOffsetX(),
                            world.ReleaseCameraX,
                            entity.FrameDelay,
                            currentFrame?.centerx ?? 0f,
                            currentFrame?.centery ?? 0f,
                            entry?.PixelWidth ?? 0f,
                            entry?.PixelHeight ?? 0f,
                            heldVisualAttachmentOffsetPixels,
                            entry?.NormalizedUv ?? Rect.zero,
                            entry?.Pivot ?? new Vector2(0.5f, 0f),
                            string.Equals(runtime.Dir, "left", StringComparison.Ordinal),
                            hasCatalogKey,
                            spriteDescriptor,
                            hitRecordStart,
                            sourceHitRecordCount,
                            entityVisible,
                            shadowVisible,
                            localOffsetPixels,
                            currentFrame?.frameId ?? -1));
                    }
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameCaptureEntities);
            }

            detailDiagnostics?.BeginPhase(
                BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            try
            {
                using (BuildCommandsMarker.Auto())
                {
                    BuildCommands(frame);
                }
            }
            finally
            {
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.RenderBeginFrameBuildCommands);
            }
        }

        private void BuildCommands(BattlePresentationFrame frame)
        {
            frame.EnsureCommandCapacity(Math.Max(16, frame.EntityCount * 8 + frame.HitRecordCount));
            for (int rank = 0; rank < frame.EntityCount; rank++)
            {
                BattlePresentationEntitySnapshot entity = frame.GetEntity(rank);
                int baseOrder = entity.PresentationBaseOrder;
                int localSequence = 0;

                bool drawShadow = entity.ShadowVisible && entity.HasCurrentFrame &&
                                  entity.State != 3005 && entity.State != 9997 &&
                                  entity.LinkState >= 0 && entity.ObjectId != 223 &&
                                  entity.ObjectId != 224 && frame.CommonShadowBinding != null &&
                                  LF2ObjectRenderer.ShouldDrawShadowForHitStop(entity.HitStop);
                if (drawShadow)
                {
                    BattleCommonVisualBinding shadow = frame.CommonShadowBinding;
                    Vector3 shadowPosition = NTSDRenderSpace.ScreenPixelToWorld(
                        entity.XInt + (int)entity.RenderOffsetX - entity.CameraX,
                        entity.ZInt,
                        0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.Shadow,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        -1,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder,
                        ObjectSortingLayerId,
                        localSequence++,
                        NTSDRenderSpace.SnapWorldPosition(shadowPosition),
                        shadow.PixelSize,
                        shadow.Pivot,
                        shadow.NormalizedUv,
                        shadow.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            shadow.SpriteInstanceId,
                            shadow.TextureInstanceId,
                            shadow.MaterialInstanceId,
                            shadow.PixelRect,
                            shadow.Pivot,
                            BattleVisualResourceKey.CommonShadow)));
                }

                bool drawEntity = entity.EntityVisible && entity.State >= 0 &&
                                  entity.EffectivePic != 999 &&
                                  entity.HasCatalogKey &&
                                  LF2ObjectRenderer.ShouldDrawEntityForHitStop(entity.HitStop);
                if (drawEntity)
                {
                    Vector2 pivotPixels = LF2ObjectRenderer.ComputeEntityBottomCenterPivotPixels(
                        entity.XInt,
                        entity.YInt,
                        entity.DisplayZ,
                        entity.RenderOffsetX,
                        entity.CameraX,
                        entity.FrameDelay,
                        frame.TickIndex,
                        entity.FlipX,
                        entity.PixelWidth,
                        entity.PixelHeight,
                        entity.CenterX,
                        entity.CenterY,
                        NTSDRenderSpace.BattleVisualScale);
                    pivotPixels += entity.HeldVisualAttachmentOffsetPixels;
                    pivotPixels += entity.LocalOffsetPixels * NTSDRenderSpace.BattleVisualScale;
                    Vector3 entityPosition = NTSDRenderSpace.ScreenPixelToWorld(pivotPixels.x, pivotPixels.y, 0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.Entity,
                        entity.Handle,
                        entity.StableId,
                        entity.VisualDataId,
                        entity.EffectivePic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 1,
                        ObjectSortingLayerId,
                        localSequence++,
                        entityPosition,
                        new Vector2(entity.PixelWidth, entity.PixelHeight),
                        entity.Pivot,
                        entity.NormalizedUv,
                        entity.FlipX,
                        entity.SpriteDescriptor));
                }

                if (entity.HasCurrentFrame)
                {
                    var overlayRuntimeSlot = new BattleEntityOverlayRuntimeSlot(
                        entity.RuntimeSlot,
                        entity.HP2Orig,
                        entity.RelationTeam,
                        entity.CurrentDatObjType,
                        entity.CurrentDatObjectId,
                        entity.HitStop,
                        entity.XInt,
                        entity.YInt,
                        entity.ZInt,
                        (int)entity.RenderOffsetX,
                        entity.CameraX,
                        (int)entity.CenterY);
                    if (BattleEntityOverlayLayout.TryBuild(
                            in overlayRuntimeSlot,
                            frame.SlotLabelChars,
                            frame.SlotLabelState,
                            overlayGlyphScratch,
                            out int overlayGlyphCount))
                    {
                        for (int glyphIndex = 0; glyphIndex < overlayGlyphCount; glyphIndex++)
                        {
                            BattleEntityOverlayGlyph glyph = overlayGlyphScratch[glyphIndex];
                            if (!frame.CommonVisualCatalog.TryGetWordGlyph(
                                    glyph.SheetIndex,
                                    glyph.CharCode,
                                    out BattleCommonVisualBinding binding))
                            {
                                continue;
                            }

                            Vector3 glyphPosition = NTSDRenderSpace.ScreenPixelToWorld(
                                glyph.PixelX,
                                glyph.PixelY,
                                0f);
                            AddCommand(frame, new BattleRenderCommand(
                                BattleRenderCommandType.OverlayGlyph,
                                entity.Handle,
                                entity.StableId,
                                glyph.SheetIndex,
                                glyph.CharCode,
                                entity.ZInt,
                                entity.RuntimeSlot,
                                baseOrder + 2,
                                ObjectSortingLayerId,
                                localSequence++,
                                glyphPosition,
                                binding.PixelSize,
                                binding.Pivot,
                                binding.NormalizedUv,
                                binding.RenderState,
                                new BattleSpriteValueDescriptor(
                                    true,
                                    true,
                                    binding.SpriteInstanceId,
                                    binding.TextureInstanceId,
                                    binding.MaterialInstanceId,
                                    binding.PixelRect,
                                    binding.Pivot,
                                    binding.Key)));
                        }
                    }
                }

                for (int hitIndex = 0; hitIndex < entity.HitRecordCount; hitIndex++)
                {
                    BattlePresentationHitRecordSnapshot hit = frame.GetHitRecord(
                        entity.HitRecordStart + hitIndex);
                    if (!TryResolveSparkFrame(
                            hit.Age,
                            out int pic,
                            out Vector2 size,
                            out Rect pixelRect))
                        continue;
                    if (!frame.CommonVisualCatalog.TryGetSpark(pic, out BattleCommonVisualBinding spark))
                        continue;

                    Vector3 hitPosition = NTSDRenderSpace.ScreenPixelToWorld(
                        hit.AnchorX + entity.RenderOffsetX - entity.CameraX,
                        hit.AnchorZ,
                        0f);
                    AddCommand(frame, new BattleRenderCommand(
                        BattleRenderCommandType.HitRecord,
                        entity.Handle,
                        entity.StableId,
                        -1,
                        pic,
                        entity.ZInt,
                        entity.RuntimeSlot,
                        baseOrder + 3,
                        ObjectSortingLayerId,
                        hitIndex,
                        hitPosition,
                        spark.PixelSize,
                        spark.Pivot,
                        spark.NormalizedUv,
                        spark.RenderState,
                        new BattleSpriteValueDescriptor(
                            true,
                            true,
                            spark.SpriteInstanceId,
                            spark.TextureInstanceId,
                            spark.MaterialInstanceId,
                            spark.PixelRect,
                            spark.Pivot,
                            spark.Key)));
                }
            }
        }

        internal static bool TryResolveSparkFrame(
            int age,
            out int pic,
            out Vector2 size,
            out Rect pixelRect)
        {
            if (!BattleCommonVisualCatalog.TryResolveSparkAge(age, out pic))
            {
                size = Vector2.zero;
                pixelRect = Rect.zero;
                return false;
            }

            pixelRect = BattleCommonVisualCatalog.GetSparkPixelRect(pic);
            size = pixelRect.size;
            return true;
        }

        internal static Rect GetAuthoritySparkPixelRect(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPixelRect(pic);
        }

        internal static Vector2 GetAuthoritySparkPivotNormalized(int pic)
        {
            return BattleCommonVisualCatalog.GetSparkPivotNormalized(pic);
        }

        private static void AddCommand(BattlePresentationFrame frame, in BattleRenderCommand command)
        {
            frame.AddCommand(command);
        }

        private void ComparePublishedFrameToLegacyProbes()
        {
            BattlePresentationFrame frame = PublishedFrame;
            Diagnostics.TickIndex = frame?.TickIndex ?? 0;
            Diagnostics.ExpectedCount = 0;
            Diagnostics.ActualCount = legacyProbeCount;
            Diagnostics.DifferenceCount = 0;
            Diagnostics.FirstDifferenceIndex = -1;
            Diagnostics.FirstDifferenceKind = BattlePresentationDifferenceKind.None;
            Diagnostics.HasFirstExpectedCommand = false;
            Diagnostics.FirstExpectedCommand = default;
            Diagnostics.HasFirstActualProbe = false;
            Diagnostics.FirstActualProbe = default;
            Diagnostics.OverlayUnsupportedCount = frame?.OverlayUnsupportedCount ?? 0;
            Diagnostics.OverlayState = BattleOverlayParityState.None;
            if (frame == null)
                return;

            SortLegacyProbes();
            int expectedIndex = 0;
            int actualIndex = 0;
            while (true)
            {
                bool hasExpected = expectedIndex < frame.CommandCount;
                bool hasActual = actualIndex < legacyProbeCount;
                if (!hasExpected && !hasActual)
                    break;

                int comparisonIndex = Diagnostics.ExpectedCount;
                if (!hasExpected)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.UnexpectedLegacy,
                        default,
                        false,
                        legacyProbes[actualIndex],
                        true);
                    actualIndex++;
                    continue;
                }
                Diagnostics.ExpectedCount++;
                if (!hasActual)
                {
                    RegisterDifference(
                        comparisonIndex,
                        BattlePresentationDifferenceKind.ExpectedMissing,
                        frame.GetCommand(expectedIndex),
                        true,
                        default,
                        false);
                    expectedIndex++;
                    continue;
                }

                BattleRenderCommand expected = frame.GetCommand(expectedIndex++);
                LegacyPresentationProbe actual = legacyProbes[actualIndex++];
                BattlePresentationDifferenceKind difference = Compare(expected, actual);
                if (difference != BattlePresentationDifferenceKind.None)
                {
                    RegisterDifference(
                        comparisonIndex,
                        difference,
                        expected,
                        true,
                        actual,
                        true);
                }
            }
        }

        private static BattlePresentationDifferenceKind Compare(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            if (expected.Type != actual.Type)
                return BattlePresentationDifferenceKind.Category;
            if (expected.Handle != actual.Handle || expected.StableId != actual.StableId)
                return BattlePresentationDifferenceKind.Identity;
            if (expected.SpriteDescriptor.RequiresSprite && !actual.SpriteDescriptor.HasSprite)
                return BattlePresentationDifferenceKind.Visual;
            if (expected.SpriteDescriptor.HasLogicalResourceKey &&
                (!actual.SpriteDescriptor.HasLogicalResourceKey ||
                 expected.SpriteDescriptor.LogicalResourceKey != actual.SpriteDescriptor.LogicalResourceKey))
            {
                return BattlePresentationDifferenceKind.ResourceKey;
            }
            Rect expectedRect = expected.SpriteDescriptor.PixelRect;
            Rect actualRect = actual.SpriteDescriptor.PixelRect;
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                ((expectedRect.position - actualRect.position).sqrMagnitude > 0.000001f ||
                 (expectedRect.size - actualRect.size).sqrMagnitude > 0.000001f))
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expectedRect.width > 0f && expectedRect.height > 0f &&
                (expected.SpriteDescriptor.PivotNormalized -
                 actual.SpriteDescriptor.PivotNormalized).sqrMagnitude > 0.000001f)
            {
                return BattlePresentationDifferenceKind.Visual;
            }
            if (expected.SortOrder != actual.SortOrder)
                return BattlePresentationDifferenceKind.SortOrder;
            if (expected.SortingLayerId != actual.SortingLayerId)
                return BattlePresentationDifferenceKind.SortOrder;
            if ((expected.Position - actual.Position).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Position;
            if (expected.Size.sqrMagnitude > 0.000001f &&
                (expected.Size - actual.Size).sqrMagnitude > 0.000001f)
                return BattlePresentationDifferenceKind.Size;
            if (!expected.RenderState.IsSupported || !actual.RenderState.IsSupported ||
                expected.RenderState.MaterialSemantic != actual.RenderState.MaterialSemantic ||
                expected.RenderState.MaskInteraction != actual.RenderState.MaskInteraction)
            {
                return BattlePresentationDifferenceKind.RenderState;
            }
            if (!expected.Color.Equals(actual.Color))
                return BattlePresentationDifferenceKind.Color;
            if (expected.FlipX != actual.FlipX || expected.FlipY != actual.FlipY)
                return BattlePresentationDifferenceKind.Flip;
            return BattlePresentationDifferenceKind.None;
        }

        internal static BattlePresentationDifferenceKind CompareForSelfCheck(
            in BattleRenderCommand expected,
            in LegacyPresentationProbe actual)
        {
            return Compare(expected, actual);
        }

        private static bool HasOverlayGlyphCommands(BattlePresentationFrame frame)
        {
            if (frame == null)
                return false;

            for (int index = 0; index < frame.CommandCount; index++)
            {
                if (frame.GetCommand(index).Type == BattleRenderCommandType.OverlayGlyph)
                    return true;
            }

            return false;
        }

        private void RegisterDifference(
            int index,
            BattlePresentationDifferenceKind kind,
            in BattleRenderCommand expected,
            bool hasExpected,
            in LegacyPresentationProbe actual,
            bool hasActual)
        {
            Diagnostics.DifferenceCount++;
            if (Diagnostics.FirstDifferenceIndex >= 0)
                return;
            Diagnostics.FirstDifferenceIndex = index;
            Diagnostics.FirstDifferenceKind = kind;
            Diagnostics.HasFirstExpectedCommand = hasExpected;
            Diagnostics.FirstExpectedCommand = expected;
            Diagnostics.HasFirstActualProbe = hasActual;
            Diagnostics.FirstActualProbe = actual;
        }

        private void SortLegacyProbes()
        {
            for (int i = 1; i < legacyProbeCount; i++)
            {
                LegacyPresentationProbe current = legacyProbes[i];
                int j = i - 1;
                while (j >= 0 && CompareProbeOrder(current, legacyProbes[j]) < 0)
                {
                    legacyProbes[j + 1] = legacyProbes[j];
                    j--;
                }
                legacyProbes[j + 1] = current;
            }
        }

        private static int CompareProbeOrder(in LegacyPresentationProbe left, in LegacyPresentationProbe right)
        {
            int order = left.SortOrder.CompareTo(right.SortOrder);
            return order != 0 ? order : left.LocalSequence.CompareTo(right.LocalSequence);
        }

        private static int CompareEntityOrder(LF2Entity left, LF2Entity right)
        {
            int z = (left?.Runtime?.ZInt ?? int.MaxValue).CompareTo(right?.Runtime?.ZInt ?? int.MaxValue);
            return z != 0
                ? z
                : (left?.Runtime?.SlotIndex ?? int.MaxValue).CompareTo(right?.Runtime?.SlotIndex ?? int.MaxValue);
        }

        private void EnsureLegacyProbeCapacity(int required)
        {
            if (required <= legacyProbes.Length)
                return;
            int next = legacyProbes.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref legacyProbes, next);
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleCentralRenderSystem.cs ---
using System;
using System.Threading;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleCentralRuntimeDiagnostics
    {
        public BattlePresentationBackendMode RequestedMode { get; internal set; }
        public BattlePresentationBackendMode EffectivePixelMode { get; internal set; }
        public bool FeatureAvailable { get; internal set; }
        public bool MaterialAvailable { get; internal set; }
        public bool FrameAvailable { get; internal set; }
        public bool AllCategoryOwnershipReady { get; internal set; }
        public bool CommonShadowBindingReady { get; internal set; }
        public bool CommonSparkBindingReady { get; internal set; }
        public bool SubmissionReady { get; internal set; }
        public bool SubmittedPixelsLastFrame { get; internal set; }
        public int SubmissionCount { get; internal set; }
        public int LastSubmissionDrawCount { get; internal set; }
        public int SimulationTick { get; internal set; }
        public int DisplayTick { get; internal set; }
        public bool IsStale { get; internal set; }
        public string Reason { get; internal set; } = string.Empty;
        public string RefusalReason { get; internal set; } = string.Empty;
    }

    public static class BattleCentralRenderSystem
    {
        private const int RendererObservationMaxAgeFrames = 2;

        private static readonly BattleDynamicMeshBackend[] Backends =
        {
            new BattleDynamicMeshBackend(),
            new BattleDynamicMeshBackend(),
        };
        private static readonly BattleCentralSubmission[] SlotSubmissions =
        {
            new BattleCentralSubmission(Backends[0]),
            new BattleCentralSubmission(Backends[1]),
        };
        private static readonly BattleDynamicMeshBackend EmptyBackend = new BattleDynamicMeshBackend();
        private static readonly BattleCatalogCentralResourceResolver CatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCatalogCentralResourceResolver DiagnosticCatalogResolver =
            new BattleCatalogCentralResourceResolver();
        private static readonly BattleCentralRuntimeDiagnostics RuntimeDiagnostics =
            new BattleCentralRuntimeDiagnostics();

        private static FeatureRegistration[] featureRegistrations = new FeatureRegistration[4];
        private static int featureRegistrationCount;
        private static BattleRenderFeature featureOwner;
        private static Material featureMaterial;
        private static Material featureArrayMaterial;
        private static BattleRenderFeature observedFeatureOwner;
        private static ScriptableRenderer observedRenderer;
        private static Camera observedWorldCamera;
        private static int observedUnityFrame = -1;
        private static BattlePresentationBackendMode requestedMode = BattlePresentationBackendMode.CentralOnly;
        private static BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleCentralDrawMode serializedDrawMode = BattleCentralDrawMode.OrderedChunks;
        private static BattleDrawPolicyDecision drawPolicyDecision = new BattleDrawPolicyDecision(
            BattleDrawPolicyMode.Auto,
            BattleCentralDrawMode.OrderedChunks,
            string.Empty);
        private static SimulationWorld publishedPlanWorld;
        private static int publishedPlanGeneration;
        private static BattleDynamicMeshBackend lastBuiltBackend = Backends[0];
        private static CharacterAnimtorManager diagnosticCatalogManager;
        private static BattleSpriteCatalog diagnosticCatalog = BattleSpriteCatalog.Empty;
        private static int nextGeneration;
        private static AttemptedBuildDiagnostics lastAttemptedBuildDiagnostics;

        public static BattleDynamicMeshBackend MeshBackend => lastBuiltBackend;
        public static BattleCentralRuntimeDiagnostics Diagnostics => RuntimeDiagnostics;
        public static BattlePixelFramePlan CurrentPixelFramePlan
        {
            get
            {
                SimulationWorld world = Volatile.Read(ref publishedPlanWorld);
                BattlePixelFramePlan plan = world != null
                    ? world.CurrentPixelFramePlan
                    : default;
                return plan.IsValid && plan.Generation == Volatile.Read(ref publishedPlanGeneration)
                    ? plan
                    : default;
            }
        }
        internal static int RegisteredFeatureCount => featureRegistrationCount;
        internal static BattleRenderFeature RegisteredFeature => featureOwner;
        public static Material RegisteredFeatureMaterialForAcceptance => featureMaterial;
        public static Material RegisteredFeatureArrayMaterialForAcceptance => featureArrayMaterial;
        internal static Material RegisteredFeatureMaterial => featureMaterial;
        internal static Material RegisteredFeatureArrayMaterial => featureArrayMaterial;
        internal static BattleCentralDrawMode RegisteredFeatureDrawMode => drawMode;
        public static BattleDrawPolicyDecision DrawPolicyDecision => drawPolicyDecision;

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            BattleCentralDrawMode mode)
        {
            RegisterFeature(owner, material, null, mode);
        }

        internal static void RegisterFeature(
            BattleRenderFeature owner,
            Material material,
            Material arrayMaterial,
            BattleCentralDrawMode mode)
        {
            if (owner == null)
                return;

            int existingIndex = FindRegistration(owner);
            if (existingIndex >= 0)
                RemoveRegistrationAt(existingIndex);
            EnsureRegistrationCapacity(featureRegistrationCount + 1);
            featureRegistrations[featureRegistrationCount++] =
                new FeatureRegistration(owner, material, arrayMaterial, mode);
            ApplyActiveRegistration();
        }

        internal static void UnregisterFeature(BattleRenderFeature owner)
        {
            int index = FindRegistration(owner);
            if (index < 0)
                return;
            RemoveRegistrationAt(index);
            ApplyActiveRegistration();
        }

        internal static void RecordFeatureCameraAvailability(
            BattleRenderFeature owner,
            ScriptableRenderer renderer,
            Camera camera,
            CameraRenderType renderType)
        {
            if (owner == null || owner != featureOwner || renderer == null ||
                !IsWorldRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera))
            {
                return;
            }

            observedFeatureOwner = owner;
            observedRenderer = renderer;
            observedWorldCamera = camera;
            observedUnityFrame = Time.frameCount;
        }

        public static BattlePixelFramePlan PrepareFrame(SimulationWorld world)
        {
            BattlePresentationBackendMode mode =
                world?.BattlePresentation?.Mode ?? BattlePresentationBackendMode.CentralOnly;
            BattlePresentationFrame frame = world?.BattlePresentation?.PublishedFrame;
            int simulationTick = frame?.TickIndex ?? world?.CurrentTickIndex ?? 0;
            BattlePixelFramePlan current = world != null ? world.CurrentPixelFramePlan : default;
            if (current.IsValid && ReferenceEquals(current.World, world) &&
                current.SimulationTick == simulationTick &&
                current.RequestedMode == mode && CurrentPixelFramePlan.Generation == current.Generation)
            {
                return current;
            }

            requestedMode = mode;
            ResetPerFrameDiagnostics(mode, frame != null);
            lastAttemptedBuildDiagnostics = default;

            if (world == null)
            {
                return CommitCentralFailurePlan(
                    null,
                    simulationTick,
                    "SimulationWorld is unavailable.");
            }
            if (mode == BattlePresentationBackendMode.LegacyOnly)
            {
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "LegacyOnly does not build or submit central geometry.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleSpriteCatalog catalog = manager != null
                ? manager.SpriteCatalog
                : BattleSpriteCatalog.Empty;
            BattleCommonVisualCatalog commonVisualCatalog = manager != null
                ? manager.CommonVisualCatalog
                : BattleCommonVisualCatalog.Empty;
            RuntimeDiagnostics.CommonShadowBindingReady = commonVisualCatalog.IsShadowValid;
            RuntimeDiagnostics.CommonSparkBindingReady = commonVisualCatalog.IsSparkValid;

            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend stagingBackend))
            {
                const string reason =
                    "No central staging backend is available because the previous submission is still leased.";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool rendererReady = TryValidateActiveRenderer(out string rendererReason);
            bool frameReady = frame != null;
            bool commonReady = commonVisualCatalog.IsComplete;
            if (mode == BattlePresentationBackendMode.CentralOnly &&
                (!rendererReady || !frameReady || !commonReady))
            {
                string reason = !rendererReady
                    ? rendererReason
                    : !frameReady
                        ? "No current immutable presentation frame is available."
                        : "The common shadow, spark, or WORDS catalog is incomplete.";
                return CommitCentralFailurePlan(world, simulationTick, reason);
            }

            try
            {
                BattleTickDetailPhaseDiagnostics detailDiagnostics =
                    world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                BattleCentralSubmission stagingSubmission = SlotSubmissions[backendIndex];
                BattlePresentationFrame buildFrame = frame != null
                    ? stagingSubmission.CaptureFrame(frame, detailDiagnostics)
                    : null;
                BattleSpriteCatalog buildCatalog = buildFrame?.BoundCatalog ?? catalog;
                BattleCommonVisualCatalog buildCommonVisualCatalog =
                    buildFrame?.CommonVisualCatalog ?? commonVisualCatalog;
                CatalogResolver.Configure(
                    buildCatalog,
                    buildCommonVisualCatalog,
                    featureMaterial,
                    featureArrayMaterial);
                stagingBackend.Build(
                    buildFrame,
                    CatalogResolver,
                    drawMode,
                    detailDiagnostics);
                lastBuiltBackend = stagingBackend;
                lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(
                    stagingBackend,
                    simulationTick);
            }
            catch (Exception exception)
            {
                stagingBackend.Clear();
                string reason =
                    $"Central geometry build failed: {exception.GetType().Name}: {exception.Message}";
                return mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(world, simulationTick, reason)
                    : CommitLegacyPlan(world, frame, mode, simulationTick, reason);
            }

            bool allCategoryOwnershipReady = frameReady && commonReady &&
                                             frame.OverlayUnsupportedCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedCategoryCount == 0 &&
                                             stagingBackend.Diagnostics.UnsupportedRenderStateCount == 0 &&
                                             stagingBackend.Diagnostics.UnresolvedCommandCount == 0;
            RuntimeDiagnostics.AllCategoryOwnershipReady = allCategoryOwnershipReady;

            if (mode == BattlePresentationBackendMode.CentralShadowBuild)
            {
                BindDiagnosticCatalog(manager, stagingBackend.BuiltFrame?.BoundCatalog ?? catalog);
                return CommitLegacyPlan(
                    world,
                    frame,
                    mode,
                    simulationTick,
                    "CentralShadowBuild builds diagnostics but fixes pixel ownership to Legacy.",
                    true);
            }

            if (!allCategoryOwnershipReady)
            {
                return CommitCentralFailurePlan(
                    world,
                    simulationTick,
                    BuildOwnershipRefusalReason(stagingBackend));
            }

            ReleaseDiagnosticCatalogBinding();
            int generation = NextGeneration();
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = stagingBackend.BuiltFrame;
            submission.Publish(
                world,
                capturedFrame,
                simulationTick,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                mode,
                BattlePixelFrameOwner.Central,
                simulationTick,
                simulationTick,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = true;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        public static bool CentralOnlyOwnsPixels(SimulationWorld world)
        {
            return world != null &&
                   world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly;
        }

        public static bool ShouldSuppressLegacyMaterializers(SimulationWorld world)
        {
            return CentralOnlyOwnsPixels(world);
        }

        public static bool ShouldUseCentralPixels(SimulationWorld world)
        {
            BattlePixelFramePlan plan = world != null ? world.CurrentPixelFramePlan : default;
            BattlePixelFramePlan globalPlan = CurrentPixelFramePlan;
            BattleCentralSubmission submission = plan.Submission;
            return plan.IsValid && globalPlan.IsValid && plan.Generation == globalPlan.Generation &&
                   ReferenceEquals(plan.World, world) &&
                   plan.Owner == BattlePixelFrameOwner.Central &&
                   plan.RequestedMode == BattlePresentationBackendMode.CentralOnly &&
                   submission != null &&
                   !submission.IsRetired && ReferenceEquals(submission.World, world) &&
                   ReferenceEquals(submission.CapturedFrame, plan.CapturedFrame) &&
                   submission.IsBackendBuildCurrent &&
                   submission.TickIndex == plan.DisplayTick &&
                   submission.Generation == plan.Generation;
        }

        internal static bool TryAcquireSubmission(
            Camera camera,
            CameraRenderType renderType,
            out BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            lease = default;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            SimulationWorld world = plan.World;
            if (!CanRenderCamera(camera, renderType, NTSDRenderSpace.WorldCamera) ||
                !ShouldUseCentralPixels(world))
            {
                return false;
            }

            if (!plan.Submission.TryAcquire(out lease))
                return false;
            if (ShouldUseCentralPixels(world) &&
                lease.Generation == plan.Generation && lease.TickIndex == plan.TickIndex)
            {
                return true;
            }

            lease.Dispose();
            lease = default;
            return false;
        }

        internal static bool IsSubmissionLeaseCurrent(
            BattleCentralSubmission.BattleCentralSubmissionLease lease)
        {
            BattleCentralSubmission submission = lease.Submission;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            return submission != null && plan.IsValid &&
                   ReferenceEquals(plan.Submission, submission) &&
                   plan.Generation == lease.Generation && plan.TickIndex == lease.TickIndex &&
                   ShouldUseCentralPixels(plan.World);
        }

        internal static BattlePixelFramePlan PublishReadyCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            int tickIndex = frame?.TickIndex ?? 0;
            if (world.BattlePresentation.Mode != BattlePresentationBackendMode.CentralOnly || frame == null)
            {
                return world.BattlePresentation.Mode == BattlePresentationBackendMode.CentralOnly
                    ? CommitCentralFailurePlan(
                        world,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.")
                    : CommitLegacyPlan(
                        world,
                        frame,
                        world.BattlePresentation.Mode,
                        tickIndex,
                        "Self-check central publication requires a current CentralOnly frame.");
            }
            if (!TryGetReusableBackend(out int backendIndex, out BattleDynamicMeshBackend backend))
            {
                return CommitCentralFailurePlan(
                    world,
                    tickIndex,
                    "Self-check central publication found no reusable backend slot.");
            }

            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            BattlePresentationFrame capturedFrame = submission.CaptureFrame(frame);
            CatalogResolver.Configure(
                capturedFrame.BoundCatalog,
                capturedFrame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            backend.Build(capturedFrame, CatalogResolver, drawMode);
            lastBuiltBackend = backend;
            lastAttemptedBuildDiagnostics = AttemptedBuildDiagnostics.Capture(backend, tickIndex);
            int generation = NextGeneration();
            submission.Publish(
                world,
                capturedFrame,
                tickIndex,
                generation,
                manager,
                capturedFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                capturedFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                tickIndex,
                tickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishBuiltCentralPlanForSelfCheck(
            SimulationWorld world)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Built central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));

            BattlePresentationFrame frame = world.BattlePresentation.PublishedFrame;
            BattlePresentationFrame builtFrame = lastBuiltBackend.BuiltFrame;
            if (frame == null || builtFrame == null || frame.TickIndex != builtFrame.TickIndex)
                throw new InvalidOperationException("The self-check requires the current immutable frame tick to be built.");

            int backendIndex = Array.IndexOf(Backends, lastBuiltBackend);
            if (backendIndex < 0)
                throw new InvalidOperationException("The built backend is not a publishable central slot.");
            BattleCentralSubmission submission = SlotSubmissions[backendIndex];
            if (!submission.IsReusable)
                throw new InvalidOperationException("The built backend submission slot is still leased.");

            int generation = NextGeneration();
            CharacterAnimtorManager manager = CharacterAnimtorManager.Instance;
            submission.Publish(
                world,
                builtFrame,
                builtFrame.TickIndex,
                generation,
                manager,
                builtFrame.BoundCatalog);
            var plan = new BattlePixelFramePlan(
                world,
                builtFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                builtFrame.TickIndex,
                builtFrame.TickIndex,
                generation,
                false,
                string.Empty,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.RequestedMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            RuntimeDiagnostics.FrameAvailable = true;
            RuntimeDiagnostics.AllCategoryOwnershipReady = true;
            RuntimeDiagnostics.SubmissionReady = true;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = string.Empty;
            return plan;
        }

        internal static BattlePixelFramePlan PublishStaleCentralPlanForSelfCheck(
            SimulationWorld world,
            int simulationTick)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Stale central publication self-check hook is editor-only.");
            if (world == null)
                throw new ArgumentNullException(nameof(world));
            BattlePixelFramePlan current = world.CurrentPixelFramePlan;
            if (!current.IsValid || current.Owner != BattlePixelFrameOwner.Central ||
                current.Submission == null || current.Submission.IsRetired)
            {
                throw new InvalidOperationException("The self-check requires a live central submission.");
            }
            return CommitCentralFailurePlan(world, simulationTick, "Self-check retained last-good frame.");
        }

        public static bool CanRenderCamera(Camera camera, CameraRenderType renderType, Camera worldCamera)
        {
            return CanRenderCamera(
                camera,
                renderType,
                worldCamera,
                camera != null ? camera.cameraType : CameraType.Game,
                Application.isPlaying);
        }

        internal static bool CanRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera,
            CameraType cameraType,
            bool isPlaying)
        {
            if (renderType != CameraRenderType.Base || camera == null || worldCamera == null)
                return false;
            if (camera == worldCamera)
                return true;
#if UNITY_EDITOR
            return isPlaying && cameraType == CameraType.SceneView;
#else
            return false;
#endif
        }

        private static bool IsWorldRenderCamera(
            Camera camera,
            CameraRenderType renderType,
            Camera worldCamera)
        {
            return camera != null && worldCamera != null && camera == worldCamera &&
                   renderType == CameraRenderType.Base;
        }

        internal static void RecordSubmission(
            BattleCentralSubmission.BattleCentralSubmissionLease lease,
            int drawCount)
        {
            if (!lease.IsValid)
                return;
            RecordSubmission(lease.Submission, lease.Generation, lease.TickIndex, drawCount);
        }

#if UNITY_EDITOR
        internal static void RecordSubmissionForSelfCheck(
            BattlePixelFramePlan plan,
            int drawCount)
        {
            if (!Application.isEditor)
                throw new InvalidOperationException("Central submission recording self-check hook is editor-only.");
            BattlePixelFramePlan current = CurrentPixelFramePlan;
            if (!plan.IsValid || plan.Submission == null ||
                !current.IsValid || current.Generation != plan.Generation ||
                !ReferenceEquals(current.Submission, plan.Submission))
            {
                throw new InvalidOperationException(
                    "The self-check can record only the current central submission generation.");
            }
            RecordSubmission(plan.Submission, plan.Generation, plan.DisplayTick, drawCount);
        }
#endif

        private static void RecordSubmission(
            BattleCentralSubmission submission,
            int generation,
            int tickIndex,
            int drawCount)
        {
            if (submission == null ||
                !submission.TryRecordExecutedDraws(generation, tickIndex, drawCount))
            {
                return;
            }

            RuntimeDiagnostics.SubmissionCount += drawCount;
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            if (!plan.IsValid || !ReferenceEquals(plan.Submission, submission) ||
                plan.Generation != generation || plan.DisplayTick != tickIndex)
            {
                return;
            }

            int executedDrawCount = submission.GetExecutedDrawCount(generation, tickIndex);
            RuntimeDiagnostics.SubmittedPixelsLastFrame = executedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = executedDrawCount;
        }

        public static BattleRenderingDiagnosticReport CaptureDiagnosticReport()
        {
            BattleAtlasDiagnosticInputs atlasInputs = CharacterAnimtorManager.Instance?.LastAtlasDiagnosticInputs;
            if (atlasInputs == null)
                return null;

            return CaptureDiagnosticReportForSelfCheck(atlasInputs);
        }

        internal static BattleRenderingDiagnosticReport CaptureDiagnosticReportForSelfCheck(
            BattleAtlasDiagnosticInputs atlasInputs)
        {
            if (atlasInputs == null)
                throw new ArgumentNullException(nameof(atlasInputs));
            BattlePixelFramePlan plan = CurrentPixelFramePlan;
            BattleCentralBuildDiagnostics build = null;
            AttemptedBuildDiagnostics attempted = default;
            BattlePresentationFrame reportFrame = null;
            int submissionDrawCount = 0;
            bool submissionBuildCurrent = false;

            if (plan.IsValid && plan.Submission != null && !plan.Submission.IsRetired &&
                plan.Submission.Generation == plan.Generation &&
                plan.Submission.TickIndex == plan.DisplayTick &&
                ReferenceEquals(plan.Submission.CapturedFrame, plan.CapturedFrame))
            {
                reportFrame = plan.Submission.CapturedFrame;
                submissionBuildCurrent = plan.Submission.IsBackendBuildCurrent;
                if (submissionBuildCurrent)
                {
                    build = plan.Submission.Backend.Diagnostics;
                    submissionDrawCount = plan.Submission.GetExecutedDrawCount(
                        plan.Generation,
                        plan.DisplayTick);
                }
            }
            else if (plan.IsValid &&
                     plan.RequestedMode == BattlePresentationBackendMode.CentralShadowBuild &&
                     lastBuiltBackend?.BuiltFrame != null &&
                     lastBuiltBackend.Diagnostics.TickIndex == plan.DisplayTick)
            {
                build = lastBuiltBackend.Diagnostics;
                reportFrame = lastBuiltBackend.BuiltFrame;
                submissionBuildCurrent = true;
            }
            else if (plan.IsValid && lastAttemptedBuildDiagnostics.IsValid &&
                     lastAttemptedBuildDiagnostics.SimulationTick == plan.SimulationTick)
            {
                attempted = lastAttemptedBuildDiagnostics;
                reportFrame = attempted.Frame;
                submissionBuildCurrent = attempted.IsValid;
            }

            int sourceCommandCount = build != null
                ? build.SourceCommandCount
                : attempted.IsValid ? attempted.SourceCommandCount : 0;
            int resolvedCommandCount = build != null
                ? build.ResolvedCommandCount
                : attempted.IsValid ? attempted.ResolvedCommandCount : 0;
            int unresolvedCommandCount = build != null
                ? build.UnresolvedCommandCount
                : attempted.IsValid ? attempted.UnresolvedCommandCount : 0;
            int unsupportedCategoryCount = build != null
                ? build.UnsupportedCategoryCount
                : attempted.IsValid ? attempted.UnsupportedCategoryCount : 0;
            int unsupportedRenderStateCount = build != null
                ? build.UnsupportedRenderStateCount
                : attempted.IsValid ? attempted.UnsupportedRenderStateCount : 0;
            int activeChunkCount = build != null
                ? build.ActiveChunkCount
                : attempted.IsValid ? attempted.ActiveChunkCount : 0;
            int segmentCount = build != null
                ? build.SegmentCount
                : attempted.IsValid ? attempted.SegmentCount : 0;
            int buildTick = build != null
                ? build.TickIndex
                : attempted.IsValid ? attempted.BuildTick : -1;
            int firstUnresolvedCommandIndex = build != null
                ? build.FirstUnresolvedCommandIndex
                : attempted.IsValid ? attempted.FirstUnresolvedCommandIndex : -1;
            BattleRenderCommandType firstUnresolvedCommandType = build != null
                ? build.FirstUnresolvedCommandType
                : attempted.FirstUnresolvedCommandType;
            BattleCentralResourceStatus firstUnresolvedStatus = build != null
                ? build.FirstUnresolvedStatus
                : attempted.FirstUnresolvedStatus;
            return new BattleRenderingDiagnosticReport(
                atlasInputs,
                drawPolicyDecision,
                sourceCommandCount,
                resolvedCommandCount,
                unresolvedCommandCount,
                unsupportedCategoryCount,
                activeChunkCount,
                segmentCount,
                submissionDrawCount,
                plan.IsValid ? plan.RequestedMode : RuntimeDiagnostics.RequestedMode,
                RuntimeDiagnostics.EffectivePixelMode,
                reportFrame?.EntityCount ?? 0,
                plan.IsValid ? plan.Generation : 0,
                buildTick,
                plan.IsValid ? plan.SimulationTick : -1,
                plan.IsValid ? plan.DisplayTick : -1,
                plan.IsValid && plan.IsStale,
                plan.IsValid ? plan.Reason : RuntimeDiagnostics.RefusalReason,
                submissionBuildCurrent,
                unsupportedRenderStateCount,
                firstUnresolvedCommandIndex,
                firstUnresolvedCommandType,
                firstUnresolvedStatus);
        }

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnostic(
            SimulationWorld world,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null || !handle.IsValid ||
                !world.TryGetRuntimeSlotReadOnlyView(handle.Slot, out RuntimeSlotTable.ReadOnlySlotView slotView))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    handle,
                    commandType);
            }
            if (!slotView.Claimed || slotView.Generation != handle.Generation)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.GenerationMismatch,
                    handle,
                    commandType);
            }

            BattlePixelFramePlan plan = world.CurrentPixelFramePlan;
            BattlePresentationFrame frame = plan.RequestedMode ==
                                                BattlePresentationBackendMode.CentralShadowBuild &&
                                            lastBuiltBackend.BuiltFrame != null
                ? lastBuiltBackend.BuiltFrame
                : plan.CapturedFrame ?? world.BattlePresentation.PublishedFrame;
            if (frame == null || !TryFindSnapshot(frame, handle, out BattlePresentationEntitySnapshot snapshot))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.MissingSnapshotEntity,
                    handle,
                    commandType);
            }

            if (!TryFindCommand(frame, handle, commandType, out int commandIndex, out BattleRenderCommand command))
            {
                BattleCentralEntityDiagnosticReason reason =
                    commandType == BattleRenderCommandType.Entity && !snapshot.EntityVisible ||
                    commandType == BattleRenderCommandType.Shadow && !snapshot.ShadowVisible
                        ? BattleCentralEntityDiagnosticReason.PresentationVisibilityFalse
                        : BattleCentralEntityDiagnosticReason.CommandSuppressed;
                return CreateEntityDiagnostic(reason, handle, commandType, snapshot, true);
            }

            if (!command.RenderState.IsSupported)
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleCentralEntityDiagnosticReason resourceReason = ResolveDiagnosticResource(
                frame,
                command,
                out BattleCentralResolvedResource resource);
            if (resourceReason != BattleCentralEntityDiagnosticReason.None)
            {
                return CreateEntityDiagnostic(
                    resourceReason,
                    handle,
                    commandType,
                    snapshot,
                    true,
                    command,
                    true,
                    commandIndex);
            }

            BattleDynamicMeshBackend backend = plan.Submission != null &&
                                                ReferenceEquals(plan.Submission.CapturedFrame, frame)
                ? plan.Submission.Backend
                : ReferenceEquals(lastBuiltBackend.BuiltFrame, frame)
                    ? lastBuiltBackend
                    : null;
            int segmentIndex = FindSegmentIndex(backend, commandIndex);
            int chunkIndex = segmentIndex >= 0 ? backend.GetSegment(segmentIndex).ChunkIndex : -1;
            bool backendBuildCurrent = plan.Submission == null ||
                                       plan.Submission.IsBackendBuildCurrent;
            bool submissionStructurallyCurrent = backendBuildCurrent &&
                                                 plan.Owner == BattlePixelFrameOwner.Central &&
                                                 plan.Submission != null &&
                                                 !plan.Submission.IsRetired &&
                                                 ReferenceEquals(plan.CapturedFrame, frame) &&
                                                 ReferenceEquals(plan.Submission.Backend, backend) &&
                                                 segmentIndex >= 0;
            bool submitted = submissionStructurallyCurrent &&
                             plan.Submission.GetExecutedDrawCount(
                                 plan.Generation,
                                 plan.DisplayTick) > 0;
            return CreateEntityDiagnostic(
                !backendBuildCurrent
                    ? BattleCentralEntityDiagnosticReason.BackendMutationMismatch
                    : !submitted
                        ? BattleCentralEntityDiagnosticReason.NotSubmitted
                        : plan.IsStale
                            ? BattleCentralEntityDiagnosticReason.StalePlan
                            : BattleCentralEntityDiagnosticReason.None,
                handle,
                commandType,
                snapshot,
                true,
                command,
                true,
                commandIndex,
                resource,
                true,
                segmentIndex,
                chunkIndex,
                submitted);
        }

#if UNITY_EDITOR
        internal static BattleCentralEntityDiagnosticReason CaptureResourceReasonForSelfCheck(
            BattlePresentationFrame frame,
            in BattleRenderCommand command)
        {
            if (!command.RenderState.IsSupported)
                return BattleCentralEntityDiagnosticReason.UnsupportedRenderState;
            return ResolveDiagnosticResource(frame, command, out _);
        }
#endif

        public static BattleCentralEntityDiagnostic CaptureEntityDiagnosticBySlot(
            SimulationWorld world,
            int runtimeSlot,
            BattleRenderCommandType commandType = BattleRenderCommandType.Entity)
        {
            if (world == null ||
                !world.TryGetRuntimeSlotReadOnlyView(runtimeSlot, out RuntimeSlotTable.ReadOnlySlotView view) ||
                !view.Claimed || view.Entity == null ||
                !world.TryGetCurrentRuntimeHandle(runtimeSlot, view.Entity, out RuntimeEntityHandle handle))
            {
                return CreateEntityDiagnostic(
                    BattleCentralEntityDiagnosticReason.InvalidRuntimeHandle,
                    RuntimeEntityHandle.Invalid,
                    commandType);
            }

            return CaptureEntityDiagnostic(world, handle, commandType);
        }

        private static BattleCentralEntityDiagnosticReason ResolveDiagnosticResource(
            BattlePresentationFrame frame,
            in BattleRenderCommand command,
            out BattleCentralResolvedResource resource)
        {
            resource = default;
            if (!command.SpriteDescriptor.HasLogicalResourceKey)
                return BattleCentralEntityDiagnosticReason.MissingCatalogKey;

            if (command.Type == BattleRenderCommandType.Entity)
            {
                BattleVisualResourceKey logicalKey = command.SpriteDescriptor.LogicalResourceKey;
                if (!logicalKey.IsEntitySprite ||
                    !frame.BoundCatalog.TryGet(logicalKey.EntitySpriteKey, out BattleSpriteEntry entry) ||
                    entry.Key.VisualDataId != command.VisualDataId ||
                    entry.Key.EffectivePic != command.EffectivePic)
                {
                    return BattleCentralEntityDiagnosticReason.MissingCatalogKey;
                }

                BattleSpriteCentralBinding binding = entry.CentralBinding;
                if (binding.Texture == null)
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;
                if (!binding.IsValid)
                    return BattleCentralEntityDiagnosticReason.InvalidCentralBinding;
                Material material = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray
                    ? featureArrayMaterial
                    : featureMaterial;
                bool expectsArray = binding.Mode == BattleSpriteCentralBindingMode.AtlasTextureArray;
                if (!BattleSpriteMaterialContract.IsDeclaredCentralMaterial(material, expectsArray))
                    return BattleCentralEntityDiagnosticReason.MissingTextureOrMaterial;

                resource = new BattleCentralResolvedResource(
                    binding.Texture,
                    material,
                    binding.NormalizedUv,
                    new Vector2(entry.PixelWidth, entry.PixelHeight),
                    entry.Pivot,
                    command.Color,
                    (int)command.RenderState.MaterialSemantic,
                    binding.AtlasSlice,
                    binding.Mode,
                    binding.AtlasPageIndex);
                return BattleCentralEntityDiagnosticReason.None;
            }

            DiagnosticCatalogResolver.Configure(
                frame.BoundCatalog,
                frame.CommonVisualCatalog,
                featureMaterial,
                featureArrayMaterial);
            BattleCentralResourceStatus status = DiagnosticCatalogResolver.Resolve(command, out resource);
            return status switch
            {
                BattleCentralResourceStatus.Resolved => BattleCentralEntityDiagnosticReason.None,
                BattleCentralResourceStatus.UnsupportedRenderState =>
                    BattleCentralEntityDiagnosticReason.UnsupportedRenderState,
                BattleCentralResourceStatus.UnsupportedCategory =>
                    BattleCentralEntityDiagnosticReason.UnresolvedResource,
                _ => BattleCentralEntityDiagnosticReason.UnresolvedResource,
            };
        }

        private static bool TryFindSnapshot(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            out BattlePresentationEntitySnapshot snapshot)
        {
            for (int index = 0; index < frame.EntityCount; index++)
            {
                BattlePresentationEntitySnapshot candidate = frame.GetEntity(index);
                if (candidate.Handle == handle)
                {
                    snapshot = candidate;
                    return true;
                }
            }

            snapshot = default;
            return false;
        }

        private static bool TryFindCommand(
            BattlePresentationFrame frame,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            out int commandIndex,
            out BattleRenderCommand command)
        {
            for (int index = 0; index < frame.CommandCount; index++)
            {
                BattleRenderCommand candidate = frame.GetCommand(index);
                if (candidate.Handle == handle && candidate.Type == commandType)
                {
                    commandIndex = index;
                    command = candidate;
                    return true;
                }
            }

            commandIndex = -1;
            command = default;
            return false;
        }

        private static int FindSegmentIndex(BattleDynamicMeshBackend backend, int commandIndex)
        {
            if (backend == null)
                return -1;
            for (int index = 0; index < backend.SegmentCount; index++)
            {
                BattleCentralRenderSegment segment = backend.GetSegment(index);
                if (commandIndex >= segment.FirstCommandIndex &&
                    commandIndex < segment.FirstCommandIndex + segment.CommandCount)
                {
                    return index;
                }
            }

            return -1;
        }

        private static BattleCentralEntityDiagnostic CreateEntityDiagnostic(
            BattleCentralEntityDiagnosticReason reason,
            RuntimeEntityHandle handle,
            BattleRenderCommandType commandType,
            BattlePresentationEntitySnapshot snapshot = default,
            bool hasSnapshot = false,
            BattleRenderCommand command = default,
            bool hasCommand = false,
            int commandIndex = -1,
            BattleCentralResolvedResource resource = default,
            bool hasResolvedResource = false,
            int segmentIndex = -1,
            int chunkIndex = -1,
            bool submitted = false)
        {
            return new BattleCentralEntityDiagnostic(
                reason,
                handle,
                commandType,
                snapshot,
                hasSnapshot,
                command,
                hasCommand,
                resource,
                hasResolvedResource,
                commandIndex,
                segmentIndex,
                chunkIndex,
                submitted);
        }

        internal static void ResolveDrawPolicyForPublication(
            GameConfig config,
            string[] commandLineArguments = null)
        {
            drawPolicyDecision = BattleRenderingPolicyResolver.ResolveDraw(
                config,
                serializedDrawMode,
                commandLineArguments);
            drawMode = drawPolicyDecision.EffectiveMode;
        }

        public static void ResetRuntime()
        {
            BattleCentralPresentationMountRegistry.ResetAllRuntimeBindings();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            Volatile.Write(ref publishedPlanGeneration, 0);
            Volatile.Write(ref publishedPlanWorld, null);
            previous.Submission?.Retire();
            previous.World?.PublishPixelFramePlan(default);
            ReleaseDiagnosticCatalogBinding();
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission submission = SlotSubmissions[index];
                submission.Retire();
                if (submission.IsReusable)
                    Backends[index].Clear();
            }
            lastBuiltBackend = Backends[0];
            lastAttemptedBuildDiagnostics = default;
            requestedMode = BattlePresentationBackendMode.CentralOnly;
            ResetPerFrameDiagnostics(BattlePresentationBackendMode.CentralOnly, false);
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static BattlePixelFramePlan CommitLegacyPlan(
            SimulationWorld world,
            BattlePresentationFrame frame,
            BattlePresentationBackendMode mode,
            int tickIndex,
            string reason,
            bool preserveBuildDiagnostics = false)
        {
            if (!preserveBuildDiagnostics)
            {
                ReleaseDiagnosticCatalogBinding();
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            var plan = new BattlePixelFramePlan(
                world,
                frame,
                mode,
                BattlePixelFrameOwner.Legacy,
                tickIndex,
                tickIndex,
                NextGeneration(),
                false,
                reason,
                null);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.LegacyOnly;
            SetPlanDiagnostics(plan);
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            return plan;
        }

        private static BattlePixelFramePlan CommitCentralFailurePlan(
            SimulationWorld world,
            int simulationTick,
            string reason)
        {
            ReleaseDiagnosticCatalogBinding();
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            BattleCentralSubmission submission = previous.IsValid &&
                                                   ReferenceEquals(previous.World, world) &&
                                                   previous.Owner == BattlePixelFrameOwner.Central &&
                                                   previous.Submission != null &&
                                                   !previous.Submission.IsRetired
                ? previous.Submission
                : null;
            BattlePresentationFrame displayFrame = submission?.CapturedFrame;
            int displayTick = submission?.TickIndex ?? -1;
            int generation = submission?.Generation ?? NextGeneration();
            var plan = new BattlePixelFramePlan(
                world,
                displayFrame,
                BattlePresentationBackendMode.CentralOnly,
                BattlePixelFrameOwner.Central,
                simulationTick,
                displayTick,
                generation,
                true,
                reason,
                submission);
            PublishPlan(world, plan);
            RuntimeDiagnostics.SubmissionReady = submission != null;
            RuntimeDiagnostics.EffectivePixelMode = BattlePresentationBackendMode.CentralOnly;
            SetPlanDiagnostics(plan);
            int retainedDrawCount = submission?.GetExecutedDrawCount(generation, displayTick) ?? 0;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = retainedDrawCount > 0;
            RuntimeDiagnostics.LastSubmissionDrawCount = retainedDrawCount;
            RuntimeDiagnostics.RefusalReason = reason ?? string.Empty;
            if (submission == null)
            {
                EmptyBackend.Clear();
                lastBuiltBackend = EmptyBackend;
            }
            return plan;
        }

        private static void PublishPlan(SimulationWorld world, BattlePixelFramePlan plan)
        {
            BattlePixelFramePlan previous = CurrentPixelFramePlan;
            world?.PublishPixelFramePlan(plan);
            Volatile.Write(ref publishedPlanWorld, world);
            Volatile.Write(ref publishedPlanGeneration, plan.Generation);
            if (previous.IsValid && !ReferenceEquals(previous.World, world))
                previous.World?.PublishPixelFramePlan(default);
            if (plan.Submission != null && !ReferenceEquals(previous.Submission, plan.Submission))
            {
                RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
                RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            }
            if (!ReferenceEquals(previous.Submission, plan.Submission))
                previous.Submission?.Retire();
        }

        private static bool TryGetReusableBackend(
            out int backendIndex,
            out BattleDynamicMeshBackend backend)
        {
            BattleCentralSubmission currentSubmission = CurrentPixelFramePlan.Submission;
            for (int index = 0; index < Backends.Length; index++)
            {
                BattleCentralSubmission slotSubmission = SlotSubmissions[index];
                if (ReferenceEquals(slotSubmission, currentSubmission))
                    continue;
                if (!slotSubmission.IsReusable)
                    continue;

                backendIndex = index;
                backend = Backends[index];
                return true;
            }

            backendIndex = -1;
            backend = null;
            return false;
        }

        private static bool TryValidateActiveRenderer(out string reason)
        {
            Camera worldCamera = NTSDRenderSpace.WorldCamera;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable =
                BattleSpriteMaterialContract.IsDeclaredCentralMaterial(featureMaterial, false);
            if (featureOwner == null || !featureOwner.isActive)
            {
                reason = "BattleRenderFeature is not registered and active; CentralOnly output is fail-closed.";
                return false;
            }
            if (!RuntimeDiagnostics.MaterialAvailable)
            {
                reason = "The central battle material is missing or violates the declared alpha contract.";
                return false;
            }
            if (worldCamera == null || !worldCamera.enabled || !worldCamera.gameObject.activeInHierarchy)
            {
                reason = "The bound battle world camera is unavailable or disabled.";
                return false;
            }
            try
            {
                if (!worldCamera.TryGetComponent(out UniversalAdditionalCameraData cameraData) ||
                    cameraData.scriptableRenderer == null ||
                    !ReferenceEquals(cameraData.scriptableRenderer, observedRenderer))
                {
                    reason = "The battle world camera is not using the renderer that invoked BattleRenderFeature.";
                    return false;
                }
            }
            catch (Exception exception)
            {
                reason = $"The battle world-camera renderer could not be validated: {exception.GetType().Name}.";
                return false;
            }
            int observationAge = observedUnityFrame < 0 ? int.MaxValue : Time.frameCount - observedUnityFrame;
            if (observedFeatureOwner != featureOwner || observedWorldCamera != worldCamera ||
                observationAge < 0 || observationAge > RendererObservationMaxAgeFrames)
            {
                reason = "The active world-camera renderer has not recently invoked the registered BattleRenderFeature.";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        private static string BuildOwnershipRefusalReason(BattleDynamicMeshBackend backend)
        {
            BattleCentralBuildDiagnostics diagnostics = backend.Diagnostics;
            return "Central frame ownership is incomplete: " +
                   $"unresolved={diagnostics.UnresolvedCommandCount}, " +
                   $"unsupportedCategory={diagnostics.UnsupportedCategoryCount}, " +
                   $"unsupportedState={diagnostics.UnsupportedRenderStateCount}.";
        }

        private static void BindDiagnosticCatalog(
            CharacterAnimtorManager manager,
            BattleSpriteCatalog catalog)
        {
            BattleSpriteCatalog nextCatalog = catalog ?? BattleSpriteCatalog.Empty;
            if (ReferenceEquals(diagnosticCatalogManager, manager) &&
                ReferenceEquals(diagnosticCatalog, nextCatalog))
            {
                return;
            }

            ReleaseDiagnosticCatalogBinding();
            diagnosticCatalogManager = manager;
            diagnosticCatalog = nextCatalog;
            diagnosticCatalogManager?.RegisterRendererCatalogBinding(diagnosticCatalog);
        }

        private static void ReleaseDiagnosticCatalogBinding()
        {
            CharacterAnimtorManager manager = diagnosticCatalogManager;
            BattleSpriteCatalog catalog = diagnosticCatalog;
            diagnosticCatalogManager = null;
            diagnosticCatalog = BattleSpriteCatalog.Empty;
            manager?.UnregisterRendererCatalogBinding(catalog);
        }

        private static void ResetPerFrameDiagnostics(
            BattlePresentationBackendMode mode,
            bool frameAvailable)
        {
            RuntimeDiagnostics.RequestedMode = mode;
            RuntimeDiagnostics.EffectivePixelMode = mode == BattlePresentationBackendMode.CentralOnly
                ? BattlePresentationBackendMode.CentralOnly
                : BattlePresentationBackendMode.LegacyOnly;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
            RuntimeDiagnostics.FrameAvailable = frameAvailable;
            RuntimeDiagnostics.AllCategoryOwnershipReady = false;
            RuntimeDiagnostics.CommonShadowBindingReady = false;
            RuntimeDiagnostics.CommonSparkBindingReady = false;
            RuntimeDiagnostics.SubmissionReady = false;
            RuntimeDiagnostics.SubmittedPixelsLastFrame = false;
            RuntimeDiagnostics.LastSubmissionDrawCount = 0;
            RuntimeDiagnostics.SimulationTick = 0;
            RuntimeDiagnostics.DisplayTick = -1;
            RuntimeDiagnostics.IsStale = false;
            RuntimeDiagnostics.Reason = string.Empty;
            RuntimeDiagnostics.RefusalReason = string.Empty;
        }

        private static void SetPlanDiagnostics(BattlePixelFramePlan plan)
        {
            RuntimeDiagnostics.SimulationTick = plan.SimulationTick;
            RuntimeDiagnostics.DisplayTick = plan.DisplayTick;
            RuntimeDiagnostics.IsStale = plan.IsStale;
            RuntimeDiagnostics.Reason = plan.Reason;
        }

        private static int NextGeneration()
        {
            int generation = Interlocked.Increment(ref nextGeneration);
            if (generation > 0)
                return generation;
            Interlocked.Exchange(ref nextGeneration, 1);
            return 1;
        }

        private readonly struct AttemptedBuildDiagnostics
        {
            private AttemptedBuildDiagnostics(
                int simulationTick,
                BattlePresentationFrame frame,
                BattleCentralBuildDiagnostics diagnostics)
            {
                SimulationTick = simulationTick;
                Frame = frame;
                BuildTick = diagnostics.TickIndex;
                SourceCommandCount = diagnostics.SourceCommandCount;
                ResolvedCommandCount = diagnostics.ResolvedCommandCount;
                UnresolvedCommandCount = diagnostics.UnresolvedCommandCount;
                UnsupportedCategoryCount = diagnostics.UnsupportedCategoryCount;
                UnsupportedRenderStateCount = diagnostics.UnsupportedRenderStateCount;
                ActiveChunkCount = diagnostics.ActiveChunkCount;
                SegmentCount = diagnostics.SegmentCount;
                FirstUnresolvedCommandIndex = diagnostics.FirstUnresolvedCommandIndex;
                FirstUnresolvedCommandType = diagnostics.FirstUnresolvedCommandType;
                FirstUnresolvedStatus = diagnostics.FirstUnresolvedStatus;
                IsValid = true;
            }

            public bool IsValid { get; }
            public int SimulationTick { get; }
            public BattlePresentationFrame Frame { get; }
            public int BuildTick { get; }
            public int SourceCommandCount { get; }
            public int ResolvedCommandCount { get; }
            public int UnresolvedCommandCount { get; }
            public int UnsupportedCategoryCount { get; }
            public int UnsupportedRenderStateCount { get; }
            public int ActiveChunkCount { get; }
            public int SegmentCount { get; }
            public int FirstUnresolvedCommandIndex { get; }
            public BattleRenderCommandType FirstUnresolvedCommandType { get; }
            public BattleCentralResourceStatus FirstUnresolvedStatus { get; }

            public static AttemptedBuildDiagnostics Capture(
                BattleDynamicMeshBackend backend,
                int simulationTick)
            {
                return backend == null
                    ? default
                    : new AttemptedBuildDiagnostics(
                        simulationTick,
                        backend.BuiltFrame,
                        backend.Diagnostics);
            }
        }

        private static int FindRegistration(BattleRenderFeature owner)
        {
            if (owner == null)
                return -1;
            for (int index = featureRegistrationCount - 1; index >= 0; index--)
            {
                if (featureRegistrations[index].Owner == owner)
                    return index;
            }
            return -1;
        }

        private static void RemoveRegistrationAt(int index)
        {
            for (int source = index + 1; source < featureRegistrationCount; source++)
                featureRegistrations[source - 1] = featureRegistrations[source];
            featureRegistrationCount--;
            featureRegistrations[featureRegistrationCount] = default;
        }

        private static void EnsureRegistrationCapacity(int required)
        {
            if (required <= featureRegistrations.Length)
                return;
            int next = featureRegistrations.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref featureRegistrations, next);
        }

        private static void ApplyActiveRegistration()
        {
            FeatureRegistration active = featureRegistrationCount > 0
                ? featureRegistrations[featureRegistrationCount - 1]
                : default;
            featureOwner = active.Owner;
            featureMaterial = active.Material;
            featureArrayMaterial = active.ArrayMaterial;
            serializedDrawMode = featureOwner != null
                ? active.DrawMode
                : BattleCentralDrawMode.OrderedChunks;
            drawPolicyDecision = featureOwner != null
                ? BattleRenderingPolicyResolver.ResolveDraw(GameConfig.Instance, serializedDrawMode)
                : new BattleDrawPolicyDecision(
                    BattleDrawPolicyMode.Auto,
                    BattleCentralDrawMode.OrderedChunks,
                    string.Empty);
            drawMode = drawPolicyDecision.EffectiveMode;
            observedFeatureOwner = null;
            observedRenderer = null;
            observedWorldCamera = null;
            observedUnityFrame = -1;
            RuntimeDiagnostics.FeatureAvailable = featureOwner != null;
            RuntimeDiagnostics.MaterialAvailable = featureMaterial != null;
        }

        private readonly struct FeatureRegistration
        {
            public FeatureRegistration(
                BattleRenderFeature owner,
                Material material,
                Material arrayMaterial,
                BattleCentralDrawMode drawMode)
            {
                Owner = owner;
                Material = material;
                ArrayMaterial = arrayMaterial;
                DrawMode = drawMode;
            }

            public BattleRenderFeature Owner { get; }
            public Material Material { get; }
            public Material ArrayMaterial { get; }
            public BattleCentralDrawMode DrawMode { get; }
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Rendering/BattleDynamicMeshBackend.cs ---
using System;
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using UnityEngine;
using UnityEngine.Rendering;

namespace NTSD.Animation.Rendering
{
    public sealed class BattleDynamicMeshBackend : IDisposable
    {
        public const int QuadsPerChunk = 4096;
        public const int VerticesPerQuad = 4;
        public const int IndicesPerQuad = 6;
        public const int VerticesPerChunk = QuadsPerChunk * VerticesPerQuad;
        public const int IndicesPerChunk = QuadsPerChunk * IndicesPerQuad;
        public const int MaxUInt16VertexIndex = VerticesPerChunk - 1;

        private readonly BattleCentralBuildDiagnostics diagnostics = new BattleCentralBuildDiagnostics();
        private BattleMeshChunk[] chunks = new BattleMeshChunk[1];
        private BattleCentralRenderSegment[] segments = new BattleCentralRenderSegment[16];
        private int activeChunkCount;
        private int segmentCount;
        private int mutationVersion;
        private bool disposed;
        private BattlePresentationFrame builtFrame;

        public BattleCentralBuildDiagnostics Diagnostics => diagnostics;
        public int ActiveChunkCount => activeChunkCount;
        public int SegmentCount => segmentCount;
        public int AllocatedChunkCount => chunks.Length;
        internal int MutationVersion => mutationVersion;
        internal BattlePresentationFrame BuiltFrame => builtFrame;

        public Mesh GetChunkMesh(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].Mesh;
        }

        public int GetChunkActiveQuadCount(int index)
        {
            if ((uint)index >= (uint)activeChunkCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return chunks[index].ActiveQuadCount;
        }

        internal ushort GetChunkIndexTemplateValue(int chunkIndex, int index)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetIndexTemplateValue(index);
        }

        internal float GetChunkVertexAtlasSlice(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexAtlasSlice(vertexIndex);
        }

        internal Color32 GetChunkVertexColor(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexColor(vertexIndex);
        }

        internal Vector2 GetChunkVertexUv(int chunkIndex, int vertexIndex)
        {
            if ((uint)chunkIndex >= (uint)chunks.Length || chunks[chunkIndex] == null)
                throw new ArgumentOutOfRangeException(nameof(chunkIndex));
            return chunks[chunkIndex].GetVertexUv(vertexIndex);
        }

        public BattleCentralRenderSegment GetSegment(int index)
        {
            if ((uint)index >= (uint)segmentCount)
                throw new ArgumentOutOfRangeException(nameof(index));
            return segments[index];
        }

        public void Build(
            BattlePresentationFrame frame,
            IBattleCentralResourceResolver resolver,
            BattleCentralDrawMode drawMode = BattleCentralDrawMode.OrderedChunks,
            BattleTickDetailPhaseDiagnostics detailDiagnostics = null)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(BattleDynamicMeshBackend));
            if (resolver == null)
                throw new ArgumentNullException(nameof(resolver));

            mutationVersion++;
            builtFrame = frame;

            int commandCount = frame?.CommandCount ?? 0;
            diagnostics.Reset(frame?.TickIndex ?? 0, commandCount, drawMode);
            segmentCount = 0;
            int resolvedCount = 0;
            int lastChunkIndex = -1;
            int lastSegmentIndex = -1;

            for (int commandIndex = 0; commandIndex < commandCount; commandIndex++)
            {
                BattleRenderCommand command = frame.GetCommand(commandIndex);
                BattleCentralResolvedResource resource;
                BattleCentralResourceStatus status;
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
                try
                {
                    status = resolver.Resolve(command, out resource);
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameResolveCommands);
                }
                if (status != BattleCentralResourceStatus.Resolved)
                {
                    if (status == BattleCentralResourceStatus.UnsupportedCategory)
                        diagnostics.UnsupportedCategoryCount++;
                    else if (status == BattleCentralResourceStatus.UnsupportedRenderState)
                        diagnostics.UnsupportedRenderStateCount++;
                    else
                        diagnostics.UnresolvedCommandCount++;
                    if (diagnostics.FirstUnresolvedCommandIndex < 0)
                    {
                        diagnostics.FirstUnresolvedCommandIndex = commandIndex;
                        diagnostics.FirstUnresolvedCommandType = command.Type;
                        diagnostics.FirstUnresolvedStatus = status;
                    }
                    // An unresolved command still occupies an authoritative position
                    // in the P3 stream. Never batch resolved commands across it.
                    lastSegmentIndex = -1;
                    lastChunkIndex = -1;
                    continue;
                }

                int chunkIndex = resolvedCount / QuadsPerChunk;
                int quadIndex = resolvedCount % QuadsPerChunk;
                EnsureChunk(chunkIndex);
                BattleMeshChunk chunk = chunks[chunkIndex];
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameWriteQuads);
                try
                {
                    chunk.WriteQuad(quadIndex, command, resource);
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameWriteQuads);
                }

                bool strict = drawMode == BattleCentralDrawMode.StrictOrderedDraw;
                bool canAppend = !strict && lastSegmentIndex >= 0 && lastChunkIndex == chunkIndex &&
                                 IsCompatible(segments[lastSegmentIndex], resource) &&
                                 segments[lastSegmentIndex].FirstQuad + segments[lastSegmentIndex].QuadCount == quadIndex;
                if (canAppend)
                {
                    BattleCentralRenderSegment previous = segments[lastSegmentIndex];
                    segments[lastSegmentIndex] = new BattleCentralRenderSegment(
                        previous.ChunkIndex,
                        previous.SubMeshIndex,
                        previous.FirstCommandIndex,
                        commandIndex - previous.FirstCommandIndex + 1,
                        previous.FirstQuad,
                        previous.QuadCount + 1,
                        previous.Texture,
                        previous.Material,
                        previous.MaterialVariant,
                        previous.AtlasSlice,
                        previous.BindingMode,
                        previous.AtlasPageIndex);
                }
                else
                {
                    EnsureSegmentCapacity(segmentCount + 1);
                    int subMeshIndex = chunk.PendingSegmentCount;
                    chunk.PendingSegmentCount++;
                    segments[segmentCount] = new BattleCentralRenderSegment(
                        chunkIndex,
                        subMeshIndex,
                        commandIndex,
                        1,
                        quadIndex,
                        1,
                        resource.Texture,
                        resource.Material,
                        resource.MaterialVariant,
                        resource.AtlasSlice,
                        resource.BindingMode,
                        resource.AtlasPageIndex);
                    lastSegmentIndex = segmentCount++;
                    lastChunkIndex = chunkIndex;
                }

                resolvedCount++;
            }

            activeChunkCount = resolvedCount == 0 ? 0 : (resolvedCount + QuadsPerChunk - 1) / QuadsPerChunk;
            int segmentCursor = 0;
            for (int chunkIndex = 0; chunkIndex < activeChunkCount; chunkIndex++)
            {
                BattleMeshChunk chunk = chunks[chunkIndex];
                int activeQuads = Math.Min(QuadsPerChunk, resolvedCount - chunkIndex * QuadsPerChunk);
                chunk.Upload(
                    chunkIndex,
                    activeQuads,
                    segments,
                    ref segmentCursor,
                    segmentCount,
                    detailDiagnostics);
            }
            for (int chunkIndex = activeChunkCount; chunkIndex < chunks.Length; chunkIndex++)
                chunks[chunkIndex]?.ClearActive();

            diagnostics.ResolvedCommandCount = resolvedCount;
            diagnostics.ActiveChunkCount = activeChunkCount;
            diagnostics.SegmentCount = segmentCount;
        }

        public void Clear()
        {
            mutationVersion++;
            builtFrame = null;
            segmentCount = 0;
            activeChunkCount = 0;
            builtFrame = null;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.ClearActive();
            diagnostics.Reset(0, 0, BattleCentralDrawMode.OrderedChunks);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;
            for (int i = 0; i < chunks.Length; i++)
                chunks[i]?.Dispose();
            chunks = Array.Empty<BattleMeshChunk>();
            segments = Array.Empty<BattleCentralRenderSegment>();
            activeChunkCount = 0;
            segmentCount = 0;
        }

        private void EnsureChunk(int chunkIndex)
        {
            if (chunkIndex >= chunks.Length)
            {
                int next = chunks.Length;
                while (next <= chunkIndex)
                    next = checked(next * 2);
                Array.Resize(ref chunks, next);
                diagnostics.CapacityGrowthCount++;
            }
            if (chunks[chunkIndex] == null)
            {
                chunks[chunkIndex] = new BattleMeshChunk(chunkIndex);
                diagnostics.CapacityGrowthCount++;
            }
        }

        private void EnsureSegmentCapacity(int required)
        {
            if (required <= segments.Length)
                return;
            int next = segments.Length;
            while (next < required)
                next = checked(next * 2);
            Array.Resize(ref segments, next);
            diagnostics.CapacityGrowthCount++;
        }

        private static bool IsCompatible(
            in BattleCentralRenderSegment segment,
            in BattleCentralResolvedResource resource)
        {
            return segment.Texture == resource.Texture &&
                   segment.Material == resource.Material &&
                   segment.MaterialVariant == resource.MaterialVariant &&
                   segment.BindingMode == resource.BindingMode &&
                   (resource.BindingMode != BattleSpriteCentralBindingMode.AtlasPageTexture2D ||
                    segment.AtlasPageIndex == resource.AtlasPageIndex) &&
                   (resource.BindingMode == BattleSpriteCentralBindingMode.AtlasTextureArray ||
                    segment.AtlasSlice == resource.AtlasSlice);
        }

        private struct BattleQuadVertex
        {
            public Vector3 Position;
            public Color32 Color;
            public Vector2 Uv;
            public float AtlasSlice;
        }

        private sealed class BattleMeshChunk : IDisposable
        {
            private static readonly VertexAttributeDescriptor[] VertexLayout =
            {
                new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
                new VertexAttributeDescriptor(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1),
            };

            private readonly BattleQuadVertex[] vertices = new BattleQuadVertex[VerticesPerChunk];
            private readonly ushort[] indexTemplate = new ushort[IndicesPerChunk];
            private readonly int chunkIndex;
            private Mesh mesh;
            private int activeSubMeshCount;
            private bool hasBounds;
            private Vector3 boundsMin;
            private Vector3 boundsMax;

            public BattleMeshChunk(int index)
            {
                chunkIndex = index;
                for (int quad = 0; quad < QuadsPerChunk; quad++)
                {
                    int vertex = quad * VerticesPerQuad;
                    int indexOffset = quad * IndicesPerQuad;
                    indexTemplate[indexOffset] = (ushort)vertex;
                    indexTemplate[indexOffset + 1] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 2] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 3] = (ushort)(vertex + 2);
                    indexTemplate[indexOffset + 4] = (ushort)(vertex + 1);
                    indexTemplate[indexOffset + 5] = (ushort)(vertex + 3);
                }
                mesh = CreateMesh();
                ClearActive();
            }

            public Mesh Mesh => EnsureMesh();
            public int ActiveQuadCount { get; private set; }
            public int PendingSegmentCount { get; set; }

            public ushort GetIndexTemplateValue(int index)
            {
                if ((uint)index >= (uint)indexTemplate.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return indexTemplate[index];
            }

            public float GetVertexAtlasSlice(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].AtlasSlice;
            }

            public Color32 GetVertexColor(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].Color;
            }

            public Vector2 GetVertexUv(int index)
            {
                if ((uint)index >= (uint)vertices.Length)
                    throw new ArgumentOutOfRangeException(nameof(index));
                return vertices[index].Uv;
            }

            public void WriteQuad(
                int quadIndex,
                in BattleRenderCommand command,
                in BattleCentralResolvedResource resource)
            {
                if ((uint)quadIndex >= QuadsPerChunk)
                    throw new ArgumentOutOfRangeException(nameof(quadIndex));

                Vector2 pixelSize = resource.PixelSize.sqrMagnitude > 0f ? resource.PixelSize : command.Size;
                Vector2 pivot = resource.Pivot;
                float width = pixelSize.x * NTSDRenderSpace.UnitsPerPixelX * NTSDRenderSpace.BattleVisualScale;
                float height = pixelSize.y * NTSDRenderSpace.UnitsPerPixelY * NTSDRenderSpace.BattleVisualScale;
                float left = command.Position.x - pivot.x * width;
                float right = left + width;
                float bottom = command.Position.y - pivot.y * height;
                float top = bottom + height;
                float z = command.Position.z;

                Rect uv = resource.NormalizedUv;
                float u0 = command.FlipX ? uv.xMax : uv.xMin;
                float u1 = command.FlipX ? uv.xMin : uv.xMax;
                float v0 = command.FlipY ? uv.yMax : uv.yMin;
                float v1 = command.FlipY ? uv.yMin : uv.yMax;
                int vertex = quadIndex * VerticesPerQuad;
                vertices[vertex] = CreateVertex(left, bottom, z, u0, v0, resource);
                vertices[vertex + 1] = CreateVertex(left, top, z, u0, v1, resource);
                vertices[vertex + 2] = CreateVertex(right, bottom, z, u1, v0, resource);
                vertices[vertex + 3] = CreateVertex(right, top, z, u1, v1, resource);

                Encapsulate(new Vector3(left, bottom, z));
                Encapsulate(new Vector3(right, top, z));
            }

            public void Upload(
                int chunkIndex,
                int activeQuads,
                BattleCentralRenderSegment[] allSegments,
                ref int segmentCursor,
                int totalSegments,
                BattleTickDetailPhaseDiagnostics detailDiagnostics)
            {
                Mesh targetMesh = EnsureMesh();
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                try
                {
                    int previousActiveSubMeshCount = activeSubMeshCount;
                    int desiredSubMeshCount = PendingSegmentCount;
                    int physicalSubMeshCount = targetMesh.subMeshCount;
                    if (desiredSubMeshCount > physicalSubMeshCount)
                    {
                        targetMesh.subMeshCount = desiredSubMeshCount;
                        // Unity does not guarantee safe default descriptors after native
                        // submesh growth, so reinitialize the complete physical range.
                        for (int subMeshIndex = 0; subMeshIndex < targetMesh.subMeshCount; subMeshIndex++)
                            SetInertSubmesh(targetMesh, subMeshIndex);
                    }
                    else
                    {
                        // Reset every descriptor that was active in the previous upload before
                        // rewriting this frame's active range. Keep the physical high-water;
                        // shrinking subMeshCount here forces Unity to rebuild native state.
                        int inertEnd = Math.Min(previousActiveSubMeshCount, physicalSubMeshCount);
                        for (int subMeshIndex = 0; subMeshIndex < inertEnd; subMeshIndex++)
                            SetInertSubmesh(targetMesh, subMeshIndex);
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                }

                ActiveQuadCount = activeQuads;
                int activeVertices = activeQuads * VerticesPerQuad;
                if (activeVertices > 0)
                {
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData);
                    try
                    {
                        targetMesh.SetVertexBufferData(
                            vertices,
                            0,
                            0,
                            activeVertices,
                            0,
                            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                            MeshUpdateFlags.DontNotifyMeshUsers);
                    }
                    finally
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.RenderPrepareFrameSetVertexBufferData);
                    }
                }

                int desiredActiveSubMeshCount = PendingSegmentCount;
                Bounds currentBounds = CurrentBounds();
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                try
                {
                    for (int activeSubMeshIndex = 0;
                         activeSubMeshIndex < desiredActiveSubMeshCount;
                         activeSubMeshIndex++)
                    {
                        if (segmentCursor >= totalSegments ||
                            allSegments[segmentCursor].ChunkIndex != chunkIndex ||
                            allSegments[segmentCursor].SubMeshIndex != activeSubMeshIndex)
                        {
                            throw new InvalidOperationException(
                                "Chunk submesh descriptors must be contiguous and sequential.");
                        }

                        BattleCentralRenderSegment segment = allSegments[segmentCursor];
                        targetMesh.SetSubMesh(
                            activeSubMeshIndex,
                            new SubMeshDescriptor(
                                segment.FirstQuad * IndicesPerQuad,
                                segment.QuadCount * IndicesPerQuad,
                                MeshTopology.Triangles)
                            {
                                baseVertex = 0,
                                firstVertex = segment.FirstQuad * VerticesPerQuad,
                                vertexCount = segment.QuadCount * VerticesPerQuad,
                                bounds = GetSegmentBounds(segment.FirstQuad, segment.QuadCount),
                            },
                            MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                            MeshUpdateFlags.DontNotifyMeshUsers);
                        segmentCursor++;
                    }
                }
                finally
                {
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.RenderPrepareFrameSetSubMeshes);
                }
                activeSubMeshCount = desiredActiveSubMeshCount;
                targetMesh.bounds = currentBounds;
                PendingSegmentCount = 0;
                hasBounds = false;
            }

            public void ClearActive()
            {
                ActiveQuadCount = 0;
                PendingSegmentCount = 0;
                Mesh targetMesh = mesh;
                if (targetMesh == null)
                {
                    activeSubMeshCount = 0;
                    hasBounds = false;
                    return;
                }
                int physicalSubMeshCount = targetMesh.subMeshCount;
                int inertSubMeshCount = Math.Min(activeSubMeshCount, physicalSubMeshCount);
                for (int subMeshIndex = 0; subMeshIndex < inertSubMeshCount; subMeshIndex++)
                    SetInertSubmesh(targetMesh, subMeshIndex);
                activeSubMeshCount = 0;
                targetMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
                hasBounds = false;
            }

            public void Dispose()
            {
                Mesh targetMesh = mesh;
                mesh = null;
                if (targetMesh == null)
                    return;
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(targetMesh);
                else
                    UnityEngine.Object.DestroyImmediate(targetMesh);
            }

            private Mesh EnsureMesh()
            {
                // With Enter Play Mode domain reload disabled, this managed chunk can
                // outlive the native Mesh Unity destroys on exiting Play Mode.
                if (mesh != null)
                    return mesh;

                activeSubMeshCount = 0;
                mesh = CreateMesh();
                return mesh;
            }

            private Mesh CreateMesh()
            {
                var createdMesh = new Mesh
                {
                    name = $"NTSD Battle Central Chunk {chunkIndex}",
                    indexFormat = IndexFormat.UInt16,
                };
                createdMesh.MarkDynamic();
                createdMesh.SetVertexBufferParams(VerticesPerChunk, VertexLayout);
                createdMesh.SetIndexBufferParams(IndicesPerChunk, IndexFormat.UInt16);
                createdMesh.SetIndexBufferData(
                    indexTemplate,
                    0,
                    0,
                    indexTemplate.Length,
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
                createdMesh.subMeshCount = 1;
                SetInertSubmesh(createdMesh, 0);
                createdMesh.bounds = new Bounds(Vector3.zero, Vector3.zero);
                return createdMesh;
            }

            private static void SetInertSubmesh(Mesh targetMesh, int subMeshIndex)
            {
                targetMesh.SetSubMesh(
                    subMeshIndex,
                    new SubMeshDescriptor(0, 0, MeshTopology.Triangles)
                    {
                        baseVertex = 0,
                        firstVertex = 0,
                        vertexCount = 0,
                        bounds = new Bounds(Vector3.zero, Vector3.zero),
                    },
                    MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                    MeshUpdateFlags.DontNotifyMeshUsers);
            }

            private static BattleQuadVertex CreateVertex(
                float x,
                float y,
                float z,
                float u,
                float v,
                in BattleCentralResolvedResource resource)
            {
                return new BattleQuadVertex
                {
                    Position = new Vector3(x, y, z),
                    Color = resource.Color,
                    Uv = new Vector2(u, v),
                    AtlasSlice = resource.AtlasSlice,
                };
            }

            private void Encapsulate(Vector3 position)
            {
                if (!hasBounds)
                {
                    boundsMin = position;
                    boundsMax = position;
                    hasBounds = true;
                    return;
                }
                boundsMin = Vector3.Min(boundsMin, position);
                boundsMax = Vector3.Max(boundsMax, position);
            }

            private Bounds CurrentBounds()
            {
                if (!hasBounds)
                    return new Bounds(Vector3.zero, Vector3.zero);
                var bounds = new Bounds();
                bounds.SetMinMax(boundsMin, boundsMax);
                return bounds;
            }

            private Bounds GetSegmentBounds(int firstQuad, int quadCount)
            {
                int firstVertex = firstQuad * VerticesPerQuad;
                int endVertex = firstVertex + quadCount * VerticesPerQuad;
                Vector3 min = vertices[firstVertex].Position;
                Vector3 max = min;
                for (int vertex = firstVertex + 1; vertex < endVertex; vertex++)
                {
                    Vector3 position = vertices[vertex].Position;
                    min = Vector3.Min(min, position);
                    max = Vector3.Max(max, position);
                }

                var bounds = new Bounds();
                bounds.SetMinMax(min, max);
                return bounds;
            }
        }
    }
}


--- File: Assets/NTSD/Scripts/Animation/Runtime/BattleSpriteCatalog.cs ---
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NTSD.Animation.Rendering;
using NTSD.Simulation.Presentation;
using UnityEngine;

namespace NTSD.Animation
{
    public enum BattleSpriteCentralBindingMode : byte
    {
        SourceTexture2D = 0,
        AtlasTextureArray = 1,
        AtlasPageTexture2D = 2,
    }

    public readonly struct BattleSpriteCentralBinding
    {
        public BattleSpriteCentralBinding(
            BattleSpriteCentralBindingMode mode,
            Texture texture,
            int atlasSlice,
            Rect normalizedUv,
            Rect atlasContentPixelRect,
            int atlasPageIndex = -1)
        {
            Mode = mode;
            Texture = texture;
            AtlasSlice = atlasSlice;
            AtlasPageIndex = atlasPageIndex;
            NormalizedUv = normalizedUv;
            AtlasContentPixelRect = atlasContentPixelRect;
        }

        public BattleSpriteCentralBindingMode Mode { get; }
        public Texture Texture { get; }
        public int AtlasSlice { get; }
        public int AtlasPageIndex { get; }
        public Rect NormalizedUv { get; }
        public Rect AtlasContentPixelRect { get; }
        public bool IsValid
        {
            get
            {
                if ((Mode != BattleSpriteCentralBindingMode.SourceTexture2D &&
                     Mode != BattleSpriteCentralBindingMode.AtlasTextureArray &&
                     Mode != BattleSpriteCentralBindingMode.AtlasPageTexture2D) ||
                    Texture == null || Texture.width <= 0 || Texture.height <= 0 ||
                    !IsFiniteRect(NormalizedUv) ||
                    NormalizedUv.x < 0f || NormalizedUv.y < 0f ||
                    NormalizedUv.width <= 0f || NormalizedUv.height <= 0f ||
                    NormalizedUv.xMax > 1f || NormalizedUv.yMax > 1f ||
                    !IsFiniteRect(AtlasContentPixelRect) ||
                    AtlasContentPixelRect.x < 0f || AtlasContentPixelRect.y < 0f ||
                    AtlasContentPixelRect.width <= 0f || AtlasContentPixelRect.height <= 0f ||
                    AtlasContentPixelRect.xMax > Texture.width ||
                    AtlasContentPixelRect.yMax > Texture.height)
                {
                    return false;
                }

                switch (Mode)
                {
                    case BattleSpriteCentralBindingMode.SourceTexture2D:
                        if (!(Texture is Texture2D) || AtlasSlice != 0 || AtlasPageIndex != -1)
                            return false;
                        break;
                    case BattleSpriteCentralBindingMode.AtlasTextureArray:
                        if (!(Texture is Texture2DArray arrayTexture) ||
                            AtlasSlice < 0 || AtlasSlice >= arrayTexture.depth ||
                            AtlasPageIndex < 0 || AtlasPageIndex != AtlasSlice)
                        {
                            return false;
                        }
                        break;
                    case BattleSpriteCentralBindingMode.AtlasPageTexture2D:
                        if (!(Texture is Texture2D) || AtlasSlice != 0 || AtlasPageIndex < 0)
                            return false;
                        break;
                    default:
                        return false;
                }

                const float epsilon = 0.001f;
                return Mathf.Abs(NormalizedUv.x * Texture.width - AtlasContentPixelRect.x) <= epsilon &&
                       Mathf.Abs(NormalizedUv.y * Texture.height - AtlasContentPixelRect.y) <= epsilon &&
                       Mathf.Abs(NormalizedUv.width * Texture.width - AtlasContentPixelRect.width) <= epsilon &&
                       Mathf.Abs(NormalizedUv.height * Texture.height - AtlasContentPixelRect.height) <= epsilon;
            }
        }

        private static bool IsFiniteRect(Rect rect)
        {
            return IsFinite(rect.x) && IsFinite(rect.y) &&
                   IsFinite(rect.width) && IsFinite(rect.height);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }

    /// <summary>
    /// Stable lookup key for a battle visual. ObjectId is intentionally not used
    /// here because an entity may replace its current DAT wrapper at runtime.
    /// </summary>
    public readonly struct BattleSpriteKey : IEquatable<BattleSpriteKey>
    {
        public readonly int VisualDataId;
        public readonly int EffectivePic;

        public BattleSpriteKey(int visualDataId, int effectivePic)
        {
            VisualDataId = visualDataId;
            EffectivePic = effectivePic;
        }

        public bool Equals(BattleSpriteKey other) =>
            VisualDataId == other.VisualDataId && EffectivePic == other.EffectivePic;

        public override bool Equals(object obj) => obj is BattleSpriteKey other && Equals(other);

        public override int GetHashCode() => unchecked((VisualDataId * 397) ^ EffectivePic);

        public static bool operator ==(BattleSpriteKey left, BattleSpriteKey right) => left.Equals(right);
        public static bool operator !=(BattleSpriteKey left, BattleSpriteKey right) => !left.Equals(right);

        public override string ToString() => $"({VisualDataId},{EffectivePic})";
    }

    public enum BattleVisualResourceKind : byte
    {
        None = 0,
        EntitySprite = 1,
        CommonShadow = 2,
        CommonSpark = 3,
        CommonWordGlyph = 4,
    }

    public readonly struct BattleVisualResourceKey : IEquatable<BattleVisualResourceKey>
    {
        private readonly BattleSpriteKey entitySpriteKey;
        private readonly int commonSparkPic;
        private readonly int commonWordSheetIndex;
        private readonly int commonWordCharCode;

        private BattleVisualResourceKey(
            BattleVisualResourceKind kind,
            BattleSpriteKey entityKey,
            int commonSparkPic = -1,
            int commonWordSheetIndex = -1,
            int commonWordCharCode = -1)
        {
            Kind = kind;
            entitySpriteKey = entityKey;
            this.commonSparkPic = commonSparkPic;
            this.commonWordSheetIndex = commonWordSheetIndex;
            this.commonWordCharCode = commonWordCharCode;
        }

        public BattleVisualResourceKind Kind { get; }
        public BattleSpriteKey EntitySpriteKey => entitySpriteKey;
        public bool IsEntitySprite => Kind == BattleVisualResourceKind.EntitySprite;
        public bool IsCommonSpark => Kind == BattleVisualResourceKind.CommonSpark;
        public bool IsCommonWordGlyph => Kind == BattleVisualResourceKind.CommonWordGlyph;
        public int CommonSparkPic => commonSparkPic;
        public int CommonWordSheetIndex => commonWordSheetIndex;
        public int CommonWordCharCode => commonWordCharCode;
        public static BattleVisualResourceKey CommonShadow { get; } =
            new BattleVisualResourceKey(BattleVisualResourceKind.CommonShadow, default);

        public static BattleVisualResourceKey CommonSpark(int pic)
        {
            if (pic < 0 || pic >= BattleCommonVisualCatalog.SparkFrameCount)
                throw new ArgumentOutOfRangeException(nameof(pic));
            return new BattleVisualResourceKey(BattleVisualResourceKind.CommonSpark, default, pic);
        }

        public static BattleVisualResourceKey CommonWordGlyph(int sheetIndex, int charCode)
        {
            if (sheetIndex < 0 || sheetIndex >= BattleCommonVisualCatalog.WordSheetCount)
                throw new ArgumentOutOfRangeException(nameof(sheetIndex));
            if (charCode < 0 || charCode >= BattleCommonVisualCatalog.WordGlyphsPerSheet)
                throw new ArgumentOutOfRangeException(nameof(charCode));
            return new BattleVisualResourceKey(
                BattleVisualResourceKind.CommonWordGlyph,
                default,
                -1,
                sheetIndex,
                charCode);
        }

        public static BattleVisualResourceKey FromEntity(BattleSpriteKey key)
        {
            return new BattleVisualResourceKey(BattleVisualResourceKind.EntitySprite, key);
        }

        public bool Equals(BattleVisualResourceKey other)
        {
            return Kind == other.Kind &&
                   (Kind != BattleVisualResourceKind.EntitySprite || entitySpriteKey == other.entitySpriteKey) &&
                   (Kind != BattleVisualResourceKind.CommonSpark || commonSparkPic == other.commonSparkPic) &&
                   (Kind != BattleVisualResourceKind.CommonWordGlyph ||
                    (commonWordSheetIndex == other.commonWordSheetIndex &&
                     commonWordCharCode == other.commonWordCharCode));
        }

        public override bool Equals(object obj) => obj is BattleVisualResourceKey other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                if (Kind == BattleVisualResourceKind.EntitySprite)
                    return ((int)Kind * 397) ^ entitySpriteKey.GetHashCode();
                if (Kind == BattleVisualResourceKind.CommonWordGlyph)
                    return (((int)Kind * 397) ^ commonWordSheetIndex) * 397 ^ commonWordCharCode;
                return ((int)Kind * 397) ^ commonSparkPic;
            }
        }
        public static bool operator ==(BattleVisualResourceKey left, BattleVisualResourceKey right) => left.Equals(right);
        public static bool operator !=(BattleVisualResourceKey left, BattleVisualResourceKey right) => !left.Equals(right);

        public override string ToString()
        {
            if (IsEntitySprite)
                return $"Entity{entitySpriteKey}";
            if (IsCommonSpark)
                return $"CommonSpark({commonSparkPic})";
            if (IsCommonWordGlyph)
                return $"CommonWordGlyph({commonWordSheetIndex},{commonWordCharCode})";
            return Kind.ToString();
        }
    }

    public sealed class BattleCommonVisualBinding
    {
        internal BattleCommonVisualBinding(
            BattleVisualResourceKey key,
            Sprite sprite,
            Texture2D texture,
            Material material,
            Rect pixelRect,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            BattleSpriteRenderState renderState)
            : this(
                key,
                sprite,
                texture,
                material,
                pixelRect,
                normalizedUv,
                pixelSize,
                pivot,
                renderState,
                CreateSourceBinding(texture, pixelRect))
        {
        }

        private BattleCommonVisualBinding(
            BattleVisualResourceKey key,
            Sprite sprite,
            Texture2D texture,
            Material material,
            Rect pixelRect,
            Rect normalizedUv,
            Vector2 pixelSize,
            Vector2 pivot,
            BattleSpriteRenderState renderState,
            BattleSpriteCentralBinding centralBinding)
        {
            Key = key;
            Sprite = sprite;
            Texture = texture;
            Material = material;
            PixelRect = pixelRect;
            NormalizedUv = normalizedUv;
            PixelSize = pixelSize;
            Pivot = pivot;
            RenderState = renderState;
            CentralBinding = centralBinding;
        }

        public BattleVisualResourceKey Key { get; }
        public Sprite Sprite { get; }
        public Texture2D Texture { get; }
        public Material Material { get; }
        public Rect PixelRect { get; }
        public Rect NormalizedUv { get; }
        public Vector2 PixelSize { get; }
        public Vector2 Pivot { get; }
        public BattleSpriteRenderState RenderState { get; }
        public BattleSpriteCentralBinding CentralBinding { get; }
        public Color32 Color => RenderState.Color;
        public int SpriteInstanceId => Sprite != null ? Sprite.GetInstanceID() : 0;
        public int TextureInstanceId => Texture != null ? Texture.GetInstanceID() : 0;
        public int MaterialInstanceId => Material != null ? Material.GetInstanceID() : 0;

        public bool MatchesSprite(Sprite sprite)
        {
            return sprite != null && ReferenceEquals(sprite, Sprite) && ReferenceEquals(sprite.texture, Texture) &&
                   sprite.rect == PixelRect;
        }

        public bool MatchesCommand(in BattleSpriteValueDescriptor descriptor)
        {
            return descriptor.HasLogicalResourceKey &&
                   descriptor.LogicalResourceKey == Key &&
                   descriptor.SpriteInstanceId == SpriteInstanceId &&
                   descriptor.TextureInstanceId == TextureInstanceId &&
                   descriptor.PixelRect == PixelRect &&
                   descriptor.PivotNormalized == Pivot;
        }

        internal BattleCommonVisualBinding WithCentralBinding(
            BattleSpriteCentralBinding centralBinding)
        {
            if (!centralBinding.IsValid)
                throw new ArgumentException("Common visual central binding must be valid.", nameof(centralBinding));

            return new BattleCommonVisualBinding(
                Key,
                Sprite,
                Texture,
                Material,
                PixelRect,
                NormalizedUv,
                PixelSize,
                Pivot,
                RenderState,
                centralBinding);
        }

        private static BattleSpriteCentralBinding CreateSourceBinding(
            Texture2D texture,
            Rect pixelRect)
        {
            float width = texture != null ? texture.width : 0f;
            float height = texture != null ? texture.height : 0f;
            Rect uv = width > 0f && height > 0f
                ? new Rect(
                    pixelRect.x / width,
                    pixelRect.y / height,
                    pixelRect.width / width,
                    pixelRect.height / height)
                : Rect.zero;
            return new BattleSpriteCentralBinding(
                BattleSpriteCentralBindingMode.SourceTexture2D,
                texture,
                0,
                uv,
                pixelRect);
        }
    }

    public sealed class BattleCommonVisualCatalog
    {
        public const int SparkFrameCount = 20;
        public const int WordSheetCount = 6;
        public const int WordGlyphsPerSheet = 256;
        public const int WordGlyphWidth = 8;
        public const int WordGlyphHeight = 16;
        public const int WordTextureWidth = 251;
        public const int WordTextureHeight = 257;
        private readonly BattleCommonVisualBinding[] sparks;
        private readonly Texture2D[] wordTextures;
        private readonly BattleCommonVisualBinding[][] wordGlyphs;

        private BattleCommonVisualCatalog(
            BattleCommonVisualBinding shadow,
            BattleCommonVisualBinding[] sparks,
            Texture2D[] wordTextures,
            BattleCommonVisualBinding[][] wordGlyphs,
            string diagnostic)
        {
            Shadow = shadow;
            this.sparks = sparks ?? Array.Empty<BattleCommonVisualBinding>();
            this.wordTextures = wordTextures ?? Array.Empty<Texture2D>();
            this.wordGlyphs = wordGlyphs ?? Array.Empty<BattleCommonVisualBinding[]>();
            Diagnostic = diagnostic ?? string.Empty;
        }

        public static BattleCommonVisualCatalog Empty { get; } =
            new BattleCommonVisualCatalog(null, null, null, null,
                "Common shadow, spark, and word bindings have not been published.");

        public BattleCommonVisualBinding Shadow { get; }
        public IReadOnlyList<BattleCommonVisualBinding> Sparks => sparks;
        public IReadOnlyList<Texture2D> WordTextures => wordTextures;
        public string Diagnostic { get; }
        public bool IsShadowValid => Shadow != null;
        public bool IsSparkValid => sparks.Length == SparkFrameCount &&
                                     Array.TrueForAll(sparks, binding => binding != null);
        public bool IsWordsValid
        {
            get
            {
                if (wordTextures.Length != WordSheetCount || wordGlyphs.Length != WordSheetCount)
                    return false;

                for (int sheetIndex = 0; sheetIndex < WordSheetCount; sheetIndex++)
                {
                    if (wordTextures[sheetIndex] == null ||
                        wordGlyphs[sheetIndex] == null ||
                        wordGlyphs[sheetIndex].Length != WordGlyphsPerSheet ||
                        Array.Exists(wordGlyphs[sheetIndex], binding => binding == null))
                        return false;
                }

                return true;
            }
        }
        public bool IsValid => IsShadowValid;
        public bool IsComplete => IsShadowValid && IsSparkValid && IsWordsValid;

        public bool TryGetSpark(int pic, out BattleCommonVisualBinding binding)
        {
            if (pic >= 0 && pic < sparks.Length)
            {
                binding = sparks[pic];
                return binding != null;
            }

            binding = null;
            return false;
        }

        public bool TryGetSparkKey(Sprite sprite, out BattleVisualResourceKey key)
        {
            if (sprite != null)
            {
                for (int pic = 0; pic < sparks.Length; pic++)
                {
                    BattleCommonVisualBinding binding = sparks[pic];
                    if (binding != null && binding.MatchesSprite(sprite))
                    {
                        key = binding.Key;
                        return true;
                    }
                }
            }

            key = default;
            return false;
        }

        public bool TryGetWordGlyph(int sheetIndex, int charCode, out BattleCommonVisualBinding binding)
        {
            if (sheetIndex >= 0 && sheetIndex < wordGlyphs.Length &&
                charCode >= 0 && charCode < wordGlyphs[sheetIndex].Length)
            {
                binding = wordGlyphs[sheetIndex][charCode];
                return binding != null;
            }

            binding = null;
            return false;
        }

        public static Rect GetWordGlyphPixelRect(int charCode)
        {
            if (charCode < 0 || charCode >= WordGlyphsPerSheet)
                return Rect.zero;

            int sourceX = WordGlyphHeight * (charCode % 16);
            int sourceYFromTop = WordGlyphHeight * (charCode / 16) + 1;
            return new Rect(
                sourceX,
                WordTextureHeight - sourceYFromTop - WordGlyphHeight,
                WordGlyphWidth,
                WordGlyphHeight);
        }

        public static Vector2 GetWordGlyphPivotNormalized() => new Vector2(0.5f, 0.5f);

        public static bool TryResolveSparkAge(int age, out int pic)
        {
            pic = -1;
            if (age >= 0 && age < 5)
                pic = age;
            else if (age >= 10 && age < 15)
                pic = age - 5;
            else if (age >= 20 && age < 29)
                pic = (age - 20) / 2 + 10;
            else if (age >= 30 && age < 39)
                pic = (age - 30) / 2 + 15;
            return pic >= 0 && pic < SparkFrameCount;
        }

        public static Rect GetSparkPixelRect(int pic)
        {
            if (pic < 0 || pic >= SparkFrameCount)
                return Rect.zero;
            const int textureHeight = 256;
            int sourceX;
            int sourceYFromTop;
            int width;
            int height;
            if (pic < 5)
            {
                sourceX = pic * 102;
                sourceYFromTop = 0;
                width = 102;
                height = 80;
            }
            else if (pic < 10)
            {
                sourceX = (pic - 5) * 61;
                sourceYFromTop = 80;
                width = 61;
                height = 48;
            }
            else if (pic < 15)
            {
                sourceX = (pic - 10) * 102;
                sourceYFromTop = 128;
                width = 102;
                height = 80;
            }
            else
            {
                sourceX = (pic - 15) * 61;
                sourceYFromTop = 208;
                width = 61;
                height = 48;
            }

            return new Rect(sourceX, textureHeight - sourceYFromTop - height, width, height);
        }

        public static Vector2 GetSparkPivotNormalized(int pic)
        {
            return pic < 5 || (pic >= 10 && pic < 15)
                ? new Vector2(51f / 102f, 40f / 80f)
                : new Vector2(30f / 61f, 24f / 48f);
        }

        public static BattleCommonVisualCatalog Build(GameObject shadowPrefab)
        {
            if (shadowPrefab == null)
                return Invalid("GameConfig.ShadowPrefab is missing.");

            BattleCommonShadowDescriptor descriptor =
                shadowPrefab.GetComponent<BattleCommonShadowDescriptor>();
            if (descriptor == null)
                return Invalid("GameConfig.ShadowPrefab is missing its root BattleCommonShadowDescriptor.");
            if (!descriptor.TryValidate(out string diagnostic))
                return Invalid(diagnostic);

            Sprite sprite = descriptor.Sprite;
            Texture2D texture = sprite.texture;
            Material material = descriptor.Material;
            BattleSpriteMaterialSemantic semantic = BattleSpriteMaterialContract.Classify(material);

            Rect pixelRect = sprite.rect;
            Vector2 pivot = new Vector2(
                sprite.pivot.x / pixelRect.width,
                sprite.pivot.y / pixelRect.height);
            Rect normalizedUv = new Rect(
                pixelRect.x / texture.width,
                pixelRect.y / texture.height,
                pixelRect.width / texture.width,
                pixelRect.height / texture.height);
            var renderState = new BattleSpriteRenderState(
                descriptor.Color,
                descriptor.FlipX,
                descriptor.FlipY,
                descriptor.MaskInteraction,
                semantic);
            return new BattleCommonVisualCatalog(
                new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonShadow,
                    sprite,
                    texture,
                    material,
                    pixelRect,
                    normalizedUv,
                    pixelRect.size,
                    pivot,
                    renderState),
                null,
                null,
                null,
                "Spark bindings have not been published.");
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites)
        {
            BattleCommonVisualCatalog shadowOnly = Build(shadowPrefab);
            if (!shadowOnly.IsShadowValid)
                return shadowOnly;
            return shadowOnly.WithSpark(sparkTexture, sparkSprites);
        }

        public static BattleCommonVisualCatalog Build(
            GameObject shadowPrefab,
            Texture2D sparkTexture,
            Sprite[] sparkSprites,
            Texture2D[] wordsTextures,
            Sprite[][] wordGlyphSprites)
        {
            BattleCommonVisualCatalog shadowAndSpark = Build(shadowPrefab, sparkTexture, sparkSprites);
            return shadowAndSpark.WithWords(wordsTextures, wordGlyphSprites);
        }

        public BattleCommonVisualCatalog WithSpark(Texture2D sparkTexture, Sprite[] sparkSprites)
        {
            if (!IsShadowValid)
                return this;
            if (sparkTexture == null || sparkTexture.width < 510 || sparkTexture.height != 256 ||
                sparkSprites == null || sparkSprites.Length != SparkFrameCount)
            {
                return new BattleCommonVisualCatalog(
                    Shadow,
                    null,
                    wordTextures,
                    wordGlyphs,
                    "SPARK.bmp is missing, corrupt, or does not contain 20 bindings.");
            }

            var bindings = new BattleCommonVisualBinding[SparkFrameCount];
            for (int pic = 0; pic < SparkFrameCount; pic++)
            {
                Sprite sprite = sparkSprites[pic];
                Rect expectedRect = GetSparkPixelRect(pic);
                Vector2 expectedPivot = GetSparkPivotNormalized(pic);
                if (sprite == null || sprite.texture != sparkTexture ||
                    sprite.rect != expectedRect ||
                    new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height) != expectedPivot)
                    return new BattleCommonVisualCatalog(
                        Shadow,
                        null,
                        wordTextures,
                        wordGlyphs,
                        $"SPARK binding {pic} is missing or references the wrong texture.");

                Rect pixelRect = sprite.rect;
                Vector2 pivot = new Vector2(
                    sprite.pivot.x / pixelRect.width,
                    sprite.pivot.y / pixelRect.height);
                Rect normalizedUv = new Rect(
                    pixelRect.x / sparkTexture.width,
                    pixelRect.y / sparkTexture.height,
                    pixelRect.width / sparkTexture.width,
                    pixelRect.height / sparkTexture.height);
                bindings[pic] = new BattleCommonVisualBinding(
                    BattleVisualResourceKey.CommonSpark(pic),
                    sprite,
                    sparkTexture,
                    null,
                    pixelRect,
                    normalizedUv,
                    pixelRect.size,
                    pivot,
                    new BattleSpriteRenderState(
                        Color.white,
                        false,
                        false,
                        SpriteMaskInteraction.None,
                        BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha));
            }

            return new BattleCommonVisualCatalog(Shadow, bindings, wordTextures, wordGlyphs,
                IsWordsValid ? string.Empty : "WORDS bindings have not been published.");
        }

        public BattleCommonVisualCatalog WithWords(Texture2D[] wordsTextures, Sprite[][] wordGlyphSprites)
        {
            if (!IsShadowValid || !IsSparkValid)
                return this;
            if (wordsTextures == null || wordsTextures.Length != WordSheetCount ||
                wordGlyphSprites == null || wordGlyphSprites.Length != WordSheetCount)
            {
                return new BattleCommonVisualCatalog(
                    Shadow,
                    sparks,
                    null,
                    null,
                    "WORDS0.bmp through WORDS5.bmp must publish six 251x257 glyph sheets.");
            }

            var textures = new Texture2D[WordSheetCount];
            var bindings = new BattleCommonVisualBinding[WordSheetCount][];
            for (int sheetIndex = 0; sheetIndex < WordSheetCount; sheetIndex++)
            {
                Texture2D texture = wordsTextures[sheetIndex];
                Sprite[] sprites = wordGlyphSprites[sheetIndex];
                if (texture == null || texture.width != WordTextureWidth || texture.height != WordTextureHeight ||
                    sprites == null || sprites.Length != WordGlyphsPerSheet)
                {
                    return new BattleCommonVisualCatalog(
                        Shadow,
                        sparks,
                        null,
                        null,
                        $"WORDS{sheetIndex}.bmp is missing, corrupt, or does not contain 256 glyph bindings.");
                }

                textures[sheetIndex] = texture;
                bindings[sheetIndex] = new BattleCommonVisualBinding[WordGlyphsPerSheet];
                for (int charCode = 0; charCode < WordGlyphsPerSheet; charCode++)
                {
                    Sprite sprite = sprites[charCode];
                    Rect expectedRect = GetWordGlyphPixelRect(charCode);
                    Vector2 expectedPivot = GetWordGlyphPivotNormalized();
                    if (sprite == null || sprite.texture != texture || sprite.rect != expectedRect ||
                        new Vector2(sprite.pivot.x / sprite.rect.width, sprite.pivot.y / sprite.rect.height) != expectedPivot)
                    {
                        return new BattleCommonVisualCatalog(
                            Shadow,
                            sparks,
                            null,
                            null,
                            $"WORDS{sheetIndex} glyph {charCode} is missing or references the wrong texture.");
                    }

                    Rect pixelRect = sprite.rect;
                    Vector2 pivot = new Vector2(
                        sprite.pivot.x / pixelRect.width,
                        sprite.pivot.y / pixelRect.height);
                    Rect normalizedUv = new Rect(
                        pixelRect.x / texture.width,
                        pixelRect.y / texture.height,
                        pixelRect.width / texture.width,
                        pixelRect.height / texture.height);
                    bindings[sheetIndex][charCode] = new BattleCommonVisualBinding(
                        BattleVisualResourceKey.CommonWordGlyph(sheetIndex, charCode),
                        sprite,
                        texture,
                        null,
                        pixelRect,
                        normalizedUv,
                        pixelRect.size,
                        pivot,
                        new BattleSpriteRenderState(
                            Color.white,
                            false,
                            false,
                            SpriteMaskInteraction.None,
                            BattleSpriteMaterialSemantic.PremultipliedSpriteAlpha));
                }
            }

            return new BattleCommonVisualCatalog(Shadow, sparks, textures, bindings, string.Empty);
        }

        internal BattleCommonVisualCatalog WithCentralBindings(
            IReadOnlyDictionary<BattleVisualResourceKey, BattleSpriteCentralBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));
            if (!IsComplete)
                throw new InvalidOperationException("A complete common visual catalog is required before central bindings can be published.");

            BattleCommonVisualBinding remappedShadow = RemapBinding(Shadow, bindings);
            var remappedSparks = new BattleCommonVisualBinding[sparks.Length];
            for (int pic = 0; pic < sparks.Length; pic++)
                remappedSparks[pic] = RemapBinding(sparks[pic], bindings);

            var remappedWords = new BattleCommonVisualBinding[wordGlyphs.Length][];
            for (int sheetIndex = 0; sheetIndex < wordGlyphs.Length; sheetIndex++)
            {
                BattleCommonVisualBinding[] sourceGlyphs = wordGlyphs[sheetIndex];
                remappedWords[sheetIndex] = new BattleCommonVisualBinding[sourceGlyphs.Length];
                for (int charCode = 0; charCode < sourceGlyphs.Length; charCode++)
                {
                    remappedWords[sheetIndex][charCode] =
                        RemapBinding(sourceGlyphs[charCode], bindings);
                }
            }

            return new BattleCommonVisualCatalog(
                remappedShadow,
                remappedSparks,
                wordTextures,
                remappedWords,
                Diagnostic);
        }

        private static BattleCommonVisualBinding RemapBinding(
            BattleCommonVisualBinding source,
            IReadOnlyDictionary<BattleVisualResourceKey, BattleSpriteCentralBinding> bindings)
        {
            if (source == null ||
                !bindings.TryGetValue(source.Key, out BattleSpriteCentralBinding centralBinding) ||
                !centralBinding.IsValid)
            {
                throw new InvalidOperationException(
                    $"Missing central atlas binding for common visual {source?.Key.ToString() ?? "<null>"}.");
            }

            return source.WithCentralBinding(centralBinding);
        }

        private static BattleCommonVisualCatalog Invalid(string diagnostic)
        {
            return new BattleCommonVisualCatalog(null, null, null, null, diagnostic);
        }
    }

    /// <summary>
    /// Immutable source-rect and metric data consumed by legacy and future
    /// render backends. Rect coordinates use Unity's bottom-left pixel origin.
    /// </summary>
    public sealed class BattleSpriteEntry
    {
        public BattleSpriteKey Key { get; }
        public string SourceSheetPath { get; }
        public Texture2D SharedTexture { get; }
        public Rect PixelRect { get; }
        public Rect NormalizedUv { get; }
        public float PixelWidth { get; }
        public float PixelHeight { get; }
        public Vector2 Pivot { get; }
        public Sprite LegacySprite { get; }
        public BattleSpriteCentralBinding CentralBinding { get; }

        public BattleSpriteEntry(
            BattleSpriteKey key,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Vector2 pivot,
            Sprite legacySprite)
            : this(
                key,
                sourceSheetPath,
                sharedTexture,
                pixelRect,
                pivot,
                legacySprite,
                CreateSourceBinding(sharedTexture, pixelRect))
        {
        }

        internal BattleSpriteEntry(
            BattleSpriteKey key,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Vector2 pivot,
            Sprite legacySprite,
            BattleSpriteCentralBinding centralBinding)
        {
            Key = key;
            SourceSheetPath = sourceSheetPath ?? string.Empty;
            SharedTexture = sharedTexture;
            PixelRect = pixelRect;
            PixelWidth = pixelRect.width;
            PixelHeight = pixelRect.height;
            Pivot = pivot;
            LegacySprite = legacySprite;
            CentralBinding = centralBinding;

            float textureWidth = sharedTexture != null ? sharedTexture.width : 0f;
            float textureHeight = sharedTexture != null ? sharedTexture.height : 0f;
            NormalizedUv = textureWidth > 0f && textureHeight > 0f
                ? new Rect(
                    pixelRect.x / textureWidth,
                    pixelRect.y / textureHeight,
                    pixelRect.width / textureWidth,
                    pixelRect.height / textureHeight)
                : Rect.zero;
        }

        internal BattleSpriteEntry WithCentralBinding(BattleSpriteCentralBinding centralBinding)
        {
            return new BattleSpriteEntry(
                Key,
                SourceSheetPath,
                SharedTexture,
                PixelRect,
                Pivot,
                LegacySprite,
                centralBinding);
        }

        private static BattleSpriteCentralBinding CreateSourceBinding(Texture2D texture, Rect pixelRect)
        {
            float width = texture != null ? texture.width : 0f;
            float height = texture != null ? texture.height : 0f;
            Rect uv = width > 0f && height > 0f
                ? new Rect(pixelRect.x / width, pixelRect.y / height, pixelRect.width / width, pixelRect.height / height)
                : Rect.zero;
            return new BattleSpriteCentralBinding(
                BattleSpriteCentralBindingMode.SourceTexture2D,
                texture,
                0,
                uv,
                pixelRect);
        }
    }

    /// <summary>
    /// Immutable catalog published only after a complete prewarm pass succeeds.
    /// The builder below is intentionally the only mutable construction API.
    /// </summary>
    public sealed class BattleSpriteCatalog
    {
        private static readonly IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> EmptyEntries =
            new ReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry>(
                new Dictionary<BattleSpriteKey, BattleSpriteEntry>());

        private readonly IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> _entries;
        private readonly IReadOnlyDictionary<Sprite, BattleSpriteKey[]> _reverseKeys;

        public static BattleSpriteCatalog Empty { get; } =
            new BattleSpriteCatalog(EmptyEntries);

        public int Count => _entries.Count;

        internal BattleSpriteCatalog(IDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            var immutableEntries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>(entries);
            _entries = new ReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry>(immutableEntries);
            _reverseKeys = BuildReverseKeys(immutableEntries);
        }

        private BattleSpriteCatalog(IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            _entries = entries;
            _reverseKeys = BuildReverseKeys(entries);
        }

        public bool TryGet(int visualDataId, int effectivePic, out BattleSpriteEntry entry)
        {
            return _entries.TryGetValue(new BattleSpriteKey(visualDataId, effectivePic), out entry);
        }

        public bool TryGet(BattleSpriteKey key, out BattleSpriteEntry entry)
        {
            return _entries.TryGetValue(key, out entry);
        }

        public bool TryGetKey(Sprite legacySprite, out BattleSpriteKey key)
        {
            if (legacySprite == null)
            {
                key = default;
                return false;
            }
            if (_reverseKeys.TryGetValue(legacySprite, out BattleSpriteKey[] keys) && keys.Length == 1)
            {
                key = keys[0];
                return true;
            }
            key = default;
            return false;
        }

        public bool TryGetKey(
            Sprite legacySprite,
            BattleSpriteKey preferredKey,
            out BattleSpriteKey key)
        {
            if (legacySprite != null &&
                _reverseKeys.TryGetValue(legacySprite, out BattleSpriteKey[] keys))
            {
                for (int index = 0; index < keys.Length; index++)
                {
                    if (keys[index] == preferredKey)
                    {
                        key = preferredKey;
                        return true;
                    }
                }
                if (keys.Length == 1)
                {
                    key = keys[0];
                    return true;
                }
            }
            key = default;
            return false;
        }

        public IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> Entries => _entries;

        internal BattleSpriteCatalog WithCentralBindings(
            IReadOnlyDictionary<BattleSpriteKey, BattleSpriteCentralBinding> bindings)
        {
            if (bindings == null)
                throw new ArgumentNullException(nameof(bindings));

            var entries = new Dictionary<BattleSpriteKey, BattleSpriteEntry>(_entries.Count);
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in _entries)
            {
                if (!bindings.TryGetValue(pair.Key, out BattleSpriteCentralBinding binding) || !binding.IsValid)
                    throw new InvalidOperationException($"Missing central atlas binding for battle sprite {pair.Key}.");
                entries.Add(pair.Key, pair.Value.WithCentralBinding(binding));
            }
            return new BattleSpriteCatalog((IDictionary<BattleSpriteKey, BattleSpriteEntry>)entries);
        }

        private static IReadOnlyDictionary<Sprite, BattleSpriteKey[]> BuildReverseKeys(
            IReadOnlyDictionary<BattleSpriteKey, BattleSpriteEntry> entries)
        {
            var mutableKeys = new Dictionary<Sprite, List<BattleSpriteKey>>();
            foreach (KeyValuePair<BattleSpriteKey, BattleSpriteEntry> pair in entries)
            {
                Sprite sprite = pair.Value?.LegacySprite;
                if (sprite == null)
                    continue;
                if (!mutableKeys.TryGetValue(sprite, out List<BattleSpriteKey> keys))
                {
                    keys = new List<BattleSpriteKey>(1);
                    mutableKeys.Add(sprite, keys);
                }
                keys.Add(pair.Key);
            }

            var reverseKeys = new Dictionary<Sprite, BattleSpriteKey[]>(mutableKeys.Count);
            foreach (KeyValuePair<Sprite, List<BattleSpriteKey>> pair in mutableKeys)
                reverseKeys.Add(pair.Key, pair.Value.ToArray());
            return new ReadOnlyDictionary<Sprite, BattleSpriteKey[]>(reverseKeys);
        }
    }

    public sealed class BattleSpriteCatalogLease : IDisposable
    {
        private Action release;

        internal BattleSpriteCatalogLease(BattleSpriteCatalog catalog, Action releaseAction)
        {
            Catalog = catalog ?? BattleSpriteCatalog.Empty;
            release = releaseAction;
        }

        public BattleSpriteCatalog Catalog { get; }
        public bool IsReleased => release == null;

        public void Dispose()
        {
            Action releaseAction = release;
            release = null;
            releaseAction?.Invoke();
        }
    }

    public sealed class BattleSpriteCatalogBuilder
    {
        private readonly Dictionary<BattleSpriteKey, BattleSpriteEntry> _entries =
            new Dictionary<BattleSpriteKey, BattleSpriteEntry>();

        public int Count => _entries.Count;

        public void Add(
            int visualDataId,
            int effectivePic,
            string sourceSheetPath,
            Texture2D sharedTexture,
            Rect pixelRect,
            Sprite legacySprite)
        {
            if (visualDataId < 0)
                throw new ArgumentOutOfRangeException(nameof(visualDataId));
            if (effectivePic < 0)
                throw new ArgumentOutOfRangeException(nameof(effectivePic));
            if (sharedTexture == null)
                throw new ArgumentNullException(nameof(sharedTexture));
            if (pixelRect.width <= 0f || pixelRect.height <= 0f)
                throw new ArgumentOutOfRangeException(nameof(pixelRect));

            var key = new BattleSpriteKey(visualDataId, effectivePic);
            if (_entries.ContainsKey(key))
            {
                throw new InvalidOperationException(
                    $"Duplicate battle sprite key {key}; overlapping DAT file ranges are not allowed.");
            }

            _entries.Add(key, new BattleSpriteEntry(
                key,
                sourceSheetPath,
                sharedTexture,
                pixelRect,
                new Vector2(0.5f, 0f),
                legacySprite));
        }

        public BattleSpriteCatalog Publish()
        {
            return new BattleSpriteCatalog(_entries);
        }
    }
}


--- File: Assets/NTSD/Scripts/Simulation/Presentation/BattleEntityOverlayLayout.cs ---
namespace NTSD.Simulation.Presentation
{
    public enum BattleEntityOverlayGlyphType : byte
    {
        Counter = 0,
        Label = 1,
    }

    /// <summary>
    /// A single glyph placement in a WORDS bitmap sheet.
    /// </summary>
    public struct BattleEntityOverlayGlyph
    {
        public int CharCode;
        public int SheetIndex;
        public int PixelX;
        public int PixelY;
        public int Sequence;
        public BattleEntityOverlayGlyphType Type;
    }

    /// <summary>
    /// Runtime-only values required by the BattleHostForm/SdlBattleRenderer overlay rules.
    /// </summary>
    public readonly struct BattleEntityOverlayRuntimeSlot
    {
        public BattleEntityOverlayRuntimeSlot(
            int slotIndex,
            int hp2Orig,
            int relationTeam,
            int objType,
            int oid,
            int hitStop,
            int xInt,
            int yInt,
            int zInt,
            int renderOffsetX,
            int cameraX,
            int centerY)
        {
            SlotIndex = slotIndex;
            HP2Orig = hp2Orig;
            RelationTeam = relationTeam;
            ObjType = objType;
            Oid = oid;
            HitStop = hitStop;
            XInt = xInt;
            YInt = yInt;
            ZInt = zInt;
            RenderOffsetX = renderOffsetX;
            CameraX = cameraX;
            CenterY = centerY;
        }

        public int SlotIndex { get; }
        public int HP2Orig { get; }
        public int RelationTeam { get; }
        public int ObjType { get; }
        public int Oid { get; }
        public int HitStop { get; }
        public int XInt { get; }
        public int YInt { get; }
        public int ZInt { get; }
        public int RenderOffsetX { get; }
        public int CameraX { get; }
        public int CenterY { get; }
    }

    /// <summary>
    /// Allocation-free layout shared by command and legacy overlay renderers.
    /// </summary>
    public static class BattleEntityOverlayLayout
    {
        public const int SlotCount = 10;
        public const int SlotLabelCharacterCapacity = 12;
        public const int GlyphAdvance = 9;

        public static bool TryBuild(
            in BattleEntityOverlayRuntimeSlot entity,
            char[,] slotLabelChars,
            int[] slotLabelState,
            BattleEntityOverlayGlyph[] glyphBuffer,
            out int glyphCount)
        {
            glyphCount = 0;
            if (slotLabelChars == null ||
                slotLabelChars.GetLength(0) < SlotCount ||
                slotLabelChars.GetLength(1) < SlotLabelCharacterCapacity ||
                slotLabelState == null ||
                slotLabelState.Length < SlotCount ||
                glyphBuffer == null)
            {
                return false;
            }

            int counterLength = entity.HP2Orig > 1 ? (entity.HP2Orig <= 9 ? 2 : 3) : 0;
            int labelLength = GetLabelLength(in entity, slotLabelChars);

            bool bracketed = entity.SlotIndex >= 0 && entity.SlotIndex < SlotCount &&
                             slotLabelState[entity.SlotIndex] == -1 &&
                             !IsSpecialCom(in entity);
            if (bracketed)
                labelLength += 2;

            int required = counterLength + labelLength;
            if (glyphBuffer.Length < required)
                return false;

            int sequence = 0;
            if (counterLength != 0)
            {
                int counterX = entity.XInt + entity.RenderOffsetX - ((GlyphAdvance * counterLength) >> 1) - entity.CameraX;
                int counterY = entity.ZInt + entity.YInt - entity.CenterY - 7;
                WriteGlyph(glyphBuffer, ref sequence, 'x', 0, counterX, counterY, BattleEntityOverlayGlyphType.Counter);
                if (counterLength == 3)
                    WriteGlyph(glyphBuffer, ref sequence, (char)('0' + ((entity.HP2Orig / 10) % 10)), 0, counterX + GlyphAdvance, counterY, BattleEntityOverlayGlyphType.Counter);
                WriteGlyph(glyphBuffer, ref sequence, (char)('0' + (entity.HP2Orig % 10)), 0, counterX + (counterLength - 1) * GlyphAdvance, counterY, BattleEntityOverlayGlyphType.Counter);
            }

            if (labelLength != 0)
            {
                int sheetIndex = IsSpecialCom(in entity) ? 5 : ResolveRelationSheet(entity.RelationTeam);
                int labelX = entity.XInt + entity.RenderOffsetX - ((GlyphAdvance * labelLength) >> 1) - entity.CameraX;
                int maxX = 794 - GlyphAdvance * labelLength;
                if (labelX < 0)
                    labelX = 0;
                if (labelX > maxX)
                    labelX = maxX;

                int labelY = entity.ZInt + 3;
                if (IsSpecialCom(in entity) || (entity.SlotIndex < 0 || entity.SlotIndex >= SlotCount))
                {
                    WriteGlyph(glyphBuffer, ref sequence, 'C', sheetIndex, labelX, labelY, BattleEntityOverlayGlyphType.Label);
                    WriteGlyph(glyphBuffer, ref sequence, 'o', sheetIndex, labelX + GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);
                    WriteGlyph(glyphBuffer, ref sequence, 'm', sheetIndex, labelX + GlyphAdvance * 2, labelY, BattleEntityOverlayGlyphType.Label);
                }
                else
                {
                    int offset = 0;
                    if (bracketed)
                    {
                        WriteGlyph(glyphBuffer, ref sequence, '[', sheetIndex, labelX, labelY, BattleEntityOverlayGlyphType.Label);
                        offset = 1;
                    }

                    for (int i = 0; i < labelLength - (bracketed ? 2 : 0); i++)
                        WriteGlyph(glyphBuffer, ref sequence, slotLabelChars[entity.SlotIndex, i], sheetIndex, labelX + (offset + i) * GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);

                    if (bracketed)
                        WriteGlyph(glyphBuffer, ref sequence, ']', sheetIndex, labelX + (labelLength - 1) * GlyphAdvance, labelY, BattleEntityOverlayGlyphType.Label);
                }
            }

            glyphCount = sequence;
            return true;
        }

        private static int GetLabelLength(in BattleEntityOverlayRuntimeSlot entity, char[,] slotLabelChars)
        {
            if ((entity.SlotIndex >= 20 && (entity.RelationTeam == 5 || entity.ObjType != 0)) || entity.HitStop <= -25)
                return IsSpecialCom(in entity) ? 3 : 0;

            if (entity.SlotIndex < 0 || entity.SlotIndex >= SlotCount)
                return 3;

            int length = 0;
            while (length < SlotLabelCharacterCapacity && slotLabelChars[entity.SlotIndex, length] != '\0')
                length++;
            return length;
        }

        private static bool IsSpecialCom(in BattleEntityOverlayRuntimeSlot entity)
        {
            return entity.SlotIndex >= 20 &&
                   entity.HitStop > -25 &&
                   entity.ObjType == 0 &&
                   entity.RelationTeam == 5 &&
                   (entity.Oid < 30 || entity.Oid >= 50 || entity.Oid == 38);
        }

        private static int ResolveRelationSheet(int relationTeam)
        {
            return relationTeam >= 1 && relationTeam <= 4 ? relationTeam : 0;
        }

        private static void WriteGlyph(
            BattleEntityOverlayGlyph[] glyphBuffer,
            ref int sequence,
            char charCode,
            int sheetIndex,
            int pixelX,
            int pixelY,
            BattleEntityOverlayGlyphType type)
        {
            glyphBuffer[sequence] = new BattleEntityOverlayGlyph
            {
                CharCode = charCode,
                SheetIndex = sheetIndex,
                PixelX = pixelX,
                PixelY = pixelY,
                Sequence = sequence,
                Type = type,
            };
            sequence++;
        }
    }
}


--- File: Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs ---
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class RoleAwareCollisionShadowSelfCheckTests
    {
        [Test]
        public void Shadow_DefaultsOff_AndOnlyEmitsAttackToBodyPairs()
        {
            var world = new SimulationWorld();
            LF2FrameData attackerFrame = MakeFrame(
                new InteractionArea { kind = 0, x = 100, y = -10, w = 20, h = 20, zwidth = 15 },
                new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 });
            LF2Character attacker = CreateCharacter("RoleShadow_Attacker", 1, attackerFrame);
            LF2Character target = CreateCharacter(
                "RoleShadow_Target",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            Assert.That(query.ShadowBroadphaseDiagnosticsEnabled, Is.False);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.RoleAwareShadowDiagnostics.RebuildCount, Is.Zero);
            world.EndCollisionCandidateConsumption();

            query.ShadowBroadphaseDiagnosticsEnabled = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            RoleAwareCollisionShadowDiagnostics diagnostics = query.RoleAwareShadowDiagnostics;
            Assert.That(diagnostics.ParticipantCount, Is.EqualTo(2));
            Assert.That(diagnostics.BodyCount, Is.EqualTo(2));
            Assert.That(diagnostics.IndexedBodyCount, Is.EqualTo(2));
            Assert.That(diagnostics.AttackItrCount, Is.EqualTo(1));
            Assert.That(diagnostics.BrutePairCount, Is.Zero);
            Assert.That(diagnostics.QuadtreePairCount, Is.Zero);
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.CollectionAborted, Is.False);
            world.EndCollisionCandidateConsumption();

            attackerFrame.itrs[0].x = -10;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(diagnostics.BrutePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.QuadtreePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            Assert.That(diagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(diagnostics.FirstDifference,
                Is.EqualTo(RoleAwareCollisionShadowDifference.None));
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void Shadow_InvalidRoleBoundsFallbackConservatively()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleShadow_InvalidItr",
                1,
                MakeFrame(
                    new InteractionArea { kind = 0, x = 0, y = 0, w = 0, h = 20, zwidth = 15 },
                    null));
            LF2Character target = CreateCharacter(
                "RoleShadow_FallbackTarget",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.ShadowBroadphaseDiagnosticsEnabled = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            RoleAwareCollisionShadowDiagnostics diagnostics = query.RoleAwareShadowDiagnostics;
            Assert.That(diagnostics.FallbackAttackItrCount, Is.EqualTo(1));
            Assert.That(diagnostics.BrutePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.QuadtreePairCount, Is.EqualTo(1));
            Assert.That(diagnostics.MismatchCount, Is.Zero);
            world.EndCollisionCandidateConsumption();
        }

        [Test]
        public void Shadow_ExceptionCannotChangeFormalCollection_AndNextRunResetsAbort()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleShadow_AbortAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                        injury = 10,
                        dvx = 1,
                        arest = 4,
                        vrest = 1,
                    },
                    null));
            LF2Character target = CreateCharacter(
                "RoleShadow_AbortTarget",
                2,
                MakeFrame(null, new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            RegisterPair(world, attacker, target);

            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.ShadowBroadphaseDiagnosticsEnabled = true;
            query.ThrowDuringRoleAwareShadowForSelfCheck = true;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker,
                out List<SceneQueryHit> candidates), Is.True);
            Assert.That(query.RoleAwareShadowDiagnostics.CollectionAborted, Is.True);
            Assert.That(query.RoleAwareShadowDiagnostics.MismatchCount, Is.Zero);
            Assert.That(query.RoleAwareShadowDiagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(query.FormalCollectionAborted, Is.False);
            Assert.That(candidates.Count, Is.EqualTo(1));
            world.EndCollisionCandidateConsumption();

            query.ThrowDuringRoleAwareShadowForSelfCheck = false;
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.RoleAwareShadowDiagnostics.CollectionAborted, Is.False);
            Assert.That(query.RoleAwareShadowDiagnostics.ParticipantCount, Is.EqualTo(2));
            Assert.That(query.RoleAwareShadowDiagnostics.MismatchCount, Is.Zero);
            Assert.That(query.RoleAwareShadowDiagnostics.FirstDifferencePair, Is.EqualTo(-1));
            Assert.That(query.FormalCollectionAborted, Is.False);
            world.EndCollisionCandidateConsumption();
        }

        private static LF2FrameData MakeFrame(InteractionArea itr, BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new ShadowSelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void RegisterPair(
            SimulationWorld world,
            LF2Character attacker,
            LF2Character target)
        {
            world.Register(attacker);
            Configure(attacker, 1);
            world.Register(target);
            Configure(target, 2);
        }

        private static void Configure(LF2Entity entity, int team)
        {
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class ShadowSelfCheckController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }

    public sealed class RoleAwareCollisionFormalCollectorSelfCheckTests
    {
        private const uint CollectionSeed = 0x41C64E6Du;

        [Test]
        public void Formal_DefaultConfiguredModeKeepsProductionCollectorUnchanged()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_DefaultAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character target = CreateCharacter(
                "RoleFormal_DefaultTarget",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, attacker, 0, 1, 0);
            Register(world, target, 1, 2, 0);

            BruteForceSceneQuery query = GetQuery(world);
            Assert.That(query.FormalCollectorMode,
                Is.EqualTo(CollisionFormalCollectorMode.Configured));
            RunCollection(world, query, CollisionFormalCollectorMode.Configured, attacker);
            Assert.That(query.LastFormalCollectorModeForDiagnostics,
                Is.EqualTo(CollisionFormalCollectorMode.ForceBruteForce));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_RoleAwareMatchesBruteExactSequenceCountAndRng_WithAuthorityOrder()
        {
            var world = new SimulationWorld();
            LF2Character registeredFirstHighSlot = CreateCharacter(
                "RoleFormal_HighSlot",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -25,
                        y = -10,
                        w = 50,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox { kind = 0, x = 5, y = -10, w = 20, h = 20 }));
            LF2Character registeredSecondLowSlot = CreateCharacter(
                "RoleFormal_LowSlot",
                2,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -25,
                        y = -10,
                        w = 50,
                        h = 20,
                        zwidth = 15,
                    },
                    new BodyBox { kind = 0, x = -5, y = -10, w = 20, h = 20 }));

            Register(world, registeredFirstHighSlot, 9, 1, -192);
            Register(world, registeredSecondLowSlot, 2, 2, -91);
            registeredFirstHighSlot.Runtime.SetPosition(0, 0, 0);
            registeredFirstHighSlot.Runtime.SyncIntegerPosition();
            registeredSecondLowSlot.Runtime.SetPosition(0, 0, 0);
            registeredSecondLowSlot.Runtime.SyncIntegerPosition();

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                registeredSecondLowSlot,
                registeredFirstHighSlot);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                registeredSecondLowSlot,
                registeredFirstHighSlot);

            AssertRunsEqual(brute, role);
            Assert.That(role.Sequences[0].Count, Is.EqualTo(1));
            Assert.That(role.Sequences[0][0].TargetSlot, Is.EqualTo(9));
            Assert.That(role.Sequences[0][0].BodyX, Is.EqualTo(5));
            Assert.That(role.Sequences[1].Count, Is.EqualTo(1));
            Assert.That(role.Sequences[1][0].TargetSlot, Is.EqualTo(2));
            Assert.That(role.Sequences[1][0].BodyX, Is.EqualTo(-5));
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(1));
            Assert.That(query.LastRoleAwareBodyEntryCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastRoleAwareItrQueryCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_RoleAwareMatchesBruteForTwentyCapAndRoleOnlyParticipants()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_CapAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -20,
                        y = -20,
                        w = 40,
                        h = 40,
                        zwidth = 15,
                    },
                    null));
            Register(world, attacker, 30, 1, 0);

            var targets = new List<LF2Character>();
            for (int slot = 0; slot < 21; slot++)
            {
                LF2Character target = CreateCharacter(
                    $"RoleFormal_CapTarget_{slot}",
                    100 + slot,
                    MakeFrame(
                        null,
                        new BodyBox
                        {
                            kind = 0,
                            x = slot - 10,
                            y = -10,
                            w = 5,
                            h = 20,
                        }));
                Register(world, target, slot, 2, 0);
                targets.Add(target);
            }

            LF2Character itrOnlyFarAway = CreateCharacter(
                "RoleFormal_ItrOnlyFarAway",
                200,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, itrOnlyFarAway, 31, 3, 10000);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);

            AssertRunsEqual(brute, role);
            Assert.That(role.Sequences[0].Count, Is.EqualTo(20));
            for (int candidateIndex = 0; candidateIndex < 20; candidateIndex++)
            {
                Assert.That(
                    role.Sequences[0][candidateIndex].TargetSlot,
                    Is.EqualTo(candidateIndex));
            }
            Assert.That(
                role.Sequences[0].Exists(hit => hit.TargetSlot == targets[20].Runtime.SlotIndex),
                Is.False);
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(21));
            Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.Zero);
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_InvalidRoleBoundsFallbackMatchesBrute()
        {
            var world = new SimulationWorld();
            LF2Character invalidItr = CreateCharacter(
                "RoleFormal_InvalidItr",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = 0,
                        y = 0,
                        w = 0,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character validBody = CreateCharacter(
                "RoleFormal_ValidBody",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character invalidBody = CreateCharacter(
                "RoleFormal_InvalidBody",
                3,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = 0, y = -10, w = 0, h = 20 }));
            LF2Character validItr = CreateCharacter(
                "RoleFormal_ValidItr",
                4,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 1,
                        x = -10,
                        y = -10,
                        w = 20,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            Register(world, invalidItr, 0, 1, 0);
            Register(world, validBody, 1, 2, 0);
            Register(world, invalidBody, 2, 2, 0);
            Register(world, validItr, 3, 3, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                invalidItr,
                validItr);
            CandidateRun role = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                invalidItr,
                validItr);

            AssertRunsEqual(brute, role);
            Assert.That(query.LastFormalPairCountForDiagnostics, Is.EqualTo(4));
            Assert.That(query.LastFormalFallbackParticipantCountForDiagnostics, Is.EqualTo(2));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.False);
        }

        [Test]
        public void Formal_ExceptionRestoresRngAndCandidatesThenRunsFullBruteFallback()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(
                "RoleFormal_RollbackAttacker",
                1,
                MakeFrame(
                    new InteractionArea
                    {
                        kind = 0,
                        vrest = 0,
                        x = -30,
                        y = -10,
                        w = 60,
                        h = 20,
                        zwidth = 15,
                    },
                    null));
            LF2Character leftTarget = CreateCharacter(
                "RoleFormal_RollbackLeft",
                2,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            LF2Character rightTarget = CreateCharacter(
                "RoleFormal_RollbackRight",
                3,
                MakeFrame(
                    null,
                    new BodyBox { kind = 0, x = -10, y = -10, w = 20, h = 20 }));
            Register(world, leftTarget, 0, 2, -10);
            Register(world, rightTarget, 1, 2, 10);
            Register(world, attacker, 2, 1, 0);

            BruteForceSceneQuery query = GetQuery(world);
            CandidateRun brute = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);

            query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = 2;
            CandidateRun recovered = RunCollection(
                world,
                query,
                CollisionFormalCollectorMode.ForceRoleAware,
                attacker);
            query.ThrowAfterRoleAwareFormalPairCountForSelfCheck = -1;

            AssertRunsEqual(brute, recovered);
            Assert.That(brute.RngCalls, Is.GreaterThan(0));
            Assert.That(query.LastFormalCollectionAbortedForDiagnostics, Is.True);
        }

        private static CandidateRun RunCollection(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            params LF2Entity[] attackers)
        {
            query.FormalCollectorMode = mode;
            world.Rng.Seed(CollectionSeed);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();

            var sequences = new List<List<SceneQueryHit>>(attackers.Length);
            var counts = new List<int>(attackers.Length);
            for (int attackerIndex = 0; attackerIndex < attackers.Length; attackerIndex++)
            {
                LF2Entity attacker = attackers[attackerIndex];
                Assert.That(
                    query.TryGetCollisionCandidateSequence(
                        attacker,
                        out List<SceneQueryHit> sequence),
                    Is.True);
                sequences.Add(new List<SceneQueryHit>(sequence));
                counts.Add(attacker.Runtime.HitCandidateCount);
            }

            var result = new CandidateRun(
                sequences,
                counts,
                world.Rng.State,
                world.Rng.CallCount);
            world.EndCollisionCandidateConsumption();
            return result;
        }

        private static void AssertRunsEqual(CandidateRun expected, CandidateRun actual)
        {
            Assert.That(actual.RngState, Is.EqualTo(expected.RngState));
            Assert.That(actual.RngCalls, Is.EqualTo(expected.RngCalls));
            Assert.That(actual.Sequences.Count, Is.EqualTo(expected.Sequences.Count));
            Assert.That(actual.Counts, Is.EqualTo(expected.Counts));
            for (int attackerIndex = 0;
                 attackerIndex < expected.Sequences.Count;
                 attackerIndex++)
            {
                List<SceneQueryHit> expectedSequence = expected.Sequences[attackerIndex];
                List<SceneQueryHit> actualSequence = actual.Sequences[attackerIndex];
                Assert.That(actualSequence.Count, Is.EqualTo(expectedSequence.Count));
                for (int candidateIndex = 0;
                     candidateIndex < expectedSequence.Count;
                     candidateIndex++)
                {
                    SceneQueryHit expectedHit = expectedSequence[candidateIndex];
                    SceneQueryHit actualHit = actualSequence[candidateIndex];
                    Assert.That(actualHit.TargetSlot, Is.EqualTo(expectedHit.TargetSlot));
                    Assert.That(actualHit.ItrIndex, Is.EqualTo(expectedHit.ItrIndex));
                    Assert.That(actualHit.BodyX, Is.EqualTo(expectedHit.BodyX));
                    Assert.That(
                        actualHit.ZeroAttackerHpOnConsume,
                        Is.EqualTo(expectedHit.ZeroAttackerHpOnConsume));
                    Assert.That(
                        actualHit.ReleaseHeavyHeldTargetOnConsume,
                        Is.EqualTo(expectedHit.ReleaseHeavyHeldTargetOnConsume));
                    Assert.That(actualHit.RuntimeItr, Is.SameAs(expectedHit.RuntimeItr));
                }
            }
        }

        private static BruteForceSceneQuery GetQuery(SimulationWorld world)
        {
            Assert.That(world.SceneQuery, Is.TypeOf<BruteForceSceneQuery>());
            return (BruteForceSceneQuery)world.SceneQuery;
        }

        private static LF2FrameData MakeFrame(InteractionArea itr, BodyBox body)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = 0,
                wait = 1,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (body != null)
                frame.bodies.Add(body);
            return frame;
        }

        private static LF2Character CreateCharacter(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 1,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new LF2Character();
            character.ModuleInitialize();
            character.Name = name;
            character.ObjectId = objectId;
            character.Controller = new FormalSelfCheckController();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.PN = 0;
            character.Frame.N = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void Register(
            SimulationWorld world,
            LF2Entity entity,
            int requiredSlot,
            int team,
            int x)
        {
            entity.SetRequiredRuntimeSlot(requiredSlot);
            world.Register(entity);
            Assert.That(entity.Runtime.SlotIndex, Is.EqualTo(requiredSlot));
            entity.Team = team;
            entity.RelationTeam = team;
            entity.Health.HP = 100;
            entity.Health.HPBound = 100;
            entity.FrameDelay = 0;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(x, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class CandidateRun
        {
            public CandidateRun(
                List<List<SceneQueryHit>> sequences,
                List<int> counts,
                uint rngState,
                ulong rngCalls)
            {
                Sequences = sequences;
                Counts = counts;
                RngState = rngState;
                RngCalls = rngCalls;
            }

            public List<List<SceneQueryHit>> Sequences { get; }
            public List<int> Counts { get; }
            public uint RngState { get; }
            public ulong RngCalls { get; }
        }

        private sealed class FormalSelfCheckController : ILF2Controller
        {
            public SimInputBuffer InputBuffer { get; set; } = new SimInputBuffer();
            bool ILF2Controller.IsUp => false;
            bool ILF2Controller.IsDown => false;
            bool ILF2Controller.IsLeft => false;
            bool ILF2Controller.IsRight => false;
            bool ILF2Controller.IsAttack => false;
            bool ILF2Controller.IsJump => false;
            bool ILF2Controller.IsDefend => false;
            public int Dirv() => 0;
            public (int dx, int dz) GetMoveInput() => (0, 0);
            public void SetInputID(int inputId)
            {
            }
        }
    }
}
#endif


--- File: Temp/NTSD_ProductionEntityStress.dispersed-air-role-render-sort-detail-20260725.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "startedUtc": "2026-07-25T12:31:51.7292253Z",
    "updatedUtc": "2026-07-25T12:33:01.5813144Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-air-role-render-sort-detail-20260725.json",
    "failure": "",
    "harnessValidity": true,
    "performanceVerdict": "EvidenceOnlyNoThreshold",
    "requestedEntityCount": 1000,
    "selectedCharacterOid": 1,
    "totalEntitiesCreated": 1000,
    "lifecycleReplacements": 0,
    "activeGameObjectCount": 0,
    "stressRootChildCount": 0,
    "worldObjectCount": 0,
    "worldEntityCount": 0,
    "peakWorldEntityCount": 1000,
    "claimedRuntimeSlotCount": 0,
    "runtimeProfile": "MobileExtended",
    "runtimeSlotCapacity": 1050,
    "broadphaseBackend": "LooseQuadtree",
    "logicTicksExecuted": 570,
    "warmupTicksCompleted": 30,
    "sampledLogicTicks": 540,
    "sampledUnityFrames": 143,
    "framesWithCatchUp": 143,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 1374,
    "aiControlledEntityTicks": 570000,
    "collisionCandidateCountSum": 17983,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 8054373,
    "broadphasePairCountPeak": 184181,
    "broadphaseFallbackParticipantPeak": 154,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 996,
    "damageStatTotal": 0,
    "killStatTotal": 0,
    "opointCounterAvailable": true,
    "observedOpointCreates": 0,
    "opointCounterReason": "Runtime-derived observable proxy: unique active non-harness runtime handles observed after each logic tick. It is not a production opoint creation counter.",
    "logicTickMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick",
        "unavailableReason": "",
        "sampleCount": 540,
        "average": 101.92430074074068,
        "maximum": 288.2826,
        "p95": 159.63932499999999,
        "p99": 234.741944
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 143,
        "average": 471.50033092373737,
        "maximum": 1333.542823791504,
        "p95": 931.7655205726625,
        "p99": 1249.4555354118354
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 540,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": true,
    "phaseTimingSource": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
    "phaseTimings": [
        {
            "phase": "BattleFlow",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.000979814814814816,
                "maximum": 0.0056,
                "p95": 0.0016,
                "p99": 0.002122000000000003
            }
        },
        {
            "phase": "Cooldown",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.9564761111111112,
                "maximum": 7.280600000000001,
                "p95": 1.6847400000000002,
                "p99": 2.087273000000001
            }
        },
        {
            "phase": "HumanInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.19098166666666684,
                "maximum": 0.44260000000000007,
                "p95": 0.264805,
                "p99": 0.3902400000000002
            }
        },
        {
            "phase": "RuntimeMaintenance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.05864518518518522,
                "maximum": 0.1443,
                "p95": 0.07740000000000001,
                "p99": 0.08981000000000002
            }
        },
        {
            "phase": "InputClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "phase": "CharacterInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 17.214670740740737,
                "maximum": 63.8254,
                "p95": 36.02383,
                "p99": 44.16780000000001
            }
        },
        {
            "phase": "EarlyFrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8936237037037036,
                "maximum": 6.2405,
                "p95": 1.467335,
                "p99": 2.0185470000000009
            }
        },
        {
            "phase": "FrameLogic",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.21494314814814823,
                "maximum": 2.2579000000000004,
                "p95": 0.3024349999999998,
                "p99": 0.38911400000000026
            }
        },
        {
            "phase": "FrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 2.5018585185185198,
                "maximum": 39.4183,
                "p95": 5.109894999999997,
                "p99": 6.172432000000001
            }
        },
        {
            "phase": "DeathCleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.21579092592592603,
                "maximum": 8.8063,
                "p95": 0.30417999999999997,
                "p99": 0.38031300000000009
            }
        },
        {
            "phase": "StageBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.8001818518518538,
                "maximum": 5.4962,
                "p95": 2.6433899999999976,
                "p99": 3.7466820000000005
            }
        },
        {
            "phase": "PreInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 2.4362992592592618,
                "maximum": 7.388000000000001,
                "p95": 3.4311399999999985,
                "p99": 5.33207800000001
            }
        },
        {
            "phase": "HeldLinkValidation",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.09795481481481479,
                "maximum": 0.2106,
                "p95": 0.12802499999999998,
                "p99": 0.158144
            }
        },
        {
            "phase": "HeldProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.08338740740740744,
                "maximum": 0.5014000000000001,
                "p95": 0.11261,
                "p99": 0.15372200000000003
            }
        },
        {
            "phase": "CollisionSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.7637801851851853,
                "maximum": 1.5228000000000002,
                "p95": 1.0757649999999995,
                "p99": 1.339814
            }
        },
        {
            "phase": "PairVRest",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.15172851851851869,
                "maximum": 0.3562,
                "p95": 0.23871,
                "p99": 0.3118230000000001
            }
        },
        {
            "phase": "CandidateCollect",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 27.854627407407418,
                "maximum": 195.4693,
                "p95": 51.572444999999927,
                "p99": 153.477489
            }
        },
        {
            "phase": "CharacterHitConsumePostInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.620282222222222,
                "maximum": 6.646800000000001,
                "p95": 2.6659499999999998,
                "p99": 3.561235000000001
            }
        },
        {
            "phase": "RandomWeaponDrop",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.2457405555555551,
                "maximum": 0.8844000000000001,
                "p95": 0.390425,
                "p99": 0.4961800000000004
            }
        },
        {
            "phase": "ObjectHitConsume",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.3168055555555554,
                "maximum": 0.8324,
                "p95": 0.42843,
                "p99": 0.5108330000000002
            }
        },
        {
            "phase": "CandidateConsumptionEnd",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.0026259259259259258,
                "maximum": 0.0099,
                "p95": 0.004704999999999996,
                "p99": 0.007361000000000001
            }
        },
        {
            "phase": "PreFrameBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.392964999999999,
                "maximum": 4.998600000000001,
                "p95": 2.0849149999999998,
                "p99": 2.704504000000001
            }
        },
        {
            "phase": "Stage",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.0008768518518518513,
                "maximum": 0.004,
                "p95": 0.0015,
                "p99": 0.0023610000000000017
            }
        },
        {
            "phase": "RenderDispatch",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 29.347652037037084,
                "maximum": 61.086800000000007,
                "p95": 43.62796999999999,
                "p99": 54.73649700000001
            }
        },
        {
            "phase": "FramePostProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8156888888888888,
                "maximum": 2.9013,
                "p95": 1.3755149999999979,
                "p99": 1.9677110000000003
            }
        },
        {
            "phase": "LateEntityUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 11.850242037037044,
                "maximum": 63.4609,
                "p95": 18.787465,
                "p99": 23.045488000000007
            }
        },
        {
            "phase": "RandomWeaponDropTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.0006685185185185177,
                "maximum": 0.0076,
                "p95": 0.0011,
                "p99": 0.0015
            }
        },
        {
            "phase": "EntityPostFrameTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8247825925925919,
                "maximum": 2.2846,
                "p95": 1.4113749999999997,
                "p99": 1.8483720000000002
            }
        },
        {
            "phase": "BattleResults",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.0006401851851851859,
                "maximum": 0.0025,
                "p95": 0.0011,
                "p99": 0.0016610000000000015
            }
        }
    ],
    "phaseTimingUnattributedMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings",
        "unavailableReason": "",
        "sampleCount": 540,
        "average": 0.0694011111111051,
        "maximum": 0.26449999999996978,
        "p95": 0.1141049999999538,
        "p99": 0.16532199999999096
    },
    "detailPhaseTimingEnabled": true,
    "detailPhaseTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
    "detailPhaseTimingUnavailableReason": "",
    "detailPhaseTimings": [
        {
            "phase": "CharacterInput/SnapshotBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.485665185185185,
                "maximum": 9.9049,
                "p95": 2.1169100000000005,
                "p99": 2.5898790000000004
            }
        },
        {
            "phase": "CharacterInput/EntityInputPass",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 15.720032592592617,
                "maximum": 53.908,
                "p95": 34.42574999999999,
                "p99": 42.65160900000001
            }
        },
        {
            "phase": "CharacterInput/SnapshotClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.004857037037037033,
                "maximum": 0.1106,
                "p95": 0.0076049999999999958,
                "p99": 0.012322000000000003
            }
        },
        {
            "phase": "LateEntityUpdate/StateSpecial",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.752122592592593,
                "maximum": 2.0904000000000004,
                "p95": 1.2551499999999996,
                "p99": 1.5691190000000009
            }
        },
        {
            "phase": "LateEntityUpdate/Recovery",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.805103703703704,
                "maximum": 1.4043,
                "p95": 1.0709849999999999,
                "p99": 1.1635490000000002
            }
        },
        {
            "phase": "LateEntityUpdate/FrameTick",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 3.7349387037037049,
                "maximum": 13.6523,
                "p95": 7.275825,
                "p99": 9.485746000000003
            }
        },
        {
            "phase": "LateEntityUpdate/EntityCollision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.060292777777777788,
                "maximum": 0.11220000000000001,
                "p95": 0.083705,
                "p99": 0.09480500000000002
            }
        },
        {
            "phase": "LateEntityUpdate/FrameExit",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.558238148148148,
                "maximum": 0.9991000000000001,
                "p95": 0.7124050000000001,
                "p99": 0.7818810000000003
            }
        },
        {
            "phase": "LateEntityUpdate/DeathOpoint",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.7220949999999999,
                "maximum": 1.2763,
                "p95": 0.9204749999999999,
                "p99": 1.011513
            }
        },
        {
            "phase": "LateEntityUpdate/OpointProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 3.082088703703704,
                "maximum": 42.4386,
                "p95": 5.541159999999999,
                "p99": 6.400600000000001
            }
        },
        {
            "phase": "LateEntityUpdate/Cleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.18779833333333327,
                "maximum": 0.5398000000000001,
                "p95": 0.27252,
                "p99": 0.31179400000000026
            }
        },
        {
            "phase": "LateEntityUpdate/TailAndQueuedFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8544444444444447,
                "maximum": 2.8785000000000005,
                "p95": 1.174575,
                "p99": 1.3431060000000002
            }
        },
        {
            "phase": "LateEntityUpdate/PrevFrameMirror",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.5501618518518517,
                "maximum": 0.8269000000000001,
                "p95": 0.700325,
                "p99": 0.7520620000000001
            }
        },
        {
            "phase": "LateEntityUpdate/FinalPendingFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.06576277777777774,
                "maximum": 0.1613,
                "p95": 0.0906,
                "p99": 0.12452600000000011
            }
        },
        {
            "phase": "Render/PresentationOrder",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8731877777777769,
                "maximum": 1.6284,
                "p95": 1.1810100000000002,
                "p99": 1.409396
            }
        },
        {
            "phase": "Render/BeginFrame",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 9.537816481481473,
                "maximum": 29.3582,
                "p95": 16.166394999999999,
                "p99": 18.447930000000004
            }
        },
        {
            "phase": "Render/PrepareFrame/LegacyCapacityGuard",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 17.34977944444446,
                "maximum": 35.9022,
                "p95": 26.656264999999999,
                "p99": 32.64869100000001
            }
        },
        {
            "phase": "Render/LateRendererUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.5847157407407416,
                "maximum": 3.6249000000000004,
                "p95": 2.542829999999997,
                "p99": 3.1523890000000005
            }
        }
    ],
    "aiInputDetailTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
    "aiInputDetailTimingUnavailableReason": "",
    "aiInputDetailTimings": [
        {
            "phase": "CharacterInput/AI/SnapshotSlotSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.3908620370370371,
                "maximum": 8.3385,
                "p95": 0.5515549999999999,
                "p99": 0.6538170000000002
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotIndexBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.3167655555555554,
                "maximum": 1.1985000000000002,
                "p95": 0.48003,
                "p99": 0.5889610000000003
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotQuadtreeSync",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.7766492592592587,
                "maximum": 5.1195,
                "p95": 1.1275749999999998,
                "p99": 1.2952590000000002
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestGround",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 8.050810185185182,
                "maximum": 39.293600000000008,
                "p95": 20.600189999999996,
                "p99": 25.749825000000006
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestAir",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 2.0518064814814815,
                "maximum": 8.642900000000001,
                "p95": 5.10474,
                "p99": 5.844304000000001
            }
        },
        {
            "phase": "CharacterInput/AI/RemainingAiDecision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 12.237551296296287,
                "maximum": 48.5462,
                "p95": 28.309319999999983,
                "p99": 35.467880000000018
            }
        },
        {
            "phase": "CharacterInput/AI/InputStateSyncFromRuntime",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.10560981481481481,
                "maximum": 0.314,
                "p95": 0.19560999999999999,
                "p99": 0.2547130000000001
            }
        },
        {
            "phase": "CharacterInput/AI/ComboUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 1.0765570370370377,
                "maximum": 4.6448,
                "p95": 2.1643849999999977,
                "p99": 2.7356230000000005
            }
        },
        {
            "phase": "CharacterInput/AI/RefreshRuntimeSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 540,
                "average": 0.8555149999999999,
                "maximum": 6.3665,
                "p95": 1.6843099999999989,
                "p99": 2.1234040000000005
            }
        }
    ],
    "aiInputDetailCounters": {
        "available": true,
        "unavailableReason": "",
        "aiCount": 540000,
        "spatialQueryCount": 539407,
        "queriedHandleVisits": 11717454,
        "candidateVisits": 11717454,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 540000,
        "radiusHistogram": [
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        ]
    },
    "loggingPolicy": {
        "originalFilterLogType": "Log",
        "runningFilterLogType": "Error",
        "policy": "Suppress Log and Warning during the stress run while retaining Error.",
        "applied": false,
        "restored": true
    },
    "teardown": {
        "attempted": true,
        "restored": true,
        "activeStateRestored": true,
        "driverStateRestored": true,
        "loggingStateRestored": true,
        "activeGameObjectsBefore": 1000,
        "activeGameObjectsAfter": 0,
        "worldObjectsBefore": 2000,
        "worldObjectsAfter": 0,
        "worldEntitiesBefore": 1000,
        "worldEntitiesAfter": 0,
        "claimedSlotsBefore": 1000,
        "claimedSlotsAfter": 0,
        "objectPoolActiveBeforeRun": 0,
        "objectPoolActiveAfter": 0,
        "objectPoolAvailableBeforeRun": 10,
        "objectPoolAvailableAfter": 1000,
        "retainedInactiveObjectPoolCapacityBeforeRun": 10,
        "retainedInactiveObjectPoolCapacityAfter": 1000,
        "retainedInactiveObjectPoolCapacityDelta": 990,
        "retainedInactiveObjectPoolCapacityPolicy": "Informational inactive cache capacity only; it is not active cleanup residue and the stress harness does not trim it.",
        "referencePoolActiveBeforeRun": 0,
        "referencePoolActiveAfter": 0,
        "cleanupExceptionCount": 0,
        "cleanupExceptions": "",
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True)"
    }
}

--- File: Temp/NTSD_ProductionEntityStress.dispersed-role-render-subphase-detail-20260725.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "startedUtc": "2026-07-25T12:45:33.5848413Z",
    "updatedUtc": "2026-07-25T12:46:23.7746848Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-role-render-subphase-detail-20260725.json",
    "failure": "",
    "harnessValidity": true,
    "performanceVerdict": "EvidenceOnlyNoThreshold",
    "requestedEntityCount": 1000,
    "selectedCharacterOid": 1,
    "totalEntitiesCreated": 1000,
    "lifecycleReplacements": 0,
    "activeGameObjectCount": 0,
    "stressRootChildCount": 0,
    "worldObjectCount": 0,
    "worldEntityCount": 0,
    "peakWorldEntityCount": 1000,
    "claimedRuntimeSlotCount": 0,
    "runtimeProfile": "MobileExtended",
    "runtimeSlotCapacity": 1050,
    "broadphaseBackend": "LooseQuadtree",
    "formalCollectorRequestedMode": "role",
    "formalCollectorMode": "role",
    "formalCollectorBodyEntries": 1986,
    "formalCollectorItrQueries": 6,
    "logicTicksExecuted": 506,
    "warmupTicksCompleted": 30,
    "sampledLogicTicks": 476,
    "sampledUnityFrames": 127,
    "framesWithCatchUp": 127,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 849,
    "aiControlledEntityTicks": 506000,
    "collisionCandidateCountSum": 17737,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 437562,
    "broadphasePairCountPeak": 23262,
    "broadphaseFallbackParticipantPeak": 0,
    "broadphaseAbortedTicks": 0,
    "broadphaseLastIndexedCount": 1986,
    "damageStatTotal": 0,
    "killStatTotal": 0,
    "opointCounterAvailable": true,
    "observedOpointCreates": 0,
    "opointCounterReason": "Runtime-derived observable proxy: unique active non-harness runtime handles observed after each logic tick. It is not a production opoint creation counter.",
    "logicTickMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Stopwatch around SimulationTickDriver.StepOneTick -> NTSDBattleTickSystem.RunReleaseTick",
        "unavailableReason": "",
        "sampleCount": 476,
        "average": 84.46239348739494,
        "maximum": 186.6359,
        "p95": 136.714525,
        "p99": 158.931175
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 127,
        "average": 374.57524075752169,
        "maximum": 726.1077165603638,
        "p95": 572.9901790618891,
        "p99": 625.0304973125458
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 476,
        "average": 0.0,
        "maximum": 0.0,
        "p95": 0.0,
        "p99": 0.0
    },
    "phaseTimingEnabled": true,
    "phaseTimingSource": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
    "phaseTimings": [
        {
            "phase": "BattleFlow",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.001154201680672269,
                "maximum": 0.0054,
                "p95": 0.0021000000000000005,
                "p99": 0.0027
            }
        },
        {
            "phase": "Cooldown",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.0827985294117646,
                "maximum": 7.1509,
                "p95": 1.7232750000000002,
                "p99": 2.2533250000000004
            }
        },
        {
            "phase": "HumanInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.19452289915966398,
                "maximum": 0.677,
                "p95": 0.27855,
                "p99": 0.34165
            }
        },
        {
            "phase": "RuntimeMaintenance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.056255882352941207,
                "maximum": 0.18910000000000003,
                "p95": 0.0747,
                "p99": 0.08040000000000002
            }
        },
        {
            "phase": "InputClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.0,
                "maximum": 0.0,
                "p95": 0.0,
                "p99": 0.0
            }
        },
        {
            "phase": "CharacterInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 18.24620525210084,
                "maximum": 56.8817,
                "p95": 39.08825,
                "p99": 49.7313
            }
        },
        {
            "phase": "EarlyFrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.9938665966386562,
                "maximum": 2.3803,
                "p95": 1.684275,
                "p99": 2.19035
            }
        },
        {
            "phase": "FrameLogic",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.24518046218487367,
                "maximum": 14.3811,
                "p95": 0.2924,
                "p99": 0.3533
            }
        },
        {
            "phase": "FrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.605298319327732,
                "maximum": 7.3204,
                "p95": 4.42735,
                "p99": 6.0836250000000009
            }
        },
        {
            "phase": "DeathCleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.21376575630252113,
                "maximum": 0.528,
                "p95": 0.360725,
                "p99": 0.425425
            }
        },
        {
            "phase": "StageBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.0656159663865529,
                "maximum": 27.168300000000003,
                "p95": 3.3036000000000005,
                "p99": 4.660299999999999
            }
        },
        {
            "phase": "PreInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.697705042016806,
                "maximum": 8.2284,
                "p95": 4.391775,
                "p99": 6.914475
            }
        },
        {
            "phase": "HeldLinkValidation",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.09805483193277309,
                "maximum": 0.3326,
                "p95": 0.1328,
                "p99": 0.18812500000000002
            }
        },
        {
            "phase": "HeldProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.08640735294117646,
                "maximum": 0.2838,
                "p95": 0.14215,
                "p99": 0.1986
            }
        },
        {
            "phase": "CollisionSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.8442787815126047,
                "maximum": 2.3847,
                "p95": 1.3341750000000002,
                "p99": 1.863925
            }
        },
        {
            "phase": "PairVRest",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.17653340336134455,
                "maximum": 0.7308,
                "p95": 0.303975,
                "p99": 0.4071
            }
        },
        {
            "phase": "CandidateCollect",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 5.082813235294115,
                "maximum": 14.986400000000002,
                "p95": 9.977175,
                "p99": 12.551
            }
        },
        {
            "phase": "CharacterHitConsumePostInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.8158987394957989,
                "maximum": 11.5257,
                "p95": 3.047375,
                "p99": 4.0074250000000009
            }
        },
        {
            "phase": "RandomWeaponDrop",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.2696579831932775,
                "maximum": 1.0337,
                "p95": 0.45677500000000006,
                "p99": 0.529
            }
        },
        {
            "phase": "ObjectHitConsume",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.3280999999999997,
                "maximum": 2.3774,
                "p95": 0.43405,
                "p99": 0.52875
            }
        },
        {
            "phase": "CandidateConsumptionEnd",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.002907563025210087,
                "maximum": 0.010100000000000002,
                "p95": 0.005625,
                "p99": 0.0068000000000000009
            }
        },
        {
            "phase": "PreFrameBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.5032586134453772,
                "maximum": 3.9729,
                "p95": 2.252825,
                "p99": 2.995025
            }
        },
        {
            "phase": "Stage",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.0009518907563025212,
                "maximum": 0.0088,
                "p95": 0.0017000000000000002,
                "p99": 0.002125
            }
        },
        {
            "phase": "RenderDispatch",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 31.029425840336147,
                "maximum": 62.7122,
                "p95": 46.625125000000007,
                "p99": 56.563325000000009
            }
        },
        {
            "phase": "FramePostProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.92016743697479,
                "maximum": 5.0437,
                "p95": 1.57515,
                "p99": 2.1141
            }
        },
        {
            "phase": "LateEntityUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 12.896768277310932,
                "maximum": 45.8023,
                "p95": 23.350725,
                "p99": 32.85905
            }
        },
        {
            "phase": "RandomWeaponDropTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.0006529411764705881,
                "maximum": 0.0046,
                "p95": 0.0011,
                "p99": 0.001525
            }
        },
        {
            "phase": "EntityPostFrameTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.9219554621848747,
                "maximum": 2.4544,
                "p95": 1.526975,
                "p99": 2.1681749999999999
            }
        },
        {
            "phase": "BattleResults",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.0008281512605042018,
                "maximum": 0.0029000000000000004,
                "p95": 0.0016,
                "p99": 0.0022
            }
        }
    ],
    "phaseTimingUnattributedMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings",
        "unavailableReason": "",
        "sampleCount": 476,
        "average": 0.08136407563024681,
        "maximum": 0.29340000000000546,
        "p95": 0.15244999999996623,
        "p99": 0.2488000000000028
    },
    "detailPhaseTimingEnabled": true,
    "detailPhaseTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
    "detailPhaseTimingUnavailableReason": "",
    "detailPhaseTimings": [
        {
            "phase": "CharacterInput/SnapshotBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.5930886554621866,
                "maximum": 2.7560000000000004,
                "p95": 2.2565,
                "p99": 2.48245
            }
        },
        {
            "phase": "CharacterInput/EntityInputPass",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 16.642204201680664,
                "maximum": 55.1032,
                "p95": 36.89815,
                "p99": 47.395450000000007
            }
        },
        {
            "phase": "CharacterInput/SnapshotClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.005370168067226888,
                "maximum": 0.0142,
                "p95": 0.009325,
                "p99": 0.011625
            }
        },
        {
            "phase": "LateEntityUpdate/StateSpecial",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.8738369747899164,
                "maximum": 1.9269,
                "p95": 1.5310000000000002,
                "p99": 1.7957
            }
        },
        {
            "phase": "LateEntityUpdate/Recovery",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.8457449579831933,
                "maximum": 2.5928,
                "p95": 1.104275,
                "p99": 1.3050000000000002
            }
        },
        {
            "phase": "LateEntityUpdate/FrameTick",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 4.345010504201681,
                "maximum": 24.800900000000003,
                "p95": 10.0291,
                "p99": 13.859875
            }
        },
        {
            "phase": "LateEntityUpdate/EntityCollision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.06079264705882355,
                "maximum": 0.11120000000000001,
                "p95": 0.0844,
                "p99": 0.08895
            }
        },
        {
            "phase": "LateEntityUpdate/FrameExit",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.5807310924369747,
                "maximum": 1.6001,
                "p95": 0.733025,
                "p99": 0.79755
            }
        },
        {
            "phase": "LateEntityUpdate/DeathOpoint",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.7466821428571424,
                "maximum": 1.3379,
                "p95": 0.93365,
                "p99": 0.97585
            }
        },
        {
            "phase": "LateEntityUpdate/OpointProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 3.166654621848739,
                "maximum": 33.5351,
                "p95": 6.4799750000000009,
                "p99": 8.66685
            }
        },
        {
            "phase": "LateEntityUpdate/Cleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.19524768907563029,
                "maximum": 0.4516,
                "p95": 0.2754,
                "p99": 0.32415
            }
        },
        {
            "phase": "LateEntityUpdate/TailAndQueuedFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.9141243697478992,
                "maximum": 1.866,
                "p95": 1.257425,
                "p99": 1.3725
            }
        },
        {
            "phase": "LateEntityUpdate/PrevFrameMirror",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.5846819327731098,
                "maximum": 7.0282,
                "p95": 0.7114,
                "p99": 0.7469750000000001
            }
        },
        {
            "phase": "LateEntityUpdate/FinalPendingFlush",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.06595336134453782,
                "maximum": 0.1857,
                "p95": 0.09582500000000001,
                "p99": 0.11837500000000001
            }
        },
        {
            "phase": "Render/PresentationOrder",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.9090798319327735,
                "maximum": 4.976100000000001,
                "p95": 1.2167750000000002,
                "p99": 1.47865
            }
        },
        {
            "phase": "Render/BeginFrame",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 10.070453361344543,
                "maximum": 34.983200000000007,
                "p95": 16.2623,
                "p99": 22.5901
            }
        },
        {
            "phase": "Render/PrepareFrame/LegacyCapacityGuard",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 18.333940336134455,
                "maximum": 47.780100000000008,
                "p95": 27.1461,
                "p99": 33.20085
            }
        },
        {
            "phase": "Render/LateRendererUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.713353781512604,
                "maximum": 4.4943,
                "p95": 2.6506,
                "p99": 3.3764250000000004
            }
        },
        {
            "phase": "Render/BeginFrame/SortEntities",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.8405344537815125,
                "maximum": 3.0151000000000005,
                "p95": 1.135825,
                "p99": 1.54655
            }
        },
        {
            "phase": "Render/BeginFrame/CaptureHitRecords",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.14639726890756306,
                "maximum": 0.9316000000000001,
                "p95": 0.30265,
                "p99": 0.42010000000000005
            }
        },
        {
            "phase": "Render/BeginFrame/CaptureEntities",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.5102634453781516,
                "maximum": 24.977,
                "p95": 4.76775,
                "p99": 6.450025
            }
        },
        {
            "phase": "Render/BeginFrame/BuildCommands",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 6.561492857142863,
                "maximum": 19.9007,
                "p95": 10.00785,
                "p99": 13.310525000000002
            }
        },
        {
            "phase": "Render/PrepareFrame/FrozenFrameCopy",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.12803046218487394,
                "maximum": 0.6483,
                "p95": 0.25035,
                "p99": 0.4631
            }
        },
        {
            "phase": "Render/PrepareFrame/ResolveCommands",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 11.701932563025198,
                "maximum": 39.529900000000008,
                "p95": 17.151175000000003,
                "p99": 21.146950000000005
            }
        },
        {
            "phase": "Render/PrepareFrame/WriteQuads",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.533836344537818,
                "maximum": 6.580100000000001,
                "p95": 3.793225,
                "p99": 4.6179250000000009
            }
        },
        {
            "phase": "Render/PrepareFrame/SetVertexBufferData",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.07710714285714293,
                "maximum": 1.8998000000000002,
                "p95": 0.14137500000000004,
                "p99": 0.218975
            }
        },
        {
            "phase": "Render/PrepareFrame/SetSubMeshes",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.436240336134454,
                "maximum": 5.3661,
                "p95": 1.7874500000000003,
                "p99": 2.021275
            }
        },
        {
            "phase": "Render/ExecuteCommandBuffer",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput and LateEntityUpdate plus render build sub-phases; ExecuteCommandBuffer is carried from the preceding render pass into the next logic-tick sample. Nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.0013142857142857145,
                "maximum": 0.0102,
                "p95": 0.006500000000000001,
                "p99": 0.007425
            }
        }
    ],
    "aiInputDetailTimingSource": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
    "aiInputDetailTimingUnavailableReason": "",
    "aiInputDetailTimings": [
        {
            "phase": "CharacterInput/AI/SnapshotSlotSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.40698865546218496,
                "maximum": 0.7395,
                "p95": 0.5682750000000001,
                "p99": 0.6457999999999999
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotIndexBuild",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.35769831932773135,
                "maximum": 0.7331000000000001,
                "p95": 0.577575,
                "p99": 0.64405
            }
        },
        {
            "phase": "CharacterInput/AI/SnapshotQuadtreeSync",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.8269271008403355,
                "maximum": 1.5095,
                "p95": 1.14065,
                "p99": 1.273925
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestGround",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 8.643047899159669,
                "maximum": 35.59,
                "p95": 23.188875000000004,
                "p99": 27.490575
            }
        },
        {
            "phase": "CharacterInput/AI/FindNearestAir",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 2.1889680672268927,
                "maximum": 19.4586,
                "p95": 5.197675,
                "p99": 5.6418
            }
        },
        {
            "phase": "CharacterInput/AI/RemainingAiDecision",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 12.990450840336127,
                "maximum": 47.9574,
                "p95": 31.46105,
                "p99": 39.941100000000009
            }
        },
        {
            "phase": "CharacterInput/AI/InputStateSyncFromRuntime",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.12272436974789917,
                "maximum": 0.3171,
                "p95": 0.19672499999999999,
                "p99": 0.297375
            }
        },
        {
            "phase": "CharacterInput/AI/ComboUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 1.1088134453781516,
                "maximum": 2.9062,
                "p95": 1.812875,
                "p99": 2.5586
            }
        },
        {
            "phase": "CharacterInput/AI/RefreshRuntimeSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Independent Stopwatch timestamps accumulated inside CharacterInput AI sub-phases; nested diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 476,
                "average": 0.859030042016807,
                "maximum": 3.3695,
                "p95": 1.4148500000000002,
                "p99": 2.138075
            }
        }
    ],
    "aiInputDetailCounters": {
        "available": true,
        "unavailableReason": "",
        "aiCount": 476000,
        "spatialQueryCount": 475416,
        "queriedHandleVisits": 10602861,
        "candidateVisits": 10602861,
        "radiusExpansions": 0,
        "bruteFallbackCount": 0,
        "bruteSlotVisits": 0,
        "phase1ListVisits": 0,
        "refreshCount": 476000,
        "radiusHistogram": [
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0,
            0
        ]
    },
    "loggingPolicy": {
        "originalFilterLogType": "Log",
        "runningFilterLogType": "Error",
        "policy": "Suppress Log and Warning during the stress run while retaining Error.",
        "applied": false,
        "restored": true
    },
    "teardown": {
        "attempted": true,
        "restored": true,
        "activeStateRestored": true,
        "driverStateRestored": true,
        "loggingStateRestored": true,
        "activeGameObjectsBefore": 1000,
        "activeGameObjectsAfter": 0,
        "worldObjectsBefore": 2000,
        "worldObjectsAfter": 0,
        "worldEntitiesBefore": 1000,
        "worldEntitiesAfter": 0,
        "claimedSlotsBefore": 1000,
        "claimedSlotsAfter": 0,
        "objectPoolActiveBeforeRun": 0,
        "objectPoolActiveAfter": 0,
        "objectPoolAvailableBeforeRun": 10,
        "objectPoolAvailableAfter": 1000,
        "retainedInactiveObjectPoolCapacityBeforeRun": 10,
        "retainedInactiveObjectPoolCapacityAfter": 1000,
        "retainedInactiveObjectPoolCapacityDelta": 990,
        "retainedInactiveObjectPoolCapacityPolicy": "Informational inactive cache capacity only; it is not active cleanup residue and the stress harness does not trim it.",
        "referencePoolActiveBeforeRun": 0,
        "referencePoolActiveAfter": 0,
        "cleanupExceptionCount": 0,
        "cleanupExceptions": "",
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1000 (delta=990; doesNotAffectRestored=True)"
    }
}

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

# Architecture review: next safe 1000-entity optimizations

Read the attached implementation and the two stress reports. This is a read-only architecture/debugging review.

Authoritative battle behavior is the C# project at `J:\QQFile\NTSD2.4\ntsd_release_C#`; Unity rendering adaptation may change representation but must preserve visible ordering, positions, glyphs, collision candidate sequence, RNG calls/state, and lifecycle.

Fresh evidence:

- baseline detail: `Temp/NTSD_ProductionEntityStress.dispersed-air-role-render-sort-detail-20260725.json`
- role-aware detail: `Temp/NTSD_ProductionEntityStress.dispersed-role-render-subphase-detail-20260725.json`
- role-aware drops collision pair peak 184181 -> 23262 and CandidateCollect to about 5.1 ms.
- role-aware production default is still disabled.
- render detail is about ResolveCommands 11.7 ms, BuildCommands 6.6 ms, CaptureEntities 2.5 ms, WriteQuads 2.5 ms, SetSubMeshes 1.4 ms, SetVertexBufferData 0.08 ms.
- about 5000 commands are 1000 shadows + 1000 entities + about 3000 `Com` overlay glyphs, with 2 draw segments / about 5 SetPass.

Please answer:

1. What exact missing parity tests/evidence are required before changing Configured+LooseQuadtree from legacy union-AABB to role-aware formal collector?
2. For ResolveCommands, identify the smallest safe cache/pre-resolution design that preserves catalog publication/lease and descriptor mismatch fail-close semantics. Point to concrete types/methods and cache invalidation keys.
3. Is precomposing the three `Com` glyph commands into one atlas binding/quad a safe Unity-only rendering adaptation? If yes, specify ordering/pivot/UV/parity constraints and tests; if no, explain.
4. Rank the next three changes by expected saved milliseconds and risk.
5. Flag any correctness or resource-lifetime issue in the current implementation.

Do not edit files. Produce an evidence-based report with exact file/method references.
