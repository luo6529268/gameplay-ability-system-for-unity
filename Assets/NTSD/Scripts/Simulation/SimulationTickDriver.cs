using System.Collections.Generic;
using MoreMountains.Tools;
using NTSD.Animation.LF2Objects;
using NTSD.App;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Simulation
{
    /// <summary>
    /// Battle scene simulation clock. It owns the fixed 30Hz gameplay tick and delegates the
    /// formal release pass order to NTSDBattleTickSystem.
    /// </summary>
    public class SimulationTickDriver : SingletonBehaviour<SimulationTickDriver>
    {
        [Tooltip("Log each simulation tick start/end.")]
        [SerializeField] private bool debugLogPerTick = false;

        [Tooltip("Start paused until BattleBootstrap resumes the simulation.")]
        [SerializeField] private bool startPaused = true;

        [Header("Debug Info (Read Only)")]
        [SerializeField][MMReadOnly] private int currentTickIndex = 0;
        [SerializeField][MMReadOnly] private float timeAccumulator = 0f;
        [SerializeField][MMReadOnly] private int objectCount = 0;
        [SerializeField][MMReadOnly] private bool paused = true;

        private float _timeAccumulator = 0f;
        private int _tickIndex = 0;

        private SimulationWorld _world;
        private NTSDBattleTickSystem _battleTickSystem;
        private NTSD.Animation.SparkRenderer _sparkRenderer;

        private int _sparkRenderFrame = 0;
        private readonly List<LF2LivingObject> _shakeObjects = new List<LF2LivingObject>(8);

        protected override void OnSingletonAwake()
        {
            paused = startPaused;

            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);

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

            _battleTickSystem?.RunReleaseTick(tickIndex, _sparkRenderFrame);

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

        public void UnbindWorld()
        {
            _world = null;
            _battleTickSystem = null;
        }

        public void RecreateWorld()
        {
            _world = new SimulationWorld();
            _battleTickSystem = new NTSDBattleTickSystem(_world);
            _tickIndex = 0;
            _timeAccumulator = 0f;
        }

        protected override void OnSingletonDestroyed()
        {
            _world = null;
            _battleTickSystem = null;
        }
    }
}