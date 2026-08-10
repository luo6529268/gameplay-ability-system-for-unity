using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
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
        private const int HitCandidateMax = CollisionCandidateStore.HitCandidateMax;
        private const int CandidateDistanceUnset = 1000;
        private const int RectMin = -1000000000;
        private const int RectMax = 1000000000;
        private const byte RoleAwareFormalExactAttackRole = 1 << 0;
        private const byte RoleAwareFormalExactBodyRole = 1 << 1;
        private const byte RoleAwareFormalParticipantHasAttackItr = 1 << 0;
        private const byte RoleAwareFormalParticipantHasFallbackAttackItr = 1 << 1;
        private const byte RoleAwareFormalParticipantHasBody = 1 << 2;
        private const byte RoleAwareFormalParticipantHasFallbackBody = 1 << 3;
        private const int AuthorityPairCountingSortThreshold = 4096;
        public const long RoleAwareDirectComparisonThreshold = 262144L;
        public const long RoleAwareSweepDirectCrossover = 8192L;

        private readonly SimulationWorld _world;
        private readonly CollisionBroadphaseBackend _collisionBroadphase;
        private readonly List<SceneQueryHit> _tmpHitResult = new List<SceneQueryHit>(16);
        private readonly List<LF2Entity> _tmpAllObjects = new List<LF2Entity>(32);
        private readonly List<SceneQueryHit> _emptyCandidateHits = new List<SceneQueryHit>(0);
        private readonly Dictionary<LF2Entity, List<SceneQueryHit>> _candidateCache =
            new Dictionary<LF2Entity, List<SceneQueryHit>>();
        private readonly Stack<List<SceneQueryHit>> _candidateListPool =
            new Stack<List<SceneQueryHit>>();
        private readonly CollisionCandidateStoreDiagnostics _candidateStoreShadowDiagnostics =
            new CollisionCandidateStoreDiagnostics();
        private readonly CollisionCandidateStoreAuthorityDiagnostics _candidateStoreAuthorityDiagnostics =
            new CollisionCandidateStoreAuthorityDiagnostics();
        private readonly CollisionCandidateStore _candidateStoreShadow;
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
        private RoleAwareFormalParticipant[] _roleFormalParticipantReadBuffer =
            new RoleAwareFormalParticipant[128];
        private byte[] _roleFormalParticipantRoleFlags = new byte[128];
        private readonly Dictionary<LF2FrameData, RoleAwareFormalBodyTemplate>
            _roleFormalBodyTemplates =
                new Dictionary<LF2FrameData, RoleAwareFormalBodyTemplate>(
                    LF2FrameDataReferenceComparer.Instance);
        private readonly List<RoleAwareFormalItrEntry> _roleFormalItrEntries =
            new List<RoleAwareFormalItrEntry>(128);
        private readonly List<RoleAwareFormalExactItrRectEntry>
            _roleFormalExactItrRects =
                new List<RoleAwareFormalExactItrRectEntry>(128);
        private readonly List<RoleAwareFormalExactBodyRectEntry>
            _roleFormalExactBodyRects =
                new List<RoleAwareFormalExactBodyRectEntry>(128);
        private readonly List<byte> _roleFormalExactRequiredRoles =
            new List<byte>(128);
        private readonly List<int> _roleFormalBodyOrdinals =
            new List<int>(128);
        private readonly List<int> _roleFormalFallbackAttackOrdinals =
            new List<int>(32);
        private readonly List<int> _roleFormalExactAttackOrdinals =
            new List<int>(128);
        private readonly List<int> _roleFormalFallbackBodyOrdinals =
            new List<int>(32);
        private readonly List<RoleAwareSweepEvent> _roleFormalSweepEvents =
            new List<RoleAwareSweepEvent>(512);
        private readonly List<int> _roleFormalSweepActiveBodyIndices =
            new List<int>(128);
        private readonly List<int> _roleFormalSweepActiveItrIndices =
            new List<int>(128);
        private readonly List<int> _roleFormalSweepBodyPositions =
            new List<int>(128);
        private readonly List<int> _roleFormalSweepItrPositions =
            new List<int>(128);
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
        private long[] _formalAuthorityPairSortScratch = Array.Empty<long>();
        private int[] _formalAuthorityPairSortCounts = Array.Empty<int>();
        private int[] _formalAuthorityPairSortOffsets = Array.Empty<int>();
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
        private bool _candidateStoreAuthorityRequestedForCurrentTick;
        private bool _candidateStoreAuthorityAppliedForCurrentTick;
        private int _candidateStoreLegacyOracleInterval = 1;
        private int _candidateProducerTickForCurrentCollection;
        private bool _candidateStoreProducerHealthyForCurrentTick;
        private CollisionCandidateProducerMode _candidateProducerModeForCurrentTick;
        private int _candidateConsumptionEpoch;
        private CollisionCandidateConsumptionSource _candidateConsumptionSource;
        private long _candidateListCreatedCount;
        private long _candidateListReusedCount;
        private long _candidateListRejectedRentCount;
        private int _formalFallbackParticipantCount;
        private bool _formalCollectionAborted;
        private bool _collisionRoleZeroItrEarlyReturnAppliedForCurrentTick;
        private int _lastFormalPairCount;
        private int _lastRoleAwareParticipantCount;
        private int _lastRoleAwareInertParticipantCount;
        private int _lastRoleAwareBodyEntryCount;
        private int _lastRoleAwareItrQueryCount;
        private int _lastRoleAwareBodyTemplateBuildCount;
        private int _lastRoleAwareBodyTemplateHitCount;
        private int _lastRoleAwareBodyTemplateFallbackCount;
        private long _lastRoleAwareDirectComparisonCount;
        private long _totalRoleAwareDirectComparisonCount;
        private long _lastRoleAwareDirectCost;
        private bool _lastRoleAwareDirectCostAvailable;
        private int _lastRoleAwareDirectTickCount;
        private int _lastRoleAwareTreeTickCount;
        private int _lastRoleAwareNestedDirectTickCount;
        private int _lastRoleAwareSweepDirectTickCount;
        private long _lastRoleAwareSweepXCandidateCount;
        private long _lastRoleAwareSweepFullOverlapCheckCount;
        private int _lastRoleAwareCheapInputValidationCount;
        private int _lastRoleAwareFullInputValidationCount;
        private int _lastRoleAwareExactItrRectBuildCount;
        private int _lastRoleAwareExactBodyRectBuildCount;
        private int _lastRoleAwareExactDirectionCount;
        private int _lastRoleAwareExactItrVisitCount;
        private int _lastRoleAwareExactBodyOverlapCheckCount;
        private long _totalRoleAwareDirectTickCount;
        private long _totalRoleAwareTreeTickCount;
        private long _totalRoleAwareNestedDirectTickCount;
        private long _totalRoleAwareSweepDirectTickCount;
        private long _totalRoleAwareSweepXCandidateCount;
        private long _totalRoleAwareSweepFullOverlapCheckCount;
        private bool _forceRoleAwareNestedDirectForDiagnostics;
        private bool _forceRoleAwareSweepDirectForDiagnostics;
        public bool ShadowBroadphaseDiagnosticsEnabled { get; set; }
        public bool CollisionCandidateStoreShadowDiagnosticsEnabled { get; set; }
        public bool CollisionCandidateStoreAuthorityEnabled { get; set; }
        public int CollisionCandidateStoreLegacyOracleInterval
        {
            get => _candidateStoreLegacyOracleInterval;
            set => _candidateStoreLegacyOracleInterval = Math.Max(0, value);
        }
        public CollisionCandidateStoreDiagnostics CollisionCandidateStoreShadowDiagnostics =>
            _candidateStoreShadowDiagnostics;
        public CollisionCandidateStoreAuthorityDiagnostics CollisionCandidateStoreAuthorityDiagnostics =>
            _candidateStoreAuthorityDiagnostics;
        public bool CollisionCandidateStoreAuthorityAppliedForCurrentTickForDiagnostics =>
            _candidateStoreAuthorityAppliedForCurrentTick;
        public bool CollisionCandidateStoreLegacyOracleSampledForCurrentTickForDiagnostics =>
            _candidateProducerModeForCurrentTick ==
            CollisionCandidateProducerMode.StoreWithLegacyOracle;
        public bool CollisionCandidateStoreOnlyForCurrentTickForDiagnostics =>
            _candidateProducerModeForCurrentTick ==
            CollisionCandidateProducerMode.StoreOnly;
        public int CollisionCandidateStoreRuntimeCapacityForDiagnostics =>
            _candidateStoreShadow.RuntimeCapacity;
        public long CandidateListCreatedCountForDiagnostics =>
            _candidateListCreatedCount;
        public long CandidateListReusedCountForDiagnostics =>
            _candidateListReusedCount;
        public long CandidateListRejectedRentCountForDiagnostics =>
            _candidateListRejectedRentCount;
        public int CandidateListPoolCountForDiagnostics => _candidateListPool.Count;
        public int ActiveCandidateListCountForDiagnostics => _candidateCache.Count;
        public CollisionFormalCollectorMode FormalCollectorMode { get; set; }
        public bool ForceLegacyRoleBodyBuildForDiagnostics { get; set; }
        public bool ForceRoleAwareTreeForDiagnostics { get; set; }
        public bool ForceRoleAwareDirectForDiagnostics { get; set; }
        public bool ForceRoleAwareNestedDirectForDiagnostics
        {
            get => _forceRoleAwareNestedDirectForDiagnostics;
            set
            {
                _forceRoleAwareNestedDirectForDiagnostics = value;
                if (value)
                    _forceRoleAwareSweepDirectForDiagnostics = false;
            }
        }
        public bool ForceRoleAwareSweepDirectForDiagnostics
        {
            get => _forceRoleAwareSweepDirectForDiagnostics;
            set
            {
                _forceRoleAwareSweepDirectForDiagnostics = value;
                if (value)
                    _forceRoleAwareNestedDirectForDiagnostics = false;
            }
        }
        public bool ForceFullRoleAwareFormalInputValidationForDiagnostics { get; set; }
        public bool CollisionRoleZeroItrFastPathEnabled { get; private set; }
        public long CollisionRoleZeroItrFastPathRequestedCountForDiagnostics { get; private set; }
        public long CollisionRoleZeroItrFastPathAppliedCountForDiagnostics { get; private set; }
        public long CollisionRoleZeroItrFastPathFallbackCountForDiagnostics { get; private set; }
        public long CollisionRoleZeroItrFastPathInvalidCountForDiagnostics { get; private set; }
        public long CollisionRoleZeroItrFastPathZeroItrCountForDiagnostics { get; private set; }
        public int CollisionRoleZeroItrFastPathParticipantCountForDiagnostics { get; private set; }
        // Compatibility name retained for existing report JSON. This now means the
        // participant count produced by the existing role-aware build; no handles are scanned.
        public int CollisionRoleZeroItrFastPathTouchedHandleCountForDiagnostics =>
            CollisionRoleZeroItrFastPathParticipantCountForDiagnostics;

        public void SetCollisionRoleZeroItrFastPathEnabledForSelfCheck(bool enabled)
        {
            if (_consumeCandidateCache)
            {
                throw new InvalidOperationException(
                    "Collision role zero-itr mode cannot change during candidate consumption.");
            }

            CollisionRoleZeroItrFastPathEnabled = enabled;
        }

        internal SpatialBroadphaseDiagnostics ShadowBroadphaseDiagnostics => _shadowDiagnostics;
        public RoleAwareCollisionShadowDiagnostics RoleAwareShadowDiagnostics =>
            _roleShadowDiagnostics;
        internal CollisionBroadphaseBackend CollisionBroadphase => _collisionBroadphase;
        internal int FormalFallbackParticipantCount => _formalFallbackParticipantCount;
        public bool FormalCollectionAborted => _formalCollectionAborted;
        internal SpatialSynchronizeResult FormalSpatialSynchronizeResult { get; private set; }
        internal LooseQuadtreeBroadphase FormalBroadphaseForSelfCheck =>
            LastFormalCollectorModeForDiagnostics == CollisionFormalCollectorMode.ForceRoleAware
                ? _roleFormalBroadphase
                : _formalBroadphase;
#if UNITY_INCLUDE_TESTS
        private readonly List<byte> _roleFormalExactCommonBuildCounts =
            new List<byte>(128);
        private readonly List<byte> _roleFormalExactAttackBuildCounts =
            new List<byte>(128);
        private readonly List<byte> _roleFormalExactBodyBuildCounts =
            new List<byte>(128);
        private readonly List<byte> _roleFormalExactValidationCounts =
            new List<byte>(128);
        public bool ThrowDuringRoleAwareShadowForSelfCheck { get; set; }
        public int ThrowAfterRoleAwareFormalPairCountForSelfCheck { get; set; } = -1;
        public bool ForceLegacyPerPairValidationForDiagnostics { get; set; }
        public bool ForceLegacyRoleAwareExactPrefilterForDiagnostics { get; set; }
        public bool ForceRoleAwareExactCacheRevalidationForDiagnostics { get; set; }
        public int ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck { get; set; } = -1;
        private int _collisionCandidateStoreWriteCountForSelfCheck;
        private bool _collisionCandidateStoreFaultInjectedForSelfCheck;
        public Action BeforeRoleAwareFormalInputValidationForSelfCheck { get; set; }
        public Action<int> AfterRoleAwareExactPairForSelfCheck { get; set; }
        public Action BeforeCollisionCandidateStoreFinalCompareForSelfCheck { get; set; }
        public bool TryAppendCollisionCandidateLegacyOracleForSelfCheck(
            LF2Entity attacker,
            in SceneQueryHit hit)
        {
            if (attacker == null ||
                !_candidateCache.TryGetValue(attacker, out List<SceneQueryHit> candidates))
            {
                return false;
            }

            candidates.Add(hit);
            return true;
        }

        public long MeasureWarmedRoleAwareDirectAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));

            _formalAuthorityPairKeys.Clear();
            CollectRoleAwareDirectBroadphasePairs();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                _formalAuthorityPairKeys.Clear();
                CollectRoleAwareDirectBroadphasePairs();
            }
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        public long MeasureWarmedRoleAwareCollectAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));

            CollectCollisionCandidates();
            EndCollisionCandidateConsumption();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                CollectCollisionCandidates();
                EndCollisionCandidateConsumption();
            }
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        public long MeasureWarmedRoleAwareFallbackPairAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));

            _formalAuthorityPairKeys.Clear();
            CollectRoleAwareFormalFallbackPairs();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                _formalAuthorityPairKeys.Clear();
                CollectRoleAwareFormalFallbackPairs();
            }
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        public long MeasureWarmedRoleAwareExactRectCacheAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));
            if (_roleFormalExactRequiredRoles.Count !=
                _roleFormalParticipants.Count)
            {
                throw new InvalidOperationException(
                    "Role-aware exact cache inputs have not been collected.");
            }

            RebuildRoleAwareFormalExactRectCachesForSelfCheck();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
                RebuildRoleAwareFormalExactRectCachesForSelfCheck();
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        private void RebuildRoleAwareFormalExactRectCachesForSelfCheck()
        {
            _roleFormalExactItrRects.Clear();
            _roleFormalExactBodyRects.Clear();
            _lastRoleAwareExactItrRectBuildCount = 0;
            _lastRoleAwareExactBodyRectBuildCount = 0;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                byte requiredRoles =
                    _roleFormalExactRequiredRoles[participantOrdinal];
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if ((requiredRoles & RoleAwareFormalExactAttackRole) != 0)
                {
                    participant.HasExactAttackCache = false;
                    participant.HasOrdinaryItrUnion = false;
                    BuildRoleAwareFormalExactAttackCache(ref participant);
                }
                if ((requiredRoles & RoleAwareFormalExactBodyRole) != 0)
                {
                    participant.HasExactBodyCache = false;
                    participant.HasBodyUnion = false;
                    BuildRoleAwareFormalExactBodyCache(ref participant);
                }

                _roleFormalParticipants[participantOrdinal] = participant;
            }
        }

        public void GetLastRoleAwareFallbackOrdinalCountsForSelfCheck(
            out int bodyCount,
            out int fallbackAttackCount,
            out int exactAttackCount,
            out int fallbackBodyCount)
        {
            bodyCount = _roleFormalBodyOrdinals.Count;
            fallbackAttackCount = _roleFormalFallbackAttackOrdinals.Count;
            exactAttackCount = _roleFormalExactAttackOrdinals.Count;
            fallbackBodyCount = _roleFormalFallbackBodyOrdinals.Count;
        }

        public void CopyLastRoleAwareFallbackPairKeysForSelfCheck(
            List<long> oldPredicateKeys,
            List<long> roleListKeys)
        {
            if (oldPredicateKeys == null)
                throw new ArgumentNullException(nameof(oldPredicateKeys));
            if (roleListKeys == null)
                throw new ArgumentNullException(nameof(roleListKeys));

            oldPredicateKeys.Clear();
            roleListKeys.Clear();
            for (int attackerOrdinal = 0;
                 attackerOrdinal < _roleFormalParticipants.Count;
                 attackerOrdinal++)
            {
                RoleAwareFormalParticipant attacker =
                    _roleFormalParticipants[attackerOrdinal];
                for (int targetOrdinal = 0;
                     targetOrdinal < _roleFormalParticipants.Count;
                     targetOrdinal++)
                {
                    if (attackerOrdinal == targetOrdinal)
                        continue;

                    RoleAwareFormalParticipant target =
                        _roleFormalParticipants[targetOrdinal];
                    if (attacker.HasAttackItr &&
                        target.HasBody &&
                        (attacker.HasFallbackAttackItr ||
                         target.HasFallbackBody))
                    {
                        AddOrdinalPair(
                            oldPredicateKeys,
                            attackerOrdinal,
                            targetOrdinal);
                    }
                }
            }

            AddRoleAwareFormalFallbackPairsTo(roleListKeys);
            SortAndDeduplicate(oldPredicateKeys);
            SortAndDeduplicate(roleListKeys);
        }

        public bool TryGetLastRoleAwareParticipantFlagsForSelfCheck(
            LF2Entity entity,
            out bool hasBody,
            out bool hasFallbackBody,
            out bool hasAttackItr,
            out bool hasFallbackAttackItr)
        {
            hasBody = false;
            hasFallbackBody = false;
            hasAttackItr = false;
            hasFallbackAttackItr = false;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (!ReferenceEquals(participant.Entity, entity))
                    continue;

                hasBody = participant.HasBody;
                hasFallbackBody = participant.HasFallbackBody;
                hasAttackItr = participant.HasAttackItr;
                hasFallbackAttackItr = participant.HasFallbackAttackItr;
                return true;
            }

            return false;
        }

        public bool TryGetLastRoleAwareExactCacheCountsForSelfCheck(
            LF2Entity entity,
            out int commonBuildCount,
            out int attackBuildCount,
            out int bodyBuildCount,
            out int validationCount,
            out bool attackRequired,
            out bool bodyRequired)
        {
            commonBuildCount = 0;
            attackBuildCount = 0;
            bodyBuildCount = 0;
            validationCount = 0;
            attackRequired = false;
            bodyRequired = false;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                if (!ReferenceEquals(
                        _roleFormalParticipants[participantOrdinal].Entity,
                        entity))
                {
                    continue;
                }

                if (participantOrdinal >= _roleFormalExactRequiredRoles.Count ||
                    participantOrdinal >= _roleFormalExactCommonBuildCounts.Count ||
                    participantOrdinal >= _roleFormalExactAttackBuildCounts.Count ||
                    participantOrdinal >= _roleFormalExactBodyBuildCounts.Count ||
                    participantOrdinal >= _roleFormalExactValidationCounts.Count)
                {
                    return false;
                }

                byte requiredRoles =
                    _roleFormalExactRequiredRoles[participantOrdinal];
                commonBuildCount =
                    _roleFormalExactCommonBuildCounts[participantOrdinal];
                attackBuildCount =
                    _roleFormalExactAttackBuildCounts[participantOrdinal];
                bodyBuildCount =
                    _roleFormalExactBodyBuildCounts[participantOrdinal];
                validationCount =
                    _roleFormalExactValidationCounts[participantOrdinal];
                attackRequired =
                    (requiredRoles & RoleAwareFormalExactAttackRole) != 0;
                bodyRequired =
                    (requiredRoles & RoleAwareFormalExactBodyRole) != 0;
                return true;
            }

            return false;
        }

        public void CopyLastFormalRuntimeSlotPairKeysForSelfCheck(List<long> destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));

            destination.Clear();
            if (LastFormalCollectorModeForDiagnostics ==
                CollisionFormalCollectorMode.ForceRoleAware)
            {
                for (int pairIndex = 0;
                     pairIndex < _formalAuthorityPairKeys.Count;
                     pairIndex++)
                {
                    long authorityPair = _formalAuthorityPairKeys[pairIndex];
                    int firstOrdinal = (int)(authorityPair >> 32);
                    int secondOrdinal = (int)(authorityPair & 0xffffffffL);
                    if (firstOrdinal < 0 ||
                        secondOrdinal <= firstOrdinal ||
                        secondOrdinal >= _roleFormalParticipants.Count)
                    {
                        continue;
                    }

                    AddRuntimeSlotPairForSelfCheck(
                        destination,
                        _roleFormalParticipants[firstOrdinal].Entity?.Runtime?.SlotIndex ?? -1,
                        _roleFormalParticipants[secondOrdinal].Entity?.Runtime?.SlotIndex ?? -1);
                }
            }
            else if (LastFormalCollectorModeForDiagnostics ==
                     CollisionFormalCollectorMode.ForceLegacyUnionAabb)
            {
                for (int pairIndex = 0;
                     pairIndex < _formalAuthorityPairKeys.Count;
                     pairIndex++)
                {
                    long authorityPair = _formalAuthorityPairKeys[pairIndex];
                    int firstOrdinal = (int)(authorityPair >> 32);
                    int secondOrdinal = (int)(authorityPair & 0xffffffffL);
                    if (firstOrdinal < 0 ||
                        secondOrdinal <= firstOrdinal ||
                        secondOrdinal >= _formalParticipants.Count)
                    {
                        continue;
                    }

                    AddRuntimeSlotPairForSelfCheck(
                        destination,
                        _formalParticipants[firstOrdinal]?.Runtime?.SlotIndex ?? -1,
                        _formalParticipants[secondOrdinal]?.Runtime?.SlotIndex ?? -1);
                }
            }

            SortAndDeduplicate(destination);
        }

        private static void AddRuntimeSlotPairForSelfCheck(
            List<long> destination,
            int firstSlot,
            int secondSlot)
        {
            if (firstSlot < 0 || secondSlot < 0 || firstSlot == secondSlot)
                return;

            uint min = (uint)Math.Min(firstSlot, secondSlot);
            uint max = (uint)Math.Max(firstSlot, secondSlot);
            destination.Add(((long)min << 32) | max);
        }

        public bool TryGetCollisionCandidateStoreRowForSelfCheck(
            RuntimeEntityHandle attackerHandle,
            out int count)
        {
            if (!_world.TryResolveRuntimeHandle(attackerHandle, out _))
            {
                count = 0;
                return false;
            }
            return _candidateStoreShadow.TryGetVisibleAttackerRow(attackerHandle, out count);
        }

        public bool TryGetCollisionCandidateStoreEntryForSelfCheck(
            RuntimeEntityHandle attackerHandle,
            int candidateIndex,
            out CollisionCandidateStoreEntry entry)
        {
            if (!_world.TryResolveRuntimeHandle(attackerHandle, out _))
            {
                entry = default;
                return false;
            }
            return _candidateStoreShadow.TryGetVisibleCandidate(
                attackerHandle,
                candidateIndex,
                out entry);
        }

        public bool TryBuildCollisionCandidateStoreCapacityForSelfCheck(
            int runtimeCapacity)
        {
            bool began = _candidateStoreShadow.BeginBuild(runtimeCapacity);
            _candidateStoreShadow.AbortBuild();
            return began;
        }

        public long MeasureWarmedCollisionCandidateStoreShadowAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));
            if (!CollisionCandidateStoreShadowDiagnosticsEnabled)
            {
                throw new InvalidOperationException(
                    "Collision candidate store shadow diagnostics are disabled.");
            }

            TouchVisibleCollisionCandidateStoreShadowForSelfCheck();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
                TouchVisibleCollisionCandidateStoreShadowForSelfCheck();
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        public long MeasureWarmedCollisionRoleZeroItrFastPathAllocationsForSelfCheck(
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));
            if (!CollisionRoleZeroItrFastPathEnabled)
            {
                throw new InvalidOperationException(
                    "Collision role zero-itr fast path is disabled.");
            }

            CollectCollisionCandidates();
            EndCollisionCandidateConsumption();
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
            {
                CollectCollisionCandidates();
                EndCollisionCandidateConsumption();
            }
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        public long MeasureWarmedCollisionCandidateStoreAuthorityAllocationsForSelfCheck(
            LF2Entity attacker,
            int iterationCount)
        {
            if (iterationCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(iterationCount));
            if (!_candidateStoreAuthorityAppliedForCurrentTick)
            {
                throw new InvalidOperationException(
                    "Collision candidate store authority is not applied for this tick.");
            }

            TouchCollisionCandidateStoreAuthorityForSelfCheck(attacker);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int iteration = 0; iteration < iterationCount; iteration++)
                TouchCollisionCandidateStoreAuthorityForSelfCheck(attacker);
            long after = GC.GetAllocatedBytesForCurrentThread();
            return after - before;
        }

        private void TouchCollisionCandidateStoreAuthorityForSelfCheck(LF2Entity attacker)
        {
            if (!TryGetCollisionCandidateRange(attacker, out CollisionCandidateRange candidates))
                return;

            int count = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < count; candidateIndex++)
                candidates.TryGet(candidateIndex, out _);
        }

        private void TouchVisibleCollisionCandidateStoreShadowForSelfCheck()
        {
            int runtimeCapacity = _world.MaxRuntimeSlotsForServices;
            for (int runtimeSlot = 0; runtimeSlot < runtimeCapacity; runtimeSlot++)
            {
                LF2Entity attacker = _world.FindEntityByRuntimeSlotForQuery(runtimeSlot);
                if (attacker == null || !_candidateCache.ContainsKey(attacker) ||
                    !_world.TryGetCurrentRuntimeHandle(
                        runtimeSlot,
                        attacker,
                        out RuntimeEntityHandle attackerHandle) ||
                    !_candidateStoreShadow.TryGetVisibleAttackerRow(
                        attackerHandle,
                        out int count))
                {
                    continue;
                }

                for (int candidateIndex = 0; candidateIndex < count; candidateIndex++)
                {
                    _candidateStoreShadow.TryGetVisibleCandidate(
                        attackerHandle,
                        candidateIndex,
                        out _);
                }
            }
        }
#endif
        public bool TryGetLastRoleAwareBodyBoundsForSelfCheck(
            LF2Entity entity,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (entity?.Runtime == null ||
                LastFormalCollectorModeForDiagnostics !=
                    CollisionFormalCollectorMode.ForceRoleAware)
            {
                return false;
            }

            int slot = entity.Runtime.SlotIndex;
            for (int entryIndex = 0;
                 entryIndex < _formalIncrementalEntries.Count;
                 entryIndex++)
            {
                IncrementalSpatialEntry entry = _formalIncrementalEntries[entryIndex];
                if (entry.Handle.Slot != slot)
                    continue;

                bounds = entry.Bounds;
                return bounds.IsValid;
            }

            return false;
        }

        public CollisionFormalCollectorMode LastFormalCollectorModeForDiagnostics { get; private set; }
        public int LastFormalPairCountForDiagnostics => _lastFormalPairCount;
        public int LastFormalFallbackParticipantCountForDiagnostics =>
            _formalFallbackParticipantCount;
        public bool LastFormalCollectionAbortedForDiagnostics => _formalCollectionAborted;
        public SpatialSynchronizeResult LastFormalSynchronizeResultForDiagnostics =>
            FormalSpatialSynchronizeResult;
        public int LastRoleAwareBodyEntryCountForDiagnostics => _lastRoleAwareBodyEntryCount;
        public int LastRoleAwareItrQueryCountForDiagnostics => _lastRoleAwareItrQueryCount;
        public int LastRoleAwareParticipantCountForDiagnostics => _lastRoleAwareParticipantCount;
        public int LastRoleAwareInertParticipantCountForDiagnostics =>
            _lastRoleAwareInertParticipantCount;
        public int LastRoleAwareBodyTemplateBuildCountForDiagnostics =>
            _lastRoleAwareBodyTemplateBuildCount;
        public int LastRoleAwareBodyTemplateHitCountForDiagnostics =>
            _lastRoleAwareBodyTemplateHitCount;
        public int LastRoleAwareBodyTemplateFallbackCountForDiagnostics =>
            _lastRoleAwareBodyTemplateFallbackCount;
        public long LastRoleAwareDirectComparisonCountForDiagnostics =>
            _lastRoleAwareDirectComparisonCount;
        public long TotalRoleAwareDirectComparisonCountForDiagnostics =>
            _totalRoleAwareDirectComparisonCount;
        // Reset to unavailable at the start of every formal collection. It becomes
        // available only after role-aware input validation reaches the direct/tree
        // selection point; later collection failure keeps the already-computed cost.
        public long LastRoleAwareDirectCostForDiagnostics =>
            _lastRoleAwareDirectCost;
        public bool LastRoleAwareDirectCostAvailableForDiagnostics =>
            _lastRoleAwareDirectCostAvailable;
        public int LastRoleAwareDirectTickCountForDiagnostics =>
            _lastRoleAwareDirectTickCount;
        public int LastRoleAwareTreeTickCountForDiagnostics =>
            _lastRoleAwareTreeTickCount;
        public int LastRoleAwareNestedDirectTickCountForDiagnostics =>
            _lastRoleAwareNestedDirectTickCount;
        public int LastRoleAwareSweepDirectTickCountForDiagnostics =>
            _lastRoleAwareSweepDirectTickCount;
        public long LastRoleAwareSweepXCandidateCountForDiagnostics =>
            _lastRoleAwareSweepXCandidateCount;
        public long LastRoleAwareSweepFullOverlapCheckCountForDiagnostics =>
            _lastRoleAwareSweepFullOverlapCheckCount;
        public int LastRoleAwareCheapInputValidationCountForDiagnostics =>
            _lastRoleAwareCheapInputValidationCount;
        public int LastRoleAwareFullInputValidationCountForDiagnostics =>
            _lastRoleAwareFullInputValidationCount;
        public int LastRoleAwareExactItrRectBuildCountForDiagnostics =>
            _lastRoleAwareExactItrRectBuildCount;
        public int LastRoleAwareExactBodyRectBuildCountForDiagnostics =>
            _lastRoleAwareExactBodyRectBuildCount;
        public int LastRoleAwareExactDirectionCountForDiagnostics =>
            _lastRoleAwareExactDirectionCount;
        public int LastRoleAwareExactItrVisitCountForDiagnostics =>
            _lastRoleAwareExactItrVisitCount;
        public int LastRoleAwareExactBodyOverlapCheckCountForDiagnostics =>
            _lastRoleAwareExactBodyOverlapCheckCount;
        public long TotalRoleAwareDirectTickCountForDiagnostics =>
            _totalRoleAwareDirectTickCount;
        public long TotalRoleAwareTreeTickCountForDiagnostics =>
            _totalRoleAwareTreeTickCount;
        public long TotalRoleAwareNestedDirectTickCountForDiagnostics =>
            _totalRoleAwareNestedDirectTickCount;
        public long TotalRoleAwareSweepDirectTickCountForDiagnostics =>
            _totalRoleAwareSweepDirectTickCount;
        public long TotalRoleAwareSweepXCandidateCountForDiagnostics =>
            _totalRoleAwareSweepXCandidateCount;
        public long TotalRoleAwareSweepFullOverlapCheckCountForDiagnostics =>
            _totalRoleAwareSweepFullOverlapCheckCount;
        public BruteForceSceneQuery(
            SimulationWorld world,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            _world = world;
            _collisionBroadphase = collisionBroadphase;
            _candidateStoreShadow = new CollisionCandidateStore(
                _candidateStoreShadowDiagnostics);
        }

        internal void PrepareBattleCapacity(
            int entityCapacity,
            int maximumBodyCountPerEntity = 1,
            int maximumItrCountPerEntity = 1)
        {
            if (entityCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(entityCapacity));

            int exactBodyRectCapacity = checked(
                entityCapacity * Math.Max(1, maximumBodyCountPerEntity));
            int exactItrRectCapacity = checked(
                entityCapacity * Math.Max(1, maximumItrCountPerEntity));

            int pairCapacity = checked(entityCapacity * (entityCapacity - 1) / 2);
            int sweepCapacity = checked(entityCapacity * 4);

            EnsureListCapacity(_tmpHitResult, HitCandidateMax);
            EnsureListCapacity(_tmpAllObjects, entityCapacity);
            _candidateCache.EnsureCapacity(entityCapacity);
            while (_candidateListPool.Count < entityCapacity)
                _candidateListPool.Push(new List<SceneQueryHit>(HitCandidateMax));
            _candidateStoreShadow.PrepareCapacity(entityCapacity);

            EnsureListCapacity(_roleFormalParticipants, entityCapacity);
            if (_roleFormalParticipantReadBuffer.Length < entityCapacity)
            {
                Array.Resize(
                    ref _roleFormalParticipantReadBuffer,
                    entityCapacity);
            }
            if (_roleFormalParticipantRoleFlags.Length < entityCapacity)
            {
                Array.Resize(
                    ref _roleFormalParticipantRoleFlags,
                    entityCapacity);
            }
            _roleFormalBodyTemplates.EnsureCapacity(entityCapacity);
            EnsureListCapacity(_roleFormalItrEntries, entityCapacity);
            EnsureListCapacity(_roleFormalExactItrRects, exactItrRectCapacity);
            EnsureListCapacity(_roleFormalExactBodyRects, exactBodyRectCapacity);
            EnsureListCapacity(_roleFormalExactRequiredRoles, entityCapacity);
            EnsureListCapacity(_roleFormalBodyOrdinals, entityCapacity);
            EnsureListCapacity(_roleFormalFallbackAttackOrdinals, entityCapacity);
            EnsureListCapacity(_roleFormalExactAttackOrdinals, entityCapacity);
            EnsureListCapacity(_roleFormalFallbackBodyOrdinals, entityCapacity);
            EnsureListCapacity(_roleFormalSweepEvents, sweepCapacity);
            EnsureListCapacity(_roleFormalSweepActiveBodyIndices, entityCapacity);
            EnsureListCapacity(_roleFormalSweepActiveItrIndices, entityCapacity);
            EnsureListCapacity(_roleFormalSweepBodyPositions, entityCapacity);
            EnsureListCapacity(_roleFormalSweepItrPositions, entityCapacity);

            EnsureListCapacity(_formalParticipants, entityCapacity);
            EnsureListCapacity(_formalParticipantHandles, entityCapacity);
            EnsureListCapacity(_formalIncrementalEntries, entityCapacity);
            EnsureListCapacity(_formalFallbackOrdinals, entityCapacity);
            EnsureListCapacity(_formalQueryHandles, entityCapacity);
            EnsureListCapacity(_formalPairKeys, pairCapacity);
            EnsureListCapacity(_formalAuthorityPairKeys, pairCapacity);
            if (_formalAuthorityPairSortScratch.Length < pairCapacity)
            {
                Array.Resize(
                    ref _formalAuthorityPairSortScratch,
                    pairCapacity);
            }
            if (_formalAuthorityPairSortCounts.Length < entityCapacity)
            {
                Array.Resize(
                    ref _formalAuthorityPairSortCounts,
                    entityCapacity);
            }
            if (_formalAuthorityPairSortOffsets.Length < entityCapacity)
            {
                Array.Resize(
                    ref _formalAuthorityPairSortOffsets,
                    entityCapacity);
            }
            _formalSlotToOrdinal.EnsureCapacity(entityCapacity);
            _formalSeenSlots.EnsureCapacity(entityCapacity);

            EnsureListCapacity(_immediateEntries, entityCapacity);
            EnsureListCapacity(_immediateQueryIndices, entityCapacity);
            EnsureListCapacity(_immediateFallbackIndices, entityCapacity);
            EnsureListCapacity(_immediateCandidateIndices, entityCapacity);
            EnsureListCapacity(_immediateTargets, entityCapacity);

            _roleFormalBroadphase.PrepareCapacity(entityCapacity);
            _formalBroadphase.PrepareCapacity(entityCapacity);
            _immediateBroadphase.PrepareCapacity(entityCapacity);
            PrewarmCollisionSorters();
        }

        private void PrewarmCollisionSorters()
        {
            if (_immediateCandidateIndices.Count == 0)
            {
                _immediateCandidateIndices.Add(1);
                _immediateCandidateIndices.Add(0);
                _immediateCandidateIndices.Sort();
                _immediateCandidateIndices.Clear();
            }

            if (_formalPairKeys.Count == 0)
            {
                _formalPairKeys.Add(1L);
                _formalPairKeys.Add(0L);
                SortAndDeduplicate(_formalPairKeys);
                _formalPairKeys.Clear();
            }

            if (_roleFormalSweepEvents.Count == 0)
            {
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    1,
                    RoleAwareSweepEventKind.BodyStart,
                    1));
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    0,
                    RoleAwareSweepEventKind.BodyEnd,
                    0));
                _roleFormalSweepEvents.Sort(RoleAwareSweepEventComparer.Instance);
                _roleFormalSweepEvents.Clear();
            }
        }

        private static void EnsureListCapacity<T>(List<T> values, int capacity)
        {
            if (values.Capacity < capacity)
                values.Capacity = capacity;
        }

        internal void ResetFormalSpatialBroadphase()
        {
            _formalBroadphase.ResetIncremental();
            _roleFormalBroadphase.ResetIncremental();
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
                TryGetCollisionCandidateRange(
                    exclude,
                    out CollisionCandidateRange cachedCandidates))
            {
                for (int i = 0; i < cachedCandidates.Count; i++)
                {
                    if (!cachedCandidates.TryGet(i, out SceneQueryHit hit))
                        continue;
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
                    TryGetCollisionCandidateRange(
                        attacker,
                        out CollisionCandidateRange cached))
                {
                    int itrIndex = ResolveItrIndex(attackerFrame, itr);
                    LF2FrameData attackerCollisionFrame = attacker.GetCollisionFrameData();
                    for (int i = 0; i < cached.Count; i++)
                    {
                        if (!cached.TryGet(i, out SceneQueryHit hit))
                            continue;
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

            if (_consumeCandidateCache)
            {
                if (!TryGetCollisionCandidateRange(
                        attacker,
                        out CollisionCandidateRange cached))
                {
                    return _tmpHitResult;
                }

                int itrIndex = ResolveItrIndex(attackerFrame, itr);
                for (int i = 0; i < cached.Count; i++)
                {
                    if (!cached.TryGet(i, out SceneQueryHit hit) ||
                        hit.ItrIndex != itrIndex)
                    {
                        continue;
                    }

                    LF2Entity target = hit.ResolveCurrentTarget(_world);
                    if (target == null || target == attacker ||
                        target.PS == null || IsPendingFlushDestroy(target))
                    {
                        continue;
                    }

                    LF2FrameData targetCollisionFrame = target.GetCollisionFrameData();
                    if (targetCollisionFrame == null ||
                        !HitsTarget(
                            volume,
                            target,
                            targetCollisionFrame,
                            out int bodyX))
                    {
                        continue;
                    }

                    InteractionArea runtimeItr = ResolveRuntimeItrForPair(
                        attacker,
                        target,
                        attackerCollisionFrame,
                        itr,
                        out bool zeroAttackerHpOnConsume,
                        out bool releaseHeavyHeldTargetOnConsume);
                    if (runtimeItr == null ||
                        IsReleaseConsumerPairBlocked(attacker, target) ||
                        !RuntimeConsumeItrAllowed(attacker, runtimeItr, target))
                    {
                        continue;
                    }

                    _tmpHitResult.Add(new SceneQueryHit(
                        target,
                        bodyX,
                        hit.ItrIndex,
                        runtimeItr,
                        zeroAttackerHpOnConsume,
                        releaseHeavyHeldTargetOnConsume));
                }

                return _tmpHitResult;
            }

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
            _candidateStoreShadow.AbortBuild();
            InvalidateCollisionCandidateRanges();
            CollisionRoleZeroItrFastPathParticipantCountForDiagnostics = 0;
            _collisionRoleZeroItrEarlyReturnAppliedForCurrentTick = false;
            int currentTick = _world?.CurrentTickIndex ?? 0;
            FreezeCollisionCandidateProducerMode(currentTick);
#if UNITY_INCLUDE_TESTS
            _collisionCandidateStoreWriteCountForSelfCheck = 0;
            _collisionCandidateStoreFaultInjectedForSelfCheck = false;
#endif
            BattleTickDetailPhaseDiagnostics cacheSetupDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (cacheSetupDiagnostics != null)
            {
                cacheSetupDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectCacheSetup);
            }

            CollisionFormalCollectorMode collectorMode;
            bool candidateListCapacityRejected = false;
            try
            {
                ReleaseCandidateListsToPool();
                _roleFormalExactItrRects.Clear();
                _roleFormalExactBodyRects.Clear();
                _consumeCandidateCache = false;
                _formalFallbackParticipantCount = 0;
                _formalCollectionAborted = false;
                _lastFormalPairCount = 0;
                _lastRoleAwareParticipantCount = 0;
                _lastRoleAwareInertParticipantCount = 0;
                _lastRoleAwareBodyEntryCount = 0;
                _lastRoleAwareItrQueryCount = 0;
                _lastRoleAwareBodyTemplateBuildCount = 0;
                _lastRoleAwareBodyTemplateHitCount = 0;
                _lastRoleAwareBodyTemplateFallbackCount = 0;
                _lastRoleAwareDirectComparisonCount = 0;
                _lastRoleAwareDirectCost = 0;
                _lastRoleAwareDirectCostAvailable = false;
                _lastRoleAwareDirectTickCount = 0;
                _lastRoleAwareTreeTickCount = 0;
                _lastRoleAwareNestedDirectTickCount = 0;
                _lastRoleAwareSweepDirectTickCount = 0;
                _lastRoleAwareSweepXCandidateCount = 0;
                _lastRoleAwareSweepFullOverlapCheckCount = 0;
                _lastRoleAwareCheapInputValidationCount = 0;
                _lastRoleAwareFullInputValidationCount = 0;
                _lastRoleAwareExactItrRectBuildCount = 0;
                _lastRoleAwareExactBodyRectBuildCount = 0;
                _lastRoleAwareExactDirectionCount = 0;
                _lastRoleAwareExactItrVisitCount = 0;
                _lastRoleAwareExactBodyOverlapCheckCount = 0;
                FormalSpatialSynchronizeResult = default;
                collectorMode = ResolveFormalCollectorMode();
                LastFormalCollectorModeForDiagnostics = collectorMode;

                _world.GetAllEntities(_tmpAllObjects);
                ResetCandidateCollectionState();

                for (int i = 0; i < _tmpAllObjects.Count; i++)
                {
                    LF2Entity attacker = _tmpAllObjects[i];
                    if (!IsCollisionCandidateAttackerEligible(attacker, currentTick))
                        continue;

                    if (LegacyCandidateListsEnabledForCurrentTick)
                    {
                        List<SceneQueryHit> candidates = RentCandidateList();
                        if (candidates == null)
                        {
                            candidateListCapacityRejected = true;
                            break;
                        }

                        _candidateCache[attacker] = candidates;
                        RecordLegacyCandidateListTouchForAuthority();
                    }
                }
            }
            finally
            {
                if (cacheSetupDiagnostics != null)
                {
                    cacheSetupDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectCacheSetup);
                }
            }

            if (candidateListCapacityRejected)
            {
                _formalCollectionAborted = true;
                ReleaseCandidateListsToPool();
                ResetCandidateCollectionState();
                _candidateStoreShadow.AbortBuild();
                _consumeCandidateCache = true;
                return;
            }

            BeginCollisionCandidateStoreShadowBuild();

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

                if (_collisionRoleZeroItrEarlyReturnAppliedForCurrentTick)
                    return;

                if (!formalSucceeded)
                {
                    _formalCollectionAborted = true;
                    _world.Rng.RestoreState(rngStateBeforeFormal, rngCallsBeforeFormal);
                    BattleTickDetailPhaseDiagnostics fallbackCacheDiagnostics =
                        _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                    if (fallbackCacheDiagnostics != null)
                    {
                        fallbackCacheDiagnostics.BeginPhase(
                            BattleTickDetailPhase.CandidateCollectCacheSetup);
                    }

                    try
                    {
                        ResetCandidateCollectionState();
                    }
                    finally
                    {
                        if (fallbackCacheDiagnostics != null)
                        {
                            fallbackCacheDiagnostics.EndPhase(
                                BattleTickDetailPhase.CandidateCollectCacheSetup);
                        }
                    }
                    RestartCollisionCandidateStoreShadowBuild();
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

#if UNITY_INCLUDE_TESTS
            BeforeCollisionCandidateStoreFinalCompareForSelfCheck?.Invoke();
#endif
            CompareAndCompleteCollisionCandidateStoreShadow();

            LockCollisionCandidateConsumptionSource();
            _consumeCandidateCache = true;
        }

        private bool CollisionCandidateStoreBuildRequestedForCurrentTick =>
            _candidateProducerModeForCurrentTick !=
            CollisionCandidateProducerMode.LegacyOnly;

        private bool LegacyCandidateListsEnabledForCurrentTick =>
            _candidateProducerModeForCurrentTick !=
            CollisionCandidateProducerMode.StoreOnly;

        private bool CandidateStoreOracleComparisonEnabledForCurrentTick =>
            _candidateProducerModeForCurrentTick ==
            CollisionCandidateProducerMode.LegacyWithStoreShadow ||
            _candidateProducerModeForCurrentTick ==
            CollisionCandidateProducerMode.StoreWithLegacyOracle;

        private void FreezeCollisionCandidateProducerMode(int currentTick)
        {
            _candidateProducerTickForCurrentCollection = currentTick;
            _candidateStoreAuthorityRequestedForCurrentTick =
                CollisionCandidateStoreAuthorityEnabled;
            _candidateStoreAuthorityAppliedForCurrentTick = false;
            _candidateStoreProducerHealthyForCurrentTick = false;

            if (!_candidateStoreAuthorityRequestedForCurrentTick)
            {
                _candidateProducerModeForCurrentTick =
                    CollisionCandidateStoreShadowDiagnosticsEnabled
                        ? CollisionCandidateProducerMode.LegacyWithStoreShadow
                        : CollisionCandidateProducerMode.LegacyOnly;
                return;
            }

            _candidateStoreAuthorityDiagnostics.RecordRequestedTick();
            int interval = _candidateStoreLegacyOracleInterval;
            bool sampleOracle = IsCollisionCandidateLegacyOracleSampleTick(
                currentTick,
                interval);
            if (sampleOracle)
            {
                _candidateProducerModeForCurrentTick =
                    CollisionCandidateProducerMode.StoreWithLegacyOracle;
                _candidateStoreAuthorityDiagnostics.RecordSampledOracleTick();
            }
            else
            {
                _candidateProducerModeForCurrentTick =
                    CollisionCandidateProducerMode.StoreOnly;
                _candidateStoreAuthorityDiagnostics.RecordStoreOnlyTick();
            }
        }

        public static bool IsCollisionCandidateLegacyOracleSampleTick(
            int frozenTickIndex,
            int legacyOracleInterval)
        {
            return legacyOracleInterval > 0 &&
                   frozenTickIndex % legacyOracleInterval == 0;
        }

        private void RecordLegacyCandidateListTouchForAuthority()
        {
            if (_candidateStoreAuthorityRequestedForCurrentTick)
            {
                _candidateStoreAuthorityDiagnostics
                    .RecordLegacyListCreatedOrWritten();
            }
        }

        private bool IsCollisionCandidateAttackerEligible(
            LF2Entity attacker,
            int currentTick)
        {
            if (attacker == null || attacker.PS == null ||
                IsPendingFlushDestroy(attacker) ||
                IsCollisionCandidateSuppressed(attacker, currentTick) ||
                GetAuthoredCurrentFrame(attacker) == null)
            {
                return false;
            }

            LF2FrameData attackerFrame = attacker.GetCollisionFrameData();
            return attackerFrame?.itrs != null && attackerFrame.itrs.Count > 0;
        }

        private void BeginCollisionCandidateStoreShadowBuild()
        {
            if (!CollisionCandidateStoreBuildRequestedForCurrentTick)
                return;

            try
            {
                if (!_candidateStoreShadow.BeginBuild(_world.MaxRuntimeSlotsForServices))
                    return;
                if (!InitializeCollisionCandidateStoreAttackerCarriers())
                {
                    _candidateStoreShadow.AbortBuild();
                    return;
                }

                _candidateStoreProducerHealthyForCurrentTick = true;
            }
            catch (Exception)
            {
                _candidateStoreShadow.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.UnexpectedShadowException);
                _candidateStoreShadow.AbortBuild();
            }
        }

        private void RestartCollisionCandidateStoreShadowBuild()
        {
            if (!CollisionCandidateStoreBuildRequestedForCurrentTick)
                return;

            _candidateStoreShadow.AbortBuild();
            _candidateStoreProducerHealthyForCurrentTick = false;
            BeginCollisionCandidateStoreShadowBuild();
        }

        private bool InitializeCollisionCandidateStoreAttackerCarriers()
        {
            int initializedCount = 0;
            int eligibleAttackerCount = 0;
            int runtimeCapacity = _world.MaxRuntimeSlotsForServices;
            for (int runtimeSlot = 0; runtimeSlot < runtimeCapacity; runtimeSlot++)
            {
                LF2Entity attacker = _world.FindEntityByRuntimeSlotForQuery(runtimeSlot);
                if (!IsCollisionCandidateAttackerEligible(
                        attacker,
                        _candidateProducerTickForCurrentCollection))
                {
                    continue;
                }

                eligibleAttackerCount++;

                if (!_world.TryGetCurrentRuntimeHandle(
                        runtimeSlot,
                        attacker,
                        out RuntimeEntityHandle attackerHandle))
                {
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.AttackerHandleNotCurrent);
                    return false;
                }

                if (!_candidateStoreShadow.TryBeginAttacker(attackerHandle))
                    return false;
                initializedCount++;
            }

            if (initializedCount == eligibleAttackerCount)
                return true;

            _candidateStoreShadow.RecordInvalid(
                CollisionCandidateStoreMismatchReason.AttackerHandleNotCurrent);
            return false;
        }

        private void CompareAndCompleteCollisionCandidateStoreShadow()
        {
            if (!CollisionCandidateStoreBuildRequestedForCurrentTick ||
                !_candidateStoreShadow.IsBuilding)
            {
                return;
            }

            try
            {
                if (CandidateStoreOracleComparisonEnabledForCurrentTick &&
                    !CompareCandidateCacheToStoreByRuntimeSlot())
                {
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                    return;
                }

                if (!_candidateStoreShadow.CompleteBuild())
                {
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.UnexpectedShadowException);
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                }
            }
            catch (Exception)
            {
                _candidateStoreShadow.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.UnexpectedShadowException);
                _candidateStoreShadow.AbortBuild();
                _candidateStoreProducerHealthyForCurrentTick = false;
            }
        }

        private void LockCollisionCandidateConsumptionSource()
        {
            _candidateConsumptionSource = CollisionCandidateConsumptionSource.LegacyOracle;
            if (!_candidateStoreAuthorityRequestedForCurrentTick)
                return;

            if (_candidateStoreShadow.IsVisible)
            {
                _candidateConsumptionSource = CollisionCandidateConsumptionSource.StoreAuthority;
                _candidateStoreAuthorityAppliedForCurrentTick = true;
                _candidateStoreAuthorityDiagnostics.RecordAppliedTick();
                return;
            }

            if (_candidateProducerModeForCurrentTick ==
                CollisionCandidateProducerMode.StoreOnly)
            {
                _candidateConsumptionSource =
                    CollisionCandidateConsumptionSource.StoreAuthorityFailedClosed;
                _candidateStoreAuthorityDiagnostics.RecordStoreOnlyHardFailure();
                _candidateStoreAuthorityDiagnostics.RecordFailure(
                    CollisionCandidateStoreAuthorityFailureReason
                        .StoreOnlyProducerUnavailable);
                return;
            }

            _candidateStoreAuthorityDiagnostics.RecordLegacyFallbackTick();
            _candidateStoreAuthorityDiagnostics.RecordFailure(
                CollisionCandidateStoreAuthorityFailureReason.StoreNotComplete);
        }

        private bool CompareCandidateCacheToStoreByRuntimeSlot()
        {
            bool matches = true;
            int comparedAttackerCount = 0;
            int runtimeCapacity = _world.MaxRuntimeSlotsForServices;
            for (int runtimeSlot = 0; runtimeSlot < runtimeCapacity; runtimeSlot++)
            {
                LF2Entity attacker = _world.FindEntityByRuntimeSlotForQuery(runtimeSlot);
                if (attacker == null ||
                    !_candidateCache.TryGetValue(attacker, out List<SceneQueryHit> hits))
                {
                    continue;
                }

                comparedAttackerCount++;
                _candidateStoreShadow.RecordComparedAttacker();
                if (!_world.TryGetCurrentRuntimeHandle(
                        runtimeSlot,
                        attacker,
                        out RuntimeEntityHandle attackerHandle))
                {
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.AttackerHandleNotCurrent);
                    matches = false;
                    continue;
                }

                if (!_candidateStoreShadow.TryGetBuildingAttackerRowForCompare(
                        attackerHandle,
                        out int storeCount))
                {
                    _candidateStoreShadow.RecordMismatch(
                        CollisionCandidateStoreMismatchReason.AttackerRowMissing);
                    matches = false;
                    continue;
                }

                int authorityCount = attacker.Runtime?.HitCandidateCount ?? hits?.Count ?? 0;
                int authorityListCount = hits?.Count ?? 0;
                if (storeCount != authorityCount || authorityListCount != authorityCount)
                {
                    _candidateStoreShadow.RecordMismatch(
                        CollisionCandidateStoreMismatchReason.CandidateCountMismatch);
                    matches = false;
                }

                int compareCount = storeCount < authorityCount
                    ? storeCount
                    : authorityCount;
                if (compareCount > authorityListCount)
                    compareCount = authorityListCount;
                for (int candidateIndex = 0;
                     candidateIndex < compareCount;
                     candidateIndex++)
                {
                    _candidateStoreShadow.RecordComparedCandidate();
                    if (!_candidateStoreShadow.TryGetBuildingCandidateForCompare(
                            attackerHandle,
                            candidateIndex,
                            out CollisionCandidateStoreEntry entry))
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.CandidateCountMismatch);
                        matches = false;
                        continue;
                    }

                    SceneQueryHit hit = hits[candidateIndex];
                    if (entry.TargetSlot != hit.TargetSlot)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.TargetSlotMismatch);
                        matches = false;
                    }

                    RuntimeEntityHandle currentTargetHandle = RuntimeEntityHandle.Invalid;
                    if (hit.TargetSlot >= 0 && hit.TargetSlot < runtimeCapacity)
                    {
                        _world.TryGetCurrentRuntimeHandle(
                            hit.TargetSlot,
                            hit.Target,
                            out currentTargetHandle);
                    }
                    if (entry.TargetHandle != currentTargetHandle)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.TargetHandleSnapshotMismatch);
                        matches = false;
                    }
                    if (entry.BodyX != hit.BodyX)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.BodyXMismatch);
                        matches = false;
                    }
                    if (entry.ItrIndex != hit.ItrIndex)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.ItrIndexMismatch);
                        matches = false;
                    }
                    if (!ReferenceEquals(entry.RuntimeItr, hit.RuntimeItr))
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.RuntimeItrIdentityMismatch);
                        matches = false;
                    }
                    if (entry.ZeroAttackerHpOnConsume !=
                        hit.ZeroAttackerHpOnConsume)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.ZeroAttackerHpOnConsumeMismatch);
                        matches = false;
                    }
                    if (entry.ReleaseHeavyHeldTargetOnConsume !=
                        hit.ReleaseHeavyHeldTargetOnConsume)
                    {
                        _candidateStoreShadow.RecordMismatch(
                            CollisionCandidateStoreMismatchReason.ReleaseHeavyHeldTargetOnConsumeMismatch);
                        matches = false;
                    }
                }
            }

            if (comparedAttackerCount != _candidateCache.Count)
            {
                _candidateStoreShadow.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.AttackerHandleNotCurrent);
                matches = false;
            }
            return matches;
        }

        private CollisionFormalCollectorMode ResolveFormalCollectorMode()
        {
            if (FormalCollectorMode != CollisionFormalCollectorMode.Configured)
                return FormalCollectorMode;

            return _collisionBroadphase == CollisionBroadphaseBackend.LooseQuadtree
                ? CollisionFormalCollectorMode.ForceRoleAware
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
            BattleTickDetailPhaseDiagnostics pairLoopDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (pairLoopDiagnostics != null)
            {
                pairLoopDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectPairExactLoop);
            }

            try
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
            finally
            {
                if (pairLoopDiagnostics != null)
                {
                    pairLoopDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectPairExactLoop);
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
            ulong roleAwareOccupancyEpoch =
                _world.RuntimeSlotOccupancyEpochForServices;
            int roleFormalBodyEntryCount = 0;
            bool directInputSafe = true;
            BattleTickDetailPhaseDiagnostics participantBuildDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (participantBuildDiagnostics != null)
            {
                participantBuildDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectParticipantBodyItrBuild);
            }

            try
            {
                _roleFormalParticipants.Clear();
                _roleFormalBodyTemplates.Clear();
                _roleFormalItrEntries.Clear();
                _roleFormalExactItrRects.Clear();
                _roleFormalExactBodyRects.Clear();
                _roleFormalExactRequiredRoles.Clear();
                _roleFormalBodyOrdinals.Clear();
                _roleFormalFallbackAttackOrdinals.Clear();
                _roleFormalExactAttackOrdinals.Clear();
                _roleFormalFallbackBodyOrdinals.Clear();
#if UNITY_INCLUDE_TESTS
                _roleFormalExactCommonBuildCounts.Clear();
                _roleFormalExactAttackBuildCounts.Clear();
                _roleFormalExactBodyBuildCounts.Clear();
                _roleFormalExactValidationCounts.Clear();
#endif
                _formalIncrementalEntries.Clear();
                _formalQueryHandles.Clear();
                _formalAuthorityPairKeys.Clear();
                _formalSlotToOrdinal.Clear();
                _formalSeenSlots.Clear();

                for (int authorityOrdinal = 0;
                     authorityOrdinal < _tmpAllObjects.Count;
                     authorityOrdinal++)
                {
                    LF2Entity entity = _tmpAllObjects[authorityOrdinal];
                    if (entity == null || entity.PS == null ||
                        IsPendingFlushDestroy(entity))
                    {
                        continue;
                    }
                    if (IsCollisionCandidateSuppressed(entity, currentTick))
                        continue;

                    int runtimeSlot = entity.Runtime?.SlotIndex ?? -1;
                    if (runtimeSlot < 0 ||
                        runtimeSlot >= _world.MaxRuntimeSlotsForServices ||
                        !ReferenceEquals(
                            _world.FindEntityByRuntimeSlotForQuery(runtimeSlot),
                            entity) ||
                        !_formalSeenSlots.Add(runtimeSlot) ||
                        !_world.TryGetCurrentRuntimeHandle(
                            runtimeSlot,
                            entity,
                            out RuntimeEntityHandle handle))
                    {
                        return AbortFormalSpatialIndex();
                    }

                    LF2FrameData currentFrame = GetAuthoredCurrentFrame(entity);
                    LF2FrameData collisionFrame = entity.GetCollisionFrameData();
                    var participant = new RoleAwareFormalParticipant(
                        entity,
                        currentFrame,
                        collisionFrame,
                        handle);
                    int participantOrdinal = _roleFormalParticipants.Count;

                    BuildRoleAwareFormalBodyState(
                        ref participant,
                        ref roleFormalBodyEntryCount,
                        out bool hasIndexableBody,
                        out SpatialAabbXZ indexableBodyBounds);
                    if (hasIndexableBody)
                    {
                        _formalIncrementalEntries.Add(new IncrementalSpatialEntry(
                            handle,
                            indexableBodyBounds));
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
                            if (TryBuildFormalItrAabb(
                                    entity,
                                    collisionFrame,
                                    itr,
                                    out SpatialAabbXZ itrBounds))
                            {
                                _roleFormalItrEntries.Add(new RoleAwareFormalItrEntry(
                                    participantOrdinal,
                                    handle,
                                    itrIndex,
                                    itr,
                                    itrBounds));
                            }
                            else
                            {
                                participant.HasFallbackAttackItr = true;
                            }
                        }
                    }

                    AddRoleAwareFormalParticipantOrdinals(
                        participantOrdinal,
                        in participant);
                    if (participant.HasFallbackAttackItr ||
                        participant.HasFallbackBody)
                    {
                        directInputSafe = false;
                    }
                    _formalSlotToOrdinal.Add(runtimeSlot, participantOrdinal);
                    _roleFormalParticipants.Add(participant);
                }
            }
            finally
            {
                if (participantBuildDiagnostics != null)
                {
                    participantBuildDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectParticipantBodyItrBuild);
                }
            }

            int participantCount = _roleFormalParticipants.Count;
            if (TryCompleteCollisionRoleZeroItrFastPath(
                    participantCount,
                    roleFormalBodyEntryCount))
            {
                return true;
            }

            InitializeRoleAwareFormalExactTracking(participantCount);
            _lastRoleAwareParticipantCount = participantCount;
            _lastRoleAwareInertParticipantCount =
                CountRoleAwareFormalInertParticipants();
            BattleTickDetailPhaseDiagnostics inputValidationDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (inputValidationDiagnostics != null)
            {
                inputValidationDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectInputValidation);
            }

            bool inputsValid;
            try
            {
                bool runFullInputValidation =
                    ForceFullRoleAwareFormalInputValidationForDiagnostics;
#if UNITY_INCLUDE_TESTS
                Action beforeInputValidation =
                    BeforeRoleAwareFormalInputValidationForSelfCheck;
                if (beforeInputValidation != null)
                {
                    beforeInputValidation.Invoke();
                    runFullInputValidation = true;
                }
#endif
                // Production reaches this gate synchronously from the construction
                // loops above, with no callback or await between them. The occupancy
                // epoch protects slot membership changes; it intentionally cannot
                // detect same-slot frame/geometry mutation. Test hooks and explicit
                // diagnostics opt into the full geometry revalidation below.
                _lastRoleAwareCheapInputValidationCount++;
                bool cheapInputsValid =
                    TryValidateRoleAwareFormalBroadphaseInputsCheap(
                        participantCount,
                        roleAwareOccupancyEpoch);
                bool fullInputsValid = true;
                if (runFullInputValidation)
                {
                    _lastRoleAwareFullInputValidationCount++;
                    fullInputsValid =
                        TryValidateRoleAwareFormalBroadphaseInputsFull(
                            out bool fullyValidatedDirectInputSafe) &&
                        fullyValidatedDirectInputSafe == directInputSafe;
                }

                inputsValid = cheapInputsValid && fullInputsValid;
            }
            finally
            {
                if (inputValidationDiagnostics != null)
                {
                    inputValidationDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectInputValidation);
                }
            }

            if (!inputsValid)
            {
                return AbortFormalSpatialIndex();
            }

            long directCost =
                (long)_roleFormalItrEntries.Count *
                _formalIncrementalEntries.Count;
            _lastRoleAwareDirectCost = directCost;
            _lastRoleAwareDirectCostAvailable = true;
            // Explicit tree diagnostics win over every direct diagnostic switch.
            bool useDirect =
                directInputSafe &&
                !ForceRoleAwareTreeForDiagnostics &&
                (ForceRoleAwareDirectForDiagnostics ||
                 ForceRoleAwareNestedDirectForDiagnostics ||
                 ForceRoleAwareSweepDirectForDiagnostics ||
                 directCost <= RoleAwareDirectComparisonThreshold);
            bool useSweepDirect =
                useDirect && ShouldUseRoleAwareSweepDirect(directCost);

            try
            {
                if (useDirect)
                {
                    BattleTickDetailPhaseDiagnostics directDiagnostics =
                        _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                    if (directDiagnostics != null)
                    {
                        directDiagnostics.BeginPhase(
                            BattleTickDetailPhase.CandidateCollectDirectBroadphase);
                    }

                    try
                    {
                        _lastRoleAwareDirectTickCount = 1;
                        _totalRoleAwareDirectTickCount =
                            SaturatingIncrement(_totalRoleAwareDirectTickCount);
                        if (useSweepDirect)
                        {
                            _lastRoleAwareSweepDirectTickCount = 1;
                            _totalRoleAwareSweepDirectTickCount =
                                SaturatingIncrement(
                                    _totalRoleAwareSweepDirectTickCount);
                            CollectRoleAwareSweepDirectBroadphasePairs();
                        }
                        else
                        {
                            _lastRoleAwareNestedDirectTickCount = 1;
                            _totalRoleAwareNestedDirectTickCount =
                                SaturatingIncrement(
                                    _totalRoleAwareNestedDirectTickCount);
                            CollectRoleAwareNestedDirectBroadphasePairs();
                        }
                    }
                    finally
                    {
                        if (directDiagnostics != null)
                        {
                            directDiagnostics.EndPhase(
                                BattleTickDetailPhase.CandidateCollectDirectBroadphase);
                        }
                    }
                }
                else
                {
                    BattleTickDetailPhaseDiagnostics treeDiagnostics =
                        _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                    if (treeDiagnostics != null)
                    {
                        treeDiagnostics.BeginPhase(
                            BattleTickDetailPhase.CandidateCollectTreeBroadphase);
                    }

                    try
                    {
                        _lastRoleAwareTreeTickCount = 1;
                        _totalRoleAwareTreeTickCount =
                            SaturatingIncrement(_totalRoleAwareTreeTickCount);
                        BattleStageRuntimeState stage = _world?.Runtime?.Stage;
                        int stageWidth = stage?.StageWidthPx ?? 800;
                        int zMin = stage?.ZMin ?? 180;
                        int zMax = stage?.ZMax ?? 350;
                        var preferredRoot = new SpatialAabbXZ(
                            0,
                            zMin,
                            stageWidth > 0 ? stageWidth : 1,
                            zMax > zMin ? zMax : zMin + 1);
                        FormalSpatialSynchronizeResult =
                            _roleFormalBroadphase.Synchronize(
                                _formalIncrementalEntries,
                                preferredRoot);
                        if (!FormalSpatialSynchronizeResult.Succeeded ||
                            FormalSpatialSynchronizeResult.IndexedCount !=
                            _formalIncrementalEntries.Count)
                        {
                            return AbortFormalSpatialIndex();
                        }

                        for (int itrEntryIndex = 0;
                             itrEntryIndex < _roleFormalItrEntries.Count;
                             itrEntryIndex++)
                        {
                            RoleAwareFormalItrEntry itrEntry =
                                _roleFormalItrEntries[itrEntryIndex];
                            _roleFormalBroadphase.QueryHandles(
                                itrEntry.Bounds,
                                _formalQueryHandles);
                            for (int resultIndex = 0;
                                 resultIndex < _formalQueryHandles.Count;
                                 resultIndex++)
                            {
                                RuntimeEntityHandle bodyHandle =
                                    _formalQueryHandles[resultIndex];
                                if (!_formalSlotToOrdinal.TryGetValue(
                                        bodyHandle.Slot,
                                        out int bodyParticipantOrdinal) ||
                                    !TryValidateRoleAwareParticipant(
                                        bodyParticipantOrdinal,
                                        bodyHandle,
                                        out _))
                                {
                                    return AbortFormalSpatialIndex();
                                }

                                AddAuthorityOrdinalPair(
                                    itrEntry.ParticipantOrdinal,
                                    bodyParticipantOrdinal);
                            }
                        }
                    }
                    finally
                    {
                        if (treeDiagnostics != null)
                        {
                            treeDiagnostics.EndPhase(
                                BattleTickDetailPhase.CandidateCollectTreeBroadphase);
                        }
                    }
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }

            BattleTickDetailPhaseDiagnostics fallbackPairDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (fallbackPairDiagnostics != null)
            {
                fallbackPairDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectFallbackPairAdd);
            }

            try
            {
                CollectRoleAwareFormalFallbackPairs();
            }
            finally
            {
                if (fallbackPairDiagnostics != null)
                {
                    fallbackPairDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectFallbackPairAdd);
                }
            }

            BattleTickDetailPhaseDiagnostics sortDiagnostics =
                _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            if (sortDiagnostics != null)
            {
                sortDiagnostics.BeginPhase(
                    BattleTickDetailPhase.CandidateCollectSortDeduplicate);
            }

            try
            {
                SortAndDeduplicateAuthorityOrdinalPairs(
                    _formalAuthorityPairKeys,
                    participantCount);
            }
            finally
            {
                if (sortDiagnostics != null)
                {
                    sortDiagnostics.EndPhase(
                        BattleTickDetailPhase.CandidateCollectSortDeduplicate);
                }
            }
            if (roleAwareOccupancyEpoch !=
                _world.RuntimeSlotOccupancyEpochForServices)
            {
                return AbortFormalSpatialIndex();
            }

            if (!TryPrepareRoleAwareFormalParticipantRoleFlags(participantCount) ||
                !TryCollectRoleAwareFormalExactRequirements(participantCount) ||
                !TryBuildRequiredRoleAwareFormalExactCaches(participantCount) ||
                roleAwareOccupancyEpoch !=
                    _world.RuntimeSlotOccupancyEpochForServices)
            {
                return AbortFormalSpatialIndex();
            }
            if (!TryPrepareRoleAwareFormalParticipantReadBuffer(participantCount))
                return AbortFormalSpatialIndex();

            _formalFallbackParticipantCount = CountRoleAwareFormalFallbackParticipants();
            _lastFormalPairCount = _formalAuthorityPairKeys.Count;
            _lastRoleAwareBodyEntryCount = roleFormalBodyEntryCount;
            _lastRoleAwareItrQueryCount = _roleFormalItrEntries.Count;

            try
            {
                BattleTickDetailPhaseDiagnostics pairLoopDiagnostics =
                    _world?.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
                if (pairLoopDiagnostics != null)
                {
                    pairLoopDiagnostics.BeginPhase(
                        BattleTickDetailPhase.CandidateCollectPairExactLoop);
                }

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

                        ref readonly RoleAwareFormalParticipant firstParticipant =
                            ref _roleFormalParticipantReadBuffer[firstOrdinal];
                        ref readonly RoleAwareFormalParticipant secondParticipant =
                            ref _roleFormalParticipantReadBuffer[secondOrdinal];
                        LF2Entity first = firstParticipant.Entity;
                        LF2Entity second = secondParticipant.Entity;
#if UNITY_INCLUDE_TESTS
                        if (ForceRoleAwareExactCacheRevalidationForDiagnostics &&
                            (!RoleAwareFormalExactCacheIsCurrent(
                                 in firstParticipant,
                                 _roleFormalExactRequiredRoles[firstOrdinal]) ||
                             !RoleAwareFormalExactCacheIsCurrent(
                                 in secondParticipant,
                                 _roleFormalExactRequiredRoles[secondOrdinal])))
                        {
                            return AbortFormalSpatialIndex();
                        }
                        if (ForceLegacyPerPairValidationForDiagnostics &&
                            (!TryValidateRoleAwareParticipant(
                                 firstOrdinal,
                                 firstParticipant.Handle,
                                 out first) ||
                             !TryValidateRoleAwareParticipant(
                                 secondOrdinal,
                                 secondParticipant.Handle,
                                 out second)))
                        {
                            return AbortFormalSpatialIndex();
                        }
#endif

                        if (firstParticipant.HasAttackItr &&
                            secondParticipant.HasBody)
                        {
#if UNITY_INCLUDE_TESTS
                            if (ForceLegacyRoleAwareExactPrefilterForDiagnostics)
                                CollectCandidatesForPair(first, second);
                            else
#endif
                                CollectCandidatesForRoleAwareFormalDirection(
                                    in firstParticipant,
                                    in secondParticipant);
                        }
                        if (secondParticipant.HasAttackItr &&
                            firstParticipant.HasBody)
                        {
#if UNITY_INCLUDE_TESTS
                            if (ForceLegacyRoleAwareExactPrefilterForDiagnostics)
                                CollectCandidatesForPair(second, first);
                            else
#endif
                                CollectCandidatesForRoleAwareFormalDirection(
                                    in secondParticipant,
                                    in firstParticipant);
                        }
#if UNITY_INCLUDE_TESTS
                        AfterRoleAwareExactPairForSelfCheck?.Invoke(pairIndex);
                        if (ThrowAfterRoleAwareFormalPairCountForSelfCheck >= 0 &&
                            pairIndex + 1 >= ThrowAfterRoleAwareFormalPairCountForSelfCheck)
                        {
                            throw new InvalidOperationException(
                                "Forced role-aware formal collector self-check failure.");
                        }
#endif
                    }
                }
                finally
                {
                    if (pairLoopDiagnostics != null)
                    {
                        pairLoopDiagnostics.EndPhase(
                            BattleTickDetailPhase.CandidateCollectPairExactLoop);
                    }
                }
            }
            catch (Exception)
            {
                return AbortFormalSpatialIndex();
            }
            finally
            {
                Array.Clear(
                    _roleFormalParticipantReadBuffer,
                    0,
                    participantCount);
            }

            if (roleAwareOccupancyEpoch !=
                _world.RuntimeSlotOccupancyEpochForServices)
            {
                return AbortFormalSpatialIndex();
            }

            return true;
        }

        private bool TryPrepareRoleAwareFormalParticipantReadBuffer(
            int participantCount)
        {
            if (participantCount > _roleFormalParticipantReadBuffer.Length)
            {
                SimulationRuntimeCapacityModule runtimeCapacity =
                    _world?.RuntimeCapacity;
                if (runtimeCapacity != null &&
                    !runtimeCapacity.TryAuthorizeGrowth())
                {
                    return false;
                }

                int nextCapacity = Math.Max(
                    participantCount,
                    checked(_roleFormalParticipantReadBuffer.Length * 2));
                Array.Resize(
                    ref _roleFormalParticipantReadBuffer,
                    nextCapacity);
            }

            _roleFormalParticipants.CopyTo(
                _roleFormalParticipantReadBuffer,
                0);
            return true;
        }

        private bool TryCompleteCollisionRoleZeroItrFastPath(
            int participantCount,
            int bodyEntryCount)
        {
            if (!CollisionRoleZeroItrFastPathEnabled)
                return false;

            CollisionRoleZeroItrFastPathRequestedCountForDiagnostics++;
            if (_roleFormalItrEntries.Count != 0)
            {
                CollisionRoleZeroItrFastPathFallbackCountForDiagnostics++;
                return false;
            }

            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (participant.HasAttackItr || participant.HasFallbackAttackItr)
                {
                    CollisionRoleZeroItrFastPathFallbackCountForDiagnostics++;
                    return false;
                }
            }

            if (_candidateProducerModeForCurrentTick != CollisionCandidateProducerMode.StoreOnly ||
                ShadowBroadphaseDiagnosticsEnabled ||
                CollisionCandidateStoreShadowDiagnosticsEnabled ||
                ForceLegacyRoleBodyBuildForDiagnostics ||
                ForceRoleAwareTreeForDiagnostics ||
                ForceRoleAwareDirectForDiagnostics ||
                ForceRoleAwareNestedDirectForDiagnostics ||
                ForceRoleAwareSweepDirectForDiagnostics ||
                ForceFullRoleAwareFormalInputValidationForDiagnostics)
            {
                CollisionRoleZeroItrFastPathFallbackCountForDiagnostics++;
                return false;
            }
#if UNITY_INCLUDE_TESTS
            if (BeforeRoleAwareFormalInputValidationForSelfCheck != null ||
                BeforeCollisionCandidateStoreFinalCompareForSelfCheck != null ||
                ForceLegacyPerPairValidationForDiagnostics ||
                ForceLegacyRoleAwareExactPrefilterForDiagnostics ||
                ForceRoleAwareExactCacheRevalidationForDiagnostics)
            {
                CollisionRoleZeroItrFastPathFallbackCountForDiagnostics++;
                return false;
            }
#endif

            CollisionRoleZeroItrFastPathZeroItrCountForDiagnostics++;
            CompareAndCompleteCollisionCandidateStoreShadow();
            if (!_candidateStoreShadow.IsVisible)
            {
                CollisionRoleZeroItrFastPathInvalidCountForDiagnostics++;
                CollisionRoleZeroItrFastPathFallbackCountForDiagnostics++;
                return false;
            }

            _lastFormalPairCount = 0;
            _lastRoleAwareParticipantCount = participantCount;
            _lastRoleAwareInertParticipantCount =
                CountRoleAwareFormalInertParticipants();
            _lastRoleAwareBodyEntryCount = bodyEntryCount;
            _lastRoleAwareItrQueryCount = 0;
            _lastRoleAwareDirectCost = 0;
            _lastRoleAwareDirectCostAvailable = true;
            CollisionRoleZeroItrFastPathParticipantCountForDiagnostics = participantCount;
            CollisionRoleZeroItrFastPathAppliedCountForDiagnostics++;
            LockCollisionCandidateConsumptionSource();
            _consumeCandidateCache = true;
            _collisionRoleZeroItrEarlyReturnAppliedForCurrentTick = true;
            return true;
        }

        private bool TryCollectRoleAwareFormalExactRequirements(
            int participantCount)
        {
            if (_roleFormalExactRequiredRoles.Count != participantCount)
                return false;

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
                    return false;
                }

                byte firstRoleFlags =
                    _roleFormalParticipantRoleFlags[firstOrdinal];
                byte secondRoleFlags =
                    _roleFormalParticipantRoleFlags[secondOrdinal];
                AddRoleAwareFormalExactDirectionRequirement(
                    firstOrdinal,
                    firstRoleFlags,
                    secondOrdinal,
                    secondRoleFlags);
                AddRoleAwareFormalExactDirectionRequirement(
                    secondOrdinal,
                    secondRoleFlags,
                    firstOrdinal,
                    firstRoleFlags);
            }

            return true;
        }

        private void AddRoleAwareFormalExactDirectionRequirement(
            int attackerOrdinal,
            byte attackerRoleFlags,
            int targetOrdinal,
            byte targetRoleFlags)
        {
            if ((attackerRoleFlags &
                    RoleAwareFormalParticipantHasAttackItr) == 0 ||
                (attackerRoleFlags &
                    RoleAwareFormalParticipantHasFallbackAttackItr) != 0 ||
                (targetRoleFlags &
                    RoleAwareFormalParticipantHasBody) == 0 ||
                (targetRoleFlags &
                    RoleAwareFormalParticipantHasFallbackBody) != 0)
            {
                return;
            }

            _roleFormalExactRequiredRoles[attackerOrdinal] |=
                RoleAwareFormalExactAttackRole;
            _roleFormalExactRequiredRoles[targetOrdinal] |=
                RoleAwareFormalExactBodyRole;
        }

        private static byte BuildRoleAwareFormalParticipantRoleFlags(
            in RoleAwareFormalParticipant participant)
        {
            byte flags = 0;
            if (participant.HasAttackItr)
                flags |= RoleAwareFormalParticipantHasAttackItr;
            if (participant.HasFallbackAttackItr)
                flags |= RoleAwareFormalParticipantHasFallbackAttackItr;
            if (participant.HasBody)
                flags |= RoleAwareFormalParticipantHasBody;
            if (participant.HasFallbackBody)
                flags |= RoleAwareFormalParticipantHasFallbackBody;
            return flags;
        }

        private bool TryPrepareRoleAwareFormalParticipantRoleFlags(
            int participantCount)
        {
            if (participantCount > _roleFormalParticipantRoleFlags.Length)
            {
                SimulationRuntimeCapacityModule runtimeCapacity =
                    _world?.RuntimeCapacity;
                if (runtimeCapacity != null &&
                    !runtimeCapacity.TryAuthorizeGrowth())
                {
                    return false;
                }

                int nextCapacity = Math.Max(
                    participantCount,
                    checked(_roleFormalParticipantRoleFlags.Length * 2));
                Array.Resize(
                    ref _roleFormalParticipantRoleFlags,
                    nextCapacity);
            }

            for (int participantOrdinal = 0;
                 participantOrdinal < participantCount;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                _roleFormalParticipantRoleFlags[participantOrdinal] =
                    BuildRoleAwareFormalParticipantRoleFlags(
                        in participant);
            }

            return true;
        }

        private bool TryValidateRoleAwareFormalBroadphaseInputsCheap(
            int participantCount,
            ulong expectedOccupancyEpoch)
        {
            return expectedOccupancyEpoch ==
                       _world.RuntimeSlotOccupancyEpochForServices &&
                   participantCount == _roleFormalParticipants.Count &&
                   participantCount == _formalSlotToOrdinal.Count &&
                   participantCount == _formalSeenSlots.Count &&
                   participantCount == _roleFormalExactRequiredRoles.Count &&
                   _roleFormalBodyOrdinals.Count <= participantCount &&
                   _roleFormalFallbackBodyOrdinals.Count <=
                       _roleFormalBodyOrdinals.Count &&
                   _roleFormalFallbackAttackOrdinals.Count +
                       _roleFormalExactAttackOrdinals.Count <= participantCount &&
                   _formalIncrementalEntries.Count <=
                       _roleFormalBodyOrdinals.Count;
        }

        private bool TryValidateRoleAwareFormalBroadphaseInputsFull(
            out bool directInputSafe)
        {
            directInputSafe = true;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (!TryValidateRoleAwareParticipant(
                        participantOrdinal,
                        participant.Handle,
                        out LF2Entity entity) ||
                    !ReferenceEquals(
                        GetAuthoredCurrentFrame(entity),
                        participant.CurrentFrame) ||
                    !ReferenceEquals(
                        entity.GetCollisionFrameData(),
                        participant.CollisionFrame) ||
                    (participant.CurrentFrame?.itrs?.Count ?? 0) !=
                        participant.CurrentItrCount ||
                    (participant.CollisionFrame?.itrs?.Count ?? 0) !=
                        participant.CollisionItrCount ||
                    (participant.CollisionFrame?.bodies?.Count ?? 0) !=
                        participant.CollisionBodyCount)
                {
                    return false;
                }
                if (participant.HasFallbackAttackItr ||
                    participant.HasFallbackBody)
                {
                    directInputSafe = false;
                }
            }

            for (int itrEntryIndex = 0;
                 itrEntryIndex < _roleFormalItrEntries.Count;
                 itrEntryIndex++)
            {
                RoleAwareFormalItrEntry itrEntry =
                    _roleFormalItrEntries[itrEntryIndex];
                if (!itrEntry.Bounds.IsValid ||
                    !TryValidateRoleAwareParticipant(
                        itrEntry.ParticipantOrdinal,
                        itrEntry.Handle,
                        out LF2Entity entity))
                {
                    return false;
                }

                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[itrEntry.ParticipantOrdinal];
                List<InteractionArea> itrs = participant.CollisionFrame?.itrs;
                if (itrs == null ||
                    itrEntry.ItrIndex < 0 ||
                    itrEntry.ItrIndex >= itrs.Count ||
                    !ReferenceEquals(itrs[itrEntry.ItrIndex], itrEntry.Itr) ||
                    !TryBuildFormalItrAabb(
                        entity,
                        participant.CollisionFrame,
                        itrEntry.Itr,
                        out SpatialAabbXZ currentBounds) ||
                    !currentBounds.Equals(itrEntry.Bounds))
                {
                    return false;
                }
            }

            for (int bodyEntryIndex = 0;
                 bodyEntryIndex < _formalIncrementalEntries.Count;
                 bodyEntryIndex++)
            {
                IncrementalSpatialEntry bodyEntry =
                    _formalIncrementalEntries[bodyEntryIndex];
                if (!bodyEntry.Bounds.IsValid ||
                    !_formalSlotToOrdinal.TryGetValue(
                        bodyEntry.Handle.Slot,
                        out int bodyParticipantOrdinal) ||
                    !TryValidateRoleAwareParticipant(
                        bodyParticipantOrdinal,
                        bodyEntry.Handle,
                        out LF2Entity entity) ||
                    !TryBuildCurrentRoleAwareFormalBodyBounds(
                        entity,
                        _roleFormalParticipants[bodyParticipantOrdinal].CollisionFrame,
                        out SpatialAabbXZ currentBounds) ||
                    !currentBounds.Equals(bodyEntry.Bounds))
                {
                    return false;
                }
            }

            return true;
        }

        private void CollectRoleAwareDirectBroadphasePairs()
        {
            long directCost =
                (long)_roleFormalItrEntries.Count *
                _formalIncrementalEntries.Count;
            if (ShouldUseRoleAwareSweepDirect(directCost))
                CollectRoleAwareSweepDirectBroadphasePairs();
            else
                CollectRoleAwareNestedDirectBroadphasePairs();
        }

        private bool ShouldUseRoleAwareSweepDirect(long directCost)
        {
            if (ForceRoleAwareNestedDirectForDiagnostics)
                return false;
            if (ForceRoleAwareSweepDirectForDiagnostics)
                return true;
            return directCost >= RoleAwareSweepDirectCrossover;
        }

        private void CollectRoleAwareNestedDirectBroadphasePairs()
        {
            long comparisonCount = 0;
            for (int itrEntryIndex = 0;
                 itrEntryIndex < _roleFormalItrEntries.Count;
                 itrEntryIndex++)
            {
                RoleAwareFormalItrEntry itrEntry =
                    _roleFormalItrEntries[itrEntryIndex];
                for (int bodyEntryIndex = 0;
                     bodyEntryIndex < _formalIncrementalEntries.Count;
                     bodyEntryIndex++)
                {
                    IncrementalSpatialEntry bodyEntry =
                        _formalIncrementalEntries[bodyEntryIndex];
                    comparisonCount++;
                    if (!itrEntry.Bounds.Overlaps(bodyEntry.Bounds))
                        continue;

                    int bodyParticipantOrdinal =
                        _formalSlotToOrdinal[bodyEntry.Handle.Slot];
                    AddAuthorityOrdinalPair(
                        itrEntry.ParticipantOrdinal,
                        bodyParticipantOrdinal);
                }
            }

            _lastRoleAwareDirectComparisonCount = comparisonCount;
            _totalRoleAwareDirectComparisonCount = SaturatingAdd(
                _totalRoleAwareDirectComparisonCount,
                comparisonCount);
        }

        private void CollectRoleAwareSweepDirectBroadphasePairs()
        {
            _roleFormalSweepEvents.Clear();
            _roleFormalSweepActiveBodyIndices.Clear();
            _roleFormalSweepActiveItrIndices.Clear();
            PrepareRoleAwareSweepPositions(
                _roleFormalSweepBodyPositions,
                _formalIncrementalEntries.Count);
            PrepareRoleAwareSweepPositions(
                _roleFormalSweepItrPositions,
                _roleFormalItrEntries.Count);

            for (int bodyEntryIndex = 0;
                 bodyEntryIndex < _formalIncrementalEntries.Count;
                 bodyEntryIndex++)
            {
                SpatialAabbXZ bounds =
                    _formalIncrementalEntries[bodyEntryIndex].Bounds;
                if (!bounds.IsValid)
                    continue;
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    bounds.MinX,
                    RoleAwareSweepEventKind.BodyStart,
                    bodyEntryIndex));
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    bounds.MaxX,
                    RoleAwareSweepEventKind.BodyEnd,
                    bodyEntryIndex));
            }
            for (int itrEntryIndex = 0;
                 itrEntryIndex < _roleFormalItrEntries.Count;
                 itrEntryIndex++)
            {
                SpatialAabbXZ bounds = _roleFormalItrEntries[itrEntryIndex].Bounds;
                if (!bounds.IsValid)
                    continue;
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    bounds.MinX,
                    RoleAwareSweepEventKind.ItrStart,
                    itrEntryIndex));
                _roleFormalSweepEvents.Add(new RoleAwareSweepEvent(
                    bounds.MaxX,
                    RoleAwareSweepEventKind.ItrEnd,
                    itrEntryIndex));
            }

            SortRoleAwareSweepEvents(_roleFormalSweepEvents);
            long xCandidateCount = 0;
            long fullOverlapCheckCount = 0;
            for (int eventIndex = 0;
                 eventIndex < _roleFormalSweepEvents.Count;
                 eventIndex++)
            {
                RoleAwareSweepEvent sweepEvent =
                    _roleFormalSweepEvents[eventIndex];
                switch (sweepEvent.Kind)
                {
                    case RoleAwareSweepEventKind.BodyEnd:
                        RemoveRoleAwareSweepActiveIndex(
                            sweepEvent.EntryIndex,
                            _roleFormalSweepActiveBodyIndices,
                            _roleFormalSweepBodyPositions);
                        break;
                    case RoleAwareSweepEventKind.ItrEnd:
                        RemoveRoleAwareSweepActiveIndex(
                            sweepEvent.EntryIndex,
                            _roleFormalSweepActiveItrIndices,
                            _roleFormalSweepItrPositions);
                        break;
                    case RoleAwareSweepEventKind.BodyStart:
                    {
                        IncrementalSpatialEntry bodyEntry =
                            _formalIncrementalEntries[sweepEvent.EntryIndex];
                        int bodyParticipantOrdinal =
                            _formalSlotToOrdinal[bodyEntry.Handle.Slot];
                        for (int activeIndex = 0;
                             activeIndex < _roleFormalSweepActiveItrIndices.Count;
                             activeIndex++)
                        {
                            RoleAwareFormalItrEntry itrEntry =
                                _roleFormalItrEntries[
                                    _roleFormalSweepActiveItrIndices[activeIndex]];
                            xCandidateCount++;
                            fullOverlapCheckCount++;
                            if (!itrEntry.Bounds.Overlaps(bodyEntry.Bounds))
                                continue;
                            AddAuthorityOrdinalPair(
                                itrEntry.ParticipantOrdinal,
                                bodyParticipantOrdinal);
                        }
                        AddRoleAwareSweepActiveIndex(
                            sweepEvent.EntryIndex,
                            _roleFormalSweepActiveBodyIndices,
                            _roleFormalSweepBodyPositions);
                        break;
                    }
                    case RoleAwareSweepEventKind.ItrStart:
                    {
                        RoleAwareFormalItrEntry itrEntry =
                            _roleFormalItrEntries[sweepEvent.EntryIndex];
                        for (int activeIndex = 0;
                             activeIndex < _roleFormalSweepActiveBodyIndices.Count;
                             activeIndex++)
                        {
                            IncrementalSpatialEntry bodyEntry =
                                _formalIncrementalEntries[
                                    _roleFormalSweepActiveBodyIndices[activeIndex]];
                            xCandidateCount++;
                            fullOverlapCheckCount++;
                            if (!itrEntry.Bounds.Overlaps(bodyEntry.Bounds))
                                continue;
                            int bodyParticipantOrdinal =
                                _formalSlotToOrdinal[bodyEntry.Handle.Slot];
                            AddAuthorityOrdinalPair(
                                itrEntry.ParticipantOrdinal,
                                bodyParticipantOrdinal);
                        }
                        AddRoleAwareSweepActiveIndex(
                            sweepEvent.EntryIndex,
                            _roleFormalSweepActiveItrIndices,
                            _roleFormalSweepItrPositions);
                        break;
                    }
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            _lastRoleAwareSweepXCandidateCount = xCandidateCount;
            _lastRoleAwareSweepFullOverlapCheckCount = fullOverlapCheckCount;
            _totalRoleAwareSweepXCandidateCount = SaturatingAdd(
                _totalRoleAwareSweepXCandidateCount,
                xCandidateCount);
            _totalRoleAwareSweepFullOverlapCheckCount = SaturatingAdd(
                _totalRoleAwareSweepFullOverlapCheckCount,
                fullOverlapCheckCount);
            _lastRoleAwareDirectComparisonCount = fullOverlapCheckCount;
            _totalRoleAwareDirectComparisonCount = SaturatingAdd(
                _totalRoleAwareDirectComparisonCount,
                fullOverlapCheckCount);
        }

        private static void PrepareRoleAwareSweepPositions(
            List<int> positions,
            int requiredCount)
        {
            while (positions.Count < requiredCount)
                positions.Add(-1);
            if (positions.Count > requiredCount)
            {
                positions.RemoveRange(
                    requiredCount,
                    positions.Count - requiredCount);
            }
            for (int index = 0; index < requiredCount; index++)
                positions[index] = -1;
        }

        private static void AddRoleAwareSweepActiveIndex(
            int entryIndex,
            List<int> activeIndices,
            List<int> positions)
        {
            positions[entryIndex] = activeIndices.Count;
            activeIndices.Add(entryIndex);
        }

        private static void RemoveRoleAwareSweepActiveIndex(
            int entryIndex,
            List<int> activeIndices,
            List<int> positions)
        {
            int position = positions[entryIndex];
            if (position < 0)
                return;

            int lastPosition = activeIndices.Count - 1;
            int lastEntryIndex = activeIndices[lastPosition];
            activeIndices[position] = lastEntryIndex;
            activeIndices.RemoveAt(lastPosition);
            positions[lastEntryIndex] = position;
            positions[entryIndex] = -1;
        }

        private static long SaturatingIncrement(long value) =>
            value == long.MaxValue ? long.MaxValue : value + 1;

        private static long SaturatingAdd(long value, long increment)
        {
            if (increment <= 0)
                return value;
            return value > long.MaxValue - increment
                ? long.MaxValue
                : value + increment;
        }

        private void BuildRoleAwareFormalBodyState(
            ref RoleAwareFormalParticipant participant,
            ref int bodyEntryCount,
            out bool hasIndexableBody,
            out SpatialAabbXZ indexableBodyBounds)
        {
            hasIndexableBody = false;
            indexableBodyBounds = default;
            LF2FrameData frame = participant.Frame;
            if (frame == null)
                return;

            if (!ForceLegacyRoleBodyBuildForDiagnostics)
            {
                RoleAwareFormalBodyTemplate template;
                if (_roleFormalBodyTemplates.TryGetValue(frame, out template))
                {
                    _lastRoleAwareBodyTemplateHitCount++;
                }
                else
                {
                    template = BuildRoleAwareFormalBodyTemplate(frame);
                    _roleFormalBodyTemplates.Add(frame, template);
                    _lastRoleAwareBodyTemplateBuildCount++;
                }

                if (TryMaterializeRoleAwareFormalBodyTemplate(
                        ref participant,
                        in template,
                        ref bodyEntryCount,
                        out hasIndexableBody,
                        out indexableBodyBounds))
                {
                    return;
                }

                _lastRoleAwareBodyTemplateFallbackCount++;
            }

            BuildRoleAwareFormalBodyStateLegacy(
                ref participant,
                ref bodyEntryCount,
                out hasIndexableBody,
                out indexableBodyBounds);
        }

        private static RoleAwareFormalBodyTemplate BuildRoleAwareFormalBodyTemplate(
            LF2FrameData frame)
        {
            List<BodyBox> bodies = frame?.bodies;
            int bodyCount = bodies?.Count ?? 0;
            int centerX = frame?.centerx ?? 0;
            int releaseBodyCount = 0;
            bool hasUnion = false;
            bool fastProof = frame != null &&
                             centerX != int.MinValue &&
                             centerX != int.MaxValue;
            long rightMinX = 0;
            long rightMaxX = 0;
            long leftMinX = 0;
            long leftMaxX = 0;

            for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
            {
                BodyBox body = bodies[bodyIndex];
                if (!IsReleaseBody(body))
                    continue;

                releaseBodyCount++;
                if (body.x == int.MinValue ||
                    body.x == int.MaxValue ||
                    body.w <= 0 ||
                    body.w == int.MaxValue)
                {
                    fastProof = false;
                    continue;
                }

                long rightX1 = (long)-centerX + body.x;
                long rightX2 = rightX1 + body.w;
                long leftX2 = (long)centerX - body.x;
                long leftX1 = leftX2 - body.w;
                if (rightX1 < RectMin ||
                    rightX2 > RectMax ||
                    leftX1 < RectMin ||
                    leftX2 > RectMax ||
                    rightX1 >= rightX2 ||
                    leftX1 >= leftX2)
                {
                    fastProof = false;
                    continue;
                }

                if (!hasUnion)
                {
                    rightMinX = rightX1;
                    rightMaxX = rightX2;
                    leftMinX = leftX1;
                    leftMaxX = leftX2;
                    hasUnion = true;
                }
                else
                {
                    rightMinX = Math.Min(rightMinX, rightX1);
                    rightMaxX = Math.Max(rightMaxX, rightX2);
                    leftMinX = Math.Min(leftMinX, leftX1);
                    leftMaxX = Math.Max(leftMaxX, leftX2);
                }
            }

            fastProof &= releaseBodyCount == 0 || hasUnion;
            return new RoleAwareFormalBodyTemplate(
                bodies,
                bodyCount,
                centerX,
                releaseBodyCount,
                rightMinX,
                rightMaxX,
                leftMinX,
                leftMaxX,
                fastProof);
        }

        private static bool TryMaterializeRoleAwareFormalBodyTemplate(
            ref RoleAwareFormalParticipant participant,
            in RoleAwareFormalBodyTemplate template,
            ref int bodyEntryCount,
            out bool hasIndexableBody,
            out SpatialAabbXZ indexableBodyBounds)
        {
            hasIndexableBody = false;
            indexableBodyBounds = default;
            LF2Entity entity = participant.Entity;
            LF2FrameData frame = participant.Frame;
            if (entity?.Runtime == null ||
                entity.PS == null ||
                frame == null ||
                !ReferenceEquals(frame.bodies, template.SourceBodies) ||
                (frame.bodies?.Count ?? 0) != template.SourceBodyCount ||
                frame.centerx != template.SourceCenterX ||
                !template.FastProof)
            {
                return false;
            }

            participant.HasBody = template.ReleaseBodyCount > 0;
            if (!participant.HasBody)
                return true;

            int entityX = entity.Runtime.XInt;
            if (entityX == int.MinValue || entityX == int.MaxValue)
                return false;

            bool facingLeft = entity.PS.dir == "left";
            long minX = (long)entityX +
                        (facingLeft ? template.LeftMinX : template.RightMinX);
            long maxX = (long)entityX +
                        (facingLeft ? template.LeftMaxX : template.RightMaxX);
            if (minX < RectMin ||
                maxX > RectMax ||
                minX >= maxX)
            {
                return false;
            }

            int collisionZ = CollisionZInt(entity, frame);
            indexableBodyBounds = new SpatialAabbXZ(
                (int)minX,
                collisionZ,
                (int)maxX,
                ClampRect((long)collisionZ + 1));
            if (!indexableBodyBounds.IsValid)
                return false;

            SaturatingAddBodyEntryCount(
                ref bodyEntryCount,
                template.ReleaseBodyCount);
            hasIndexableBody = true;
            return true;
        }

        private static void BuildRoleAwareFormalBodyStateLegacy(
            ref RoleAwareFormalParticipant participant,
            ref int bodyEntryCount,
            out bool hasIndexableBody,
            out SpatialAabbXZ indexableBodyBounds)
        {
            hasIndexableBody = false;
            indexableBodyBounds = default;
            LF2Entity entity = participant.Entity;
            LF2FrameData frame = participant.Frame;
            if (frame?.bodies == null)
                return;

            for (int bodyIndex = 0;
                 bodyIndex < frame.bodies.Count;
                 bodyIndex++)
            {
                BodyBox body = frame.bodies[bodyIndex];
                if (!IsReleaseBody(body))
                    continue;

                participant.HasBody = true;
                if (TryBuildFormalBodyAabb(
                        entity,
                        frame,
                        body,
                        out SpatialAabbXZ bodyBounds))
                {
                    SaturatingAddBodyEntryCount(ref bodyEntryCount, 1);
                    if (!hasIndexableBody)
                    {
                        indexableBodyBounds = bodyBounds;
                        hasIndexableBody = true;
                    }
                    else
                    {
                        indexableBodyBounds = new SpatialAabbXZ(
                            Math.Min(indexableBodyBounds.MinX, bodyBounds.MinX),
                            Math.Min(indexableBodyBounds.MinZ, bodyBounds.MinZ),
                            Math.Max(indexableBodyBounds.MaxX, bodyBounds.MaxX),
                            Math.Max(indexableBodyBounds.MaxZ, bodyBounds.MaxZ));
                    }
                }
                else
                {
                    participant.HasFallbackBody = true;
                }
            }
        }

        private static bool TryBuildCurrentRoleAwareFormalBodyBounds(
            LF2Entity entity,
            LF2FrameData frame,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            bool hasBounds = false;
            if (frame?.bodies == null)
                return false;

            for (int bodyIndex = 0; bodyIndex < frame.bodies.Count; bodyIndex++)
            {
                BodyBox body = frame.bodies[bodyIndex];
                if (!IsReleaseBody(body) ||
                    !TryBuildFormalBodyAabb(
                        entity,
                        frame,
                        body,
                        out SpatialAabbXZ bodyBounds))
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = bodyBounds;
                    hasBounds = true;
                }
                else
                {
                    bounds = new SpatialAabbXZ(
                        Math.Min(bounds.MinX, bodyBounds.MinX),
                        Math.Min(bounds.MinZ, bodyBounds.MinZ),
                        Math.Max(bounds.MaxX, bodyBounds.MaxX),
                        Math.Max(bounds.MaxZ, bodyBounds.MaxZ));
                }
            }

            return hasBounds && bounds.IsValid;
        }

        private void InitializeRoleAwareFormalExactTracking(int participantCount)
        {
            _roleFormalExactRequiredRoles.Clear();
#if UNITY_INCLUDE_TESTS
            _roleFormalExactCommonBuildCounts.Clear();
            _roleFormalExactAttackBuildCounts.Clear();
            _roleFormalExactBodyBuildCounts.Clear();
            _roleFormalExactValidationCounts.Clear();
#endif
            for (int participantOrdinal = 0;
                 participantOrdinal < participantCount;
                 participantOrdinal++)
            {
                _roleFormalExactRequiredRoles.Add(0);
#if UNITY_INCLUDE_TESTS
                _roleFormalExactCommonBuildCounts.Add(0);
                _roleFormalExactAttackBuildCounts.Add(0);
                _roleFormalExactBodyBuildCounts.Add(0);
                _roleFormalExactValidationCounts.Add(0);
#endif
            }
        }

        private bool TryBuildRequiredRoleAwareFormalExactCaches(
            int participantCount)
        {
            if (_roleFormalExactRequiredRoles.Count != participantCount)
                return false;

            for (int participantOrdinal = 0;
                 participantOrdinal < participantCount;
                 participantOrdinal++)
            {
                byte requiredRoles =
                    _roleFormalExactRequiredRoles[participantOrdinal];
                if (requiredRoles == 0)
                    continue;

                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (!BuildRoleAwareFormalExactCommonCache(ref participant))
                    return false;
#if UNITY_INCLUDE_TESTS
                IncrementRoleAwareFormalExactCounter(
                    _roleFormalExactCommonBuildCounts,
                    participantOrdinal);
#endif

                if ((requiredRoles & RoleAwareFormalExactAttackRole) != 0)
                {
                    BuildRoleAwareFormalExactAttackCache(ref participant);
#if UNITY_INCLUDE_TESTS
                    IncrementRoleAwareFormalExactCounter(
                        _roleFormalExactAttackBuildCounts,
                        participantOrdinal);
#endif
                }

                if ((requiredRoles & RoleAwareFormalExactBodyRole) != 0)
                {
                    BuildRoleAwareFormalExactBodyCache(ref participant);
#if UNITY_INCLUDE_TESTS
                    IncrementRoleAwareFormalExactCounter(
                        _roleFormalExactBodyBuildCounts,
                        participantOrdinal);
#endif
                }

                _roleFormalParticipants[participantOrdinal] = participant;
#if UNITY_INCLUDE_TESTS
                IncrementRoleAwareFormalExactCounter(
                    _roleFormalExactValidationCounts,
                    participantOrdinal);
#endif
                if (!RoleAwareFormalExactCacheIsCurrent(
                        in participant,
                        requiredRoles))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool BuildRoleAwareFormalExactCommonCache(
            ref RoleAwareFormalParticipant participant)
        {
            if (participant.HasExactCommonCache)
                return true;

            LF2Entity entity = participant.Entity;
            LF2FrameData collisionFrame = participant.CollisionFrame;
            if (entity?.PS == null ||
                entity.Runtime == null ||
                collisionFrame == null)
            {
                return false;
            }

            participant.CollisionX = entity.Runtime.XInt;
            participant.CollisionY = entity.Runtime.YInt;
            participant.CollisionZ = CollisionZInt(entity, collisionFrame);
            participant.FacingLeft = entity.PS.dir == "left";
            participant.DataObjectType = GetCurrentDataObjectType(entity);
            participant.CurrentDataObjectId =
                LF2Entity.ResolveCurrentDataObjectId(entity);
            participant.PairCollectionBaseAllowed =
                !IsPendingFlushDestroy(entity) &&
                !IsPureTransitionSmoke(entity);
            participant.AttackPairCollectionBaseAllowed =
                participant.PairCollectionBaseAllowed &&
                entity.AttackExempt <= 0;
            participant.HasExactCommonCache = true;
            return true;
        }

        private void BuildRoleAwareFormalExactAttackCache(
            ref RoleAwareFormalParticipant participant)
        {
            if (participant.HasExactAttackCache)
                return;

            LF2FrameData collisionFrame = participant.CollisionFrame;
            participant.CandidateAttackerCarrier =
                IsCandidateAttackerCarrierForCurrentTick(participant.Entity);
            participant.ExactItrRectOffset = _roleFormalExactItrRects.Count;
            participant.ExactItrRectCount = 0;
            List<InteractionArea> itrs = collisionFrame?.itrs;
            if (itrs != null)
            {
                for (int itrIndex = 0; itrIndex < itrs.Count; itrIndex++)
                {
                    InteractionArea itr = itrs[itrIndex];
                    if (itr == null)
                        continue;

                    _roleFormalExactItrRects.Add(
                        new RoleAwareFormalExactItrRectEntry(
                            itr,
                            itrIndex,
                            ItrWorldRect(
                                participant.Entity,
                                collisionFrame,
                                itr)));
                    participant.ExactItrRectCount++;
                    _lastRoleAwareExactItrRectBuildCount++;
                }
            }

            if (TryUnionItrRect(collisionFrame, out LocalRect ordinaryItrUnion))
            {
                participant.OrdinaryItrUnionWorld = LocalRectWorldRect(
                    participant.Entity,
                    collisionFrame,
                    ordinaryItrUnion,
                    fullHeight: false);
                participant.HasOrdinaryItrUnion = true;
            }

            participant.HasExactAttackCache = true;
        }

        private void BuildRoleAwareFormalExactBodyCache(
            ref RoleAwareFormalParticipant participant)
        {
            if (participant.HasExactBodyCache)
                return;

            LF2FrameData currentFrame = participant.CurrentFrame;
            LF2FrameData collisionFrame = participant.CollisionFrame;
            participant.ExactBodyRectOffset = _roleFormalExactBodyRects.Count;
            participant.ExactBodyRectCount = 0;
            List<BodyBox> bodies = collisionFrame?.bodies;
            if (bodies != null)
            {
                for (int bodyIndex = 0; bodyIndex < bodies.Count; bodyIndex++)
                {
                    BodyBox body = bodies[bodyIndex];
                    if (!IsReleaseBody(body))
                        continue;

                    _roleFormalExactBodyRects.Add(
                        new RoleAwareFormalExactBodyRectEntry(
                            body.x,
                            BodyWorldRect(
                                participant.Entity,
                                collisionFrame,
                                body,
                                collectSemantics: true)));
                    participant.ExactBodyRectCount++;
                    _lastRoleAwareExactBodyRectBuildCount++;
                }
            }

            bool hasCollisionReleaseBody = TryUnionBodyRect(
                    collisionFrame,
                    out LocalRect bodyUnion,
                    out bool fullHeight);
            participant.HasCollisionReleaseBody = hasCollisionReleaseBody;
            participant.HasCurrentReleaseBody =
                ReferenceEquals(currentFrame, collisionFrame)
                    ? hasCollisionReleaseBody
                    : HasAnyReleaseBody(currentFrame);
            if (hasCollisionReleaseBody)
            {
                participant.BodyUnionWorld = LocalRectWorldRect(
                    participant.Entity,
                    collisionFrame,
                    bodyUnion,
                    fullHeight);
                participant.HasBodyUnion = true;
            }

            participant.HasExactBodyCache = true;
        }

#if UNITY_INCLUDE_TESTS
        private static void IncrementRoleAwareFormalExactCounter(
            List<byte> counters,
            int participantOrdinal)
        {
            byte count = counters[participantOrdinal];
            counters[participantOrdinal] = count == byte.MaxValue
                ? byte.MaxValue
                : (byte)(count + 1);
        }
#endif

        private static void SaturatingAddBodyEntryCount(
            ref int bodyEntryCount,
            int increment)
        {
            if (increment <= 0 || bodyEntryCount == int.MaxValue)
                return;
            bodyEntryCount = bodyEntryCount > int.MaxValue - increment
                ? int.MaxValue
                : bodyEntryCount + increment;
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
                // The legacy union-AABB collector treats a participant with no
                // authored collision role as unindexable too. It cannot produce
                // a candidate by itself, so pair reconciliation deliberately does
                // not add pairs for it; retain it in diagnostics nevertheless so
                // an omitted data role is never mistaken for a successful index.
                if (participant.HasFallbackAttackItr ||
                    participant.HasFallbackBody ||
                    (!participant.HasAttackItr && !participant.HasBody))
                    count++;
            }

            return count;
        }

        private int CountRoleAwareFormalInertParticipants()
        {
            int count = 0;
            for (int participantOrdinal = 0;
                 participantOrdinal < _roleFormalParticipants.Count;
                 participantOrdinal++)
            {
                RoleAwareFormalParticipant participant =
                    _roleFormalParticipants[participantOrdinal];
                if (!participant.HasAttackItr && !participant.HasBody)
                    count++;
            }

            return count;
        }

        private void AddRoleAwareFormalParticipantOrdinals(
            int participantOrdinal,
            in RoleAwareFormalParticipant participant)
        {
            if (participant.HasBody)
                _roleFormalBodyOrdinals.Add(participantOrdinal);
            if (participant.HasFallbackBody && participant.HasBody)
                _roleFormalFallbackBodyOrdinals.Add(participantOrdinal);
            if (!participant.HasAttackItr)
                return;

            if (participant.HasFallbackAttackItr)
                _roleFormalFallbackAttackOrdinals.Add(participantOrdinal);
            else
                _roleFormalExactAttackOrdinals.Add(participantOrdinal);
        }

        private void CollectRoleAwareFormalFallbackPairs()
        {
            AddRoleAwareFormalFallbackPairsTo(_formalAuthorityPairKeys);
        }

        private void AddRoleAwareFormalFallbackPairsTo(List<long> destination)
        {
            for (int attackerIndex = 0;
                 attackerIndex < _roleFormalFallbackAttackOrdinals.Count;
                 attackerIndex++)
            {
                int attackerOrdinal =
                    _roleFormalFallbackAttackOrdinals[attackerIndex];
                for (int bodyIndex = 0;
                     bodyIndex < _roleFormalBodyOrdinals.Count;
                     bodyIndex++)
                {
                    AddOrdinalPair(
                        destination,
                        attackerOrdinal,
                        _roleFormalBodyOrdinals[bodyIndex]);
                }
            }

            for (int attackerIndex = 0;
                 attackerIndex < _roleFormalExactAttackOrdinals.Count;
                 attackerIndex++)
            {
                int attackerOrdinal =
                    _roleFormalExactAttackOrdinals[attackerIndex];
                for (int bodyIndex = 0;
                     bodyIndex < _roleFormalFallbackBodyOrdinals.Count;
                     bodyIndex++)
                {
                    AddOrdinalPair(
                        destination,
                        attackerOrdinal,
                        _roleFormalFallbackBodyOrdinals[bodyIndex]);
                }
            }
        }

        private void AddAuthorityOrdinalPair(int firstOrdinal, int secondOrdinal)
        {
            AddOrdinalPair(
                _formalAuthorityPairKeys,
                firstOrdinal,
                secondOrdinal);
        }

        private static void AddOrdinalPair(
            List<long> destination,
            int firstOrdinal,
            int secondOrdinal)
        {
            if (destination == null ||
                firstOrdinal == secondOrdinal ||
                firstOrdinal < 0 ||
                secondOrdinal < 0)
            {
                return;
            }

            uint min = (uint)Math.Min(firstOrdinal, secondOrdinal);
            uint max = (uint)Math.Max(firstOrdinal, secondOrdinal);
            destination.Add(((long)min << 32) | max);
        }

        private bool AbortFormalSpatialIndex()
        {
            _formalBroadphase.ResetIncremental();
            _roleFormalBroadphase.ResetIncremental();
            // Diagnostics must not report the last successful synchronize after the
            // collector has discarded that index and fallen back to the authority path.
            FormalSpatialSynchronizeResult = SpatialSynchronizeResult.Failed;
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
                if (participant.HasFallbackBody ||
                    participant.HasFallbackAttackItr ||
                    (!participant.HasAttackItr && !participant.HasBody))
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

        private void SortAndDeduplicateAuthorityOrdinalPairs(
            List<long> values,
            int participantCount)
        {
            int valueCount = values.Count;
            if (valueCount < AuthorityPairCountingSortThreshold ||
                participantCount <= 0 ||
                participantCount > _formalAuthorityPairSortCounts.Length ||
                participantCount > _formalAuthorityPairSortOffsets.Length ||
                valueCount > _formalAuthorityPairSortScratch.Length)
            {
                SortAndDeduplicate(values);
                return;
            }

            Array.Clear(
                _formalAuthorityPairSortCounts,
                0,
                participantCount);
            for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                long value = values[valueIndex];
                int firstOrdinal = (int)(value >> 32);
                int secondOrdinal = (int)(value & 0xffffffffL);
                if (firstOrdinal < 0 ||
                    firstOrdinal >= participantCount ||
                    secondOrdinal < 0 ||
                    secondOrdinal >= participantCount)
                {
                    SortAndDeduplicate(values);
                    return;
                }

                _formalAuthorityPairSortCounts[secondOrdinal]++;
            }

            BuildCountingSortOffsets(participantCount);
            for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                long value = values[valueIndex];
                int secondOrdinal = (int)(value & 0xffffffffL);
                _formalAuthorityPairSortScratch[
                    _formalAuthorityPairSortOffsets[secondOrdinal]++] = value;
            }

            Array.Clear(
                _formalAuthorityPairSortCounts,
                0,
                participantCount);
            for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                int firstOrdinal =
                    (int)(_formalAuthorityPairSortScratch[valueIndex] >> 32);
                _formalAuthorityPairSortCounts[firstOrdinal]++;
            }

            BuildCountingSortOffsets(participantCount);
            for (int valueIndex = 0; valueIndex < valueCount; valueIndex++)
            {
                long value = _formalAuthorityPairSortScratch[valueIndex];
                int firstOrdinal = (int)(value >> 32);
                values[_formalAuthorityPairSortOffsets[firstOrdinal]++] = value;
            }

            DeduplicateSorted(values);
        }

        private void BuildCountingSortOffsets(int participantCount)
        {
            int nextOffset = 0;
            for (int ordinal = 0; ordinal < participantCount; ordinal++)
            {
                _formalAuthorityPairSortOffsets[ordinal] = nextOffset;
                nextOffset += _formalAuthorityPairSortCounts[ordinal];
            }
        }

        private static void SortAndDeduplicate(List<long> values)
        {
            if (values.Count < 2)
                return;

            SortLongs(values);
            DeduplicateSorted(values);
        }

        private static void SortLongs(List<long> values)
        {
            QuickSortLongs(values, 0, values.Count - 1);
        }

        private static void QuickSortLongs(List<long> values, int left, int right)
        {
            while (left < right)
            {
                int low = left;
                int high = right;
                long pivot = Median(
                    values[left],
                    values[left + ((right - left) >> 1)],
                    values[right]);
                while (low <= high)
                {
                    while (values[low] < pivot)
                        low++;
                    while (values[high] > pivot)
                        high--;
                    if (low > high)
                        break;

                    long value = values[low];
                    values[low] = values[high];
                    values[high] = value;
                    low++;
                    high--;
                }

                if (high - left < right - low)
                {
                    if (left < high)
                        QuickSortLongs(values, left, high);
                    left = low;
                }
                else
                {
                    if (low < right)
                        QuickSortLongs(values, low, right);
                    right = high;
                }
            }
        }

        private static long Median(long first, long second, long third)
        {
            if (first > second)
            {
                long value = first;
                first = second;
                second = value;
            }
            if (second > third)
            {
                second = third;
                if (first > second)
                    second = first;
            }
            return second;
        }

        private static void SortRoleAwareSweepEvents(
            List<RoleAwareSweepEvent> values)
        {
            QuickSortRoleAwareSweepEvents(values, 0, values.Count - 1);
        }

        private static void QuickSortRoleAwareSweepEvents(
            List<RoleAwareSweepEvent> values,
            int left,
            int right)
        {
            while (left < right)
            {
                int low = left;
                int high = right;
                RoleAwareSweepEvent pivot = MedianRoleAwareSweepEvent(
                    values[left],
                    values[left + ((right - left) >> 1)],
                    values[right]);
                while (low <= high)
                {
                    while (CompareRoleAwareSweepEvents(values[low], pivot) < 0)
                        low++;
                    while (CompareRoleAwareSweepEvents(values[high], pivot) > 0)
                        high--;
                    if (low > high)
                        break;

                    RoleAwareSweepEvent value = values[low];
                    values[low] = values[high];
                    values[high] = value;
                    low++;
                    high--;
                }

                if (high - left < right - low)
                {
                    if (left < high)
                        QuickSortRoleAwareSweepEvents(values, left, high);
                    left = low;
                }
                else
                {
                    if (low < right)
                        QuickSortRoleAwareSweepEvents(values, low, right);
                    right = high;
                }
            }
        }

        private static RoleAwareSweepEvent MedianRoleAwareSweepEvent(
            RoleAwareSweepEvent first,
            RoleAwareSweepEvent second,
            RoleAwareSweepEvent third)
        {
            if (CompareRoleAwareSweepEvents(first, second) > 0)
            {
                RoleAwareSweepEvent value = first;
                first = second;
                second = value;
            }
            if (CompareRoleAwareSweepEvents(second, third) > 0)
            {
                second = third;
                if (CompareRoleAwareSweepEvents(first, second) > 0)
                    second = first;
            }
            return second;
        }

        private static int CompareRoleAwareSweepEvents(
            RoleAwareSweepEvent left,
            RoleAwareSweepEvent right)
        {
            int xComparison = left.X.CompareTo(right.X);
            if (xComparison != 0)
                return xComparison;
            int kindComparison = ((byte)left.Kind).CompareTo((byte)right.Kind);
            return kindComparison != 0
                ? kindComparison
                : left.EntryIndex.CompareTo(right.EntryIndex);
        }

        private static void DeduplicateSorted(List<long> values)
        {
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

        public bool TryGetCollisionCandidateRange(
            LF2Entity attacker,
            out CollisionCandidateRange candidates)
        {
            if (!_consumeCandidateCache)
            {
                candidates = default;
                return false;
            }

            if (_candidateConsumptionSource ==
                CollisionCandidateConsumptionSource.StoreAuthority)
            {
                _candidateStoreAuthorityDiagnostics.RecordRangeRead();
                RuntimeEntityHandle attackerHandle = RuntimeEntityHandle.Invalid;
                int count = 0;
                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                if (!_world.TryGetCurrentRuntimeHandle(
                        attackerSlot,
                        attacker,
                        out attackerHandle))
                {
                    candidates = CreateCollisionCandidateRange(
                        null,
                        RuntimeEntityHandle.Invalid,
                        0,
                        storeAuthority: true);
                    return true;
                }

                if (!_candidateStoreShadow.TryGetVisibleAttackerRow(
                        attackerHandle,
                        out count))
                {
                    // Step8 newborn attackers intentionally have no step6 row and
                    // therefore see an empty range for the rest of this tick.
                    count = 0;
                    attackerHandle = RuntimeEntityHandle.Invalid;
                }

                candidates = CreateCollisionCandidateRange(
                    null,
                    attackerHandle,
                    count,
                    storeAuthority: true);
                return true;
            }

            if (_candidateConsumptionSource ==
                CollisionCandidateConsumptionSource.StoreAuthorityFailedClosed)
            {
                _candidateStoreAuthorityDiagnostics.RecordRangeRead();
                candidates = CreateCollisionCandidateRange(
                    null,
                    RuntimeEntityHandle.Invalid,
                    0,
                    storeAuthority: false);
                return true;
            }

            List<SceneQueryHit> legacyCandidates = _emptyCandidateHits;
            if (attacker != null &&
                _candidateCache.TryGetValue(attacker, out List<SceneQueryHit> cached))
            {
                legacyCandidates = cached;
            }

            candidates = CreateCollisionCandidateRange(
                legacyCandidates,
                RuntimeEntityHandle.Invalid,
                legacyCandidates.Count,
                storeAuthority: false);
            return true;
        }

        internal bool IsCollisionCandidateRangeValidForServices(int consumptionEpoch)
        {
            return _consumeCandidateCache &&
                   consumptionEpoch == _candidateConsumptionEpoch;
        }

        internal bool TryReadCollisionCandidateRangeEntryForServices(
            List<SceneQueryHit> legacyCandidates,
            RuntimeEntityHandle attackerHandle,
            int count,
            int candidateIndex,
            int consumptionEpoch,
            bool storeAuthority,
            out SceneQueryHit hit)
        {
            hit = default;
            if (!IsCollisionCandidateRangeValidForServices(consumptionEpoch) ||
                candidateIndex < 0 ||
                candidateIndex >= count)
            {
                return false;
            }

            if (!storeAuthority)
            {
                if (legacyCandidates == null || candidateIndex >= legacyCandidates.Count)
                    return false;

                hit = legacyCandidates[candidateIndex];
                return true;
            }

            _candidateStoreAuthorityDiagnostics.RecordEntryRead();
            if (!_world.TryResolveRuntimeHandle(attackerHandle, out _))
            {
                _candidateStoreAuthorityDiagnostics.RecordFailure(
                    CollisionCandidateStoreAuthorityFailureReason.AttackerHandleNotCurrent);
                return false;
            }
            if (!_candidateStoreShadow.TryGetVisibleCandidate(
                    attackerHandle,
                    candidateIndex,
                    out CollisionCandidateStoreEntry entry))
            {
                _candidateStoreAuthorityDiagnostics.RecordFailure(
                    CollisionCandidateStoreAuthorityFailureReason.CandidateReadFailed);
                return false;
            }

            // C++/legacy consumption follows the current occupant of TargetSlot.
            // TargetHandle is only shadow/oracle metadata and never gates reads.
            LF2Entity target = _world.FindEntityByRuntimeSlotForQuery(entry.TargetSlot);
            hit = new SceneQueryHit(
                target,
                entry.TargetSlot,
                entry.BodyX,
                entry.ItrIndex,
                entry.RuntimeItr,
                entry.ZeroAttackerHpOnConsume,
                entry.ReleaseHeavyHeldTargetOnConsume);
            return true;
        }

        private CollisionCandidateRange CreateCollisionCandidateRange(
            List<SceneQueryHit> legacyCandidates,
            RuntimeEntityHandle attackerHandle,
            int count,
            bool storeAuthority)
        {
            return new CollisionCandidateRange(
                this,
                legacyCandidates,
                attackerHandle,
                count,
                _candidateConsumptionEpoch,
                storeAuthority);
        }

        public void EndCollisionCandidateConsumption()
        {
            InvalidateCollisionCandidateRanges();
            ReleaseCandidateListsToPool();
            _candidateStoreShadow.EndTickVisibility();
        }

        private List<SceneQueryHit> RentCandidateList()
        {
            if (_candidateListPool.Count == 0)
            {
                if (_world?.RuntimeCapacity?.IsSealed == true)
                {
                    _candidateListRejectedRentCount++;
                    return null;
                }

                _candidateListCreatedCount++;
                return new List<SceneQueryHit>(HitCandidateMax);
            }

            List<SceneQueryHit> candidates = _candidateListPool.Pop();
            candidates.Clear();
            _candidateListReusedCount++;
            return candidates;
        }

        private void ReleaseCandidateListsToPool()
        {
            foreach (KeyValuePair<LF2Entity, List<SceneQueryHit>> pair in _candidateCache)
            {
                List<SceneQueryHit> candidates = pair.Value;
                if (candidates == null)
                    continue;

                candidates.Clear();
                _candidateListPool.Push(candidates);
            }

            _candidateCache.Clear();
        }

        private void InvalidateCollisionCandidateRanges()
        {
            _consumeCandidateCache = false;
            _candidateStoreAuthorityAppliedForCurrentTick = false;
            _candidateStoreProducerHealthyForCurrentTick = false;
            _candidateConsumptionSource = CollisionCandidateConsumptionSource.LegacyOracle;
            unchecked
            {
                _candidateConsumptionEpoch++;
            }
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

            if (!IsCandidateAttackerCarrierForCurrentTick(attacker))
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
                    TryRecordReleaseCandidate(
                        attacker,
                        target,
                        itr,
                        targetCurrentFrame,
                        bodyX,
                        itrIndex);
                }
            }
        }

        private void CollectCandidatesForRoleAwareFormalDirection(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalParticipant targetParticipant)
        {
            LF2Entity attacker = attackerParticipant.Entity;
            LF2Entity target = targetParticipant.Entity;
            if (attackerParticipant.HasFallbackAttackItr ||
                targetParticipant.HasFallbackBody ||
                !attackerParticipant.HasExactAttackCache ||
                !targetParticipant.HasExactBodyCache ||
                !attackerParticipant.HasExactCommonCache ||
                !targetParticipant.HasExactCommonCache)
            {
                CollectCandidatesForPair(attacker, target);
                return;
            }

            CollectCandidatesForPairCached(
                in attackerParticipant,
                in targetParticipant);
        }

        private void CollectCandidatesForPairCached(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalParticipant targetParticipant)
        {
            _lastRoleAwareExactDirectionCount++;
            LF2Entity attacker = attackerParticipant.Entity;
            LF2Entity target = targetParticipant.Entity;

            if (attacker == null || target == null || attacker == target)
                return;
            if (!CandidateCollectionPairAllowedCached(
                    in attackerParticipant,
                    in targetParticipant))
                return;

            LF2FrameData attackerCurrentFrame = attackerParticipant.CurrentFrame;
            LF2FrameData attackerCollisionFrame = attackerParticipant.CollisionFrame;
            if (attackerCurrentFrame == null ||
                attackerParticipant.CurrentItrCount == 0 ||
                attackerCollisionFrame?.itrs == null ||
                attackerParticipant.CollisionItrCount == 0)
            {
                return;
            }

            if (!attackerParticipant.CandidateAttackerCarrier)
                return;

            LF2FrameData targetCurrentFrame = targetParticipant.CurrentFrame;
            if (targetCurrentFrame == null ||
                !targetParticipant.HasCurrentReleaseBody)
            {
                return;
            }

            if (!PassesReleaseCoarsePrefilterCached(
                    in attackerParticipant,
                    in targetParticipant))
            {
                return;
            }

            int itrRectEnd = attackerParticipant.ExactItrRectOffset +
                             attackerParticipant.ExactItrRectCount;
            for (int itrRectIndex = attackerParticipant.ExactItrRectOffset;
                 itrRectIndex < itrRectEnd;
                 itrRectIndex++)
            {
                RoleAwareFormalExactItrRectEntry itrEntry =
                    _roleFormalExactItrRects[itrRectIndex];
                InteractionArea itr = itrEntry.Itr;
                _lastRoleAwareExactItrVisitCount++;
                if (!ItrAllowedCached(
                        attacker,
                        attackerCurrentFrame,
                        attackerCollisionFrame,
                        itr,
                        target,
                        targetCurrentFrame))
                {
                    continue;
                }

                if (HitsTargetCached(
                        in attackerParticipant,
                        in itrEntry,
                        in targetParticipant,
                        out int bodyX))
                {
                    TryRecordReleaseCandidate(
                        attacker,
                        target,
                        itr,
                        targetCurrentFrame,
                        bodyX,
                        itrEntry.ItrIndex,
                        attackerParticipant.Handle);
                }
            }
        }

        private bool RoleAwareFormalExactCacheIsCurrent(
            in RoleAwareFormalParticipant participant,
            byte requiredRoles)
        {
            if (requiredRoles == 0)
                return true;

            LF2Entity entity = participant.Entity;
            if (entity?.Runtime == null || entity.PS == null)
                return false;
            if (!ReferenceEquals(
                    GetAuthoredCurrentFrame(entity),
                    participant.CurrentFrame) ||
                !ReferenceEquals(
                    entity.GetCollisionFrameData(),
                    participant.CollisionFrame))
            {
                return false;
            }

            if (!participant.HasExactCommonCache ||
                entity.Runtime.XInt != participant.CollisionX ||
                entity.Runtime.YInt != participant.CollisionY ||
                (entity.PS.dir == "left") != participant.FacingLeft ||
                CollisionZInt(entity, participant.CollisionFrame) !=
                    participant.CollisionZ)
            {
                return false;
            }

            if ((requiredRoles & RoleAwareFormalExactAttackRole) != 0 &&
                (!participant.HasExactAttackCache ||
                 (participant.CurrentFrame?.itrs?.Count ?? 0) !=
                    participant.CurrentItrCount ||
                 (participant.CollisionFrame?.itrs?.Count ?? 0) !=
                    participant.CollisionItrCount ||
                 !ExactCacheRangeIsValid(
                     participant.ExactItrRectOffset,
                     participant.ExactItrRectCount,
                     _roleFormalExactItrRects.Count)))
            {
                return false;
            }

            if ((requiredRoles & RoleAwareFormalExactBodyRole) != 0 &&
                (!participant.HasExactBodyCache ||
                 (participant.CollisionFrame?.bodies?.Count ?? 0) !=
                    participant.CollisionBodyCount ||
                 (participant.HasCollisionReleaseBody &&
                  !participant.HasBodyUnion) ||
                 !ExactCacheRangeIsValid(
                     participant.ExactBodyRectOffset,
                     participant.ExactBodyRectCount,
                     _roleFormalExactBodyRects.Count)))
            {
                return false;
            }

            return true;
        }

        private static bool ExactCacheRangeIsValid(
            int offset,
            int count,
            int listCount)
        {
            return offset >= 0 &&
                   count >= 0 &&
                   offset <= listCount &&
                   count <= listCount - offset;
        }

        private bool PassesReleaseCoarsePrefilterCached(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalParticipant targetParticipant)
        {
            LF2Entity attacker = attackerParticipant.Entity;
            LF2Entity target = targetParticipant.Entity;
            LF2FrameData attackerCurrentFrame = attackerParticipant.CurrentFrame;
            LF2FrameData attackerCollisionFrame = attackerParticipant.CollisionFrame;
            LF2FrameData targetCurrentFrame = targetParticipant.CurrentFrame;
            if (attacker == null || target == null ||
                attackerCurrentFrame?.itrs == null ||
                attackerParticipant.CurrentItrCount == 0 ||
                attackerCollisionFrame?.itrs == null ||
                attackerParticipant.CollisionItrCount == 0 ||
                !targetParticipant.HasCollisionReleaseBody ||
                !targetParticipant.HasBodyUnion)
            {
                return false;
            }

            if (attackerParticipant.HasOrdinaryItrUnion &&
                Overlap(
                    attackerParticipant.OrdinaryItrUnionWorld,
                    targetParticipant.BodyUnionWorld))
            {
                return true;
            }

            int targetType = targetParticipant.DataObjectType;
            int targetState = targetCurrentFrame?.state ?? 0;
            int itrRectEnd = attackerParticipant.ExactItrRectOffset +
                             attackerParticipant.ExactItrRectCount;
            for (int itrRectIndex = attackerParticipant.ExactItrRectOffset;
                 itrRectIndex < itrRectEnd;
                 itrRectIndex++)
            {
                RoleAwareFormalExactItrRectEntry itrEntry =
                    _roleFormalExactItrRects[itrRectIndex];
                InteractionArea itr = itrEntry.Itr;
                if (itr.kind != 5 ||
                    !Kind5Allowed(attacker, target, targetState, targetType))
                {
                    continue;
                }

                if (Overlap(
                        itrEntry.WorldRect,
                        targetParticipant.BodyUnionWorld))
                    return true;
            }

            return false;
        }

        private bool HitsTargetCached(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalExactItrRectEntry itrEntry,
            in RoleAwareFormalParticipant targetParticipant,
            out int bodyX)
        {
            bodyX = 0;
            LF2Entity attacker = attackerParticipant.Entity;
            LF2FrameData attackerCollisionFrame = attackerParticipant.CollisionFrame;
            LF2Entity target = targetParticipant.Entity;
            LF2FrameData targetCollisionFrame = targetParticipant.CollisionFrame;
            InteractionArea itr = itrEntry.Itr;
            if (attacker?.PS == null || attackerCollisionFrame == null || itr == null ||
                target?.PS == null || targetCollisionFrame?.bodies == null ||
                !targetParticipant.HasCollisionReleaseBody ||
                !IsReleaseItrGeometry(itr))
            {
                return false;
            }

            int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
            int zDelta = targetParticipant.CollisionZ -
                         attackerParticipant.CollisionZ;
            if (zDelta >= zHalf || zDelta <= -zHalf)
                return false;

            int bodyRectEnd = targetParticipant.ExactBodyRectOffset +
                              targetParticipant.ExactBodyRectCount;
            for (int bodyRectIndex = targetParticipant.ExactBodyRectOffset;
                 bodyRectIndex < bodyRectEnd;
                 bodyRectIndex++)
            {
                RoleAwareFormalExactBodyRectEntry bodyEntry =
                    _roleFormalExactBodyRects[bodyRectIndex];
                _lastRoleAwareExactBodyOverlapCheckCount++;
                if (!Overlap(itrEntry.WorldRect, bodyEntry.WorldRect))
                    continue;

                bodyX = bodyEntry.BodyX;
                return true;
            }

            return false;
        }

        private void TryRecordReleaseCandidate(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            LF2FrameData targetFrame,
            int bodyX,
            int itrIndex,
            RuntimeEntityHandle validatedAttackerHandle = default)
        {
            if (attacker == null || target == null || itr == null ||
                targetFrame == null)
            {
                return;
            }

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
                    validatedAttackerHandle))
                return;

            int candidateCount = GetCollisionCandidateProducerCount(
                attacker,
                validatedAttackerHandle);
            if (candidateCount >= HitCandidateMax)
                return;

            rejectFlag = ApplyPrev2GroundRejectFlag(attacker, target, itr, targetFrame, rejectFlag);
            if (rejectFlag == 2)
                return;

            if (!AcceptReleaseSelectFlagCandidate(attacker, target, itr, targetFrame, rejectFlag))
                return;

            SceneQueryHit candidate = new SceneQueryHit(target, bodyX, itrIndex, itr);
            WriteCollisionCandidateStoreFirst(
                attacker,
                validatedAttackerHandle,
                candidateCount,
                in candidate,
                replaceSingle: false);

            if (LegacyCandidateListsEnabledForCurrentTick &&
                _candidateCache.TryGetValue(
                    attacker,
                    out List<SceneQueryHit> legacyCandidates))
            {
                if (candidateCount < legacyCandidates.Count)
                    legacyCandidates[candidateCount] = candidate;
                else
                    legacyCandidates.Add(candidate);
                RecordLegacyCandidateListTouchForAuthority();
            }

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
            RuntimeEntityHandle validatedAttackerHandle)
        {
            if (attacker == null || target == null || itr == null ||
                targetFrame == null)
            {
                return false;
            }

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
            var candidate = new SceneQueryHit(target, bodyX, itrIndex, itr);
            WriteCollisionCandidateStoreFirst(
                attacker,
                validatedAttackerHandle,
                0,
                in candidate,
                replaceSingle: true);
            if (LegacyCandidateListsEnabledForCurrentTick &&
                _candidateCache.TryGetValue(
                    attacker,
                    out List<SceneQueryHit> legacyCandidates))
            {
                legacyCandidates.Clear();
                legacyCandidates.Add(candidate);
                RecordLegacyCandidateListTouchForAuthority();
            }

            attacker.Runtime.HitCandidateCount = 1;
            return true;
        }

        private int GetCollisionCandidateProducerCount(
            LF2Entity attacker,
            RuntimeEntityHandle validatedAttackerHandle)
        {
            if (_candidateStoreProducerHealthyForCurrentTick &&
                CollisionCandidateStoreBuildRequestedForCurrentTick)
            {
                RuntimeEntityHandle attackerHandle = validatedAttackerHandle;
                if (!attackerHandle.IsValid &&
                    !TryGetCurrentCollisionCandidateAttackerHandle(
                        attacker,
                        out attackerHandle))
                {
                    return attacker?.Runtime?.HitCandidateCount ?? 0;
                }

                if (_candidateStoreShadow.TryGetProducerAttackerRow(
                        attackerHandle,
                        out int storeCount))
                {
                    return storeCount;
                }
            }

            return attacker?.Runtime?.HitCandidateCount ?? 0;
        }

        private bool IsCandidateAttackerCarrierForCurrentTick(LF2Entity attacker)
        {
            if (attacker == null)
                return false;

            if (CollisionCandidateStoreBuildRequestedForCurrentTick &&
                TryGetCurrentCollisionCandidateAttackerHandle(
                    attacker,
                    out RuntimeEntityHandle attackerHandle) &&
                _candidateStoreShadow.TryGetProducerAttackerRow(
                    attackerHandle,
                    out _))
            {
                return true;
            }

            if (LegacyCandidateListsEnabledForCurrentTick)
                return _candidateCache.ContainsKey(attacker);

            // A failed store-only build must not trigger a second collector pass.
            // Continue the original pass only to preserve RNG call order; the
            // consumption source will be locked fail-closed for this tick.
            return IsCollisionCandidateAttackerEligible(
                attacker,
                _candidateProducerTickForCurrentCollection);
        }

        private bool TryGetCurrentCollisionCandidateAttackerHandle(
            LF2Entity attacker,
            out RuntimeEntityHandle attackerHandle)
        {
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            return _world.TryGetCurrentRuntimeHandle(
                attackerSlot,
                attacker,
                out attackerHandle);
        }

        private void WriteCollisionCandidateStoreFirst(
            LF2Entity attacker,
            RuntimeEntityHandle validatedAttackerHandle,
            int candidateIndex,
            in SceneQueryHit hit,
            bool replaceSingle)
        {
            if (!CollisionCandidateStoreBuildRequestedForCurrentTick)
            {
                return;
            }

            if (!_candidateStoreProducerHealthyForCurrentTick ||
                !_candidateStoreShadow.IsBuilding)
            {
                _candidateStoreProducerHealthyForCurrentTick = false;
                return;
            }

            try
            {
                RuntimeEntityHandle attackerHandle = validatedAttackerHandle;
                if (!attackerHandle.IsValid &&
                    !TryGetCurrentCollisionCandidateAttackerHandle(
                        attacker,
                        out attackerHandle))
                {
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.AttackerHandleNotCurrent);
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                    return;
                }

                int targetSlot = hit.TargetSlot;
                if (targetSlot < 0 ||
                    targetSlot >= _world.MaxRuntimeSlotsForServices)
                {
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.TargetSlotOutOfRange);
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                    return;
                }
                if (!_world.TryGetCurrentRuntimeHandle(
                        targetSlot,
                        hit.Target,
                        out RuntimeEntityHandle targetHandle))
                {
                    // Target generation is diagnostic metadata only. It never gates
                    // the authoritative slot-based candidate cache or consumption.
                    _candidateStoreShadow.RecordInvalid(
                        CollisionCandidateStoreMismatchReason.TargetHandleNotCurrent);
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                    return;
                }

                var entry = new CollisionCandidateStoreEntry(
                    targetSlot,
                    targetHandle,
                    hit.BodyX,
                    hit.ItrIndex,
                    hit.RuntimeItr,
                    hit.ZeroAttackerHpOnConsume,
                    hit.ReleaseHeavyHeldTargetOnConsume);
                bool wrote = replaceSingle
                    ? _candidateStoreShadow.TryReplaceSingle(attackerHandle, in entry)
                    : _candidateStoreShadow.TryWriteAt(
                        attackerHandle,
                        candidateIndex,
                        in entry);
                if (!wrote)
                {
                    _candidateStoreShadow.AbortBuild();
                    _candidateStoreProducerHealthyForCurrentTick = false;
                    return;
                }

#if UNITY_INCLUDE_TESTS
                _collisionCandidateStoreWriteCountForSelfCheck++;
                if (!_collisionCandidateStoreFaultInjectedForSelfCheck &&
                    ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck >= 0 &&
                    _collisionCandidateStoreWriteCountForSelfCheck >=
                    ThrowAfterCollisionCandidateStoreAppendCountForSelfCheck)
                {
                    _collisionCandidateStoreFaultInjectedForSelfCheck = true;
                    throw new InvalidOperationException(
                        "Injected collision candidate store dual-write failure.");
                }
#endif
            }
            catch (Exception)
            {
                _candidateStoreShadow.RecordInvalid(
                    CollisionCandidateStoreMismatchReason.UnexpectedShadowException);
                _candidateStoreShadow.AbortBuild();
                _candidateStoreProducerHealthyForCurrentTick = false;
            }
        }

        private enum CollisionCandidateProducerMode
        {
            LegacyOnly = 0,
            LegacyWithStoreShadow = 1,
            StoreWithLegacyOracle = 2,
            StoreOnly = 3,
        }

        private enum CollisionCandidateConsumptionSource
        {
            LegacyOracle = 0,
            StoreAuthority = 1,
            StoreAuthorityFailedClosed = 2,
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

        private static bool CandidateCollectionPairAllowedCached(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalParticipant targetParticipant)
        {
            if (!attackerParticipant.AttackPairCollectionBaseAllowed ||
                !targetParticipant.PairCollectionBaseAllowed)
            {
                return false;
            }

            LF2Entity attacker = attackerParticipant.Entity;
            LF2Entity target = targetParticipant.Entity;
            int attackerVrestKey = attacker.Runtime?.SlotIndex ?? -1;
            if (attackerVrestKey >= 0 &&
                target.ItrRest != null &&
                target.ItrRest.HasVrest(attackerVrestKey))
            {
                return false;
            }

            return !IsBlockedReleasePairCached(
                in attackerParticipant,
                in targetParticipant);
        }

        private static bool IsBlockedReleasePairCached(
            in RoleAwareFormalParticipant attackerParticipant,
            in RoleAwareFormalParticipant targetParticipant)
        {
            int attackerOid = attackerParticipant.CurrentDataObjectId;
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

            LF2Entity attacker = attackerParticipant.Entity;
            LF2Entity target = targetParticipant.Entity;
            LF2FrameData targetFrame = targetParticipant.CurrentFrame;
            return targetParticipant.CurrentDataObjectId == 9 &&
                   (target.Frame?.N ?? -1) == 301 &&
                   targetFrame != null &&
                   targetFrame.hit_a == 999 &&
                   targetFrame.hit_d == 999 &&
                   targetFrame.hit_j == 999 &&
                   attacker.RelationTeam == target.RelationTeam &&
                   attacker.RelationTeam != 0;
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
            return entity?.GetCurrentDataObjectTypeForSimulation() ?? -1;
        }

        private static bool ItrAllowed(
            LF2Entity attacker,
            LF2FrameData attackerFrame,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetFrame)
        {
            LF2FrameData attackerCurrentFrame =
                GetAuthoredCurrentFrame(attacker) ?? attackerFrame;
            LF2FrameData attackerCollisionFrame =
                attacker?.GetCollisionFrameData() ?? attackerFrame;
            return ItrAllowedCore(
                attacker,
                attackerCurrentFrame,
                attackerCollisionFrame,
                itr,
                target,
                targetFrame);
        }

        private static bool ItrAllowedCached(
            LF2Entity attacker,
            LF2FrameData attackerCurrentFrame,
            LF2FrameData attackerCollisionFrame,
            InteractionArea itr,
            LF2Entity target,
            LF2FrameData targetFrame)
        {
            return ItrAllowedCore(
                attacker,
                attackerCurrentFrame,
                attackerCollisionFrame,
                itr,
                target,
                targetFrame);
        }

        private static bool ItrAllowedCore(
            LF2Entity attacker,
            LF2FrameData attackerCurrentFrame,
            LF2FrameData attackerCollisionFrame,
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

        private static bool TryBuildFormalItrAabb(
            LF2Entity attacker,
            LF2FrameData frame,
            InteractionArea itr,
            out SpatialAabbXZ bounds)
        {
            bounds = default;
            if (attacker == null || frame == null || itr == null ||
                !IsReleaseItrGeometry(itr))
            {
                return false;
            }

            WorldRect rect = ItrWorldRect(attacker, frame, itr);
            BuildConservativeNonEmptyXRange(
                rect.X1,
                rect.X2,
                out int minX,
                out int maxX);
            int collisionZ = CollisionZInt(attacker, frame);
            int zHalf = itr.zwidth > 0 ? itr.zwidth : 15;
            bounds = new SpatialAabbXZ(
                minX,
                ClampRect((long)collisionZ - zHalf),
                maxX,
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
            BuildConservativeNonEmptyXRange(
                rect.X1,
                rect.X2,
                out int minX,
                out int maxX);
            int collisionZ = CollisionZInt(target, frame);
            bounds = new SpatialAabbXZ(
                minX,
                collisionZ,
                maxX,
                ClampRect((long)collisionZ + 1));
            return bounds.IsValid;
        }

        private static void BuildConservativeNonEmptyXRange(
            int x1,
            int x2,
            out int minX,
            out int maxX)
        {
            minX = Math.Min(x1, x2);
            maxX = Math.Max(x1, x2);
            if (minX != maxX)
            {
                return;
            }

            minX = ClampRect((long)minX - 1L);
            maxX = ClampRect((long)maxX + 1L);
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

    internal struct RoleAwareFormalParticipant
    {
        public RoleAwareFormalParticipant(
            LF2Entity entity,
            LF2FrameData currentFrame,
            LF2FrameData collisionFrame,
            RuntimeEntityHandle handle)
        {
            Entity = entity;
            CurrentFrame = currentFrame;
            CollisionFrame = collisionFrame;
            Handle = handle;
            HasBody = false;
            HasFallbackBody = false;
            HasAttackItr = false;
            HasFallbackAttackItr = false;
            HasCurrentReleaseBody = false;
            HasCollisionReleaseBody = false;
            HasBodyUnion = false;
            BodyUnionWorld = default;
            HasOrdinaryItrUnion = false;
            OrdinaryItrUnionWorld = default;
            CollisionX = 0;
            CollisionY = 0;
            CollisionZ = 0;
            FacingLeft = false;
            DataObjectType = -1;
            CandidateAttackerCarrier = false;
            CurrentDataObjectId = -1;
            PairCollectionBaseAllowed = false;
            AttackPairCollectionBaseAllowed = false;
            CurrentItrCount = currentFrame?.itrs?.Count ?? 0;
            CollisionItrCount = ReferenceEquals(currentFrame, collisionFrame)
                ? CurrentItrCount
                : collisionFrame?.itrs?.Count ?? 0;
            CollisionBodyCount = collisionFrame?.bodies?.Count ?? 0;
            ExactItrRectOffset = 0;
            ExactItrRectCount = 0;
            ExactBodyRectOffset = 0;
            ExactBodyRectCount = 0;
            HasExactCommonCache = false;
            HasExactAttackCache = false;
            HasExactBodyCache = false;
        }

        public LF2Entity Entity { get; }
        public LF2FrameData CurrentFrame { get; }
        public LF2FrameData CollisionFrame { get; }
        public LF2FrameData Frame => CollisionFrame;
        public RuntimeEntityHandle Handle { get; }
        public bool HasBody { get; set; }
        public bool HasFallbackBody { get; set; }
        public bool HasAttackItr { get; set; }
        public bool HasFallbackAttackItr { get; set; }
        public bool HasCurrentReleaseBody { get; set; }
        public bool HasCollisionReleaseBody { get; set; }
        public bool HasBodyUnion { get; set; }
        public WorldRect BodyUnionWorld { get; set; }
        public bool HasOrdinaryItrUnion { get; set; }
        public WorldRect OrdinaryItrUnionWorld { get; set; }
        public int CollisionX { get; set; }
        public int CollisionY { get; set; }
        public int CollisionZ { get; set; }
        public bool FacingLeft { get; set; }
        public int DataObjectType { get; set; }
        public bool CandidateAttackerCarrier { get; set; }
        public int CurrentDataObjectId { get; set; }
        public bool PairCollectionBaseAllowed { get; set; }
        public bool AttackPairCollectionBaseAllowed { get; set; }
        public int CurrentItrCount { get; set; }
        public int CollisionItrCount { get; set; }
        public int CollisionBodyCount { get; set; }
        public int ExactItrRectOffset { get; set; }
        public int ExactItrRectCount { get; set; }
        public int ExactBodyRectOffset { get; set; }
        public int ExactBodyRectCount { get; set; }
        public bool HasExactCommonCache { get; set; }
        public bool HasExactAttackCache { get; set; }
        public bool HasExactBodyCache { get; set; }
    }

    internal readonly struct RoleAwareFormalExactItrRectEntry
    {
        public RoleAwareFormalExactItrRectEntry(
            InteractionArea itr,
            int itrIndex,
            in WorldRect worldRect)
        {
            Itr = itr;
            ItrIndex = itrIndex;
            WorldRect = worldRect;
        }

        public InteractionArea Itr { get; }
        public int ItrIndex { get; }
        public WorldRect WorldRect { get; }
    }

    internal readonly struct RoleAwareFormalExactBodyRectEntry
    {
        public RoleAwareFormalExactBodyRectEntry(
            int bodyX,
            in WorldRect worldRect)
        {
            BodyX = bodyX;
            WorldRect = worldRect;
        }

        public int BodyX { get; }
        public WorldRect WorldRect { get; }
    }

    internal readonly struct RoleAwareFormalBodyTemplate
    {
        public RoleAwareFormalBodyTemplate(
            List<BodyBox> sourceBodies,
            int sourceBodyCount,
            int sourceCenterX,
            int releaseBodyCount,
            long rightMinX,
            long rightMaxX,
            long leftMinX,
            long leftMaxX,
            bool fastProof)
        {
            SourceBodies = sourceBodies;
            SourceBodyCount = sourceBodyCount;
            SourceCenterX = sourceCenterX;
            ReleaseBodyCount = releaseBodyCount;
            RightMinX = rightMinX;
            RightMaxX = rightMaxX;
            LeftMinX = leftMinX;
            LeftMaxX = leftMaxX;
            FastProof = fastProof;
        }

        public List<BodyBox> SourceBodies { get; }
        public int SourceBodyCount { get; }
        public int SourceCenterX { get; }
        public int ReleaseBodyCount { get; }
        public long RightMinX { get; }
        public long RightMaxX { get; }
        public long LeftMinX { get; }
        public long LeftMaxX { get; }
        public bool FastProof { get; }
    }

    internal sealed class LF2FrameDataReferenceComparer :
        IEqualityComparer<LF2FrameData>
    {
        public static readonly LF2FrameDataReferenceComparer Instance =
            new LF2FrameDataReferenceComparer();

        private LF2FrameDataReferenceComparer()
        {
        }

        public bool Equals(LF2FrameData x, LF2FrameData y) =>
            ReferenceEquals(x, y);

        public int GetHashCode(LF2FrameData obj) =>
            obj == null ? 0 : RuntimeHelpers.GetHashCode(obj);
    }

    internal readonly struct RoleAwareFormalItrEntry
    {
        public RoleAwareFormalItrEntry(
            int participantOrdinal,
            RuntimeEntityHandle handle,
            int itrIndex,
            InteractionArea itr,
            in SpatialAabbXZ bounds)
        {
            ParticipantOrdinal = participantOrdinal;
            Handle = handle;
            ItrIndex = itrIndex;
            Itr = itr;
            Bounds = bounds;
        }

        public int ParticipantOrdinal { get; }
        public RuntimeEntityHandle Handle { get; }
        public int ItrIndex { get; }
        public InteractionArea Itr { get; }
        public SpatialAabbXZ Bounds { get; }
    }

    internal enum RoleAwareSweepEventKind : byte
    {
        // Both end kinds sort before either start kind so X intervals are
        // strictly half-open. BodyStart before ItrStart makes equal-start
        // body/itr entries meet exactly once, at ItrStart.
        BodyEnd = 0,
        ItrEnd = 1,
        BodyStart = 2,
        ItrStart = 3,
    }

    internal readonly struct RoleAwareSweepEvent
    {
        public RoleAwareSweepEvent(
            int x,
            RoleAwareSweepEventKind kind,
            int entryIndex)
        {
            X = x;
            Kind = kind;
            EntryIndex = entryIndex;
        }

        public int X { get; }
        public RoleAwareSweepEventKind Kind { get; }
        public int EntryIndex { get; }
    }

    internal sealed class RoleAwareSweepEventComparer :
        IComparer<RoleAwareSweepEvent>
    {
        public static readonly RoleAwareSweepEventComparer Instance =
            new RoleAwareSweepEventComparer();

        private RoleAwareSweepEventComparer()
        {
        }

        public int Compare(RoleAwareSweepEvent left, RoleAwareSweepEvent right)
        {
            int xComparison = left.X.CompareTo(right.X);
            if (xComparison != 0)
                return xComparison;
            int kindComparison = ((byte)left.Kind).CompareTo((byte)right.Kind);
            return kindComparison != 0
                ? kindComparison
                : left.EntryIndex.CompareTo(right.EntryIndex);
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
