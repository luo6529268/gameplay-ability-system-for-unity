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
        private const int MaxRuntimeSlots = 400;
        private const int DynamicRuntimeSlotStart = 50;
        private readonly bool[] _runtimeSlotUsed = new bool[MaxRuntimeSlots];
        private readonly NTSDEntityRuntime[] _rawRuntimeSlots = CreateRawRuntimeSlots();
        private readonly LF2ItrRestTracker.StateSnapshot[] _rawRestSlots =
            new LF2ItrRestTracker.StateSnapshot[MaxRuntimeSlots];
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
        internal int MaxRuntimeSlotsForServices => MaxRuntimeSlots;
        internal int DynamicRuntimeSlotStartForServices => DynamicRuntimeSlotStart;

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
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        private static NTSDEntityRuntime[] CreateRawRuntimeSlots()
        {
            var slots = new NTSDEntityRuntime[MaxRuntimeSlots];
            for (int slot = 0; slot < slots.Length; slot++)
            {
                slots[slot] = new NTSDEntityRuntime();
                slots[slot].Reset();
            }

            return slots;
        }

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return runtimeSlot >= 0 && runtimeSlot < _rawRuntimeSlots.Length
                ? _rawRuntimeSlots[runtimeSlot]
                : null;
        }

        private void ResetRawRuntimeSlotState(int runtimeSlot)
        {
            GetRawRuntimeSlotState(runtimeSlot)?.Reset();
        }

        private void ResetAllRawRuntimeSlotStates()
        {
            for (int slot = 0; slot < _rawRuntimeSlots.Length; slot++)
                _rawRuntimeSlots[slot].Reset();

            System.Array.Clear(_rawRestSlots, 0, _rawRestSlots.Length);
        }

        public void ResetRuntimeState()
        {
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
            System.Array.Clear(_runtimeSlotUsed, 0, _runtimeSlotUsed.Length);
            ResetAllRawRuntimeSlotStates();
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
                    ResetCooldownsForRuntimeSlot(runtimeSlot, registeredEntity);
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(_context);
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
                if (obj is LF2Entity pendingEntity)
                    ReleaseRuntimeSlot(pendingEntity);
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

            if (bucket == null || !bucket.items.Remove(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            if (obj is LF2Entity entity)
                ReleaseRuntimeSlot(entity);
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

            for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
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

            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int requiredSlot = entity.RequiredRuntimeSlot;
            if (requiredSlot != -1)
            {
                bool requiredSlotInRange = requiredSlot >= 0 && requiredSlot < MaxRuntimeSlots;
                if (!requiredSlotInRange || _runtimeSlotUsed[requiredSlot])
                    return -1;

                _runtimeSlotUsed[requiredSlot] = true;
                return requiredSlot;
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 && existingSlot < MaxRuntimeSlots;
            bool existingSlotInAllowedRange = !requiresDynamicSlot || existingSlot >= DynamicRuntimeSlotStart;
            if (existingSlotInRange && existingSlotInAllowedRange && !_runtimeSlotUsed[existingSlot])
            {
                _runtimeSlotUsed[existingSlot] = true;
                return existingSlot;
            }

            int startSlot = requiresDynamicSlot ? DynamicRuntimeSlotStart : 0;
            int slot = FindFreeRuntimeSlot(startSlot);
            if (slot >= 0)
                return slot;

            return -1;
        }

        private int FindFirstFreeRuntimeSlot(int startSlot, int endSlotExclusive)
        {
            ReleasePendingDestroySlots();

            int end = Mathf.Min(endSlotExclusive, MaxRuntimeSlots);
            for (int slot = Mathf.Max(0, startSlot); slot < end; slot++)
            {
                if (!_runtimeSlotUsed[slot])
                    return slot;
            }

            return -1;
        }

        private int FindFreeRuntimeSlot(int startSlot)
        {
            for (int i = Mathf.Max(0, startSlot); i < MaxRuntimeSlots; i++)
            {
                if (_runtimeSlotUsed[i]) continue;
                _runtimeSlotUsed[i] = true;
                return i;
            }

            return -1;
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
                    if (slot < 0 || slot >= MaxRuntimeSlots)
                        continue;

                    CaptureRawRestSlotState(slot, entity);
                    _runtimeSlotUsed[slot] = false;
                    entity.SetRuntimeSlotIndex(-1);
                    if (!_pendingSlotReleasedDestroy.Contains(entity))
                        _pendingSlotReleasedDestroy.Add(entity);
                }
            }
        }

        private void ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 && slot < MaxRuntimeSlots)
            {
                CaptureRawRestSlotState(slot, entity);
                _runtimeSlotUsed[slot] = false;
            }

            entity.SetRuntimeSlotIndex(-1);
        }

        private void CaptureRawRestSlotState(int runtimeSlot, LF2Entity entity)
        {
            if (runtimeSlot < 0 || runtimeSlot >= _rawRestSlots.Length)
                return;

            _rawRestSlots[runtimeSlot] = entity?.ItrRest?.CaptureState();
        }

        internal void RestoreStageSpawnRestState(int runtimeSlot, LF2Entity entity)
        {
            if (runtimeSlot < 0 || runtimeSlot >= _rawRestSlots.Length ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != runtimeSlot ||
                entity.Runtime.SpawnSemantic != (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return;
            }

            entity.ItrRest?.RestoreState(_rawRestSlots[runtimeSlot]);
            _rawRestSlots[runtimeSlot] = null;
        }

        internal int GetRawRestArest(int runtimeSlot)
        {
            if (runtimeSlot < 0 || runtimeSlot >= _rawRestSlots.Length)
                return 0;

            return _rawRestSlots[runtimeSlot]?.Arest ?? 0;
        }

        internal int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            if (victimSlot < 0 || victimSlot >= _rawRestSlots.Length ||
                attackerSlot < 0 || attackerSlot >= _rawRestSlots.Length)
            {
                return 0;
            }

            LF2ItrRestTracker.StateSnapshot snapshot = _rawRestSlots[victimSlot];
            if (snapshot?.VrestByAttacker == null ||
                !snapshot.VrestByAttacker.TryGetValue(attackerSlot, out int value))
            {
                return 0;
            }

            return value;
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
