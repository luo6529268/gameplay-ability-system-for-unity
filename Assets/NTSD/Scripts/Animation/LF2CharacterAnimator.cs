using BeatEmUpTemplate2D;
using MoreMountains.Tools;
using MoreMountains.TopDownEngine;
using NTSD.Animation.LF2Objects;
using NTSD.Extensions;
using NTSD.Help;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation
{

    public struct LF2AnimationInfo 
    {
        public int frameIndex;
        public bool IsUp;
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
    public class LF2CharacterAnimator : MMMonoBehaviour//, ICharacterModule
    {
        //[Header("Determinism")]
        //[SerializeField]
        //[Tooltip("确定性 StableId（用于碰撞/输入等排序）。0 表示自动分配（单机本地）。多人模式应由服务器分配。")]
        //private int _stableId = 0;

        //public int StableId => _stableId;

        //// ==================== NTSD 扩展属性 ====================
        //[Header("NTSD Extensions")]
        //[SerializeField]
        //[Tooltip("队伍ID (0=无队伍, 1-4=队伍编号)")]
        //private int _team = 0;
        
        //[SerializeField]
        //[Tooltip("所有者ID (用于投射物等)")]
        //private int _ownerId = -1;
        
        //[SerializeField]
        //[Tooltip("对象类型 (0=角色, 1=投射物, 2=武器等)")]
        //private int _objectType = 0;
        
        ///// <summary>队伍ID</summary>
        //public int Team { get => _team; set => _team = value; }
        
        ///// <summary>所有者ID (用于投射物归属判定)</summary>
        //public int OwnerId { get => _ownerId; set => _ownerId = value; }
        
        ///// <summary>对象类型 (0=角色, 1=投射物, 2=武器等)</summary>
        //public int ObjectType { get => _objectType; set => _objectType = value; }
        
        ///// <summary>NTSD 角色属性系统 - 委托到 LF2Character</summary>
        //public NTSDCharacterStats CharacterStats => _Character?._LF2Character?.CharacterStats;

        //public LF2CharacterDataWrapper _FrameDataWrapper => FrameCache.Wrapper;
        //public Character _Character { get; private set; }



        ///// <summary>
        ///// 当前状态值（对应帧数据的 state 字段）
        ///// </summary>
        //public int CurrentState { get; private set; } = 0;

        ///// <summary>
        ///// 状态内存（状态切换时自动清空）
        ///// 对应 FLF 的 $.statemem
        ///// </summary>
        //public Dictionary<string, object> StateMem { get; private set; } = new Dictionary<string, object>();

        //// ==================== 连招缓冲区 - 委托到 LF2Character ====================
        //public LF2ComboBufferModule ComboBuffer => _Character?._LF2Character?.ComboBuffer;

        //// ==================== OPoint / WPoint - 委托到 LF2Character ====================
        //public LF2ObjectPointModule ObjectPointModule => _Character?._LF2Character?.ObjectPointModule;
        //public LF2WeaponPointModule WeaponPointModule => _Character?._LF2Character?.WeaponPointModule;

        //// ==================== 帧数据缓存（剥离为独立模块）====================
        //public LF2FrameCache FrameCache { get; } = new LF2FrameCache();

        //// ==================== 组件缓存 ====================
        //private List<Sprite> mergedSprites => CharacterAnimtorManager.Instance.GetCharacterSpriteByID(_Character.CharacterID);
        //private SpriteRenderer spriteRenderer;

        //// 视觉偏移：ps.y（跳跃/击飞高度）只影响子节点（Model）的本地 Y，不影响地面平面位置与排序。
        //private Vector3 _baseLocalPosition;

        //[SerializeField]
        //private bool _debugCollisionLog = false;

        //[Header("Debug / Combo")]
        //[SerializeField]
        //private bool _debugComboLog = false;

        //// ==================== 当前状态 ====================

        //private LF2AnimationInfo _animationInfo;
        //public bool _AllowSwitchDir { get; set; }

        //private bool _comboDetectorInitialized = false;

        //// FLF mechanics.js: this.mass (from spec or global default)
        //private float _mass = NTSDGlobal.Default.Machanics.Mass;

        //// FLF mech 体系：物理计算层（不继承 Mono，只处理数据/运算）
        //private CharacterMechanics _mech;

        //// ==================== 缓存委托（避免 per-tick 分配）====================
        //// 日志 wrapper：Log.Warn(string, params object[]) 不能直接赋值给 Action<string>
        //private static void LogWarnWrapper(string msg) => Tools.Log.Warn(msg);
        //private static readonly Action<string> s_logWarn = LogWarnWrapper;
        
        //// 边界检测委托缓存（在 ModuleInitialize 中初始化）
        //private Func<Vector2, bool> _cachedIsPointWalkable;

        //public void ModuleSetup(Character character)
        //{
        //    _Character = character;
        //    spriteRenderer = this.GetComponent<SpriteRenderer>();

        //}

        //public void ModuleInitialize()
        //{
        //    _baseLocalPosition = transform.localPosition;

        //    // 初始化物理计算层（对应 FLF 的 mech 体系）
        //    _mech = new CharacterMechanics();
            
        //    // 缓存边界检测委托（避免 per-tick 分配）
        //    _cachedIsPointWalkable = BoundaryWallManager.Instance.IsPointWalkable;

        //    _AllowSwitchDir = true;

        //    // Scheme C: StableId 统一来源为 Character.StableIdRuntime
        //    if (_Character != null && _Character.StableIdRuntime > 0)
        //    {
        //        _stableId = _Character.StableIdRuntime;
        //    }

        //    // 初始化连招检测器（不依赖 CharacterID）
        //    InitializeComboDetector();
        //}

        //public void ModuleBind()
        //{
        //    ReloadCharacterFrameData();
        //    InitializeDate();
            
        //    // 模块现在在 LF2Character 中，通过委托访问
        //    ComboBuffer?.Reset();
        //    ItrRest?.Reset();
        //    HitCounters?.Reset();

        //    // Bind FLF spec/properties (mass etc.)
        //    if (_Character != null)
        //    {
        //        _mass = NTSDSpec.GetMassOrDefault(_Character.CharacterID);
        //    }

        //    // 绑定 OPoint Factory (如果存在)
        //    if (ObjectPointModule != null && ObjectPointModule.Factory == null && LF2ObjectPointFactory.Instance != null)
        //    {
        //        ObjectPointModule.SetFactory(LF2ObjectPointFactory.Instance);
        //    }
        //}

        //public void ModuleUnbind()
        //{
        //    FrameCache.Clear();
        //}

        ///// <summary>
        ///// Scheme C: StableId should be driven by Character.
        ///// </summary>
        //public void SetStableId(int stableId)
        //{
        //    _stableId = stableId;
        //}

        ///// <summary>
        ///// 初始化连招检测器
        ///// </summary>
        //private void InitializeComboDetector()
        //{
        //    if (_comboDetectorInitialized) return;

        //    {
        //        _Character._ActionSequenceDetector.OnComboDetected += OnComboDetected;
        //        _comboDetectorInitialized = true;
        //    }
        //}

        //private void InitializeDate() 
        //{
        //    _animationInfo.frameIndex = 0;
        //    _animationInfo.IsUp = true;
        //}

        //private void OnDestroy()
        //{
        //    // 取消订阅连招事件
        //    {
        //        _Character._ActionSequenceDetector.OnComboDetected -= OnComboDetected;
        //    }
        //}

        //private void LoadCharacterFrameData()
        //{
        //    FrameCache.Load(CharacterAnimtorManager.Instance.GetCharacterConfig(_Character.CharacterID));
        //}

        ///// <summary>
        ///// 重新加载角色数据（用于运行时切换 Character.CharacterID）。
        ///// 注意：该方法应尽量在角色 Start 之前调用（例如 LevelManager 刚 Instantiate 后）。
        ///// </summary>
        //public void ReloadCharacterFrameData()
        //{
        //    _Character?._LF2Character?.ReloadCharacterFrameData();
        //}

        ///// <summary>
        ///// 设置朝向（表现层 + 数据层同步）
        ///// </summary>
        ///// <param name="dir">目标朝向</param>
        ///// <param name="syncPs">是否同步写回 ps.dir（默认 true）</param>
        //public void SetFacingDir(DIRECTION dir, bool syncPs = true)
        //{
        //    _Character?._LF2Character?.SetFacingDir(dir, syncPs);
        //}

        ///// <summary>
        ///// 连招检测到回调
        ///// 对应 FLF character.js:1684-1700 的 combo_event
        ///// </summary>
        //private void OnComboDetected(ComboConfig.ComboDefinition combo)
        //{
        //    if (FrameAniInfo.frameData == null) return;

        //    if (_debugComboLog)
        //    {
        //        Tools.Log.Info(
        //            "[NTSD][ComboDetected] StableId={0} detected={1} state={2} frame={3} pre={4} " +
        //            "bufBefore={5} allowSwitchDir={6} ps.dir={7} FacingDir={8} " +
        //            "IsLeft={9} IsRight={10}",
        //            StableId, combo.name, FrameAniInfo.frameData.state, CurrentFrameId, PreviousFrameId,
        //            ComboBuffer.Combo ?? "null", _AllowSwitchDir, ps?.dir ?? "null", FacingDir.ToString(),
        //            _Character?._CharacterInput?.IsLeft, _Character?._CharacterInput?.IsRight
        //        );
        //    }

        //    ComboBuffer?.OnComboDetected(
        //        combo: combo,
        //        allowSwitchDir: _AllowSwitchDir,
        //        setDirectionByString: SetDirectionByString,
        //        timeoutFrames: NTSDGlobal.Combo.Timeout,
        //        debugLog: _debugComboLog,
        //        stableId: StableId
        //    );
        //}

        ///// <summary>
        ///// 应用物理 - 调用 CharacterMechanics.Step() + Unity 写回
        /////
        ///// 职责：
        ///// 1. 构造 CharacterMechanicsContext
        ///// 2. 调用 _mech.Step() 执行物理计算
        ///// 3. 将结果写回 Unity 组件（Transform, Character hub）
        ///// 4. 不处理 TU/落地事件（fell/fall_onto_ground 属于 TU_Update 阶段）
        /////
        ///// 对齐 FLF：
        ///// - mechanics.js: mech.dynamics()
        ///// - mechanics.js: mech.blocking_xz()（blocked -> move * 0.1）
        ///// </summary>
        //private void ApplyDynamicsInternal()
        //{
        //    LF2DynamicsApplier.Apply(
        //        animator: this,
        //        mechanics: _mech,
        //        mass: _mass,
        //        isPointWalkable: _cachedIsPointWalkable,
        //        logWarning: _debugCollisionLog ? s_logWarn : null,
        //        debugCollisionLog: _debugCollisionLog,
        //        groundTransform: GetGroundTransform(),
        //        baseLocalPosition: _baseLocalPosition
        //    );
        //}

        ///// <summary>
        ///// 对齐 FLF trans.trans() 中的切帧+frame_update 流程
        ///// </summary>
        //internal void FrameTransitInternal(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        //{
        //    FrameAniInfo.preNext = FrameAniInfo.next;
        //    FrameAniInfo.next = targetFrameId;

        //    LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
        //    if (targetFrame == null)
        //    {
        //        Tools.Log.Warn("[LF2CharacterAnimator] Invalid frame ID: {0}", targetFrameId);
        //        return;
        //    }

        //    bool _IsTrans = FrameAniInfo.frameData.state != targetFrame.state;
        //    if(_IsTrans)
        //    {
        //        if (_debugComboLog)
        //        {
        //            Tools.Log.Info(
        //                "[NTSD][FrameTransit:StateExit] StableId={0} oldState={1} oldFrame={2} " +
        //                "toFrame={3} toState={4} bufCombo={5}",
        //                StableId, FrameAniInfo.frameData.state, CurrentFrameId,
        //                targetFrameId, targetFrame.state, ComboBuffer.Combo ?? "null"
        //            );
        //        }

        //        // 保留 state_exit 事件分发（给状态机做额外清理）；当前不需要传参
        //        CharacterStates.Instance.HandleStateEvent(this, "state_exit", ComboBuffer.Combo);
        //    }

        //    FrameAniInfo.frameData = targetFrame;

        //    if (_IsTrans) 
        //    {
        //        StateMem.Clear();

        //        bool oldSwitchDir = _AllowSwitchDir;
        //        _AllowSwitchDir = CharacterStates.Instance.GetStatesSwitchDir(FrameAniInfo.frameData.state);

        //        CharacterStates.Instance.HandleStateEvent(this, "state_entry", null);

        //        if (_debugComboLog)
        //        {
        //            Tools.Log.Info(
        //                "[NTSD][FrameTransit:StateEnter] StableId={0} newState={1} newFrame={2} " +
        //                "allowSwitchDir {3}->{4} bufCombo={5} " +
        //                "IsLeft={6} IsRight={7}",
        //                StableId, FrameAniInfo.frameData.state, CurrentFrameId,
        //                oldSwitchDir, _AllowSwitchDir, ComboBuffer.Combo ?? "null",
        //                _Character?._CharacterInput?.IsLeft, _Character?._CharacterInput?.IsRight
        //            );
        //        }

        //        if (!switchDirAfterTrans) 
        //        {
        //            if (_AllowSwitchDir && !oldSwitchDir) 
        //            {
        //                //处理转换后切向
        //                if (_Character._CharacterInput.IsLeft)
        //                    SetDirection(DIRECTION.LEFT);
        //                if(_Character._CharacterInput.IsRight)
        //                    SetDirection(DIRECTION.RIGHT);
        //            }
        //        }
        //    }

        //    if (switchDirAfterTrans) 
        //        SetDirection(FacingDir == DIRECTION.RIGHT ? DIRECTION.LEFT : DIRECTION.RIGHT);
            
        //    FrameUpdate();
        //}

        //public void SetDirection(DIRECTION direction)
        //{
        //    SetFacingDir(direction, true);
        //}

        //public void SetDirectionByString(string dir) 
        //{
        //    DIRECTION targetDir = (dir == "left") ? DIRECTION.LEFT : DIRECTION.RIGHT;
        //    SetDirection(targetDir);
        //}

        //internal void FrameUpdate() 
        //{
        //    spriteRenderer.sprite = mergedSprites[FrameAniInfo.frameData.pic];

        //    // 重置摩擦力
        //    trans.SetWait(FrameAniInfo.frameData.wait, 99);
        //    trans.SetNext(FrameAniInfo.frameData.next, 99);

        //    // 状态 frame 事件
        //    CharacterStates.Instance.HandleStateEvent(this, "frame", null);
            
        //    // P1: OPoint 入口已移至 CharacterStates.HandleGenericFrame()
        //    // 不在此处调用，避免双触发

        //    // 播放音效
        //    if (!string.IsNullOrEmpty(FrameAniInfo.frameData.sound))
        //    {
        //        // TODO: 播放音效
        //        // AudioManager.PlaySound(_CurrentFrameData.sound);
        //    }
        //}

        //// ==================== 动画事件系统 ====================

        ///// <summary>
        ///// 帧力应用 - 应用动画数据驱动的速度变化
        ///// 对应 FLF 的 frame_force() 方法
        /////
        ///// 职责：将帧数据中的 dvx, dvy 转换为实际的物理速度
        ///// 注意：这是动画数据驱动的，属于动画系统的一部分
        ///// </summary>
        //private void Frame_Force()
        //{
        //    if (FrameAniInfo.frameData == null) return;
        //    int dirv = _Character != null && _Character._CharacterInput != null ? _Character._CharacterInput.Dirv : 0;
        //    LF2FrameForceApplier.Apply(ps, FrameAniInfo.frameData, dirv);
        //}


        ///// <summary>
        ///// 直接跳转到指定帧ID
        ///// 对应 LF2 的 $.trans.frame()
        ///// </summary>
        ///// <param name="frameId">目标帧ID</param>
        ///// <param name="authority">权限等级（默认20）</param>
        //public void TransitionToFrame(int frameId, int authority = 20)
        //{
        //    trans.Frame(frameId, authority);
        //}


        //public LF2FrameData GetFrameDataById(int frameId)
        //{
        //    return FrameCache.GetFrameDataById(frameId);
        //}

        //private float GetCurrentSpriteWidthPx()
        //{
        //    if (spriteRenderer == null || spriteRenderer.sprite == null) return 0f;
        //    return spriteRenderer.sprite.textureRect.width;
        //}

        //public float GetSpriteWidthPxForCollision()
        //{
        //    return GetCurrentSpriteWidthPx();
        //}

        //// ==================== FLF ITR Rest - 委托到 LF2Character ====================
        //public LF2ItrRestTracker ItrRest => _Character?._LF2Character?.ItrRest;

        //// ==================== Phase 1: hit counters - 委托到 LF2Character ====================
        //public LF2HitCountersModule HitCounters => _Character?._LF2Character?.HitCounters;

        //private Transform GetGroundTransform()
        //{
        //    // Step 2: 优先使用 Character hub 的 transform（不再依赖 unitActions）
        //    if (_Character != null) return _Character.transform;
        //    if (transform.parent != null) return transform.parent;
        //    return transform;
        //}

      
    }
}
