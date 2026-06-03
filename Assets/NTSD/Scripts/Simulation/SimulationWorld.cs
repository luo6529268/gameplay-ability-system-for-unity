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
    /// NTSD 对象的确定性模拟调度器。
    /// 对象按 SimOrder 分桶，并按运行时 StableId 排序，从而保证每帧执行顺序稳定。
    /// </summary>
    public class SimulationWorld
    {

        /// <summary>同一 SimOrder 的对象桶；只有桶内容变化后才延迟重新排序。</summary>
        private class Bucket
        {
            public List<ISimObject> items = new List<ISimObject>();

            public bool dirty = false;

            public void EnsureSorted()
            {
                if (dirty)
                {
                    items = items.OrderBy(GetRuntimeStableId).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>按 SimOrder 建立的模拟桶；SortedDictionary 保证 pass 顺序。</summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();

        /// <summary>注册对象时注入的模拟上下文。</summary>
        private SimContext _context;

        /// <summary>给没有显式运行时 ID 的对象自动分配 StableId。</summary>
        private int _nextAutoStableId = 100;

        /// <summary>遍历桶快照期间延迟处理的注销请求。</summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();

        /// <summary>世界正在遍历模拟对象时为 true。</summary>
        private bool _ticking = false;

        private static int GetRuntimeStableId(ISimObject obj)
        {
            return obj is LF2Entity entity ? entity.Runtime.StableId : obj.StableId;
        }

        private static void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        public ILF2SceneQuery SceneQuery { get; private set; }

        public INTSDItrKindService ItrKindService { get; private set; }

        /// <summary>对齐正式版 ntsd_rand() 行为的确定性随机数生成器。</summary>
        public DeterministicRng Rng { get; private set; }

        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);

            // C++ 正式版通过 ntsd_srand() 初始化全局战斗随机数。
            // Unity 暂时使用固定种子，后续由菜单或战斗启动流程接管播种。
            Rng = new DeterministicRng(0x4E545344u);
        }

        /// <summary>将对象注册到对应 SimOrder 桶，并调用 OnAdded 生命周期钩子。</summary>
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

            bucket.items.Add(obj);
            if (obj is LF2Entity registeredEntity)
            {
                registeredEntity.SetRuntimeSlotIndex(obj.StableId);
                registeredEntity.RefreshRuntimeSnapshot();
            }
            bucket.dirty = true;

            obj.OnAdded(_context);

            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        /// <summary>注销对象；如果当前正在 tick 遍历，则延迟到本轮 pass 结束后移除。</summary>
        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (_ticking)
            {
                if (!_pendingUnregister.Contains(obj))
                    _pendingUnregister.Add(obj);
                return;
            }

            UnregisterImmediate(obj);
        }

        /// <summary>从桶中立即移除对象，并调用 OnRemoved 生命周期钩子。</summary>
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

        /// <summary>为动态创建对象分配确定性的 StableId。</summary>
        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        /// <summary>
        /// 执行逐对象串行 tick：Transit、opoint 任务刷新、TU。
        /// opoint 创建出的对象可以进入同一帧后续 pass。
        /// </summary>
        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                var bucketKeys = new List<int>(_buckets.Keys);

                foreach (var key in bucketKeys)
                {
                    if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                    bucket.EnsureSorted();

                    var snapshot = bucket.items.Count > 0
                        ? new List<ISimObject>(bucket.items)
                        : null;

                    if (snapshot == null) continue;

                    foreach (var obj in snapshot)
                    {
                        if (obj == null) continue;
                        obj.SimTransit(tickIndex);
                        RefreshRuntimeSnapshot(obj);
                        var factory = NTSD.Animation.LF2ObjectPointFactory.Instance;
                        if (factory != null)
                        {
                            factory.FlushTasks();
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning("[SimWorld] LF2ObjectPointFactory.Instance is null");
                        }
                        obj.SimTU(tickIndex);
                        RefreshRuntimeSnapshot(obj);
                    }
                }
            }
            finally
            {
                _ticking = false;
                FlushPendingUnregister();
            }
        }

        /// <summary>
        /// 将累计击退写入 living object 速度，并清空本帧击退累计器。
        /// </summary>
        public void FramePostProcessAll()
        {
            foreach (var kvp in _buckets)
            {
                foreach (var obj in kvp.Value.items)
                {
                    if (obj is not LF2LivingObject living) continue;
                    if (living.FrameDelay != 0) continue;

                    if (living.HitCount > 0)
                    {
                        float denom = living.HitCount + 1;
                        living.PS.vx = living.KnockbackVx * 2f / denom;
                        living.PS.vy = living.KnockbackVy * 2f / denom;
                        living.PS.vz = living.KnockbackVz * 2f / denom;
                    }
                    living.KnockbackVx = 0f;
                    living.KnockbackVy = 0f;
                    living.KnockbackVz = 0f;
                    living.HitCount    = 0;
                }
            }
        }

        /// <summary>在交互 pass 前推进 vrest/arest 冷却计数。</summary>
        public void VrestTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2LivingObject living)
                        living.ItrRest?.Tick();
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>执行用于命中和攻击碰撞逻辑的 post-interaction pass。</summary>
        public void PostInteractionTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimPostInteraction(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>执行用于抓取、拾取等早期检测的 pre-interaction pass。</summary>
        public void PreInteractionTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;

                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimPreInteraction(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>按确定性的 SimOrder/StableId 顺序执行对象后期更新。</summary>
        public void LateTick(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                int simOrder = kvp.Key;
                Bucket bucket = kvp.Value;

                foreach (var obj in bucket.items)
                {
                    if (obj == null)
                    {
                        Debug.LogWarning($"[SimulationWorld] Null object in bucket SimOrder={simOrder}, skipping");
                        continue;
                    }

                    obj.SimLateTick(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>按确定性桶顺序收集 living object。</summary>
        public void GetAllLivingObjects(List<LF2LivingObject> dst)
        {
            if (dst == null) return;
            dst.Clear();

            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;
                bucket.EnsureSorted();

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2LivingObject living)
                    {
                        dst.Add(living);
                    }
                }
            }
        }

        /// <summary>按确定性桶顺序收集所有 LF2 entity。</summary>
        public void GetAllEntities(List<LF2Entity> dst)
        {
            if (dst == null) return;
            dst.Clear();

            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;
                bucket.EnsureSorted();

                for (int i = 0; i < bucket.items.Count; i++)
                {
                    if (bucket.items[i] is LF2Entity entity)
                    {
                        dst.Add(entity);
                    }
                }
            }
        }

        public int ObjectCount
        {
            get
            {
                int count = 0;
                foreach (var bucket in _buckets.Values)
                {
                    count += bucket.items.Count;
                }
                return count;
            }
        }

        /// <summary>按模拟 tick 节奏推进对象的 spark timer。</summary>
        public void TickSparkTimers(int renderFrame)
        {
            foreach (var kvp in _buckets)
            {
                foreach (var obj in kvp.Value.items)
                {
                    if (obj is LF2LivingObject living && living.SparkSlotCount > 0)
                        living.TickAllSparkTimers(renderFrame);
                }
            }
        }

        public SimContext Context => _context;

        /// <summary>执行 entity-collision pass，用于处理身体/对象碰撞副作用。</summary>
        public void EntityCollisionTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                var snapshot = kvp.Value.items.Count > 0
                    ? new List<ISimObject>(kvp.Value.items)
                    : null;
                if (snapshot == null) continue;
                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimEntityCollision(tickIndex);
                    RefreshRuntimeSnapshot(obj);
                }
            }
        }

        /// <summary>
        /// 当场上武器数量低于正式版阈值时随机生成场景武器。
        /// 生成位置从可走边界区域中采样。
        /// </summary>
        public void RandomWeaponDropTickAll(int tickIndex)
        {
            int weaponCount = 0;
            foreach (var kvp in _buckets)
            {
                foreach (var obj in kvp.Value.items)
                {
                    if (obj is LF2WeaponBase wb)
                    {
                        int wt = wb.WeaponType;
                        if (wt == 1 || wt == 2 || wt == 4 || wt == 6)
                            weaponCount++;
                    }
                }
            }
            if (weaponCount >= 4) return;
            if (UnityEngine.Random.Range(0, 200) != 0) return;

            var manager = CharacterAnimtorManager.Instance;
            if (manager == null) return;

            var candidates = new System.Collections.Generic.List<int>();
            for (int oid = 100; oid < 200; oid++)
            {
                var wrapper = manager.GetCharacterConfig(oid);
                if (wrapper == null) continue;
                if (oid == 122 || oid == 123)
                {
                    if (UnityEngine.Random.Range(0, 2) == 0) continue;
                }
                candidates.Add(oid);
            }
            if (candidates.Count == 0) return;

            int selectedOid = candidates[UnityEngine.Random.Range(0, candidates.Count)];

            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;

            var boundaryManager = BoundaryWallManager.Instance;
            if (boundaryManager == null || !boundaryManager.TryGetRandomWalkablePoint(out var walkablePoint, insetWorld: 0.9f))
                return;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(selectedOid);
            int flyFrame = -1;
            int minFrame = int.MaxValue;
            if (charData?.frames != null)
            {
                foreach (var f in charData.frames)
                {
                    if (f == null) continue;
                    if (f.frameId > 0 && f.frameId < minFrame) minFrame = f.frameId;
                    if (flyFrame < 0 && f.frameId > 0 && (
                        f.state == LF2States.WeaponInSky ||
                        f.state == LF2States.WeaponThrowing ||
                        f.state == LF2States.HeavyWeaponInSky))
                        flyFrame = f.frameId;
                }
            }
            if (flyFrame < 0) flyFrame = minFrame != int.MaxValue ? minFrame : 0;

            float lf2X = walkablePoint.x * SimulationConstants.PIXELS_PER_UNIT;
            float lf2Z = walkablePoint.y * SimulationConstants.PIXELS_PER_UNIT;
            const float lf2Y = -500f;

            var spawnTask = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();

            spawnTask.opoint = new ObjectPoint
            {
                    oid = selectedOid,
                    kind = 0,
                    action = flyFrame,
                    x = Mathf.RoundToInt(lf2X),
                    y = Mathf.RoundToInt(lf2Y),
                    dvx = 0,
                    dvy = 0,
                    facing = 0,
            };
            spawnTask.parent = null; spawnTask.team = 0;
            spawnTask.pos = new UnityEngine.Vector3(lf2X, lf2Y, 0);
            spawnTask.z = lf2Z; spawnTask.dir = "right"; spawnTask.dvz = 0;
            factory.EnqueueCreateObject(spawnTask);
        }
    }
}
