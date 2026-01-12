using BeatEmUpTemplate2D;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using NTSD.Help;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using static NTSD.Simulation.NTSDGlobal;

namespace NTSD.Animation
{

    public struct LF2AnimationInfo 
    {
        public int frameIndex;
        public bool IsUp;
    }

    public class FrameAnimationInfo
    {
        public int preNext;
        public int next;
        public LF2FrameData frameData;

    }

    /// <summary>
    /// LF2角色动画播放器 - 运行时组件
    ///
    /// 改进版本：完整复刻 FLF 原版生命周期
    /// - TU_Update: 每个时间单位的主循环（对应 FLF 的 TU_update）
    /// - Frame_Update: 帧数据更新（对应 frame_update）
    /// - Combo_Update: 输入处理（对应 combo_update）
    /// - Frame_Force: 帧力应用（对应 frame_force）
    ///
    /// ⚠️ Step 2 变更：
    /// - 实现 ISimTickable，由外部 SimulationTickDriver 驱动（30Hz）
    /// - 移除内部 FixedUpdate 时钟（不再自己维护 accumulator）
    /// </summary>
    /// <summary>
    /// Physics Plan A Step P6: 移除 ISimTickable
    /// LF2CharacterAnimator 现在由 CharacterSim.SimTick() 驱动（Plan B）
    /// 不再需要 legacy ISimTickable 接口
    /// </summary>
    public class LF2CharacterAnimator : MMMonoBehaviour, ICharacterModule
    {
        [Header("Determinism")]
        [SerializeField]
        [Tooltip("确定性 StableId（用于碰撞/输入等排序）。0 表示自动分配（单机本地）。多人模式应由服务器分配。")]
        private int _stableId = 0;

        public int StableId => _stableId;

        // ==================== 公共属性 ====================
        public int CurrentFrameId => FrameAniInfo.frameData.frameId;
        public LF2FrameData CurrentFrame => FrameAniInfo.frameData;

        // Back-compat: some state logic still reads character._FrameDataWrapper.characterData.* (frame rates etc.)
        // Keep it as a property mapping to FrameCache.Wrapper so we can continue refactoring without breaking callers.
        public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache.Wrapper;
        public Character _Character => GetComponentInParent<Character>();

        // ==================== 物理状态对象（对应 FLF 的 $.ps）====================
        /// <summary>
        /// 物理状态对象（对应 FLF 的 $.ps）
        /// 存储速度和位置，由 ApplyDynamics() 更新
        /// </summary>
        public PhysicsState ps { get; private set; }

        // ==================== 连招检测器 ====================
        private ActionSequenceDetectorModule _actionDetector;

        /// <summary>
        /// 上一帧的 frameId（对应 FLF 的 $.frame.PN）
        /// 用于角色特定逻辑中的帧判断
        /// </summary>
        public int PreviousFrameId => FrameAniInfo.preNext;

        public int NextFrameId => FrameAniInfo.next;

        public FrameAnimationInfo FrameAniInfo = new FrameAnimationInfo();

        /// <summary>
        /// 当前状态值（对应帧数据的 state 字段）
        /// </summary>
        public int CurrentState { get; private set; } = 0;

        /// <summary>
        /// 状态内存（状态切换时自动清空）
        /// 对应 FLF 的 $.statemem
        /// </summary>
        public Dictionary<string, object> StateMem { get; private set; } = new Dictionary<string, object>();

        // ==================== 连招缓冲区（对应 LF2 的 combo_buffer）====================
        public LF2ComboBufferModule ComboBuffer { get; } = new LF2ComboBufferModule();

        // ==================== OPoint / WPoint（剥离为纯数据模块 + factory 注入）====================
        public LF2ObjectPointModule ObjectPointModule { get; } = new LF2ObjectPointModule();
        public LF2WeaponPointModule WeaponPointModule { get; } = new LF2WeaponPointModule();

        // ==================== 帧数据缓存（剥离为独立模块）====================
        public LF2FrameCache FrameCache { get; } = new LF2FrameCache();

        // ==================== 组件缓存 ====================
        private List<Sprite> mergedSprites => CharacterAnimtorManager.Instance.GetCharacterSpriteByID(_Character.CharacterID);
        private SpriteRenderer spriteRenderer;

        // ==================== Step 2: 朝向权威（替代 UnitActions.dir / TurnToDir）====================
        /// <summary>
        /// 当前朝向（表现层权威）
        /// 主要从 ps.dir 推导，兜底用 transform.localRotation
        /// </summary>
        public DIRECTION FacingDir
        {
            get
            {
                if (ps != null && !string.IsNullOrEmpty(ps.dir))
                {
                    return ps.dir == "left" ? DIRECTION.LEFT : DIRECTION.RIGHT;
                }
                // 兜底：从 transform 推导
                return transform.parent != null && transform.parent.localRotation == Quaternion.Euler(0, 180, 0)
                    ? DIRECTION.LEFT
                    : DIRECTION.RIGHT;
            }
        }

        /// <summary>
        /// 设置朝向（表现层 + 数据层同步）
        /// </summary>
        /// <param name="dir">目标朝向</param>
        /// <param name="syncPs">是否同步写回 ps.dir（默认 true）</param>
        public void SetFacingDir(DIRECTION dir, bool syncPs = true)
        {
            // 表现层：翻转角色（通过 Character hub 的 transform）
            var groundTransform = GetGroundTransform();
            if (groundTransform != null)
            {
                groundTransform.localRotation = (dir == DIRECTION.LEFT)
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
            }

            // 数据层：同步 ps.dir
            if (syncPs && ps != null)
            {
                ps.dir = (dir == DIRECTION.LEFT) ? "left" : "right";
            }
        }

        // 视觉偏移：ps.y（跳跃/击飞高度）只影响子节点（Model）的本地 Y，不影响地面平面位置与排序。
        private Vector3 _baseLocalPosition;

        [SerializeField]
        private bool _debugCollisionLog = false;

        [Header("Debug / Combo")]
        [SerializeField]
        private bool _debugComboLog = false;

        // ==================== 帧转换器（对应 LF2 的 trans）====================
        public FrameTransistor trans { get; private set; }

        // ==================== 当前状态 ====================

        /// <summary>
        /// 播放速度倍率（仅调试用，默认 1.0）
        /// ⚠️ Plan B: 仅作为时间缩放调试工具，不用于帧率修正
        /// </summary>
        [SerializeField] [Range(0.1f, 3f)] private float playbackSpeed = 1.0f;

        private LF2AnimationInfo _animationInfo;
        public bool _AllowSwitchDir { get; set; }

        // ⭐ 播放速度倍率（1.0 = 原速，0.5 = 半速，2.0 = 2倍速）

        // 事件
        public System.Action<int> OnFrameChanged;
        public System.Action<int> OnStateChanged;

        private Character _hub;
        private bool _comboDetectorInitialized = false;

        // FLF mechanics.js: this.mass (from spec or global default)
        private float _mass = NTSDGlobal.Default.Machanics.Mass;

        // FLF mech 体系：物理计算层（不继承 Mono，只处理数据/运算）
        private CharacterMechanics _mech;

        // ==================== 缓存委托（避免 per-tick 分配）====================
        // 日志 wrapper：Log.Warn(string, params object[]) 不能直接赋值给 Action<string>
        private static void LogWarnWrapper(string msg) => Tools.Log.Warn(msg);
        private static readonly Action<string> s_logWarn = LogWarnWrapper;
        
        // 边界检测委托缓存（在 ModuleInitialize 中初始化）
        private Func<Vector2, bool> _cachedIsPointWalkable;

        public int ModuleOrder => CharacterModuleOrder.Animator;

        public void ModuleSetup(Character character)
        {
            _hub = character;
        }

        public void ModuleInitialize()
        {
            // 初始化组件缓存（不依赖 CharacterID）
            if (spriteRenderer == null)
            {
                spriteRenderer = this.GetComponent<SpriteRenderer>();
            }

            _baseLocalPosition = transform.localPosition;

            // 初始化帧转换器
            trans = new FrameTransistor(this);

            // 初始化物理计算层（对应 FLF 的 mech 体系）
            _mech = new CharacterMechanics();
            
            // 缓存边界检测委托（避免 per-tick 分配）
            var boundaryMgr = BoundaryWallManager.Instance;
            _cachedIsPointWalkable = boundaryMgr != null ? boundaryMgr.IsPointWalkable : null;

            // 初始化物理状态对象（对应 FLF 的 $.ps）
            ps = new PhysicsState();
            ps.FromUnityPosition(GetGroundTransform().position);  // 从地面平面位置初始化（Character hub transform）
            ps.y = 0;   // 初始在地面
            ps.vx = 0;  // 初始速度为 0
            ps.vy = 0;
            ps.vz = 0;

            _AllowSwitchDir = true;

            // Scheme C: StableId 统一来源为 Character.StableIdRuntime
            if (_hub != null && _hub.StableIdRuntime > 0)
            {
                _stableId = _hub.StableIdRuntime;
            }

            // 初始化连招检测器（不依赖 CharacterID）
            InitializeComboDetector();
        }

        public void ModuleBind()
        {
            // Scheme C: CharacterID-driven bind (dat/frame data)
            ReloadCharacterFrameData();
            OnInitFrameData();
            InitializeDate();
            ComboBuffer.Reset();
            ItrRest.Reset();
            HitCounters.Reset();

            // Bind FLF spec/properties (mass etc.)
            if (_hub != null)
            {
                _mass = NTSDSpec.GetMassOrDefault(_hub.CharacterID);
            }
        }

        public void ModuleUnbind()
        {
            // Keep ps/trans/subscriptions; only release CharacterID-driven caches.
            FrameCache.Clear();
        }

        /// <summary>
        /// Scheme C: StableId should be driven by Character.
        /// </summary>
        public void SetStableId(int stableId)
        {
            _stableId = stableId;
        }

        /// <summary>
        /// 初始化连招检测器
        /// </summary>
        private void InitializeComboDetector()
        {
            if (_comboDetectorInitialized) return;
            _actionDetector = _hub != null ? _hub._ActionSequenceDetector : null;
            if (_actionDetector != null)
            {
                _actionDetector.OnComboDetected += OnComboDetected;
                _comboDetectorInitialized = true;
            }
            else
            {
                Tools.Log.Warn("[LF2CharacterAnimator] ActionSequenceDetectorModule not found on Character hub!");
            }
        }

        private void InitializeDate() 
        {
            _animationInfo.frameIndex = 0;
            _animationInfo.IsUp = true;
            FrameAniInfo.frameData = FrameCache.GetFrameDataById(0);
            FrameAniInfo.preNext = 0;
            FrameAniInfo.next = 0;
        }

        private void OnDestroy()
        {
            // 取消订阅连招事件
            if (_actionDetector != null)
            {
                _actionDetector.OnComboDetected -= OnComboDetected;
            }
        }

        private void OnInitFrameData()
        {
        }

        private void LoadCharacterFrameData()
        {
            FrameCache.Load(CharacterAnimtorManager.Instance.GetCharacterConfig(_Character.CharacterID));
        }

        /// <summary>
        /// 重新加载角色数据（用于运行时切换 Character.CharacterID）。
        /// 注意：该方法应尽量在角色 Start 之前调用（例如 LevelManager 刚 Instantiate 后）。
        /// </summary>
        public void ReloadCharacterFrameData()
        {
            if (_Character == null) return;

            LoadCharacterFrameData();

            // 重置到第一帧（避免引用旧 wrapper 的 frameData）
            FrameAniInfo.frameData = FrameCache.GetFrameDataById(0);
            FrameAniInfo.preNext = 0;
            FrameAniInfo.next = 0;
        }

        // ==================== Physics Plan A Step P6: ISimTickable 已移除 ====================
        // LF2CharacterAnimator 不再实现 ISimTickable
        // 改为由 CharacterSim.SimTick() 调用 Transit() 和 TU_Update()
        // 这消除了 dual clock 和 legacy path

        // ==================== 核心生命周期（对应 FLF 原版）====================

        /// <summary>
        /// Transit 阶段 - 对应 FLF 的 livingobject.transit()
        ///
        /// 职责：
        /// 1. 处理输入（combo_update）
        /// 2. 帧转换（trans.trans - 已在 TU_Update 中处理）
        /// 3. 触发 transit 事件
        /// 4. 应用物理（mech.dynamics）
        ///
        /// 对应 FLF livingobject.js:315-333
        ///
        /// Plan B: Public - 由 CharacterSim.SimTick 调用
        /// </summary>
        public void Transit()
        {
            // Debug: collision logs (global static flag) - set from any instance
            LF2CollisionSystem.DebugLog = _debugCollisionLog;

            // 对齐 FLF: 每个 TU 递减 arest/vrest（最小实现放在 Transit 入口处执行）
            ItrRest.Tick();

            // 1. 处理输入和连招识别
            Combo_Update();

            // 1.5 Phase 1: pre_interaction（基于下一帧的 itr）
            LF2CollisionSystem.ProcessPreInteractionTick();

            // 2. 帧转换（对齐 FLF：wait 递减 + 切帧 + frame_update）
            trans.Trans();

            // 3. 触发 transit 事件（允许状态处理器响应）
            CharacterStates.Instance.HandleStateEvent(this, "transit", null);

            // 5. Phase 0: 最小碰撞闭环（ITR vs BDY overlap）
            // 按 sim tick 只执行一次（内部去重），与具体角色调用顺序无关。
            LF2CollisionSystem.ProcessPostInteractionTick();
        }

        /// <summary>
        /// 输入处理和连招识别 - 对应 FLF 的 combo_update()
        ///
        /// 职责：
        /// - 从 combo_buffer 读取连招
        /// - 触发状态机的 'combo' 事件
        /// - 清除缓冲区（如果处理成功）
        ///
        /// 对应 FLF character.js:1800-1846
        /// </summary>
        private void Combo_Update()
        {
            // ==================== 从缓冲区读取连招 ====================
            // 对应 FLF character.js:1813-1817
            string rawCombo = ComboBuffer.Combo;
            string K = rawCombo;
            if (string.IsNullOrEmpty(K)) { K = null; }

            // 特殊处理：跳跃攻击组合
            if (rawCombo == "jump-att") { K = "jump"; }

            // ==================== 触发状态机的 'combo' 事件 ====================
            // 对应 FLF character.js:1819-1826

            // 1. 当前状态优先处理
            bool res1 = CharacterStates.Instance.HandleStateEvent(this, "combo", K, true);

            // 注意：CharacterStates.HandleStateEvent 内部已经实现了优先级逻辑
            // （当前状态 → 通用状态）
            // ==================== 调用后处理 ====================
            // 对应 FLF character.js:1828-1830

            // 调用当前状态的组合后处理
            CharacterStates.Instance.HandleStateEvent(this, "post_combo", null);

            // 注意：CharacterStates.HandleStateEvent 内部已经处理了
            // 当前状态和通用状态，所以只需要调用一次

            // ==================== 清除缓冲区 ====================
            // 对应 FLF character.js:1832-1845

            ComboBuffer.AfterComboUpdate(handledByState: res1, rawCombo: rawCombo, mappedCombo: K);
        }

        /// <summary>
        /// 对齐 FLF: 在 transit 阶段执行 mech.dynamics() + wpoint()。
        /// - dynamics: position / boundary / friction / gravity
        /// - wpoint: weapon follow update (当前为占位实现)
        ///
        /// 注意：该方法由 CharacterStates 的 "transit" 事件调用，避免在别处重复执行。
        /// </summary>
        internal void Transit_DynamicsAndWPoint()
        {
            ApplyDynamics();
            WPoint_Update();
        }

        /// <summary>
        /// 连招检测到回调
        /// 对应 FLF character.js:1684-1700 的 combo_event
        /// </summary>
        private void OnComboDetected(ComboConfig.ComboDefinition combo)
        {
            if (FrameAniInfo.frameData == null) return;

            if (_debugComboLog)
            {
                Tools.Log.Info(
                    "[NTSD][ComboDetected] StableId={0} detected={1} state={2} frame={3} pre={4} " +
                    "bufBefore={5} allowSwitchDir={6} ps.dir={7} FacingDir={8} " +
                    "IsLeft={9} IsRight={10}",
                    StableId, combo.name, FrameAniInfo.frameData.state, CurrentFrameId, PreviousFrameId,
                    ComboBuffer.Combo ?? "null", _AllowSwitchDir, ps?.dir ?? "null", FacingDir.ToString(),
                    _Character?._CharacterInput?.IsLeft, _Character?._CharacterInput?.IsRight
                );
            }

            ComboBuffer.OnComboDetected(
                combo: combo,
                allowSwitchDir: _AllowSwitchDir,
                setDirectionByString: SetDirectionByString,
                timeoutFrames: NTSDGlobal.Combo.Timeout,
                debugLog: _debugComboLog,
                stableId: StableId
            );
        }

        /// <summary>
        /// 应用物理 - 调用 CharacterMechanics.Step() + Unity 写回
        ///
        /// 职责：
        /// 1. 构造 CharacterMechanicsContext
        /// 2. 调用 _mech.Step() 执行物理计算
        /// 3. 将结果写回 Unity 组件（Transform, Character hub）
        /// 4. 不处理 TU/落地事件（fell/fall_onto_ground 属于 TU_Update 阶段）
        ///
        /// 对齐 FLF：
        /// - mechanics.js: mech.dynamics()
        /// - mechanics.js: mech.blocking_xz()（blocked -> move * 0.1）
        /// </summary>
        private void ApplyDynamics()
        {
            LF2DynamicsApplier.Apply(
                animator: this,
                mechanics: _mech,
                mass: _mass,
                isPointWalkable: _cachedIsPointWalkable,
                logWarning: _debugCollisionLog ? s_logWarn : null,
                debugCollisionLog: _debugCollisionLog,
                groundTransform: GetGroundTransform(),
                baseLocalPosition: _baseLocalPosition
            );
        }

        /// <summary>
        /// Time Unit Update - 每个时间单位的主循环
        /// 对应 FLF 的 TU_update() 方法
        ///
        /// 职责：只负责动画帧的播放和切换
        /// 注意：输入处理在 CharacterInput 中，物理处理在 Character 中
        ///
        /// Plan B: Public - 由 CharacterSim.SimTick 调用
        /// </summary>
        public void TU_Update()
        {
            // ==================== 重置摩擦力（每个 TU 开始）====================
            // 对应 FLF livingobject.js:114
            ps?.ResetFriction();

            // 1. TU 事件（状态更新）
            CharacterStates.Instance.HandleStateEvent(this, "TU", null);
            // 2. 与转换器状态保持同步（调试/外部查看）
        }

        /// <summary>
        /// 对齐 FLF trans.trans() 中的切帧+frame_update 流程
        /// </summary>
        internal void FrameTransitInternal(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            FrameAniInfo.preNext = FrameAniInfo.next;
            FrameAniInfo.next = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
            {
                Tools.Log.Warn("[LF2CharacterAnimator] Invalid frame ID: {0}", targetFrameId);
                return;
            }

            bool _IsTrans = FrameAniInfo.frameData.state != targetFrame.state;
            if(_IsTrans)
            {
                if (_debugComboLog)
                {
                    Tools.Log.Info(
                        "[NTSD][FrameTransit:StateExit] StableId={0} oldState={1} oldFrame={2} " +
                        "toFrame={3} toState={4} bufCombo={5}",
                        StableId, FrameAniInfo.frameData.state, CurrentFrameId,
                        targetFrameId, targetFrame.state, ComboBuffer.Combo ?? "null"
                    );
                }

                // 保留 state_exit 事件分发（给状态机做额外清理）；当前不需要传参
                CharacterStates.Instance.HandleStateEvent(this, "state_exit", ComboBuffer.Combo);
            }

            FrameAniInfo.frameData = targetFrame;

            // 帧变化事件
            OnFrameChanged?.Invoke(FrameAniInfo.frameData.frameId);

            if (_IsTrans) 
            {
                StateMem.Clear();

                bool oldSwitchDir = _AllowSwitchDir;
                _AllowSwitchDir = CharacterStates.Instance.GetStatesSwitchDir(FrameAniInfo.frameData.state);

                CharacterStates.Instance.HandleStateEvent(this, "state_entry", null);
                OnStateChanged?.Invoke(CurrentState);

                if (_debugComboLog)
                {
                    Tools.Log.Info(
                        "[NTSD][FrameTransit:StateEnter] StableId={0} newState={1} newFrame={2} " +
                        "allowSwitchDir {3}->{4} bufCombo={5} " +
                        "IsLeft={6} IsRight={7}",
                        StableId, FrameAniInfo.frameData.state, CurrentFrameId,
                        oldSwitchDir, _AllowSwitchDir, ComboBuffer.Combo ?? "null",
                        _Character?._CharacterInput?.IsLeft, _Character?._CharacterInput?.IsRight
                    );
                }

                if (!switchDirAfterTrans) 
                {
                    if (_AllowSwitchDir && !oldSwitchDir) 
                    {
                        //处理转换后切向
                        if (_Character._CharacterInput.IsLeft)
                            SetDirection(DIRECTION.LEFT);
                        if(_Character._CharacterInput.IsRight)
                            SetDirection(DIRECTION.RIGHT);


                    }
                }
            }

            if (switchDirAfterTrans) 
                SetDirection(FacingDir == DIRECTION.RIGHT ? DIRECTION.LEFT : DIRECTION.RIGHT);
            
            FrameUpdate();
        }

        public void SetDirection(DIRECTION direction)
        {
            SetFacingDir(direction, true);
        }

        public void SetDirectionByString(string dir) 
        {
            DIRECTION targetDir = (dir == "left") ? DIRECTION.LEFT : DIRECTION.RIGHT;
            SetDirection(targetDir);
        }

        internal void FrameUpdate() 
        {
            spriteRenderer.sprite = mergedSprites[FrameAniInfo.frameData.pic];

            // 重置摩擦力

            // 应用帧力（允许状态覆盖）
            if (!CharacterStates.Instance.HandleStateEvent(this, "frame_force", null))
            {
                Frame_Force();
            }

            trans.SetWait(FrameAniInfo.frameData.wait, 99);
            trans.SetNext(FrameAniInfo.frameData.next, 99);

            // 状态 frame 事件
            CharacterStates.Instance.HandleStateEvent(this, "frame", null);
            
            // 对象点处理
            OPoint_Process();

            // 播放音效
            if (!string.IsNullOrEmpty(FrameAniInfo.frameData.sound))
            {
                // TODO: 播放音效
                // AudioManager.PlaySound(_CurrentFrameData.sound);
            }
        }

        // ==================== 动画事件系统 ====================

        /// <summary>
        /// 帧力应用 - 应用动画数据驱动的速度变化
        /// 对应 FLF 的 frame_force() 方法
        ///
        /// 职责：将帧数据中的 dvx, dvy 转换为实际的物理速度
        /// 注意：这是动画数据驱动的，属于动画系统的一部分
        /// </summary>
        private void Frame_Force()
        {
            if (FrameAniInfo.frameData == null) return;
            int dirv = _Character != null && _Character._CharacterInput != null ? _Character._CharacterInput.Dirv : 0;
            LF2FrameForceApplier.Apply(ps, FrameAniInfo.frameData, dirv);
        }

        /// <summary>
        /// 武器点更新 - 更新持有武器的位置跟随角色
        /// 对应 FLF 的 wpoint() 方法
        ///
        /// 职责：根据当前帧的武器点数据，更新武器的位置和状态
        /// </summary>
        private void WPoint_Update()
        {
            WeaponPointModule.ProcessTransit(this);
        }

        /// <summary>
        /// 对象点处理 - 生成投射物、特效等
        /// 对应 FLF 的 opoint() 方法
        ///
        /// 职责：根据当前帧的对象点数据，生成游戏对象（投射物、特效等）
        /// </summary>
        private void OPoint_Process()
        {
            ObjectPointModule.ProcessFrame(this);
        }

        // ==================== 帧播放策略方法 ====================

        /// <summary>
        /// 往返循环播放（对应 FLF 的 frame_ani_oscillate）
        /// 例如：5→6→7→8→7→6→5→6→7...
        ///
        /// 工作原理：
        /// - 超出范围：跳回起始帧
        /// - 到达终点：往回走（to-1）
        /// - 到达起点：往前走（from+1）
        /// - 中间帧：根据上一帧位置判断方向
        /// </summary>
        /// <param name="from">起始帧</param>
        /// <param name="to">结束帧</param>
        /// <param name="au">权限等级（默认50，高于默认的99）</param>
        public void FrameAniOscillate(int from, int to, int au = 50)
        {
            if (_animationInfo.frameIndex < from || _animationInfo.frameIndex > to)
            {
                _animationInfo.IsUp = true;
                _animationInfo.frameIndex = from + 1;
            }

            if (_animationInfo.frameIndex < to && _animationInfo.IsUp)
                trans.SetNext(_animationInfo.frameIndex++, au);
            else if(_animationInfo.frameIndex > from && !_animationInfo.IsUp)
                trans.SetNext(_animationInfo.frameIndex--, au);

            if(_animationInfo.frameIndex == to)
                _animationInfo.IsUp = false;
            if(_animationInfo.frameIndex == from)
                _animationInfo.IsUp = true;
        }

        // ==================== 公共接口方法 ====================

        /// <summary>
        /// 播放指定状态的第一帧
        /// </summary>
        /// <param name="stateName">状态名称（如 "standing", "walking" 等）</param>
        public void PlayFrame(string stateName, bool immediate = false)
        {
            if (!FrameCache.TryGetFramesByName(stateName, out List<LF2FrameData> frameDataList))
            {
                Tools.Log.Warn("[LF2CharacterAnimator] State '{0}' not found in character data!", stateName);
                return;
            }

            // 排序并获取第一帧的 frameId
            frameDataList.Sort((a, b) => a.frameId.CompareTo(b.frameId));
            int targetFrameId = frameDataList[0].frameId;

            // 如需立即应用（初始化等），直接切帧一次，否则仅排队
            if (immediate || FrameAniInfo.frameData == null)
            {
                FrameTransitInternal(targetFrameId, false, 0);
            }
            trans.Frame(targetFrameId, 20);  // 权限20，高于默认权限
        }

        public void PlayFrameByID(int frameid, bool immediate = false)
        {
            if (frameid >= LF2FrameCache.MaxFrameIdExclusive || frameid < 0)
            {
                Tools.Log.Error("Frame ID out of range");
                return;
            }

            // 如需立即应用（初始化等），直接切帧一次，否则仅排队
            if (immediate || FrameAniInfo.frameData == null)
            {
                FrameTransitInternal(frameid, false, 0);
            }

            // 使用帧转换器切换到该帧
            trans.Frame(frameid, 20);  // 权限20，高于默认权限
        }

        /// <summary>
        /// 直接跳转到指定帧ID
        /// 对应 LF2 的 $.trans.frame()
        /// </summary>
        /// <param name="frameId">目标帧ID</param>
        /// <param name="authority">权限等级（默认20）</param>
        public void TransitionToFrame(int frameId, int authority = 20)
        {
            trans.Frame(frameId, authority);
        }

        /// <summary>
        /// 设置下一帧（不立即切换，等 wait 结束后切换）
        /// </summary>
        /// <param name="frameId">目标帧ID</param>
        /// <param name="authority">权限等级（默认99）</param>
        public void SetNextFrame(int frameId, int authority = 99)
        {
            trans.SetNext(frameId, authority);
        }

        public bool GetStateMemory<T>(string key, out T value)
        {
            if (StateMem.TryGetValue(key, out object obj) && obj is T t)
            {
                value = t;
                return true;
            }

            value = default;
            return false;
        }

        public void SetStateMemory<T>(string key, T value)
        {
            StateMem[key] = value;
        }

        /// <summary>
        /// 通过State值获取第一帧ID
        /// 用于 GAS Ability 通过状态值激活技能
        /// </summary>
        /// <param name="targetState">目标状态值（如 0=Standing, 301=技能1）</param>
        /// <returns>该状态的第一帧ID，未找到返回-1</returns>
        public int GetFirstFrameByState(int targetState)
        {
            int id = FrameCache.GetFirstFrameByState(targetState);
            if (id < 0)
            {
                Tools.Log.Warn("[LF2CharacterAnimator] State {0} not found in character data!", targetState);
            }
            return id;
        }

        public LF2FrameData GetFrameDataById(int frameId)
        {
            return FrameCache.GetFrameDataById(frameId);
        }

        private float GetCurrentSpriteWidthPx()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null) return 0f;
            return spriteRenderer.sprite.textureRect.width;
        }

        public float GetSpriteWidthPxForCollision()
        {
            return GetCurrentSpriteWidthPx();
        }

        // ==================== FLF ITR Rest（arest/vrest）====================
        // 对齐 FLF: livingobject.js itr_arest_update / itr_vrest_update / TU_update 递减

        public LF2ItrRestTracker ItrRest { get; } = new LF2ItrRestTracker();

        // ==================== Phase 1: hit counters ====================
        // 对齐 FLF: character.js hit() 中的 health.fall / health.bdefend（Phase 1 仅用于动画/边界）
        public LF2HitCountersModule HitCounters { get; } = new LF2HitCountersModule();

        private Transform GetGroundTransform()
        {
            // Step 2: 优先使用 Character hub 的 transform（不再依赖 unitActions）
            if (_Character != null) return _Character.transform;
            if (transform.parent != null) return transform.parent;
            return transform;
        }
    }
}
