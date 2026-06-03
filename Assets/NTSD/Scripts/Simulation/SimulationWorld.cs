using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.LevelEditor;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟世界 - 管理所�?ISimObject 的生命周期和执行顺序
    ///
    /// 职责�?
    /// - 注册/反注�?ISimObject
    /// - 按确定性顺序执行所有对象的 SimTick/SimLateTick
    /// - 提供 SimContext 依赖注入
    ///
    /// 架构原则（Plan B）：
    /// - 确定性排序：SimOrder（第一优先级）�?StableId（第二优先级�?
    /// - Lazy sorting：只�?bucket 变脏时排序，避免每帧开销
    /// - �?C# 实现：不依赖 Unity 生命周期（MonoBehaviour�?
    /// </summary>
    public class SimulationWorld
    {
        // ==================== 内部数据结构 ====================

        /// <summary>
        /// Bucket - 存储相同 SimOrder 的对�?
        /// </summary>
        private class Bucket
        {
            /// <summary>
            /// 对象列表（可能无序）
            /// </summary>
            public List<ISimObject> items = new List<ISimObject>();

            /// <summary>
            /// 是否需要重新排序（当有对象添加/移除时设置为 true�?
            /// </summary>
            public bool dirty = false;

            /// <summary>
            /// Lazy sort: 只在需要时�?StableId 排序
            /// </summary>
            public void EnsureSorted()
            {
                if (dirty)
                {
                    items = items.OrderBy(obj => obj.StableId).ToList();
                    dirty = false;
                }
            }
        }

        /// <summary>
        /// �?SimOrder 组织�?bucket 字典
        /// SortedDictionary 保证�?key (SimOrder) 升序遍历
        /// </summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();

        /// <summary>
        /// 模拟上下文（提供给所�?ISimObject�?
        /// </summary>
        private SimContext _context;

        /// <summary>
        /// 下一个自动分配的 StableId（用于本�?AI 等没有网�?ID 的对象）
        /// 单机模式：从 100 开始自动递增
        /// 多人模式：服务器会显式设�?StableId
        /// </summary>
        private int _nextAutoStableId = 100;

        /// <summary>
        /// 延迟注销队列：Tick 期间调用 Unregister() 的对象先入队，Tick 结束后统一移除�?
        /// 防止在遍�?_buckets 期间修改字典结构导致 InvalidOperationException�?
        /// </summary>
        private readonly List<ISimObject> _pendingUnregister = new List<ISimObject>();

        /// <summary>
        /// 是否正在执行 Tick（SerialTickAll 等遍历期间为 true�?
        /// </summary>
        private bool _ticking = false;

        /// <summary>
        /// 场景查询服务（当前为暴力遍历实现，后续可替换为四叉树实现�?
        /// </summary>
        public ILF2SceneQuery SceneQuery { get; private set; }

        /// <summary>
        /// ITR kind 语义服务（业务规则层，可替换�?
        /// </summary>
        public INTSDItrKindService ItrKindService { get; private set; }

        // ==================== 初始�?====================

        /// <summary>
        /// 确定性随机数生成器（对应 FLF match.js:787-795 $.randomseed�?
        /// </summary>
        public DeterministicRng Rng { get; private set; }

        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);

            // FLF manager.js:229-230  randomseed.seed(824163532)
            // FLF match.js:787-789    rand.seed(this.manager.random())
            var managerRng = new DeterministicRng(824163532);
            Rng = new DeterministicRng((int)(managerRng.Next() * int.MaxValue));
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 注册对象到世�?
        ///
        /// 调用时机�?
        /// - Character Hub �?OnEnable()
        /// - 动态创建的 sim 对象初始化时
        ///
        /// 线程安全：仅在主线程调用（Unity 约束�?
        /// </summary>
        /// <param name="obj">要注册的对象</param>
        public void Register(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot register null object");
                return;
            }

            // 获取或创建对应的 bucket
            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                bucket = new Bucket();
                _buckets[simOrder] = bucket;
            }

            // 检查是否已注册（避免重复）
            if (bucket.items.Contains(obj))
            {
                Debug.LogWarning($"[SimulationWorld] Object already registered: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            // 添加�?bucket
            bucket.items.Add(obj);
            bucket.dirty = true;  // 标记为需要重新排�?

            // 调用对象�?OnAdded 生命周期方法
            obj.OnAdded(_context);

            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        /// <summary>
        /// 从世界移除对�?
        ///
        /// 调用时机�?
        /// - Character Hub �?OnDisable()
        /// - 对象销毁时
        ///
        /// 线程安全：仅在主线程调用（Unity 约束�?
        /// </summary>
        /// <param name="obj">要移除的对象</param>
        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            // Tick 期间延迟注销，防止在遍历 _buckets 时修改字典结�?
            if (_ticking)
            {
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

        /// <summary>
        /// 分配一个新�?StableId
        /// 对应 FLF scene.add() 中的 this.uid++
        ///
        /// StableId 分配规则�?
        /// - �?100 开始自动递增（避免与玩家角色 1-99 冲突�?
        /// - 全局递增，与对象类型无关
        /// - 用于确定性排序（同一 SimOrder 内按 StableId 升序执行�?
        ///
        /// 调用时机�?
        /// - LF2 逻辑对象初始化时（LF2LivingObject.AllocateStableId()�?
        /// </summary>
        /// <returns>新分配的 StableId</returns>
        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        /// <summary>
        /// 串行执行所有对象的完整 Tick（Transit �?FlushTasks �?TU�?
        /// 对应 NTSD 反汇�?GameMode_Process�?x41BDA0）的对象串行处理顺序�?
        ///   对象 A 完整执行后才开始对�?B，而非分层执行�?
        /// 效果：低 StableId 对象优先命中；opoint 在当前对�?TU 前已注册到世界�?
        /// </summary>
        public void SerialTickAll(int tickIndex)
        {
            _ticking = true;
            try
            {
                // 快照 _buckets �?key 列表，防�?FlushTasks 触发 Register() �?SortedDictionary 添加�?key
                var bucketKeys = new List<int>(_buckets.Keys);

                foreach (var key in bucketKeys)
                {
                    if (!_buckets.TryGetValue(key, out Bucket bucket)) continue;
                    bucket.EnsureSorted();

                    // 快照当前 items，防�?FlushTasks 触发新对象注册时修改集合
                    var snapshot = bucket.items.Count > 0
                        ? new List<ISimObject>(bucket.items)
                        : null;

                    if (snapshot == null) continue;

                    foreach (var obj in snapshot)
                    {
                        if (obj == null) continue;
                        // 对齐反汇�?sub_416240 串行顺序（循�?）：
                        //   Entity_FrameAdvance（帧推进/物理�?
                        // 碰撞检测（PostInteraction）在所�?entity SerialTickAll 完成后统一执行（循�?�?
                        obj.SimTransit(tickIndex);
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
        /// Knockback 累加器写入物理速度 pass（对应反汇编 Frame_PostProcess 0x0041BF00�?
        ///
        /// �?SerialTickAll 完成后立即调用，�?state 限制，对所有激�?entity 执行�?
        ///   if (FrameDelay==0 && HitCount>0): vx = KnockbackVx*2/(HitCount+1); vy 同理
        ///   清零 KnockbackVx/Vy/Vz �?HitCount（HitCount==0 时也清零 Knockback�?
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
                        // 反汇�?0x41BF5B-0x41BF6B：[+38h](KnockbackVz) 同样写入 [+50h](vz)
                        living.PS.vz = living.KnockbackVz * 2f / denom;
                    }
                    living.KnockbackVx = 0f;
                    living.KnockbackVy = 0f;
                    living.KnockbackVz = 0f;
                    living.HitCount    = 0;
                }
            }
        }

        /// <summary>
        /// vrest/arest 全局递减 pass（对�?NTSD 反汇�?GameMode_Process sub_41BDA0 碰撞判定前循环）
        ///
        /// �?SerialTickAll 完成后、PreInteractionTickAll 之前统一执行一次，
        /// 对所有对象递减 vrest/arest，与反汇�?先递减 �?再判�?顺序对齐�?
        /// </summary>
        public void VrestTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;

                // 快照防止 Tick 触发新对象注册时修改集合（同 PreInteractionTickAll�?
                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj is LF2LivingObject living)
                        living.ItrRest?.Tick();
                }
            }
        }

        /// <summary>
        /// PostInteraction 全局 pass（对�?NTSD 反汇�?sub_42C8C0 循环2�?
        ///
        /// �?SerialTickAll（循�?，所�?entity 帧推进）完成后统一执行，处�?kind=0/4 碰撞判定�?
        /// 对齐原版：所�?entity 先全部推进帧，再统一�?hit 检测�?
        /// </summary>
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
                }
            }
        }

        /// <summary>
        /// PreInteraction 全局 pass（对�?NTSD 反汇�?GameMode_Process sub_41BDA0�?
        ///
        /// �?SerialTickAll 完成后执行，处理 kind=1/2/3/7（抓取、拾取）的碰撞判定�?
        /// 先推进帧再判定，与原版帧推进后碰撞检测顺序对齐�?
        /// </summary>
        public void PreInteractionTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;

                // 快照防止 PreInteraction 触发新对象注册时修改集合（同 SerialTickAll�?
                var snapshot = bucket.items.Count > 0
                    ? new List<ISimObject>(bucket.items)
                    : null;

                if (snapshot == null) continue;

                foreach (var obj in snapshot)
                {
                    if (obj == null) continue;
                    obj.SimPreInteraction(tickIndex);
                }
            }
        }

        /// <summary>
        /// 执行一�?LateTick（后期处理）
        ///
        /// 调用时机：所有对象的 Tick 完成�?
        /// 用途：视图更新、调试绘制、延迟清�?
        ///
        /// 执行顺序：与 Tick 相同（SimOrder �?StableId�?
        /// </summary>
        /// <param name="tickIndex">当前 Tick 索引</param>
        public void LateTick(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                int simOrder = kvp.Key;
                Bucket bucket = kvp.Value;

                // 使用相同的排序（bucket 已在 Tick 中排序）
                foreach (var obj in bucket.items)
                {
                    if (obj == null)
                    {
                        Debug.LogWarning($"[SimulationWorld] Null object in bucket SimOrder={simOrder}, skipping");
                        continue;
                    }

                    obj.SimLateTick(tickIndex);
                }
            }
        }

        /// <summary>
        /// 收集当前世界中的所�?LF2Entity（按 SimOrder �?StableId 顺序�?
        /// </summary>
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

        /// <summary>
        /// 收集当前世界中的所�?LF2Entity（角�?+ 武器 + 技能）
        /// </summary>
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

        /// <summary>
        /// 获取当前注册的对象总数（调试用�?
        /// </summary>
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

        /// <summary>
        /// 推进所�?LF2LivingObject �?spark timer�?0Hz sim tick 内调用）�?
        /// </summary>
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

        /// <summary>
        /// 获取模拟上下文（只读�?
        /// </summary>
        public SimContext Context => _context;

        /// <summary>
        /// EntityCollision pass（对应反汇编 Entity_Collision sub_4138F0 0x00421FBB�?
        ///
        /// �?FramePostProcessAll 之后执行，处理武器地�?边界碰撞�?N-1~N-5 特殊分支�?
        /// 反汇编中此循环紧�?Frame_PostProcess�?x004219CB）之后�?
        /// </summary>
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
                }
            }
        }

        /// <summary>
        /// N-6: 随机场景掉落武器全局 pass（反汇编 0x004215FA 区域�?
        /// �?SerialTickAll 之后调用一次：场上武器/特效实体�?&lt; 4 �?rand(200)==0 时随机生成武�?
        /// </summary>
        public void RandomWeaponDropTickAll(int tickIndex)
        {
            // 反汇�?0x4215BA-0x4215CE：仅统计 entity_type==1/2/4/6 的实体（不含 type=0 特效�?type=3 粘附武器�?
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

            // 反汇�?0x004216B5~0x0042178C：位置基于可走区域随机，不读角色坐标
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
