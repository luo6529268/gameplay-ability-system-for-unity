using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// SimulationWorld 注册、运行时槽位和基础上下文。
    /// </summary>
    public partial class SimulationWorld
    {
        /// <summary>同一 SimOrder 的对象桶；只有桶内容变化后才延迟重新排序。</summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();
            public bool dirty = false;

            public void EnsureSorted(System.Func<ISimObject, int> stableIdSelector)
            {
                if (dirty)
                {
                    items = items.OrderBy(stableIdSelector).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>按 SimOrder 建立的模拟桶，SortedDictionary 保证 pass 顺序。</summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();
        /// <summary>注册对象时注入的模拟上下文。</summary>
        private SimContext _context;
        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId = 100;
        internal const int AuthorityRuntimeSlotCapacity =
            BattleRuntimeProfilePolicy.AuthorityRuntimeSlotCapacity;
        private const int DynamicRuntimeSlotStart = 50;
        private readonly BattleRuntimeProfile activeRuntimeProfile;
        private readonly RuntimeSlotTable _runtimeSlots;
        private readonly RuntimeRestStore _runtimeRestStore;
        private readonly int maxActiveRuntimeEntities;
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();
        private readonly List<LF2Entity> _pendingSlotReleasedDestroy = new List<LF2Entity>();
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);
        private int _cameraX;
        private int _cameraVel;

        public int ReleaseCameraX => _cameraX;
        internal bool IsUnityFixedWorldCameraStateClear => _cameraX == 0 && _cameraVel == 0;
        internal int RuntimeSlotCapacity => _runtimeSlots.LogicalCapacity;
        internal int MaxRuntimeSlotsForServices => RuntimeSlotCapacity;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;
        internal BattleRuntimeProfile RuntimeProfileForServices => activeRuntimeProfile;
        internal CollisionBroadphaseBackend CollisionBroadphaseForServices { get; }
        internal int ClaimedRuntimeSlotCountForServices => _runtimeSlots.ClaimedCount;
        internal ulong RuntimeSlotOccupancyEpochForServices =>
            _runtimeSlots.OccupancyEpoch;
        public BattleRuntimeProfile RuntimeProfileForDiagnostics => activeRuntimeProfile;
        public int RuntimeSlotCapacityForDiagnostics => _runtimeSlots.LogicalCapacity;
        public CollisionBroadphaseBackend CollisionBroadphaseForDiagnostics =>
            CollisionBroadphaseForServices;
        public int ClaimedRuntimeSlotCountForDiagnostics => _runtimeSlots.ClaimedCount;
        internal RuntimeRestStore RuntimeRestStoreForServices => _runtimeRestStore;

        private int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null) return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        private int CompareRuntimeSlotOrder(LF2Entity a, LF2Entity b)
        {
            int cmp = GetRuntimeSlotOrder(a).CompareTo(GetRuntimeSlotOrder(b));
            if (cmp != 0) return cmp;
            return (a?.StableId ?? int.MaxValue).CompareTo(b?.StableId ?? int.MaxValue);
        }

        private void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        private List<int> GetBucketKeySnapshot()
        {
            return _buckets.Count > 0 ? new List<int>(_buckets.Keys) : null;
        }

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

        public SimulationWorld(
            BattleRuntimeProfile runtimeProfile,
            int runtimeSlotCapacity,
            CollisionBroadphaseBackend collisionBroadphase = CollisionBroadphaseBackend.BruteForce)
        {
            if (runtimeSlotCapacity < DynamicRuntimeSlotStart)
                throw new System.ArgumentOutOfRangeException(nameof(runtimeSlotCapacity),
                    "Runtime slot capacity must include the dynamic slot band.");
            if (runtimeProfile == BattleRuntimeProfile.Authority400 &&
                runtimeSlotCapacity != AuthorityRuntimeSlotCapacity)
            {
                throw new System.ArgumentException(
                    "Authority400 worlds must use exactly 400 runtime slots.",
                    nameof(runtimeSlotCapacity));
            }

            activeRuntimeProfile = runtimeProfile;
            CollisionBroadphaseForServices = collisionBroadphase;
            maxActiveRuntimeEntities = runtimeProfile == BattleRuntimeProfile.MobileExtended
                ? BattleRuntimeProfilePolicy.MobileMaxActiveRuntimeEntities
                : int.MaxValue;
            _runtimeSlots = new RuntimeSlotTable(runtimeSlotCapacity, 20, DynamicRuntimeSlotStart);
            _runtimeRestStore = new RuntimeRestStore(runtimeSlotCapacity);
            aiInputSlots = new LF2Entity[runtimeSlotCapacity];
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this, collisionBroadphase);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return _runtimeSlots.GetRawRuntime(runtimeSlot);
        }

        internal bool TryGetCurrentRuntimeHandle(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        internal bool TryResolveRuntimeHandle(RuntimeEntityHandle handle, out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        public bool TryGetCurrentRuntimeHandleForDiagnostics(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return _runtimeSlots.TryGetCurrentHandle(runtimeSlot, expectedEntity, out handle);
        }

        public bool TryResolveRuntimeHandleForDiagnostics(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return _runtimeSlots.TryResolve(handle, out entity);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Returns currently claimed, pass-active runtime entities by resolving a fresh
        /// generation-checked handle for every runtime slot. This intentionally does
        /// not use bucket/pass queries so diagnostic cleanup can find leaked entries.
        /// </summary>
        public void GetActiveRuntimeEntitySnapshotForDiagnostics(List<LF2Entity> dst)
        {
            if (dst == null)
                return;

            dst.Clear();
            for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed || view.Entity == null || view.Generation == 0)
                    continue;

                var handle = new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!_runtimeSlots.TryResolve(handle, out LF2Entity entity) ||
                    entity == null ||
                    entity.Runtime?.PendingFlushDestroy == true ||
                    dst.Contains(entity))
                {
                    continue;
                }

                dst.Add(entity);
            }

            dst.Sort(CompareRuntimeSlotOrder);
        }

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
            if (!_runtimeSlots.IsAddressable(runtimeSlot))
            {
                view = default;
                return false;
            }

            view = _runtimeSlots.GetReadOnlyView(runtimeSlot);
            return true;
        }

        private void ResetRawRuntimeSlotState(int runtimeSlot)
        {
            GetRawRuntimeSlotState(runtimeSlot)?.Reset();
        }

        public void ResetRuntimeState()
        {
            _battlePresentation.Reset();
            ResetRegisteredObjects();

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

        private void ResetRegisteredObjects()
        {
            (SceneQuery as BruteForceSceneQuery)?.ResetFormalSpatialBroadphase();

            var registeredObjects = new HashSet<ISimObject>();
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys != null)
            {
                for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
                {
                    int key = bucketKeys[keyIndex];
                    if (!_buckets.TryGetValue(key, out Bucket bucket))
                        continue;

                    for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                    {
                        ISimObject item = bucket.items[itemIndex];
                        if (item != null)
                            registeredObjects.Add(item);
                    }
                }
            }

            _ticking = false;
            _pendingUnregister.Clear();
            _pendingSlotReleasedDestroy.Clear();
            _entityScratch.Clear();

            foreach (ISimObject item in registeredObjects)
            {
                item.OnRemoved(_context);
                if (item is not LF2Entity entity)
                    continue;

                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                entity.ItrRest?.Unbind(false);
                entity.ItrRest?.Reset();
                entity.Reset();
                entity.Runtime?.Reset();
                entity.SetRuntimeSlotIndex(-1);
                entity.ClearRequiredRuntimeSlot();
                entity.FrameCache?.Clear();
                if (entity.Frame != null)
                {
                    entity.Frame.PN = 0;
                    entity.Frame.Prev = 0;
                    entity.Frame.N = 0;
                    entity.Frame.D = null;
                    entity.Frame.Prev2 = 0;
                    entity.Frame.Prev2D = null;
                }

                entity.Trans?.Reset();
                entity.Effect?.Reset();
                entity.Sprite?.SetPresentationSuppressed(true);
                entity.Sprite?.Hide();
                entity.Sprite?.HideShadow();
            }

            _buckets.Clear();
            _runtimeSlots.Reset();
            _runtimeRestStore.ResetWorld();
        }

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
        public bool NeedClearInput => Runtime?.Flow?.NeedClearInput ?? false;
        public List<BattleStageCampaignData> StageCampaigns => Runtime?.StageCampaigns;
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

        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // A pooled instance can be reused during the same dynamic late-slot scan.
            // Finalize its queued old lifecycle before registering the new one, and
            // remove the pending entry so the pass-finally flush cannot delete it.
            if (_pendingUnregister.Remove(obj))
                UnregisterImmediate(obj);

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity registeredEntity)
            {
                _pendingSlotReleasedDestroy.Remove(registeredEntity);
                registeredEntity.ItrRest?.Unbind(false);
                int runtimeSlot = AllocateRuntimeSlot(registeredEntity);
                registeredEntity.SetRuntimeSlotIndex(runtimeSlot);
                registeredEntity.ClearRequiredRuntimeSlot();
                if (runtimeSlot < 0)
                {
                    if (bucket.items.Count == 0)
                        _buckets.Remove(simOrder);
                    Debug.LogWarning(
                        $"[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                        $"StableId={registeredEntity.StableId}, Type={registeredEntity.GetType().Name}");
                    return;
                }

                ResetRawRuntimeSlotState(runtimeSlot);
                if (registeredEntity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
                {
                    if (!ResetCooldownsForRuntimeSlot(runtimeSlot, registeredEntity))
                    {
                        RollbackRuntimeSlotRegistration(registeredEntity, runtimeSlot);
                        if (bucket.items.Count == 0)
                            _buckets.Remove(simOrder);
                        Debug.LogError(
                            $"[SimulationWorld] Runtime rest bind failed; registration rejected: " +
                            $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                        return;
                    }
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(_context);
            if (obj is LF2Entity addedEntity &&
                TryGetCurrentRuntimeHandle(
                    addedEntity.Runtime.SlotIndex,
                    addedEntity,
                    out RuntimeEntityHandle runtimeHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    addedEntity.Renderer,
                    runtimeHandle);
            }
            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (obj is LF2Entity pendingEntity &&
                    !ReleaseRuntimeSlotAndClearPresentationBinding(pendingEntity))
                {
                    return;
                }
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            _buckets.TryGetValue(bucketKey, out Bucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                List<int> bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys != null)
                {
                    for (int i = 0; i < bucketKeys.Count; i++)
                    {
                        int candidateKey = bucketKeys[i];
                        if (!_buckets.TryGetValue(candidateKey, out Bucket candidateBucket) ||
                            !candidateBucket.items.Contains(obj))
                        {
                            continue;
                        }

                        bucketKey = candidateKey;
                        bucket = candidateBucket;
                        break;
                    }
                }
            }

            if (bucket == null)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            if (obj is LF2Entity entity &&
                entity.Runtime?.SlotIndex >= 0 &&
                !ReleaseRuntimeSlotAndClearPresentationBinding(entity))
            {
                return;
            }

            if (!bucket.items.Remove(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(bucketKey);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={bucketKey}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        private void FlushPendingUnregister()
        {
            if (_pendingUnregister.Count == 0) return;
            foreach (var obj in _pendingUnregister)
                UnregisterImmediate(obj);
            _pendingUnregister.Clear();
        }

        private void FlushPendingEntityDestroy()
        {
            // Pending entities are deliberately hidden from active pass queries. Scan the
            // runtime registry directly so the C# authority's late FreeEntity boundary still finalizes them.
            _entityScratch.Clear();
            for (int i = 0; i < _pendingSlotReleasedDestroy.Count; i++)
            {
                LF2Entity released = _pendingSlotReleasedDestroy[i];
                if (released != null && !_entityScratch.Contains(released))
                    _entityScratch.Add(released);
            }
            _pendingSlotReleasedDestroy.Clear();

            for (int runtimeSlot = 0; runtimeSlot < RuntimeSlotCapacity; runtimeSlot++)
            {
                LF2Entity entity = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (entity?.Runtime != null &&
                    entity.Runtime.PendingFlushDestroy &&
                    !_entityScratch.Contains(entity))
                {
                    _entityScratch.Add(entity);
                }
            }

            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity.Runtime != null)
                    entity.Runtime.PendingFlushDestroy = false;

                entity.FreeEntityLikeExe();
            }

            _entityScratch.Clear();
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            if (obj == null || _pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null)
            {
                if (entity.Runtime.OidMergeDormant)
                    return false;

                if (entity.Runtime.PendingFlushDestroy)
                    return false;
            }

            return true;
        }

        internal bool IsActiveForCurrentPassInternal(ISimObject obj)
        {
            return IsActiveForCurrentPass(obj);
        }

        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        private int AllocateRuntimeSlot(LF2Entity entity)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int requiredSlot = entity.RequiredRuntimeSlot;
            if (requiredSlot != -1)
            {
                if (requiredSlot >= RuntimeSlotCapacity &&
                    !TryGrowDesktopRuntimeSlots((long)requiredSlot + 1))
                {
                    return -1;
                }

                if (!_runtimeSlots.TryClaim(requiredSlot, entity, out _))
                    return -1;

                return requiredSlot;
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < RuntimeSlotCapacity;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            int minimumExistingSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            if (existingSlotInRange && existingSlotInAllowedRange &&
                existingSlot >= minimumExistingSlot &&
                _runtimeSlots.TryClaim(existingSlot, entity, out _))
            {
                return existingSlot;
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int allocatedSlot = _runtimeSlots.AllocateLowest(startSlot, entity, out _);
            if (allocatedSlot >= 0 || !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
                return allocatedSlot;

            return _runtimeSlots.AllocateLowest(startSlot, entity, out _);
        }

        private int FindFirstFreeRuntimeSlot(int startSlot, int endSlotExclusive)
        {
            ReleasePendingDestroySlots();

            if (_runtimeSlots.ClaimedCount >= maxActiveRuntimeEntities)
                return -1;

            bool scansCurrentTail = endSlotExclusive >= RuntimeSlotCapacity;
            int slot = _runtimeSlots.PeekLowest(startSlot, endSlotExclusive);
            if (slot >= 0 || !scansCurrentTail ||
                !TryGrowDesktopRuntimeSlots((long)RuntimeSlotCapacity + 1))
            {
                return slot;
            }

            return _runtimeSlots.PeekLowest(startSlot, RuntimeSlotCapacity);
        }

        private bool TryGrowDesktopRuntimeSlots(long minimumCapacity)
        {
            if (minimumCapacity <= RuntimeSlotCapacity)
                return true;
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
            return true;
        }

        private void ReleasePendingDestroySlots()
        {
            List<int> bucketKeys = GetBucketKeySnapshot();
            if (bucketKeys == null)
                return;

            for (int keyIndex = 0; keyIndex < bucketKeys.Count; keyIndex++)
            {
                int key = bucketKeys[keyIndex];
                if (!_buckets.TryGetValue(key, out Bucket bucket))
                    continue;

                for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                {
                    if (bucket.items[itemIndex] is not LF2Entity entity ||
                        entity.Runtime == null ||
                        !entity.Runtime.PendingFlushDestroy)
                    {
                        continue;
                    }

                    int slot = entity.Runtime.SlotIndex;
                    if (slot < 0 || slot >= RuntimeSlotCapacity)
                        continue;

                    if (object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity) &&
                        ReleaseRuntimeSlotAndClearPresentationBinding(entity) &&
                        !_pendingSlotReleasedDestroy.Contains(entity))
                    {
                        _pendingSlotReleasedDestroy.Add(entity);
                    }
                }
            }
        }

        private bool ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                return true;
            if (slot >= RuntimeSlotCapacity ||
                !object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(slot), entity))
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release without the matching claim: " +
                    $"EntitySlot={slot}, StableId={entity.StableId}");
                return false;
            }

            bool wasBound = entity.ItrRest?.IsBound == true;
            if (wasBound && entity.ItrRest.BoundVictimSlot != slot)
            {
                Debug.LogError(
                    $"[SimulationWorld] Refusing runtime slot release with a mismatched rest binding: " +
                    $"EntitySlot={slot}, BoundVictimSlot={entity.ItrRest.BoundVictimSlot}, " +
                    $"StableId={entity.StableId}");
                return false;
            }
            if (wasBound && !entity.ItrRest.Unbind(false))
                return false;

            if (!_runtimeSlots.Release(slot, entity))
            {
                if (wasBound && !entity.ItrRest.Bind(_runtimeRestStore, slot, false))
                {
                    Debug.LogError(
                        $"[SimulationWorld] Failed to restore runtime rest binding after slot release rollback: " +
                        $"Slot={slot}, StableId={entity.StableId}");
                }
                return false;
            }

            entity.SetRuntimeSlotIndex(-1);
            return true;
        }

        private bool ReleaseRuntimeSlotAndClearPresentationBinding(LF2Entity entity)
        {
            NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                entity?.Renderer);
            if (ReleaseRuntimeSlot(entity))
                return true;

            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 &&
                TryGetCurrentRuntimeHandle(slot, entity, out RuntimeEntityHandle restoredHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.BindOwnerRuntime(
                    entity.Renderer,
                    restoredHandle);
            }

            return false;
        }

        private void RollbackRuntimeSlotRegistration(LF2Entity entity, int runtimeSlot)
        {
            entity?.ItrRest?.Unbind(false);
            if (entity != null &&
                object.ReferenceEquals(_runtimeSlots.GetCurrentOccupant(runtimeSlot), entity))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                _runtimeSlots.Release(runtimeSlot, entity);
            }
            entity?.SetRuntimeSlotIndex(-1);
        }

        internal bool RestoreStageSpawnRestState(int runtimeSlot, LF2Entity entity)
        {
            if (!_runtimeSlots.IsAddressable(runtimeSlot) ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != runtimeSlot ||
                entity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return false;
            }

            return entity.ItrRest != null &&
                   entity.ItrRest.Bind(_runtimeRestStore, runtimeSlot, false);
        }

        internal int GetRawRestArest(int runtimeSlot)
        {
            return _runtimeRestStore.GetARest(runtimeSlot);
        }

        internal int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            return _runtimeRestStore.GetVRest(victimSlot, attackerSlot);
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                var bucketKeys = GetBucketKeySnapshot();
                if (bucketKeys == null) return 0;

                foreach (int simOrder in bucketKeys)
                {
                    if (!_buckets.TryGetValue(simOrder, out Bucket bucket)) continue;
                    for (int i = 0; i < bucket.items.Count; i++)
                    {
                        ISimObject obj = bucket.items[i];
                        if (obj is LF2Entity entity)
                        {
                            if (_pendingUnregister.Contains(entity))
                                continue;

                            if (entity.Runtime != null &&
                                (entity.Runtime.OidMergeDormant || entity.Runtime.PendingFlushDestroy))
                                continue;
                        }

                        count++;
                    }
                }
                return count;
            }
        }

        public SimContext Context => _context;
    }
}
