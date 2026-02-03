using UnityEngine;
using MoreMountains.Tools;
using NTSD.Tools;
using NTSD.Animation;

namespace NTSD.Simulation
{
    /// <summary>
    /// 模拟系统驱动器 - 游戏逻辑的唯一时钟源（Battle 场景内单例）
    ///
    /// 职责：
    /// - 在 FixedUpdate 中积累时间，以固定 30Hz 频率驱动 SimulationWorld
    /// - 保证所有游戏逻辑（状态机、连招、物理）在统一时钟下运行
    /// - 对应 FLF 的主循环 (match.TU_trans)
    ///
    /// 约束：
    /// - 只应存在于 Battle 场景（Additive 加载）
    /// - 不允许自动创建（避免 Menu/Loading 阶段被动污染）
    /// - 默认暂停；由 BattleBootstrap 在一切准备完成后 Resume
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("是否显示每个 Tick 的调试日志（用于验证执行顺序）")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("是否在 Awake 后默认暂停（推荐 true；由 BattleBootstrap 解除暂停）")]
        [SerializeField] private bool startPaused = true;

        [Header("Debug Info (Read Only)")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;

        protected override void OnSingletonAwake()
        {
            paused = startPaused;

            // 创建 SimulationWorld，但默认不运行 Tick（paused=true）。
            // 这样可确保对象在 OnEnable() 注册时 Instance.World 已可用。
            _world = new SimulationWorld();

            Log.Info($"[SimulationTickDriver] Awake. paused={paused}, World created");
        }

        private void FixedUpdate()
        {
            if (paused || _world == null)
            {
                return;
            }

            _timeAccumulator += Time.fixedDeltaTime;

            while (_timeAccumulator >= SimulationConstants.SIM_DT)
            {
                _timeAccumulator -= SimulationConstants.SIM_DT;
                _tickIndex++;

                RunOneSimTick(_tickIndex);
            }

            currentTickIndex = _tickIndex;
            timeAccumulator = _timeAccumulator;
            objectCount = _world.ObjectCount;
        }

        private void RunOneSimTick(int tickIndex)
        {
            if (debugLogPerTick)
            {
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");
            }

            if (_world != null)
            {
                if (debugLogPerTick)
                {
                    Log.Info($"[SimulationTickDriver] World.TransitTickAll({tickIndex}) - {_world.ObjectCount} objects");
                }
                _world.TransitTickAll(tickIndex);
            }

            LF2ObjectPointFactory.Instance.FlushTasks();

            if (_world != null)
            {
                if (debugLogPerTick)
                {
                    Log.Info($"[SimulationTickDriver] World.TUTickAll({tickIndex})");
                }
                _world.TUTickAll(tickIndex);
            }

            if (_world != null)
            {
                if (debugLogPerTick)
                {
                    Log.Info($"[SimulationTickDriver] World.LateTick({tickIndex})");
                }
                _world.LateTick(tickIndex);
            }

            if (debugLogPerTick)
            {
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");
            }
        }

        public SimulationWorld World => _world;

        public int CurrentTickIndex => _tickIndex;

        public float RemainingAccumulatorTime => _timeAccumulator;

        public bool IsPaused => paused;

        public void SetPaused(bool value)
        {
            paused = value;
        }

        /// <summary>
        /// 彻底解绑世界引用（用于 Battle unload 前的清理）
        /// </summary>
        public void UnbindWorld()
        {
            _world = null;
        }

        /// <summary>
        /// 重新创建一个新的世界（仅 Battle 生命周期内使用）
        /// </summary>
        public void RecreateWorld()
        {
            _world = new SimulationWorld();
            _tickIndex = 0;
            _timeAccumulator = 0f;
        }

        protected override void OnSingletonDestroyed()
        {
            _world = null;
        }
    }
}
