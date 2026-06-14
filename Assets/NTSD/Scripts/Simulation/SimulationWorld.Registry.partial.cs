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
        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();
        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;
        private readonly List<LF2Entity> _entityScratch = new List<LF2Entity>(128);
        private int _cameraX;
        private int _cameraVel;

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

        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);
            Rng = new DeterministicRng(0x4E545344u);
            Runtime = new BattleRuntimeState();
            Runtime.Reset();
        }

        public void ResetRuntimeState()
        {
            Runtime ??= new BattleRuntimeState();
            Runtime.Reset();
            Rng?.Seed(0x4E545344u);
        }

        public int CurrentTickIndex => Runtime?.Flow?.CurrentTickIndex ?? 0;
        public int SparkRenderFrame => Runtime?.Flow?.SparkRenderFrame ?? 0;
        public int BattleGameModeId => Runtime?.Match?.BattleGameModeId ?? 0;
        public int LocalGameModeId => Runtime?.Match?.LocalGameModeId ?? 0;
        public int Difficulty => Runtime?.Match?.Difficulty ?? 2;
        public int BackgroundId => Runtime?.Match?.BackgroundId ?? -1;
        public int MatchSeed => Runtime?.Match?.Seed ?? 0;
        public int AiPhaseGate => Runtime?.Flow?.AiPhaseGate ?? 0;
        public int BattleExitCountdown => Runtime?.Flow?.BattleExitCountdown ?? 0;
        public int RouteOutRequest => Runtime?.Flow?.RouteOutRequest ?? 0;
        public int Mode2Request => Runtime?.Flow?.Mode2Request ?? 0;

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

        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

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
                registeredEntity.SetRuntimeSlotIndex(AllocateRuntimeSlot(registeredEntity));
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
            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: SimOrder={simOrder}");
                return;
            }

            bool removed = bucket.items.Remove(obj);
            if (!removed)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in bucket: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;
            if (obj is LF2Entity entity)
                ReleaseRuntimeSlot(entity);
            obj.OnRemoved(_context);

            if (bucket.items.Count == 0)
                _buckets.Remove(simOrder);

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
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
            GetAllEntities(_entityScratch);
            for (int i = 0; i < _entityScratch.Count; i++)
            {
                LF2Entity entity = _entityScratch[i];
                if (entity?.Runtime == null || !entity.Runtime.PendingFlushDestroy)
                    continue;

                entity.Runtime.PendingFlushDestroy = false;
                entity.OnTransitDestroy();
            }

            _entityScratch.Clear();
        }

        private bool IsActiveForCurrentPass(ISimObject obj)
        {
            if (obj == null || _pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null && entity.Runtime.PendingFlushDestroy)
                return false;

            return true;
        }

        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        private int AllocateRuntimeSlot(LF2Entity entity)
        {
            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            if (existingSlot >= 0 && existingSlot < MaxRuntimeSlots && !_runtimeSlotUsed[existingSlot])
            {
                _runtimeSlotUsed[existingSlot] = true;
                return existingSlot;
            }

            int startSlot = entity.UsesDynamicRuntimeSlot() ? DynamicRuntimeSlotStart : 0;
            int slot = FindFreeRuntimeSlot(startSlot);
            if (slot >= 0)
                return slot;

            if (startSlot > 0)
            {
                slot = FindFreeRuntimeSlot(0);
                if (slot >= 0)
                    return slot;
            }

            Debug.LogWarning($"[SimulationWorld] Runtime slot exhausted, fallback to StableId order: StableId={entity.StableId}, Type={entity.GetType().Name}");
            return entity.StableId;
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

        private void ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 && slot < MaxRuntimeSlots)
                _runtimeSlotUsed[slot] = false;

            entity.SetRuntimeSlotIndex(-1);
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
                    count += bucket.items.Count;
                }
                return count;
            }
        }

        public SimContext Context => _context;
    }
}
