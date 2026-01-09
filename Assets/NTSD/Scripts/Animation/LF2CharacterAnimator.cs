using BeatEmUpTemplate2D;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using NTSD.Help;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Pool;
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
        /// <summary>
        /// 连招缓冲区
        /// 对应 FLF 的 $.combo_buffer
        /// </summary>
        private class ComboBuffer
        {
            public string combo;      // 当前连招名称
            public int timeout;       // 超时时间
        }
        private ComboBuffer _comboBuffer = new ComboBuffer();

        [MMReadOnly] public LF2CharacterDataWrapper _FrameDataWrapper;

        // ==================== 帧数据缓存 ====================
        /// <summary>
        /// 主数据结构：按 frameId 索引的帧数组（运行时高频访问，O(1)）
        /// frameId 范围：0-399（999等特殊值单独处理）
        /// </summary>
        private readonly LF2FrameData[] _frames = new LF2FrameData[400];

        /// <summary>
        /// 辅助数据结构：按 frameName 分组的字典（初始化/状态切换时使用，低频）
        /// 用于 PlayFrame(stateName) 等方法
        /// </summary>
        private readonly Dictionary<string, List<LF2FrameData>> _framesByName = new Dictionary<string, List<LF2FrameData>>();

        // ==================== 组件缓存 ====================
        private List<Sprite> mergedSprites => CharacterAnimtorManager.Instance.GetCharacterSpriteByID(_Character.CharacterID);
        private SpriteRenderer spriteRenderer;
        public UnitActions unitActions { get; private set; }  // 改为公共属性，供 CharacterStates 访问

        // 视觉偏移：ps.y（跳跃/击飞高度）只影响子节点（Model）的本地 Y，不影响地面平面位置与排序。
        private Vector3 _baseLocalPosition;

        [Header("Debug / Collision Volumes")]
        [SerializeField]
        private bool _debugDrawBodyVolumes = false;

        [SerializeField]
        private bool _debugDrawItrVolumes = false;

        [SerializeField]
        private bool _debugCollisionLog = false;

        [Header("Debug / Combo")]
        [SerializeField]
        private bool _debugComboLog = false;

        // ==================== 帧转换器（对应 LF2 的 trans）====================
        public FrameTransistor trans { get; private set; }

        // ==================== 当前状态 ====================


        // ⭐ LF2 帧率控制（对应 FLF global.js:94 GC.framerate = 30）
        // ⚠️ Step 2 变更：不再使用内部时钟，由外部 SimulationTickDriver 驱动
        private const float LF2_FRAME_RATE = 60f;  // 保留常量（未来 Step 4 将改为 30）
        private const float LF2_FRAME_TIME = 1f / LF2_FRAME_RATE;

        // ⚠️ 已废弃：不再使用内部 accumulator（由 SimulationTickDriver 控制）
        // private float _frameAccumulator = 0f;

        /// <summary>
        /// 播放速度倍率（仅调试用，默认 1.0）
        /// ⚠️ Plan B: 仅作为时间缩放调试工具，不用于帧率修正
        /// </summary>
        [SerializeField] [Range(0.1f, 3f)] private float playbackSpeed = 1.0f;

        private LF2AnimationInfo _animationInfo;
        private bool _AllowSwitchDir;

        // ⭐ 播放速度倍率（1.0 = 原速，0.5 = 半速，2.0 = 2倍速）

        // 事件
        public System.Action<int> OnFrameChanged;
        public System.Action<int> OnStateChanged;

        private Character _hub;
        private bool _comboDetectorInitialized = false;

        // FLF mechanics.js: this.mass (from spec or global default)
        private float _mass = NTSDGlobal.Default.Machanics.Mass;

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

            unitActions = GetComponentInParent<UnitActions>();
            _baseLocalPosition = transform.localPosition;

            // 初始化帧转换器
            trans = new FrameTransistor(this);

            // 初始化物理状态对象（对应 FLF 的 $.ps）
            ps = new PhysicsState();
            ps.FromUnityPosition(GetGroundTransform().position);  // 从地面平面位置初始化（父节点/UnitActions）
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

            // Bind FLF spec/properties (mass etc.)
            if (_hub != null)
            {
                _mass = NTSDSpec.GetMassOrDefault(_hub.CharacterID);
            }
        }

        public void ModuleUnbind()
        {
            // Keep ps/trans/subscriptions; only release CharacterID-driven caches.
            _FrameDataWrapper = null;
            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = null;
            }
            _framesByName.Clear();
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
                Debug.LogWarning("[LF2CharacterAnimator] ActionSequenceDetectorModule not found on Character hub!");
            }
        }

        private void InitializeDate() 
        {
            _animationInfo.frameIndex = 0;
            _animationInfo.IsUp = true;
            FrameAniInfo.frameData = _frames[0];
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
            _FrameDataWrapper = CharacterAnimtorManager.Instance.GetCharacterConfig(_Character.CharacterID);

            OnInitFrameDataList();
        }

        /// <summary>
        /// 重新加载角色数据（用于运行时切换 Character.CharacterID）。
        /// 注意：该方法应尽量在角色 Start 之前调用（例如 LevelManager 刚 Instantiate 后）。
        /// </summary>
        public void ReloadCharacterFrameData()
        {
            if (_Character == null) return;

            // 清空缓存
            for (int i = 0; i < _frames.Length; i++)
            {
                _frames[i] = null;
            }
            _framesByName.Clear();

            LoadCharacterFrameData();

            // 重置到第一帧（避免引用旧 wrapper 的 frameData）
            FrameAniInfo.frameData = _frames[0];
            FrameAniInfo.preNext = 0;
            FrameAniInfo.next = 0;
        }

        private void OnInitFrameDataList()
        {
            foreach (var frameData in _FrameDataWrapper.characterData.frames)
            {
                // 1. 填充主数组（按 frameId 索引，运行时高频访问）
                // frameId 范围：0-399，超出范围的（如999）跳过
                if (frameData.frameId >= 0 && frameData.frameId < _frames.Length)
                {
                    _frames[frameData.frameId] = frameData;
                }

                // 2. 填充辅助字典（按 frameName 分组，低频访问）
                if (_framesByName.TryGetValue(frameData.frameName, out List<LF2FrameData> frameDataList))
                {
                    frameDataList.Add(frameData);
                }
                else
                {
                    frameDataList = new List<LF2FrameData>(5);
                    frameDataList.Add(frameData);
                    _framesByName.Add(frameData.frameName, frameDataList);
                }
            }
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
            TickItrRest();

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
            string K = _comboBuffer.combo;
            if (string.IsNullOrEmpty(K)) { K = null; }

            // 特殊处理：跳跃攻击组合
            if (_comboBuffer.combo == "jump-att") { K = "jump"; }

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

            if (_comboBuffer.combo == "jump-att")
            {
                // 特殊处理跳跃攻击
                if (res1)
                {
                    _comboBuffer.combo = "att";  // 降级为普通攻击
                }
            }
            else
            {
                // 清理缓冲区的条件：
                // 1. 连招事件被处理（res1 或 res2 为 true）
                // 2. 或者是方向键（不持久化）
                if (res1 ||K == "left" || K == "right" || K == "up" || K == "down")
                {
                    _comboBuffer.combo = null;
                }
            }
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
                Debug.Log(
                    $"[NTSD][ComboDetected] StableId={StableId} detected={combo.name} state={FrameAniInfo.frameData.state} frame={CurrentFrameId} pre={PreviousFrameId} " +
                    $"bufBefore={_comboBuffer.combo ?? "null"} allowSwitchDir={_AllowSwitchDir} ps.dir={ps?.dir ?? "null"} ua.dir={unitActions?.dir.ToString() ?? "null"} " +
                    $"IsLeft={_Character?._CharacterInput?.IsLeft} IsRight={_Character?._CharacterInput?.IsRight}"
                );
            }

            // ==================== combo_event 逻辑 ====================
            // 对应 FLF character.js:1684-1700

            string K = combo.name;

            // 1. 处理方向键切换（对应 FLF character.js:1687-1692）
            switch (K)
            {
                case "left":
                case "right":
                    if (_AllowSwitchDir) { SetDirectionByString(K); }
                    break;
            }

            // 2. 处理连招优先级冲突（对应 FLF character.js:1694-1699）
            // 同一帧内的连击冲突，优先级高的生效
            // TODO: 实现优先级系统（需要 priority 映射）

            if (_comboBuffer.timeout == NTSDGlobal.Combo.Timeout && ComboConfig.GetComboPriority(K) < ComboConfig.GetComboPriority(_comboBuffer.combo))
                return;

            // 3. 设置到缓冲区（不立即播放！）
            _comboBuffer.combo = K;
            _comboBuffer.timeout = NTSDGlobal.Combo.Timeout;  // TODO: 从配置读取

            if (_debugComboLog)
            {
                Debug.Log($"[NTSD][ComboDetected] StableId={StableId} bufAfter={_comboBuffer.combo ?? "null"}");
            }
        }

        /// <summary>
        /// 应用物理 - 完全对齐 FLF 的 mech.dynamics()
        ///
        /// 职责（严格按 FLF 顺序）：
        /// 1. 应用速度到位置：ps.x += ps.vx
        /// 2. 边界检测与修正
        /// 3. 更新 Unity 组件位置（Rigidbody2D.velocity, transform.position）
        /// 4. 应用摩擦力（只在地面）
        /// 5. 应用重力（只在空中）
        ///
        /// 对应 FLF mechanics.js:319-377
        /// </summary>
        private void ApplyDynamics()
        {
            // Physics Plan A Step P2: 移除 Rigidbody2D 依赖检查
            // ps 是唯一的物理真值，不需要 Rigidbody2D
            if (unitActions == null || ps == null) return;

            // ==================== 1. P3: 确定性位移解算（blocking_xz + BoundaryWall）====================
            // 对应 FLF Line 326-327: if (!this.blocking_xz()) { ps.x += ps.vx; ps.z += ps.vz; }
            // P3 实现：candidate full → X-only → Z-only → stop

            // 保存旧位置（用于回退）
            float oldX = ps.x;
            float oldZ = ps.z;

            // 尝试 full 移动
            ps.x += ps.vx;
            ps.z += ps.vz;

            // P3: 检测是否越出边界
            // FLF 一致性说明：
            // - FLF 的舞台边界只用脚底点 (ps.x/ps.z) 与 bg.width / bg.zboundary 约束（clamp）
            // - 不使用 bdy/footprint Rect 来约束舞台边界（否则会造成可行走区域被碰撞盒“缩小”）
            // - 因此这里用脚底点检测 BoundaryWall（Unity X/Y 平面 = ps.x/ps.z）
            {
                Vector2 footPoint = ps.GetGroundPoint2D();

                // 单层边界：Walkable union（ps.y 为视觉高度，不参与边界）
                if (!BoundaryWallManager.Instance.IsPointWalkable(footPoint))
                {
                    // Full 移动越界，尝试 X-only
                    if (_debugCollisionLog) Debug.LogWarning("[Boundary] Full 越界，尝试 X-only");
                    ps.x = oldX + ps.vx;
                    ps.z = oldZ;

                    footPoint = ps.GetGroundPoint2D();
                    if (!BoundaryWallManager.Instance.IsPointWalkable(footPoint))
                    {
                        // X-only 也越界，尝试 Z-only
                        if (_debugCollisionLog) Debug.LogWarning("[Boundary] X-only 越界，尝试 Z-only");
                        ps.x = oldX;
                        ps.z = oldZ + ps.vz;

                        footPoint = ps.GetGroundPoint2D();
                        if (!BoundaryWallManager.Instance.IsPointWalkable(footPoint))
                        {
                            // Z-only 也越界，stop（回退到原位置，速度归零）
                            if (_debugCollisionLog) Debug.LogWarning("[Boundary] Z-only 越界，Stop");
                            ps.x = oldX;
                            ps.z = oldZ;
                            ps.vx = 0;
                            ps.vz = 0;
                        }
                    }
                }
            }

            // ==================== 3. 应用垂直速度（FLF Line 347）====================
            ps.y += ps.vy;

            // ==================== 4. 地面修正（FLF Line 350-354）====================
            if (ps.y > 0)  // 不允许低于地面
            {
                ps.y = 0;
                // 触发落地事件（对应 FLF character.js:117 fell_onto_ground）
                if (ps.vy > 0)  // 只在向下运动时触发
                {
                    ps.vy = 0;
                    CharacterStates.Instance.HandleStateEvent(this, "fell_onto_ground", null);
                }
            }

            // ==================== 5. 更新 Unity 组件位置 ====================
            // P2: 使用 PhysicsState.ToUnityPosition() 应用新坐标映射
            // 新映射：Unity X/Y = FLF x/z（地面平面），ps.y 仅作为子节点视觉偏移（不影响排序/边界）
            Vector3 newPos = ps.ToUnityPosition();

            // 1) 地面平面位置：写到 UnitActions/父节点（权威位置）
            Transform groundTransform = GetGroundTransform();
            newPos.z = groundTransform.position.z;
            groundTransform.position = newPos;

            // 2) 视觉高度：ps.y（像素，向上为负）映射到子节点 localPosition.y（向上为正）
            float visualYOffset = (-ps.y) / SimulationConstants.PIXELS_PER_UNIT;
            transform.localPosition = _baseLocalPosition + new Vector3(0f, visualYOffset, 0f);

            // 3) 保留 isGrounded / groundPos 供 BeatEmUp 排序与影子使用
            unitActions.yForce = 0f;                        // 弃用：跳跃高度已转为子节点视觉偏移
            unitActions.isGrounded = (ps.y == 0);
            unitActions.groundPos = groundTransform.position.y;

            // Physics Plan A Step P2: 移除 Rigidbody2D.velocity 写入
            // ps 是唯一的物理真值，transform.position 已在上方更新
            // 不再需要通过 Rigidbody2D 驱动移动

            // ==================== 6. 应用摩擦力（FLF Line 368-375）====================
            // 只在地面时应用
            if (ps.y == 0 && _mass > 0f)  // 对应 FLF: if (ps.y === 0 && this.mass > 0)
            {
                // Step D8: 30Hz SimTick 直接对应 FLF 数据，不再需要缩放
                // FLF: ps.vx += sign(vx) * ps.fric; min_speed = GC.min_speed
                float minSpeed = NTSDGlobal.Gameplay.MinSpeed;

                // 应用线性摩擦力（X轴）
                if (ps.vx != 0)
                {
                    ps.vx += (ps.vx > 0 ? -1 : 1) * ps.fric;
                    // 最小速度截断
                    if (Mathf.Abs(ps.vx) < minSpeed) ps.vx = 0;
                }

                // 应用线性摩擦力（Z轴）
                if (ps.vz != 0)
                {
                    ps.vz += (ps.vz > 0 ? -1 : 1) * ps.fric;
                    // 最小速度截断
                    if (Mathf.Abs(ps.vz) < minSpeed) ps.vz = 0;
                }
            }

            // ==================== 7. 应用重力（FLF Line 377）====================
            // 只在空中时应用
            if (ps.y < 0)  // 对应 FLF: if (ps.y < 0)
            {
                // Step D8: 30Hz SimTick 直接对应 FLF 数据，不再需要缩放
                // FLF: ps.vy += this.mass * GC.gravity
                ps.vy += _mass * NTSDGlobal.Gameplay.Gravity;
            }
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


            bool _IsTrans = FrameAniInfo.frameData.state != _frames[targetFrameId].state;
            if(_IsTrans)
            {
                if (_debugComboLog)
                {
                    Debug.Log(
                        $"[NTSD][FrameTransit:StateExit] StableId={StableId} oldState={FrameAniInfo.frameData.state} oldFrame={CurrentFrameId} " +
                        $"toFrame={targetFrameId} toState={_frames[targetFrameId]?.state} bufCombo={_comboBuffer.combo ?? "null"}"
                    );
                }

                // 保留 state_exit 事件分发（给状态机做额外清理）；当前不需要传参
                CharacterStates.Instance.HandleStateEvent(this, "state_exit", _comboBuffer.combo);
            }

            if (targetFrameId >= 0 && targetFrameId < _frames.Length)
            {
                FrameAniInfo.frameData = _frames[targetFrameId];
            }
            else
            {
                Debug.LogWarning($"[LF2CharacterAnimator] Invalid frame ID: {targetFrameId}");
                return;
            }

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
                    Debug.Log(
                        $"[NTSD][FrameTransit:StateEnter] StableId={StableId} newState={FrameAniInfo.frameData.state} newFrame={CurrentFrameId} " +
                        $"allowSwitchDir {oldSwitchDir}->{_AllowSwitchDir} bufCombo={_comboBuffer.combo ?? "null"} " +
                        $"IsLeft={_Character?._CharacterInput?.IsLeft} IsRight={_Character?._CharacterInput?.IsRight}"
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
                SetDirection(unitActions.dir == DIRECTION.RIGHT ? DIRECTION.LEFT : DIRECTION.RIGHT);
            
            FrameUpdate();
        }

        /// <summary>
        /// 状态退出时清理连招缓冲。
        /// FLF 规则：双击方向键（left-left/right-right）不能跨状态保留，否则会在切状态后误触发奔跑。
        /// 对应 FLF character.js:221-228
        /// </summary>
        public void ClearComboBufferOnStateExit()
        {
            switch (_comboBuffer.combo)
            {
                case "left-left":
                case "right-right":
                    _comboBuffer.combo = null;
                    break;
            }
        }

        public void OnReduceComboBufferTimeout()
        {
            if (_comboBuffer.timeout <= 0)
                return;

            _comboBuffer.timeout--;
            if (_comboBuffer.timeout == 0)
            {
                switch (_comboBuffer.combo)
                {
                    case "def":
                    case "jump":
                    case "att":
                    case "left-left":
                    case "right-right":
                        _comboBuffer.combo = null;
                        break;
                }
            }
            
        }

        public void SetDirection(DIRECTION direction)
        {
            if (unitActions == null) return;
            unitActions.TurnToDir(direction);

            if (ps != null)
            {
                ps.dir = (direction == DIRECTION.LEFT) ? "left" : "right";
            }
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

            // dvx: 水平速度（需要考虑角色朝向）
            if (FrameAniInfo.frameData.dvx != 0)
            {
                if (FrameAniInfo.frameData.dvx == 550)
                {
                    // 550 是 LF2 的特殊值，表示停止水平移动
                    // Physics Plan A Step P2: 修改 ps.vx 而非 Rigidbody2D.velocity
                    ps.vx = 0;
                }
                else
                {
                    // Physics Plan A Step P2: FLF 语义 - dvx 是 delta velocity（像素/帧）
                    // 直接应用到 ps.vx（已经是像素/帧单位）
                    float directionH = (ps != null && ps.dir == "left") ? -1f : 1f;
                    ps.vx += directionH * FrameAniInfo.frameData.dvx;
                }
            }

            // dvy: 垂直速度（跳跃）
            if (FrameAniInfo.frameData.dvy != 0)
            {
                if (FrameAniInfo.frameData.dvy == 550)
                {
                    // 550 表示停止跳跃
                    // Physics Plan A Step P2: 修改 ps.vy 而非 unitActions.yForce
                    ps.vy = 0;
                }
                else
                {
                    // Physics Plan A Step P2: FLF 语义 - dvy 是 delta velocity（像素/帧）
                    // 注意：FLF 的 Y 轴向下为正，所以负号表示向上跳跃
                    ps.vy += -FrameAniInfo.frameData.dvy;  // 负号：FLF Y轴约定
                }
            }

            // 注意：LF2 的 dvz（深度移动）在 BeatEmUp 中通过 groundPos 处理
            // 这里可能不需要处理，或者由 Character 脚本处理
        }

        /// <summary>
        /// 武器点更新 - 更新持有武器的位置跟随角色
        /// 对应 FLF 的 wpoint() 方法
        ///
        /// 职责：根据当前帧的武器点数据，更新武器的位置和状态
        /// </summary>
        private void WPoint_Update()
        {
            // TODO: 实现武器点逻辑
            // 当前简化实现，后续可以扩展

            // 示例逻辑（需要根据实际项目调整）：
            // if (currentHoldWeapon != null && _CurrentFrameData.wpoints.Count > 0)
            // {
            //     var wpoint = _CurrentFrameData.wpoints[0];
            //
            //     // 计算武器位置
            //     Vector3 weaponPos = transform.position;
            //     float directionH = transform.localScale.x > 0 ? 1f : -1f;
            //     weaponPos.x += directionH * wpoint.x * GlobalConfig.PIXEL_TO_UNIT;
            //     weaponPos.y += wpoint.y * GlobalConfig.PIXEL_TO_UNIT;
            //
            //     // 更新武器位置
            //     currentHoldWeapon.transform.position = weaponPos;
            // }
        }

        /// <summary>
        /// 对象点处理 - 生成投射物、特效等
        /// 对应 FLF 的 opoint() 方法
        ///
        /// 职责：根据当前帧的对象点数据，生成游戏对象（投射物、特效等）
        /// </summary>
        private void OPoint_Process()
        {
            if (FrameAniInfo.frameData.opoint == null) return;

            var op = FrameAniInfo.frameData.opoint;

            // TODO: 实现对象点逻辑
            // 当前简化实现，后续可以扩展

            // 示例逻辑（需要根据实际项目调整）：
            // switch (op.kind)
            // {
            //     case 1:  // 生成投射物
            //         SpawnProjectile(op);
            //         break;
            //     case 2:  // 生成特效
            //         SpawnEffect(op);
            //         break;
            //     case 3:  // 生成其他对象
            //         // ...
            //         break;
            // }
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
            if (!_framesByName.TryGetValue(stateName, out List<LF2FrameData> frameDataList))
            {
                Debug.LogWarning($"[LF2CharacterAnimator] State '{stateName}' not found in character data!");
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
            if (frameid >= _frames.Length || frameid < 0)
            {
                Debug.LogError("Frame ID out of range");
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

        /// <summary>
        /// 通过State值获取第一帧ID
        /// 用于 GAS Ability 通过状态值激活技能
        /// </summary>
        /// <param name="targetState">目标状态值（如 0=Standing, 301=技能1）</param>
        /// <returns>该状态的第一帧ID，未找到返回-1</returns>
        public int GetFirstFrameByState(int targetState)
        {
            for (int i = 0; i < _frames.Length; i++)
            {
                if (_frames[i] != null && _frames[i].state == targetState)
                {
                    return i;
                }
            }

            Debug.LogWarning($"[LF2CharacterAnimator] State {targetState} not found in character data!");
            return -1;
        }

        public LF2FrameData GetFrameDataById(int frameId)
        {
            if (frameId < 0 || frameId >= _frames.Length) return null;
            return _frames[frameId];
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

        private const int FLF_DEFAULT_CHARACTER_AREST = 7; // FLF global.js: GC.default.character.arest
        private int _itrARest = 0;
        private readonly Dictionary<int, int> _itrVRestByAttacker = new Dictionary<int, int>();

        public bool ItrArestTest()
        {
            return _itrARest <= 0;
        }

        public bool ItrVrestTest(int attackerStableId)
        {
            return !_itrVRestByAttacker.TryGetValue(attackerStableId, out int v) || v <= 0;
        }

        public void ItrArestUpdate(InteractionArea itr)
        {
            // FLF: if (ITR && ITR.arest) arest=ITR.arest; else if (!ITR || !ITR.vrest) arest=default
            if (itr != null && itr.arest > 0)
            {
                _itrARest = itr.arest;
            }
            else if (itr == null || itr.vrest <= 0)
            {
                _itrARest = FLF_DEFAULT_CHARACTER_AREST;
            }
        }

        public void ItrVrestUpdate(int attackerStableId, InteractionArea itr)
        {
            // FLF: if (ITR && ITR.vrest) vrest[uid]=ITR.vrest
            if (itr != null && itr.vrest > 0)
            {
                _itrVRestByAttacker[attackerStableId] = itr.vrest;
            }
        }

        private void TickItrRest()
        {
            // 对齐 FLF livingobject.js: 每 TU 递减 vrest/arest
            if (_itrARest > 0) _itrARest--;

            if (_itrVRestByAttacker.Count == 0) return;
            var keys = ListPool<int>.Get();
            keys.AddRange(_itrVRestByAttacker.Keys);
            for (int i = 0; i < keys.Count; i++)
            {
                int k = keys[i];
                if (_itrVRestByAttacker[k] > 0) _itrVRestByAttacker[k]--;
            }
            ListPool<int>.Release(keys);
        }

        // ==================== Phase 1: fall / bdefend / boundary flags ====================

        // 对齐 FLF: character.js hit() 中的 health.fall / health.bdefend（Phase 1 仅用于动画/边界）
        private int _fall = 0;
        private int _bdefend = 0;

        public int Fall => _fall;
        public int Bdefend => _bdefend;

        public void AddFall(int amount)
        {
            _fall += Mathf.Abs(amount);
        }

        public void ResetFall()
        {
            _fall = 0;
        }

        public void AddBdefend(int amount)
        {
            _bdefend += Mathf.Abs(amount);
        }

        public void ResetBdefend()
        {
            _bdefend = 0;
        }

        private Transform GetGroundTransform()
        {
            if (unitActions != null) return unitActions.transform;
            if (transform.parent != null) return transform.parent;
            return transform;
        }

        private void OnDrawGizmosSelected()
        {
            if (!_debugDrawBodyVolumes && !_debugDrawItrVolumes) return;
            if (ps == null) return;
            if (FrameAniInfo?.frameData == null) return;

            float spriteWidthPx = GetCurrentSpriteWidthPx();
            if (spriteWidthPx <= 0f) return;

            // 说明：
            // - 这里绘制的是 FLF 语义的“真实体积”（bdy/itr 的 volume：x/y/z + vx/vy + w/h + zwidth）
            // - 但为了在 Unity 的 2.5D 显示坐标中直观对齐角色，采用与运行时一致的投影方式：
            //   Unity 世界 (X,Y) = 地面平面 (ps.x/ps.z)，并在 Y 上叠加竖直像素偏移（topPx）
            //   => centerY = (ps.z - pixelY) / PPU
            // - Z 轴用于“厚度”表现，不参与地面移动
            float ppu = SimulationConstants.PIXELS_PER_UNIT;
            float planeZ = GetGroundTransform().position.z;

            if (_debugDrawBodyVolumes)
            {
                var bodyVolumes = ps.GetBodyVolumes(
                    FrameAniInfo.frameData.bodies,
                    FrameAniInfo.frameData.centerx,
                    FrameAniInfo.frameData.centery,
                    spriteWidthPx
                );

                Gizmos.color = Color.yellow;
                foreach (var v in bodyVolumes)
                {
                    float leftPx = v.x + v.vx;
                    float topPx = v.y + v.vy;
                    float wPx = v.w;
                    float hPx = v.h;

                    float centerX = (leftPx + wPx * 0.5f) / ppu;
                    float centerY = (ps.z - (topPx + hPx * 0.5f)) / ppu;

                    float sizeX = Mathf.Max(0.001f, wPx / ppu);
                    float sizeY = Mathf.Max(0.001f, hPx / ppu);
                    float sizeZ = Mathf.Max(0.001f, (v.zwidth * 2f) / ppu);

                    Gizmos.DrawWireCube(new Vector3(centerX, centerY, planeZ), new Vector3(sizeX, sizeY, sizeZ));
                }
            }

            if (_debugDrawItrVolumes)
            {
                var itrVolumes = ps.GetItrVolumes(
                    FrameAniInfo.frameData.itrs,
                    FrameAniInfo.frameData.centerx,
                    FrameAniInfo.frameData.centery,
                    spriteWidthPx,
                    itrZWidthPx: 0f
                );

                Gizmos.color = Color.red;
                foreach (var v in itrVolumes)
                {
                    float leftPx = v.x + v.vx;
                    float topPx = v.y + v.vy;
                    float wPx = v.w;
                    float hPx = v.h;

                    float centerX = (leftPx + wPx * 0.5f) / ppu;
                    float centerY = (ps.z - (topPx + hPx * 0.5f)) / ppu;

                    float sizeX = Mathf.Max(0.001f, wPx / ppu);
                    float sizeY = Mathf.Max(0.001f, hPx / ppu);
                    float sizeZ = Mathf.Max(0.001f, (v.zwidth * 2f) / ppu);

                    Gizmos.DrawWireCube(new Vector3(centerX, centerY, planeZ), new Vector3(sizeX, sizeY, sizeZ));
                }
            }
        }
    }
}
