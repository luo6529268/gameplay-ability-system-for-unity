using System.Collections.Generic;
using System.Linq;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟世界 - 管理所有 ISimObject 的生命周期和执行顺序
    ///
    /// 职责：
    /// - 注册/反注册 ISimObject
    /// - 按确定性顺序执行所有对象的 SimTick/SimLateTick
    /// - 提供 SimContext 依赖注入
    ///
    /// 架构原则（Plan B）：
    /// - 确定性排序：SimOrder（第一优先级）→ StableId（第二优先级）
    /// - Lazy sorting：只在 bucket 变脏时排序，避免每帧开销
    /// - 纯 C# 实现：不依赖 Unity 生命周期（MonoBehaviour）
    /// </summary>
    public class SimulationWorld
    {
        // ==================== 内部数据结构 ====================

        /// <summary>
        /// Bucket - 存储相同 SimOrder 的对象
        /// </summary>
        private class Bucket
        {
            /// <summary>
            /// 对象列表（可能无序）
            /// </summary>
            public List<ISimObject> items = new List<ISimObject>();

            /// <summary>
            /// 是否需要重新排序（当有对象添加/移除时设置为 true）
            /// </summary>
            public bool dirty = false;

            /// <summary>
            /// Lazy sort: 只在需要时按 StableId 排序
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
        /// 按 SimOrder 组织的 bucket 字典
        /// SortedDictionary 保证按 key (SimOrder) 升序遍历
        /// </summary>
        private SortedDictionary<int, Bucket> _buckets = new SortedDictionary<int, Bucket>();

        /// <summary>
        /// 模拟上下文（提供给所有 ISimObject）
        /// </summary>
        private SimContext _context;

        /// <summary>
        /// 下一个自动分配的 StableId（用于本地 AI 等没有网络 ID 的对象）
        /// 单机模式：从 100 开始自动递增
        /// 多人模式：服务器会显式设置 StableId
        /// </summary>
        private int _nextAutoStableId = 100;

        /// <summary>
        /// 场景查询服务（当前为暴力遍历实现，后续可替换为四叉树实现）
        /// </summary>
        public ILF2SceneQuery SceneQuery { get; private set; }

        /// <summary>
        /// ITR kind 语义服务（业务规则层，可替换）
        /// </summary>
        public INTSDItrKindService ItrKindService { get; private set; }

        // ==================== 初始化 ====================

        /// <summary>
        /// 创建 SimulationWorld
        /// </summary>
        public SimulationWorld()
        {
            _context = new SimContext(this);
            ItrKindService = new NTSDItrKindService();
            SceneQuery = new BruteForceSceneQuery(this);
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 注册对象到世界
        ///
        /// 调用时机：
        /// - Character Hub 的 OnEnable()
        /// - 动态创建的 sim 对象初始化时
        ///
        /// 线程安全：仅在主线程调用（Unity 约束）
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

            // 添加到 bucket
            bucket.items.Add(obj);
            bucket.dirty = true;  // 标记为需要重新排序

            // 调用对象的 OnAdded 生命周期方法
            obj.OnAdded(_context);

            Debug.Log($"[SimulationWorld] Registered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        /// <summary>
        /// 从世界移除对象
        ///
        /// 调用时机：
        /// - Character Hub 的 OnDisable()
        /// - 对象销毁时
        ///
        /// 线程安全：仅在主线程调用（Unity 约束）
        /// </summary>
        /// <param name="obj">要移除的对象</param>
        public void Unregister(ISimObject obj)
        {
            if (obj == null)
            {
                Debug.LogError("[SimulationWorld] Cannot unregister null object");
                return;
            }

            int simOrder = obj.SimOrder;
            if (!_buckets.TryGetValue(simOrder, out Bucket bucket))
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in buckets: SimOrder={simOrder}");
                return;
            }

            // 从 bucket 移除
            bool removed = bucket.items.Remove(obj);
            if (!removed)
            {
                Debug.LogWarning($"[SimulationWorld] Object not found in bucket: SimOrder={simOrder}, StableId={obj.StableId}");
                return;
            }

            bucket.dirty = true;  // 标记为需要重新排序（虽然移除不一定需要，但保持一致）

            // 调用对象的 OnRemoved 生命周期方法
            obj.OnRemoved(_context);

            // 如果 bucket 空了，可以考虑移除（可选优化）
            if (bucket.items.Count == 0)
            {
                _buckets.Remove(simOrder);
            }

            Debug.Log($"[SimulationWorld] Unregistered: SimOrder={simOrder}, StableId={obj.StableId}, Type={obj.GetType().Name}");
        }

        /// <summary>
        /// 分配一个新的 StableId
        /// 对应 FLF scene.add() 中的 this.uid++
        ///
        /// StableId 分配规则：
        /// - 从 100 开始自动递增（避免与玩家角色 1-99 冲突）
        /// - 全局递增，与对象类型无关
        /// - 用于确定性排序（同一 SimOrder 内按 StableId 升序执行）
        ///
        /// 调用时机：
        /// - LF2 逻辑对象初始化时（LF2LivingObject.AllocateStableId()）
        /// </summary>
        /// <returns>新分配的 StableId</returns>
        public int AllocateStableId()
        {
            return _nextAutoStableId++;
        }

        /// <summary>
        /// Transit 阶段 - 所有对象执行 Transit
        /// 对应 FLF match.TU_trans 中的 emit_event('transit') 循环
        /// </summary>
        public void TransitTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;
                bucket.EnsureSorted();

                foreach (var obj in bucket.items)
                {
                    if (obj == null) continue;
                    obj.SimTransit(tickIndex);
                }
            }
        }

        /// <summary>
        /// TU 阶段 - 所有对象执行 TU
        /// 对应 FLF match.TU_trans 中的 emit_event('TU') 循环
        /// </summary>
        public void TUTickAll(int tickIndex)
        {
            foreach (var kvp in _buckets)
            {
                Bucket bucket = kvp.Value;
                // 0.2: FlushTasks 后可能有新对象注册，需重新排序确保稳定性
                bucket.EnsureSorted();

                foreach (var obj in bucket.items)
                {
                    if (obj == null) continue;
                    obj.SimTU(tickIndex);
                }
            }
        }

        /// <summary>
        /// 执行一次 LateTick（后期处理）
        ///
        /// 调用时机：所有对象的 Tick 完成后
        /// 用途：视图更新、调试绘制、延迟清理
        ///
        /// 执行顺序：与 Tick 相同（SimOrder → StableId）
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
        /// 收集当前世界中的所有 LF2LivingObject（按 SimOrder → StableId 顺序）
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
        /// 获取当前注册的对象总数（调试用）
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
        /// 获取模拟上下文（只读）
        /// </summary>
        public SimContext Context => _context;
    }
}
