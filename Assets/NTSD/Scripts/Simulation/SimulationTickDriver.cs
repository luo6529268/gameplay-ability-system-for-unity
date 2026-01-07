using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using MoreMountains.Tools;

namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟系统驱动器 - 游戏逻辑的唯一时钟源（单例）
    ///
    /// 职责：
    /// - 在 FixedUpdate 中积累时间，以固定 30Hz 频率驱动 SimulationWorld
    /// - 保证所有游戏逻辑（状态机、连招、物理）在统一时钟下运行
    /// - 对应 FLF 的主循环 (match.TU_trans)
    ///
    /// 架构原则：
    /// - 单一时间源：所有游戏逻辑必须从这里获取时间，不能自己维护 accumulator
    /// - 确定性顺序：SimulationWorld 负责按 SimOrder → StableId 排序
    /// - 渲染分离：Unity 可以在不同帧率渲染，但游戏逻辑必须在 30Hz 运行
    ///
    /// Plan B 变更：
    /// - 改为单例（MMSingleton），自动创建 GameObject
    /// - 不再扫描 ISimTickable，改为驱动 SimulationWorld
    /// - ISimObject 通过 World.Register/Unregister 注册
    ///
    /// Step D1: 强制早期初始化
    /// - RuntimeInitializeOnLoadMethod 确保在任何 Character.Awake() 之前创建
    /// - 保证 StableId 分配不会因为 Instance==null 而 fallback
    /// </summary>
    public class SimulationTickDriver : MMSingleton<SimulationTickDriver>
    {
        /// <summary>
        /// Step D1: 强制早期初始化单例
        /// 确保在任何 Character.Awake() 之前 SimulationTickDriver.Instance 就存在
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void EnsureInstanceExists()
        {
            // 触发 MMSingleton 的 Instance 属性，强制创建单例
            if (Instance == null)
            {
                Debug.LogError("[SimulationTickDriver] EnsureInstanceExists failed! This should never happen.");
            }
            else
            {
                Debug.Log("[SimulationTickDriver] Early initialization complete. Ready for Character StableId allocation.");
            }
        }
        // ==================== 配置 ====================
        [Header("Simulation Settings")]
        [Tooltip("是否启用模拟驱动（Step 1 阶段默认禁用，避免影响现有行为）")]
        [SerializeField] private bool enableDriver = true;

        [Tooltip("是否显示每个 Tick 的调试日志（用于验证执行顺序）")]
        [SerializeField] private bool debugLogPerTick = false;

        [Header("Debug Info (Read Only)")]
        [SerializeField] private int currentTickIndex = 0;
        [SerializeField] private float timeAccumulator = 0f;
        [SerializeField] private int objectCount = 0;  // Plan B: 从 SimulationWorld 获取

        // ==================== 内部状态 ====================
        /// <summary>
        /// 时间累加器（用于推进 30Hz tick）
        /// 每次 FixedUpdate 累加 Time.fixedDeltaTime
        /// 当 >= SIM_DT 时执行一次 SimTick 并减去 SIM_DT
        /// </summary>
        private float _timeAccumulator = 0f;

        /// <summary>
        /// 当前 Tick 索引（从游戏启动开始累加）
        /// </summary>
        private int _tickIndex = 0;

        /// <summary>
        /// 模拟世界（Plan B: 管理所有 ISimObject）
        /// 替代 Plan A 的 ISimTickable 扫描机制
        /// </summary>
        private SimulationWorld _world;

        // Step D2: Legacy ISimTickable 支持已移除
        // 所有 gameplay 逻辑必须通过 ISimObject 注册到 SimulationWorld

        // ==================== Unity 生命周期 ====================

        protected override void Awake()
        {
            base.Awake();  // MMSingleton 初始化

            // 创建 SimulationWorld（Plan B）
            _world = new SimulationWorld();

            Debug.Log("[SimulationTickDriver] Singleton Awake: World created");
        }

        // Step D2: Start() 已移除（不再需要初始化 legacy ISimTickable）

        private void FixedUpdate()
        {
            if (!enableDriver) return;  // 默认禁用

            // 累加时间
            _timeAccumulator += Time.fixedDeltaTime;

            // 推进 30Hz tick
            // 使用 while 循环确保在低帧率时补偿丢失的 tick
            while (_timeAccumulator >= SimulationConstants.SIM_DT)
            {
                _timeAccumulator -= SimulationConstants.SIM_DT;
                _tickIndex++;

                RunOneSimTick(_tickIndex);
            }

            // 同步调试信息
            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world != null ? _world.ObjectCount : 0;
        }

        // ==================== 核心逻辑 ====================
        // Step D2: InitializeLegacyTickables() 已移除

        /// <summary>
        /// 执行一次模拟 Tick
        ///
        /// 对应 FLF 的 match.TU_trans()（Line 284-300）
        /// 按确定性顺序执行所有系统的 SimTick
        ///
        /// Step D2: 只驱动 SimulationWorld（legacy ISimTickable 已移除）
        /// </summary>
        /// <param name="tickIndex">当前 Tick 索引</param>
        private void RunOneSimTick(int tickIndex)
        {
            // ==================== 预留：OnBeforeSimTick 钩子 ====================
            // 未来用途：
            // - 网络输入注入（lockstep）
            // - 回放系统（deterministic replay）
            // - 录制/验证（recording/validation）
            // OnBeforeSimTick?.Invoke(tickIndex);

            if (debugLogPerTick)
            {
                Debug.Log($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");
            }

            // ==================== 执行 SimulationWorld ====================
            if (_world != null)
            {
                if (debugLogPerTick)
                {
                    Debug.Log($"[SimulationTickDriver] World.Tick({tickIndex}) - {_world.ObjectCount} objects");
                }

                _world.Tick(tickIndex);
            }

            // ==================== 执行 LateTick ====================
            if (_world != null)
            {
                if (debugLogPerTick)
                {
                    Debug.Log($"[SimulationTickDriver] World.LateTick({tickIndex})");
                }

                _world.LateTick(tickIndex);
            }

            if (debugLogPerTick)
            {
                Debug.Log($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");
            }

            // ==================== 预留：OnAfterSimTick 钩子 ====================
            // 未来用途：
            // - 网络状态同步
            // - 快照保存（rollback netcode）
            // - 性能分析
            // OnAfterSimTick?.Invoke(tickIndex);
        }

        // ==================== 公共 API ====================

        /// <summary>
        /// 获取 SimulationWorld 实例（Plan B）
        ///
        /// 用途：
        /// - Character Hub 通过 SimulationTickDriver.Instance.World.Register(obj) 注册
        /// - 系统查询世界状态
        /// </summary>
        public SimulationWorld World => _world;

        // Step D2: RefreshLegacyTickables() 已移除（legacy ISimTickable 不再支持）

        /// <summary>
        /// 获取当前 Tick 索引
        /// </summary>
        public int CurrentTickIndex => _tickIndex;

        /// <summary>
        /// 获取当前累加器剩余时间（调试用）
        /// </summary>
        public float RemainingAccumulatorTime => _timeAccumulator;
    }
}
