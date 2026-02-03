using BeatEmUpTemplate2D;
using MoreMountains.TopDownEngine;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色专用逻辑（继承 LF2LivingObject，对应 FLF character.js）
    /// 包含连招、OPoint、WPoint、武器持有等角色特有功能
    /// 
    /// 继承层次：LF2LivingObject → LF2Character
    /// 对应 FLF：livingobject → character
    /// 
    /// 参考：I:\C++Test\NTSD\F.LF-master\LF\character.js
    /// </summary>
    public class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

        // ========== 角色专用模块 ==========

        /// <summary>
        /// 连招缓冲区（对应 FLF $.combo_buffer）
        /// </summary>
        private readonly LF2ComboBufferModule _comboBuffer;
        public override LF2ComboBufferModule ComboBuffer => _comboBuffer;

        /// <summary>
        /// OPoint 模块（对应 FLF character.opoint）
        /// </summary>
        public LF2ObjectPointModule ObjectPointModule { get; private set; }

        /// <summary>
        /// WPoint 模块（对应 FLF character.wpoint）
        /// </summary>
        public LF2WeaponPointModule WeaponPointModule { get; private set; }

        /// <summary>
        /// 受击计数器（对应 FLF $.health.fall/bdefend）
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;

        /// <summary>
        /// 角色属性（HP/MP 等）
        /// </summary>
        private readonly NTSDCharacterStats _characterStats;
        public override NTSDCharacterStats CharacterStats => _characterStats;

        // ========== 武器持有（对应 FLF $.hold）==========

        /// <summary>
        /// 当前持有的武器（对应 FLF $.hold.obj）
        /// </summary>
        private LF2ObjectRenderer _heldWeapon;

        // ========== Unity 组件引用 ==========

        private SpriteRenderer _spriteRenderer;
        private List<Sprite> _sprites;
        private Transform _groundTransform;
        private Vector3 _baseLocalPosition;

        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;

        // ========== 调试 ==========

        private bool _debugCollisionLog = false;
        private bool _debugComboLog = false;

        // ========== 构造函数 ==========

        public LF2Character(MoreMountains.TopDownEngine.Character hub)
        {
            _CharacterHub = hub;

            // 创建角色专用模块
            _comboBuffer = new LF2ComboBufferModule();
            ObjectPointModule = new LF2ObjectPointModule();
            WeaponPointModule = new LF2WeaponPointModule();
            _hitCounters = new LF2HitCountersModule();
            _characterStats = new NTSDCharacterStats();
            
            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            PS = new PhysicsState();
            Trans = new FrameTransistor();
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Sprite = new LF2Sprite();

            // 设置帧转换回调
            Trans.SetFrameTransitCallback(OnFrameTransit);
        }

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            // 角色通过 Character Hub 初始化，不使用此方法
        }

        public override void Reset()
        {
            _comboBuffer?.Reset();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            _heldWeapon = null;
        }

        public override void Destroy()
        {
            Reset();
        }

        // ========== 初始化（由 Character Hub 调用）==========

        /// <summary>
        /// 模块初始化（对应 LF2CharacterAnimator.ModuleInitialize）
        /// </summary>
        public void ModuleInitialize(
            SpriteRenderer spriteRenderer,
            List<Sprite> sprites,
            Transform groundTransform,
            Vector3 baseLocalPosition)
        {
            _spriteRenderer = spriteRenderer;
            _sprites = sprites;
            _groundTransform = groundTransform;
            _baseLocalPosition = baseLocalPosition;

            // 初始化物理计算层
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null 
                ? BoundaryWallManager.Instance.IsPointWalkable 
                : null;

            // 初始化物理状态
            PS.FromUnityPosition(groundTransform.position);
            PS.vx = 0;
            PS.vy = 0;
            PS.vz = 0;

            // 初始化精灵模块
            Sprite.Initialize(spriteRenderer, sprites);

            AllowSwitchDir = true;
        }

        /// <summary>
        /// 模块绑定（对应 LF2CharacterAnimator.ModuleBind）
        /// </summary>
        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            // 加载帧数据
            FrameCache.Load(frameDataWrapper);

            // 初始化帧信息
            Frame.D = FrameCache.GetFrameDataById(0);
            Frame.PN = 0;
            Frame.N = 0;

            // 重置模块
            _comboBuffer?.Reset();
            ItrRest?.Reset();
            _hitCounters?.Reset();

            // 绑定 mass
            _mass = NTSDSpec.GetMassOrDefault(characterId);

            // 绑定 OPoint Factory
            if (ObjectPointModule != null && ObjectPointModule.Factory == null && LF2ObjectPointFactory.Instance != null)
            {
                ObjectPointModule.SetFactory(LF2ObjectPointFactory.Instance);
            }
        }

        /// <summary>
        /// 初始化角色属性
        /// </summary>
        public void Initialize(int maxHp, int maxMp)
        {
            CharacterStats.Initialize(maxHp, maxMp);
            Health.HP = maxHp;
            Health.MP = maxMp;
            _comboBuffer.Reset();
            HitCounters.Reset();
            ItrRest.Reset();
        }

        /// <summary>
        /// 绑定 OPoint Factory
        /// </summary>
        public void BindOPointFactory(LF2ObjectPointFactory factory)
        {
            ObjectPointModule.SetFactory(factory);
        }

        // ========== 核心生命周期（对应 FLF livingobject/character）==========

        /// <summary>
        /// TU Update - 每个时间单位的主循环
        /// 对应 FLF livingobject.TU_update()
        /// </summary>
        public new void TUUpdate()
        {
            // 重置摩擦力
            PS?.ResetFriction();

            // TU 事件
            CharacterStates.Instance.HandleStateEvent(this, "TU", null);
        }

        /// <summary>
        /// 连招更新 - 对应 FLF character.combo_update()
        /// 参考：FLF character.js:1800-1846
        /// </summary>
        protected override void ComboUpdate()
        {
            string rawCombo = ComboBuffer?.Combo;
            string K = rawCombo;
            if (string.IsNullOrEmpty(K)) { K = null; }

            // 特殊处理：跳跃攻击组合
            if (rawCombo == "jump-att") { K = "jump"; }

            // 触发状态机的 'combo' 事件
            bool res1 = CharacterStates.Instance.HandleStateEvent(this, "combo", K, true);

            // 调用当前状态的组合后处理
            CharacterStates.Instance.HandleStateEvent(this, "post_combo", null);

            ComboBuffer?.AfterComboUpdate(handledByState: res1, rawCombo: rawCombo, mappedCombo: K);
        }

        /// <summary>
        /// 物理+武器点更新
        /// 对应 FLF transit 阶段的 mech.dynamics() + wpoint()
        /// </summary>
        public void TransitDynamicsAndWPoint()
        {
            ApplyDynamics();
            WPointUpdate();
        }

        /// <summary>
        /// Transit 阶段的物理和武器点处理（兼容 LF2CharacterAnimator）
        /// </summary>
        public override void Transit_DynamicsAndWPoint()
        {
            TransitDynamicsAndWPoint();
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            LF2DynamicsApplier.Apply(
                _character: _CharacterHub,
                mechanics: _mech,
                mass: _mass,
                isPointWalkable: _cachedIsPointWalkable,
                logWarning: _debugCollisionLog ? s => Log.Warn(s) : (Action<string>)null,
                debugCollisionLog: _debugCollisionLog,
                groundTransform: _groundTransform,
                baseLocalPosition: _baseLocalPosition
            );
        }

        /// <summary>
        /// 武器点更新
        /// 对应 FLF wpoint()
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 帧转换回调 ==========

        /// <summary>
        /// 帧转换回调（由 FrameTransistor 调用）
        /// 对应 FLF trans.trans() 中的切帧逻辑
        /// </summary>
        private void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
            {
                Log.Warn("[LF2Character] Invalid frame ID: {0}", targetFrameId);
                return;
            }

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
            {
                // 状态退出事件
                CharacterStates.Instance.HandleStateEvent(this, "state_exit", ComboBuffer?.Combo);
            }

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                StateMem.Clear();

                bool oldSwitchDir = AllowSwitchDir;
                AllowSwitchDir = CharacterStates.Instance.GetStatesSwitchDir(Frame.D.state);

                CharacterStates.Instance.HandleStateEvent(this, "state_entry", null);

                if (!switchDirAfterTrans)
                {
                    if (AllowSwitchDir && !oldSwitchDir)
                    {
                        var input = _CharacterHub?._CharacterInput;
                        if (input != null)
                        {
                            if (input.IsLeft)
                                SetDirection(DIRECTION.LEFT);
                            if (input.IsRight)
                                SetDirection(DIRECTION.RIGHT);
                        }
                    }
                }
            }

            if (switchDirAfterTrans)
            {
                DIRECTION currentDir = PS.dir == "left" ? DIRECTION.LEFT : DIRECTION.RIGHT;
                SetDirection(currentDir == DIRECTION.RIGHT ? DIRECTION.LEFT : DIRECTION.RIGHT);
            }

            FrameUpdateInternal();
        }

        /// <summary>
        /// 帧更新（内部）
        /// 对应 FLF frame_update()
        /// </summary>
        private void FrameUpdateInternal()
        {
            // 更新精灵
            if (_spriteRenderer != null && _sprites != null && Frame.D != null)
            {
                int picIndex = Frame.D.pic;
                if (picIndex >= 0 && picIndex < _sprites.Count)
                {
                    _spriteRenderer.sprite = _sprites[picIndex];
                }
            }

            // 应用帧力
            if (!CharacterStates.Instance.HandleStateEvent(this, "frame_force", null))
            {
                FrameForceInternal();
            }

            // 设置等待和下一帧
            Trans.SetWait(Frame.D?.wait ?? 1, 99);
            Trans.SetNext(Frame.D?.next ?? 0, 99);

            // 状态 frame 事件
            CharacterStates.Instance.HandleStateEvent(this, "frame", null);

            // 播放音效
            if (Frame.D != null && !string.IsNullOrEmpty(Frame.D.sound))
            {
                // TODO: 播放音效
            }
        }

        /// <summary>
        /// 帧力应用（内部）
        /// 对应 FLF frame_force()
        /// </summary>
        private void FrameForceInternal()
        {
            if (Frame.D == null) return;
            int dirv = _CharacterHub?._CharacterInput?.Dirv ?? 0;
            LF2FrameForceApplier.Apply(PS, Frame.D, dirv);
        }

        // ========== 方向控制 ==========

        /// <summary>
        /// 设置方向
        /// </summary>
        public void SetDirection(DIRECTION direction)
        {
            // 表现层：翻转角色
            if (_groundTransform != null)
            {
                _groundTransform.localRotation = (direction == DIRECTION.LEFT)
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
            }

            // 数据层：同步 ps.dir
            PS.dir = (direction == DIRECTION.LEFT) ? "left" : "right";
        }

        /// <summary>
        /// 通过字符串设置方向
        /// </summary>
        public void SetDirectionByString(string dir)
        {
            DIRECTION targetDir = (dir == "left") ? DIRECTION.LEFT : DIRECTION.RIGHT;
            SetDirection(targetDir);
        }

        /// <summary>
        /// 获取当前朝向
        /// </summary>
        public DIRECTION FacingDir
        {
            get
            {
                if (PS != null && !string.IsNullOrEmpty(PS.dir))
                {
                    return PS.dir == "left" ? DIRECTION.LEFT : DIRECTION.RIGHT;
                }
                return DIRECTION.RIGHT;
            }
        }

        // ========== 连招处理 ==========

        /// <summary>
        /// 连招检测回调
        /// 参考：FLF character.js:1684-1700
        /// </summary>
        public void OnComboDetected(ComboConfig.ComboDefinition combo)
        {
            if (Frame.D == null) return;

            ComboBuffer.OnComboDetected(
                combo: combo,
                allowSwitchDir: AllowSwitchDir,
                setDirectionByString: SetDirectionByString,
                timeoutFrames: NTSDGlobal.Combo.Timeout,
                debugLog: _debugComboLog,
                stableId: StableId
            );
        }

        // ========== 武器持有 ==========

        /// <summary>
        /// 持有武器（对应 FLF character.prototype.hold_weapon）
        /// </summary>
        public void HoldWeapon(LF2ObjectRenderer weapon)
        {
            _heldWeapon = weapon;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public LF2ObjectRenderer GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 丢弃武器（对应 FLF character.prototype.drop_weapon）
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            if (_heldWeapon?.LogicObject is LF2LightWeapon lightWeapon)
            {
                lightWeapon.Drop(dvx, dvy);
            }
            else if (_heldWeapon?.LogicObject is LF2HeavyWeapon heavyWeapon)
            {
                heavyWeapon.Drop(dvx, dvy);
            }

            _heldWeapon = null;
        }

        // ========== 帧播放接口 ==========

        /// <summary>
        /// 播放指定状态的第一帧
        /// </summary>
        public void PlayFrame(string stateName, bool immediate = false)
        {
            if (!FrameCache.TryGetFramesByName(stateName, out List<LF2FrameData> frameDataList))
            {
                Log.Warn("[LF2Character] State '{0}' not found!", stateName);
                return;
            }

            frameDataList.Sort((a, b) => a.frameId.CompareTo(b.frameId));
            int targetFrameId = frameDataList[0].frameId;

            if (immediate || Frame.D == null)
            {
                OnFrameTransit(targetFrameId, false, 0);
            }
            Trans.Frame(targetFrameId, 20);
        }

        /// <summary>
        /// 按ID播放帧
        /// </summary>
        public void PlayFrameByID(int frameid, bool immediate = false)
        {
            if (frameid >= LF2FrameCache.MaxFrameIdExclusive || frameid < 0)
            {
                Log.Error("[LF2Character] Frame ID out of range: {0}", frameid);
                return;
            }

            if (immediate || Frame.D == null)
            {
                OnFrameTransit(frameid, false, 0);
            }
            Trans.Frame(frameid, 20);
        }

        /// <summary>
        /// 转换到指定帧
        /// </summary>
        public void TransitionToFrame(int frameId, int authority = 20)
        {
            Trans.Frame(frameId, authority);
        }

        /// <summary>
        /// 设置下一帧
        /// </summary>
        public void SetNextFrame(int frameId, int authority = 99)
        {
            Trans.SetNext(frameId, authority);
        }

        /// <summary>
        /// 设置等待时间
        /// </summary>
        public void SetWait(int value, int authority = 99)
        {
            Trans.SetWait(value, authority);
        }

        /// <summary>
        /// 执行帧转换
        /// </summary>
        public void TransTrans()
        {
            Trans.Trans();
        }

        // ========== 状态内存 ==========

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

        // ========== 辅助方法 ==========

        /// <summary>
        /// 通过State值获取第一帧ID
        /// </summary>
        public int GetFirstFrameByState(int targetState)
        {
            int id = FrameCache.GetFirstFrameByState(targetState);
            if (id < 0)
            {
                Log.Warn("[LF2Character] State {0} not found!", targetState);
            }
            return id;
        }

        /// <summary>
        /// 获取帧数据
        /// </summary>
        public LF2FrameData GetFrameDataById(int frameId)
        {
            return FrameCache.GetFrameDataById(frameId);
        }

        /// <summary>
        /// 获取精灵宽度（用于碰撞）
        /// </summary>
        public float GetSpriteWidthPxForCollision()
        {
            if (_spriteRenderer == null || _spriteRenderer.sprite == null) return 0f;
            return _spriteRenderer.sprite.textureRect.width;
        }

        // ========== 当前帧信息（兼容属性）==========

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;
        public int CurrentState => Frame.D?.state ?? 0;

        // ========== 额外方法 ==========

        /// <summary>
        /// 重新加载角色帧数据
        /// </summary>
        public void ReloadCharacterFrameData()
        {
            if (_CharacterHub == null) return;

            // 重新绑定帧数据
            if (_FrameDataWrapper != null)
            {
                FrameCache.Load(_FrameDataWrapper);
            }

            // 重置到第一帧
            Frame.D = FrameCache.GetFrameDataById(0);
            Frame.PN = 0;
            Frame.N = 0;
        }

        /// <summary>
        /// 设置朝向（表现层 + 数据层同步）
        /// </summary>
        public void SetFacingDir(DIRECTION dir, bool syncPs = true)
        {
            // 表现层：翻转角色
            if (_groundTransform != null)
            {
                _groundTransform.localRotation = (dir == DIRECTION.LEFT)
                    ? Quaternion.Euler(0, 180, 0)
                    : Quaternion.identity;
            }

            // 数据层：同步 ps.dir
            if (syncPs)
            {
                PS.dir = (dir == DIRECTION.LEFT) ? "left" : "right";
            }
        }

        /// <summary>
        /// 帧动画振荡（在指定帧范围内来回播放）
        /// </summary>
        public void FrameAniOscillate(int from, int to)
        {
            if (_animationInfo.frameIndex < from || _animationInfo.frameIndex > to)
            {
                _animationInfo.IsUp = true;
                _animationInfo.frameIndex = from + 1;
            }

            if (_animationInfo.frameIndex < to && _animationInfo.IsUp)
                Trans.SetNext(_animationInfo.frameIndex++);
            else if (_animationInfo.frameIndex > from && !_animationInfo.IsUp)
                Trans.SetNext(_animationInfo.frameIndex--);

            if (_animationInfo.frameIndex == to)
                _animationInfo.IsUp = false;
            if (_animationInfo.frameIndex == from)
                _animationInfo.IsUp = true;
        }

        /// <summary>
        /// 动画信息（用于振荡动画）
        /// </summary>
        private struct AnimationInfo
        {
            public int frameIndex;
            public bool IsUp;
        }
        private AnimationInfo _animationInfo;
    }
}
