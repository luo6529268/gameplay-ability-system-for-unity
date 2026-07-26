---
provider: "codex"
agent_role: "code-reviewer"
model: "gpt-5.3-codex"
files:
  - "Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs"
  - "Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs"
  - "Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json"
  - "J:/QQFile/NTSD2.4/ntsd_release_C#/src/BattleCore/Interaction/CollisionCollect.cs"
timestamp: "2026-07-25T08:26:33.002Z"
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
        public bool ShadowBroadphaseDiagnosticsEnabled { get; set; }
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
#endif
        public int LastFormalPairCountForDiagnostics => _formalPairKeys.Count;
        public int LastFormalFallbackParticipantCountForDiagnostics =>
            _formalFallbackParticipantCount;
        public bool LastFormalCollectionAbortedForDiagnostics => _formalCollectionAborted;
        public SpatialSynchronizeResult LastFormalSynchronizeResultForDiagnostics =>
            FormalSpatialSynchronizeResult;
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

            if (_collisionBroadphase == CollisionBroadphaseBackend.LooseQuadtree)
            {
                uint rngStateBeforeFormal = _world.Rng.State;
                ulong rngCallsBeforeFormal = _world.Rng.CallCount;
                bool formalSucceeded = true;
                if (formalSucceeded)
                {
                    try
                    {
                        formalSucceeded = TryCollectCollisionCandidatesLoose(currentTick);
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

            return true;
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
}
#endif


--- File: Temp/NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json ---
{
    "schema": "ntsd-production-entity-stress/v1",
    "status": "StoppedCleanly",
    "mode": "Dispersed1000",
    "startedUtc": "2026-07-25T08:21:54.1464783Z",
    "updatedUtc": "2026-07-25T08:23:47.9588441Z",
    "unityVersion": "2022.3.34f1c1",
    "platform": "WindowsEditor",
    "scene": "NTSD_Battle",
    "stressRootName": "NTSD Production Entity Stress [Dispersed1000]",
    "outputPath": "I:\\GitHub\\Unity_GAS\\gameplay-ability-system-for-unity\\Temp\\NTSD_ProductionEntityStress.dispersed-ai-nosort-render-sort-20260725.json",
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
    "logicTicksExecuted": 388,
    "warmupTicksCompleted": 5,
    "sampledLogicTicks": 383,
    "sampledUnityFrames": 97,
    "framesWithCatchUp": 97,
    "maximumCatchUpTicksInFrame": 4,
    "currentBacklogTicks": 4,
    "maximumBacklogTicks": 4,
    "droppedBacklogTicks": 2881,
    "aiControlledEntityTicks": 388000,
    "collisionCandidateCountSum": 17062,
    "collisionCandidateCountPeak": 735,
    "broadphasePairCountSum": 7625891,
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
        "sampleCount": 383,
        "average": 277.30811644908627,
        "maximum": 710.9017,
        "p95": 447.0875799999999,
        "p99": 576.819974
    },
    "unityFrameMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Time.unscaledDeltaTime for visible Play Mode frames",
        "unavailableReason": "",
        "sampleCount": 97,
        "average": 1142.293440005214,
        "maximum": 2097.487688064575,
        "p95": 1653.32453250885,
        "p99": 1996.4633655548087
    },
    "logicTickAllocatedBytes": {
        "available": true,
        "unit": "bytes",
        "source": "GC.GetAllocatedBytesForCurrentThread around production logic tick",
        "unavailableReason": "",
        "sampleCount": 383,
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
                "sampleCount": 383,
                "average": 0.001123759791122716,
                "maximum": 0.0056,
                "p95": 0.0018899999999999979,
                "p99": 0.0023180000000000008
            }
        },
        {
            "phase": "Cooldown",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.8524926892950391,
                "maximum": 2.3381000000000005,
                "p95": 1.15035,
                "p99": 1.563018000000001
            }
        },
        {
            "phase": "HumanInput",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.1787386422976503,
                "maximum": 0.28350000000000005,
                "p95": 0.21813,
                "p99": 0.25677200000000008
            }
        },
        {
            "phase": "RuntimeMaintenance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.05515248041775454,
                "maximum": 0.15030000000000003,
                "p95": 0.06523999999999998,
                "p99": 0.08710600000000013
            }
        },
        {
            "phase": "InputClear",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
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
                "sampleCount": 383,
                "average": 84.50532741514367,
                "maximum": 234.1571,
                "p95": 206.46412999999994,
                "p99": 223.15703200000008
            }
        },
        {
            "phase": "EarlyFrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7886206266318536,
                "maximum": 1.7825000000000003,
                "p95": 1.02588,
                "p99": 1.3863400000000015
            }
        },
        {
            "phase": "FrameLogic",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.2063013054830289,
                "maximum": 0.38230000000000005,
                "p95": 0.25158,
                "p99": 0.3182400000000001
            }
        },
        {
            "phase": "FrameAdvance",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 2.105249869451696,
                "maximum": 8.8215,
                "p95": 2.7652199999999995,
                "p99": 3.3124660000000016
            }
        },
        {
            "phase": "DeathCleanup",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.18793394255874663,
                "maximum": 0.5192,
                "p95": 0.24027999999999989,
                "p99": 0.32116600000000025
            }
        },
        {
            "phase": "StageBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.6602498694516977,
                "maximum": 5.6694,
                "p95": 2.0003599999999994,
                "p99": 2.7226380000000005
            }
        },
        {
            "phase": "PreInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 2.2807524804177548,
                "maximum": 8.5838,
                "p95": 2.72096,
                "p99": 4.258308000000001
            }
        },
        {
            "phase": "HeldLinkValidation",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.09448825065274151,
                "maximum": 0.18280000000000003,
                "p95": 0.10989999999999996,
                "p99": 0.13495400000000006
            }
        },
        {
            "phase": "HeldProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.07970365535248046,
                "maximum": 0.1826,
                "p95": 0.09731999999999996,
                "p99": 0.11642800000000003
            }
        },
        {
            "phase": "CollisionSnapshot",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7147772845953001,
                "maximum": 1.7043000000000002,
                "p95": 0.8860899999999998,
                "p99": 1.0607260000000003
            }
        },
        {
            "phase": "PairVRest",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.1412814621409922,
                "maximum": 0.33,
                "p95": 0.18254999999999997,
                "p99": 0.24474
            }
        },
        {
            "phase": "CandidateCollect",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 39.71472375979117,
                "maximum": 271.5792,
                "p95": 155.43277999999999,
                "p99": 212.27134600000006
            }
        },
        {
            "phase": "CharacterHitConsumePostInteraction",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.5591537859007823,
                "maximum": 16.5423,
                "p95": 2.2641299999999999,
                "p99": 3.318210000000001
            }
        },
        {
            "phase": "RandomWeaponDrop",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.23201958224543094,
                "maximum": 0.755,
                "p95": 0.32289999999999999,
                "p99": 0.4366960000000001
            }
        },
        {
            "phase": "ObjectHitConsume",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.29963603133159269,
                "maximum": 0.5174,
                "p95": 0.3794399999999998,
                "p99": 0.4293580000000001
            }
        },
        {
            "phase": "CandidateConsumptionEnd",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0024477806788511735,
                "maximum": 0.0082,
                "p95": 0.004,
                "p99": 0.005718000000000001
            }
        },
        {
            "phase": "PreFrameBounds",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 1.2777383812010444,
                "maximum": 2.782,
                "p95": 1.7585599999999995,
                "p99": 2.0339420000000008
            }
        },
        {
            "phase": "Stage",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0011634464751958246,
                "maximum": 0.1032,
                "p95": 0.0015899999999999979,
                "p99": 0.0021540000000000024
            }
        },
        {
            "phase": "RenderDispatch",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 48.084583289817278,
                "maximum": 80.1949,
                "p95": 57.62239,
                "p99": 69.81298200000004
            }
        },
        {
            "phase": "FramePostProcess",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7225080939947779,
                "maximum": 1.6394,
                "p95": 0.9699099999999999,
                "p99": 1.3158560000000006
            }
        },
        {
            "phase": "LateEntityUpdate",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 90.72906631853788,
                "maximum": 229.53310000000003,
                "p95": 180.48702999999999,
                "p99": 217.539704
            }
        },
        {
            "phase": "RandomWeaponDropTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0005373368146214112,
                "maximum": 0.0028,
                "p95": 0.0008,
                "p99": 0.0011
            }
        },
        {
            "phase": "EntityPostFrameTail",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.7696002610966067,
                "maximum": 1.7313,
                "p95": 1.02711,
                "p99": 1.1971600000000005
            }
        },
        {
            "phase": "BattleResults",
            "timing": {
                "available": true,
                "unit": "ms",
                "source": "Stopwatch timestamps around NTSDBattleTickSystem pass boundaries; diagnostic evidence only, with no performance threshold.",
                "unavailableReason": "",
                "sampleCount": 383,
                "average": 0.0006853785900783291,
                "maximum": 0.0021000000000000005,
                "p95": 0.001,
                "p99": 0.0013180000000000009
            }
        }
    ],
    "phaseTimingUnattributedMilliseconds": {
        "available": true,
        "unit": "ms",
        "source": "Outer SimulationTickDriver.StepOneTick time minus the sum of attributed pass timings",
        "unavailableReason": "",
        "sampleCount": 383,
        "average": 0.062059268929487879,
        "maximum": 0.15119999999996026,
        "p95": 0.07919999999998595,
        "p99": 0.11374599999997098
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
        "objectPoolAvailableAfter": 1001,
        "retainedInactiveObjectPoolCapacityBeforeRun": 10,
        "retainedInactiveObjectPoolCapacityAfter": 1001,
        "retainedInactiveObjectPoolCapacityDelta": 991,
        "retainedInactiveObjectPoolCapacityPolicy": "Informational inactive cache capacity only; it is not active cleanup residue and the stress harness does not trim it.",
        "referencePoolActiveBeforeRun": 0,
        "referencePoolActiveAfter": 0,
        "cleanupExceptionCount": 0,
        "cleanupExceptions": "",
        "evidence": "reason=stop-request; restored=True; activeCleanupRestored=True; driverRestored=True; loggerRestored=True; cleanupExceptions=0; activeGO=1000->0; worldObjects=2000->0; worldEntities=1000->0; claimed=1000->0; objectPoolActive=0->0; referencePoolActive=0->0; retainedInactiveObjectPoolCapacity=10->1001 (delta=991; doesNotAffectRestored=True)"
    }
}

[BLOCKED] File 'J:/QQFile/NTSD2.4/ntsd_release_C#/src/BattleCore/Interaction/CollisionCollect.cs' is outside the working directory. Only files within the project are allowed.

[HEADLESS SESSION] You are running non-interactively in a headless pipeline. Produce your FULL, comprehensive analysis directly in your response. Do NOT ask for clarification or confirmation - work thoroughly with all provided context. Do NOT write brief acknowledgments - your response IS the deliverable.

Role: collision architecture reviewer, read-only analysis.

Review the newly completed default-off role-aware collision shadow in:
`Assets/NTSD/Scripts/Animation/Character/BruteForceSceneQuery.cs`
and its focused tests:
`Assets/NTSD/Scripts/Test/Editor/RoleAwareCollisionShadowSelfCheckTests.cs`.

Authority is:
`J:\QQFile\NTSD2.4\ntsd_release_C#\src\BattleCore\Interaction\CollisionCollect.cs`
plus called geometry/role helpers.

Current formal 1000 dispersed broadphase still reports pair peak 184181 and CandidateCollect ~39.72 ms.

Determine:
1. whether the shadow pair set is conservative and authority-complete for current/Prev2 frames, kind=5, invalid/degenerate/unbounded geometry, suppressed/dormant/pending entities;
2. whether diagnostics compare against the correct formal and accepted pair semantics;
3. the minimum large-sample parity instrumentation needed before switching formal consumption;
4. the safest way to run shadow parity without adding an O(N^2) brute reference that distorts the stress timing;
5. exact blockers to enabling role-aware broadphase formally.

Do not edit files. Return severity-rated findings and a concrete validation gate.
