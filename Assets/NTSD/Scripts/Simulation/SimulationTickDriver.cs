using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Tools;
using UnityEngine;

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
        private NTSD.Animation.SparkRenderer _sparkRenderer;

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

        private int _sparkRenderFrame = 0;
        private readonly List<LF2LivingObject> _shakeObjects = new List<LF2LivingObject>(8);

        private void LateUpdate()
        {
            if (_sparkRenderer == null)
            {
                _sparkRenderer = AppManager.Instance?.SparkRenderer;
                if (_sparkRenderer == null)
                    _sparkRenderer = gameObject.MMGetOrAddComponent<NTSD.Animation.SparkRenderer>();
            }
            _sparkRenderFrame++;
            _sparkRenderer.RenderAll(_world);
            ApplyVisualShakeAll();
        }

        // 反汇编 0x41D272: ShakeTimer(this+8) > -25 才调用 sub_413E10
        // 反汇编 sub_413E10 0x413E19: FrameDelay < 0 时 x_offset = toggle*6-3（±3 像素）
        // dword_449098 每渲染帧 0/1 交替 → 对应 _sparkRenderFrame & 1
        private void ApplyVisualShakeAll()
        {
            if (_world == null) return;
            _world.GetAllLivingObjects(_shakeObjects);
            if (_shakeObjects.Count == 0) return;
            int toggle = _sparkRenderFrame & 1;
            const float ppu = 100f;
            float xOffset = (toggle * 6 - 3) / ppu;
            for (int i = 0; i < _shakeObjects.Count; i++)
            {
                var obj = _shakeObjects[i];
                if (obj.ShakeTimer <= -25) continue;
                if (obj.FrameDelay >= 0) continue;
                
                var root = (obj as LF2Character)?.EntityTransform ?? obj.Renderer?.transform;
                if (root == null) continue;
                
                var pos = root.position;
                pos.x += xOffset;
                root.position = pos;
            }
        }

        private void RunOneSimTick(int tickIndex)
        {
            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} START ==========");

            if (_world != null)
            {
                // vrest/arest 全局递减 pass：对应反汇编 GameMode_Process 循环1（伪C 15440-15468）
                // 反汇编中 vrest/arest 递减在 kind=1/2/3/7 碰撞判定同一循环，发生在 TU/kind=0 碰撞之前
                _world.VrestTickAll(tickIndex);
            }

            if (_world != null)
            {
                // PreInteraction pass：对应反汇编 GameMode_Process kind=1/2/3/7 抓取/拾取
                // 在 SerialTickAll 之前执行，与反汇编循环1（vrest递减 + 抓取碰撞）顺序一致
                _world.PreInteractionTickAll(tickIndex);
            }

            if (_world != null)
            {
                // 串行执行：对齐反汇编 sub_416240 循环1，所有 entity 先全部推进帧
                _world.SerialTickAll(tickIndex);
            }

            if (_world != null)
            {
                // PostInteraction pass：对齐反汇编 sub_42C8C0 循环2
                // 所有 entity 帧推进完成后统一做碰撞检测，消除帧推进顺序影响
                _world.PostInteractionTickAll(tickIndex);
            }

            if (_world != null)
            {
                // Frame_PostProcess pass：对应反汇编 Frame_PostProcess（0x0041BF00）
                // Knockback 累加器 → PS.vx/vy，完成后清零，在 SerialTickAll 之后立即执行
                _world.FramePostProcessAll();
            }

            if (_world != null)
                _world.LateTick(tickIndex);

            if (_world != null)
                _world.TickSparkTimers(_sparkRenderFrame);

            if (debugLogPerTick)
                Log.Info($"[SimulationTickDriver] ========== SimTick {tickIndex} END ==========");
        }

        public SimulationWorld World => _world;
        public int SparkRenderFrame => _sparkRenderFrame;
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
