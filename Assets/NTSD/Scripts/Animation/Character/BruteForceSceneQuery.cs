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
        private readonly List<SceneQueryHit> _tmpHitResult = new List<SceneQueryHit>(16);
        private readonly List<LF2Entity> _tmpAllObjects = new List<LF2Entity>(32);
        private readonly List<SceneQueryHit> _emptyCandidateHits = new List<SceneQueryHit>(0);
        private readonly Dictionary<LF2Entity, List<SceneQueryHit>> _candidateCache =
            new Dictionary<LF2Entity, List<SceneQueryHit>>();
        private readonly LooseQuadtreeBroadphase _shadowBroadphase = new LooseQuadtreeBroadphase();
        private readonly List<SpatialBroadphaseEntry> _shadowEntries = new List<SpatialBroadphaseEntry>(128);
        private readonly List<int> _shadowQueryIndices = new List<int>(64);
        private readonly List<long> _shadowBrutePairs = new List<long>(256);
        private readonly List<long> _shadowTreePairs = new List<long>(256);
        private readonly List<long> _shadowAcceptedPairs = new List<long>(64);
        private readonly SpatialBroadphaseDiagnostics _shadowDiagnostics = new SpatialBroadphaseDiagnostics();
        private bool _consumeCandidateCache;
        internal bool ShadowBroadphaseDiagnosticsEnabled { get; set; }
        internal SpatialBroadphaseDiagnostics ShadowBroadphaseDiagnostics => _shadowDiagnostics;
        public BruteForceSceneQuery(SimulationWorld world)
        {
            _world = world;
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

            _world.GetAllEntities(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
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

            _world.GetAllEntities(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
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
            int currentTick = _world?.CurrentTickIndex ?? 0;

            _world.GetAllEntities(_tmpAllObjects);
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
                BuildShadowBroadphase(currentTick);

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

            if (ShadowBroadphaseDiagnosticsEnabled)
                CompareShadowBroadphaseResults();

            _consumeCandidateCache = true;
        }

        private void BuildShadowBroadphase(int currentTick)
        {
            _shadowEntries.Clear();
            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity entity = _tmpAllObjects[i];
                if (entity == null || entity.PS == null || IsPendingFlushDestroy(entity))
                    continue;
                if (IsCollisionCandidateSuppressed(entity, currentTick))
                    continue;
                if (!TryBuildCollisionBroadphaseAabb(
                        entity,
                        entity.GetCollisionFrameData(),
                        out SpatialAabbXZ bounds))
                {
                    continue;
                }

                int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                _shadowEntries.Add(new SpatialBroadphaseEntry(runtimeSlot, _shadowEntries.Count, bounds));
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

            _shadowBrutePairs.Clear();
            for (int i = 0; i < _shadowEntries.Count; i++)
            {
                SpatialBroadphaseEntry a = _shadowEntries[i];
                for (int j = i + 1; j < _shadowEntries.Count; j++)
                {
                    SpatialBroadphaseEntry b = _shadowEntries[j];
                    if (a.Bounds.Overlaps(b.Bounds) &&
                        TryBuildPairKey(a.RuntimeSlot, b.RuntimeSlot, out long pairKey))
                    {
                        _shadowBrutePairs.Add(pairKey);
                    }
                }
            }

            _shadowTreePairs.Clear();
            for (int i = 0; i < _shadowEntries.Count; i++)
            {
                SpatialBroadphaseEntry a = _shadowEntries[i];
                _shadowBroadphase.Query(a.Bounds, _shadowQueryIndices);
                for (int resultIndex = 0; resultIndex < _shadowQueryIndices.Count; resultIndex++)
                {
                    int j = _shadowQueryIndices[resultIndex];
                    if (j <= i || j >= _shadowEntries.Count)
                        continue;

                    SpatialBroadphaseEntry b = _shadowEntries[j];
                    if (a.Bounds.Overlaps(b.Bounds) &&
                        TryBuildPairKey(a.RuntimeSlot, b.RuntimeSlot, out long pairKey))
                    {
                        _shadowTreePairs.Add(pairKey);
                    }
                }
            }

            SortAndDeduplicate(_shadowBrutePairs);
            SortAndDeduplicate(_shadowTreePairs);
            _shadowDiagnostics.Begin(_shadowEntries.Count);
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

            int bruteIndex = 0;
            int treeIndex = 0;
            while (bruteIndex < _shadowBrutePairs.Count || treeIndex < _shadowTreePairs.Count)
            {
                if (treeIndex >= _shadowTreePairs.Count ||
                    (bruteIndex < _shadowBrutePairs.Count &&
                     _shadowBrutePairs[bruteIndex] < _shadowTreePairs[treeIndex]))
                {
                    _shadowDiagnostics.MismatchCount++;
                    if (_shadowDiagnostics.FirstMissingPair < 0)
                        _shadowDiagnostics.FirstMissingPair = _shadowBrutePairs[bruteIndex];
                    bruteIndex++;
                }
                else if (bruteIndex >= _shadowBrutePairs.Count ||
                         _shadowTreePairs[treeIndex] < _shadowBrutePairs[bruteIndex])
                {
                    _shadowDiagnostics.MismatchCount++;
                    if (_shadowDiagnostics.FirstExtraPair < 0)
                        _shadowDiagnostics.FirstExtraPair = _shadowTreePairs[treeIndex];
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
                if (_shadowDiagnostics.FirstAcceptedPairMissingFromTree < 0)
                    _shadowDiagnostics.FirstAcceptedPairMissingFromTree = pairKey;
            }
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

            _world.GetAllEntities(_tmpAllObjects);

            for (int i = 0; i < _tmpAllObjects.Count; i++)
            {
                LF2Entity target = _tmpAllObjects[i];
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
