using BeatEmUpTemplate2D;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEditor.U2D.Animation;
using UnityEngine;
using UnityEngine.Pool;

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
    public partial class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

        // ========== 角色专用模块 ==========

        /// <summary>
        /// 连招缓冲区（对应 FLF $.combo_buffer）
        /// </summary>
        public LF2ComboBufferModule ComboBuffer { get; private set; }

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
        private ILF2Object _heldWeapon;

        // ========== Unity 组件引用 ==========
        private Vector3 _baseLocalPosition;

        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;

        // ========== 抓取系统字段（Catching System Fields）==========

        protected Vector3 caught_b_holdpoint;
        protected CatchPoint caught_b_cpoint;
        protected int caught_b_adir;
        protected int caught_b_vdir;
        protected int? caught_throwz;
        protected int? caught_throwinjury;
        protected int caught_decrease_counter;  // 抓取递减计数器（被抓者按键时递减，归零释放）

        // ========== 调试 ==========

        private bool _debugCollisionLog = false;
        private bool _debugComboLog = false;

        // ========== 构造函数 ==========

        public LF2Character(MoreMountains.TopDownEngine.Character hub)
        {
            _CharacterHub = hub;

            // 创建角色专用模块
            ComboBuffer = new LF2ComboBufferModule();
            ObjectPointModule = new LF2ObjectPointModule();
            WeaponPointModule = new LF2WeaponPointModule();
            _hitCounters = new LF2HitCountersModule();
            _characterStats = new NTSDCharacterStats();
            
            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            PS = new PhysicsState();
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);

            // 初始化状态处理器
            InitializeStates();
        }

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            // 角色通过 Character Hub 初始化，不使用此方法
        }

        public override void Reset()
        {
            ComboBuffer?.Reset();
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
        public void ModuleInitialize(SpriteRenderer spriteRenderer,List<Sprite> sprites,Vector3 baseLocalPosition)
        {
            _baseLocalPosition = baseLocalPosition;

            // 用 GameObject 名作为日志标识（可在 Inspector 改名来区分多角色）
            Name = _CharacterHub?.gameObject.name ?? "Character";

            // 初始化物理计算层
            _mech = new CharacterMechanics();
            Controller = _CharacterHub._CharacterInput;
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null ? BoundaryWallManager.Instance.IsPointWalkable : null;

            // 初始化物理状态
            PS.FromUnityPosition(_CharacterHub.transform.position);
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
            ComboBuffer?.Reset();
            ItrRest?.Reset();
            _hitCounters?.Reset();

            // 绑定 mass
            _mass = NTSDSpec.GetMassOrDefault(characterId);

            // 绑定 OPoint Factory
            if (ObjectPointModule != null && ObjectPointModule.Factory == null && LF2ObjectPointFactory.Instance != null)
            {
                ObjectPointModule.SetFactory(LF2ObjectPointFactory.Instance);
            }

            // 绑定 WPoint Factory
            if (WeaponPointModule != null && WeaponPointModule.Factory == null && LF2WeaponPointFactory.Instance != null)
            {
                WeaponPointModule.SetFactory(LF2WeaponPointFactory.Instance);
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
            ComboBuffer.Reset();
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

        // ========== 状态机初始化 ==========

        protected override void InitializeStates()
        {
            // 注册基础状态处理器 (0-17)
            _states[LF2States.Standing] = State_Standing;
            _states[LF2States.Walking] = State_Walking;
            _states[LF2States.Running] = State_Running;
            _states[LF2States.Attack] = State_Attack;
            _states[LF2States.Jump] = State_Jump;
            _states[LF2States.Dash] = State_Dash;
            _states[LF2States.Rowing] = State_Rowing;
            _states[LF2States.Defending] = State_Defending;
            _states[LF2States.BrokenDefend] = State_BrokenDefend;
            _states[LF2States.Catching] = State_Catching;
            _states[LF2States.BeingCaught] = State_BeingCaught;
            _states[LF2States.Injured] = State_Injured;
            _states[LF2States.Falling] = State_Falling;
            _states[LF2States.Frozen] = State_Frozen;
            _states[LF2States.Lying] = State_Lying;
            _states[LF2States.StopRunning] = State_StopRunning;
            _states[LF2States.Injured2] = State_Injured2;
            _states[LF2States.Charging] = State_Charging;
            _states[LF2States.Burning] = State_Burning;
        }

        /// <summary>
        /// 通用状态处理器
        /// 对应 LF2 源码的 states.generic
        /// 处理所有状态共享的逻辑，如物理更新、输入缓冲、全局受击判定等
        /// </summary>
        protected override bool OnGenericStateEvent(string eventType, object eventData = null)
        {
            switch (eventType)
            {
                case "frame":
                    // 🖼️ 每帧执行的通用逻辑 (MP/HP恢复, OPoint生成等) (对应 FLF character.js:14-52)
                    return Generic_Frame();

                case "TU":
                    // ⏱️ 时间单元(Time Unit)更新 (状态机, buff更新, 物理重置) (对应 FLF character.js:54-183)
                    return Generic_TU();

                case "transit":
                    // 🚀 动态物理更新 (摩擦力, 位置更新) (对应 FLF character.js:185-190)
                    return Generic_Transit();

                case "combo":
                    // 🎮 通用输入处理 (多键连招映射, 方向键处理) (对应 FLF character.js:191-215)
                    return Generic_Combo(eventData as string);

                case "post_combo":
                    // 🛑 连招后处理 (清理缓存等) (对应 FLF character.js:217-220)
                    // TODO: 实现 pre_interaction() - 预处理交互 (武器拾取, 对象交互)
                    Generic_PreInteraction();
                    return false;

                case "state_exit":
                    // 🚪 状态退出清理 (清理连招缓冲) (对应 FLF character.js:221-228)
                    return Generic_StateExit();
            }

            return false;
        }


        // ========== 核心生命周期（对应 FLF livingobject/character）==========

        /// <summary>
        /// 连招更新 - 对应 FLF character.combo_update()
        /// 参考：FLF character.js:1800-1846
        /// </summary>
        protected override void ComboUpdate()
        {
            string rawCombo = ComboBuffer?.Combo;
            string K = rawCombo;
            if (string.IsNullOrEmpty(K)) { K = null; }

            Log.LogState(Name, "Combo", $"tick: buffer='{rawCombo}' state={Frame.D?.state} frame={Frame.N}");

            // 特殊处理：跳跃攻击组合
            if (rawCombo == "jump-att") { K = "jump"; }

            StateHandler CurStateHandler = _states[Frame.D.state];

            bool CurStateResult = CurStateHandler?.Invoke("combo", K) ?? false;
            bool generalResult = false;
            if (!CurStateResult)            {                generalResult = OnGenericStateEvent("combo", K);
            }

            CurStateHandler?.Invoke("post_combo");
            OnGenericStateEvent("post_combo");

            ComboBuffer?.AfterComboUpdate(CurStateResult, generalResult, rawCombo: rawCombo, mappedCombo: K);
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            float blockedMoveScale = Match?.SceneQuery?.TestBlockingXZ(this, PS.vx, PS.vz) == true ? 0.1f : 1f;

            bool hasStageBounds = false;
            LF2StageBoundsPx stageBoundsPx = default;
            var boundsProvider = NTSD.LevelEditor.BoundaryWallManager.Instance;
            if (boundsProvider != null && boundsProvider.TryGetStageBoundsPx(out stageBoundsPx))            {                hasStageBounds = true;
            }
            var ctx = new CharacterMechanicsContext(
                PS,
                Frame.D,
                GetSpriteWidthPxForCollision(),
                hasStageBounds,
                stageBoundsPx,
                _mass,
                 NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                blockedMoveScale,
                _cachedIsPointWalkable
            );

            var result = _mech.Step(ctx);

            if (_debugCollisionLog && result.boundaryMode != BoundaryResolveMode.None)
            {
                Tools.Log.Info("[Boundary] ResolveMode={0}", result.boundaryMode);
            }

            // ground plane（Unity X/Y）写回
            const float ppu = 100f;
            _CharacterHub.transform.position = new Vector3(
                Mathf.Round(result.groundPlanePos.x * ppu) / ppu,
                Mathf.Round(result.groundPlanePos.y * ppu) / ppu,
                _CharacterHub.transform.position.z
            );

            // 视觉高度偏移（Unity local Y），同样对齐像素网格
            float snappedVisualY = Mathf.Round(result.visualYOffset * ppu) / ppu;
            _CharacterHub._ModeTrans.localPosition = _baseLocalPosition + new Vector3(0f, snappedVisualY, 0f);

            _CharacterHub.SetGrounding(_CharacterHub.transform.position.y, result.grounded);

        }

        /// <summary>
        /// 武器点更新
        /// 对应 FLF wpoint()
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 方向控制 ==========

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
                setDirectionByString: SwitchDir,
                timeoutFrames: NTSDGlobal.Combo.Timeout,
                debugLog: _debugComboLog,
                stableId: StableId
            );
        }

        // ========== 武器持有 ==========

        /// <summary>
        /// 持有武器（对应 FLF character.prototype.hold_weapon）
        /// </summary>
        public void HoldWeapon(ILF2Object weapon)
        {
            _heldWeapon = weapon;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public ILF2Object GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 重型武器
        /// </summary>
        /// <returns></returns>
        public bool IsHeavyWeapon() 
        {
            if(_heldWeapon == null)
                return false;
            return _heldWeapon is LF2HeavyWeapon;
        }

        /// <summary>
        /// 丢弃武器（对应 FLF character.prototype.drop_weapon）
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            if (_heldWeapon is LF2LightWeapon lightWeapon)
            {
                lightWeapon.Drop(dvx, dvy);
            }
            else if (_heldWeapon is LF2HeavyWeapon heavyWeapon)
            {
                heavyWeapon.Drop(dvx, dvy);
            }

            _heldWeapon = null;
        }

        // ========== 帧播放接口 ==========

        /// <summary>
        /// 转换到指定帧
        /// </summary>
        public override void TransitionToFrame(int frameId, int authority = 20)
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

       
        // ========== 抓取系统（Catching System）==========

        /// <summary>
        /// 每帧被抓取者接收位置同步数据（对应 FLF character.js:2475-2481 caught_b）
        /// 由抓取者在 state 9 的 TU 事件中每帧调用
        /// </summary>
        public void caught_b(Vector3 holdpoint, CatchPoint cpoint, int adir, int vdir)
        {
            caught_b_holdpoint = holdpoint;
            caught_b_cpoint = cpoint;
            caught_b_adir = adir;
            caught_b_vdir = vdir;
        }

        /// <summary>
        /// 返回当前帧的 cpoint.kind（对应 FLF character.js:2486-2489 caught_cpointkind）
        /// 用于抓取者/被抓者双向验证 cpoint 匹配
        /// </summary>
        public int caught_cpointkind()
        {
            var cpoint = CurrentFrame?.cpoint;
            return cpoint?.kind ?? 0;
        }

        /// <summary>
        /// 检查被抓时是否可受伤（对应 FLF character.js:2494-2501 caught_cpointhurtable）
        /// </summary>
        public bool caught_cpointhurtable()
        {
            var cpoint = CurrentFrame?.cpoint;
            if (cpoint == null) return true;
            return cpoint.hurtable != 0;
        }

        /// <summary>
        /// 被投掷时的处理（对应 FLF character.js:2506-2514 caught_throw）
        /// 由抓取者在投掷时调用
        /// </summary>
        public void caught_throw(CatchPoint cpoint, int vdir)
        {
            if (cpoint.vaction != 0)
            {
                TransitionToFrame(cpoint.vaction, 22);
            }
            else
            {
                TransitionToFrame(LF2StandardFrames.JumpingAir, 22);
            }
            caught_throwz = vdir;
        }

        /// <summary>
        /// 被释放时的处理（对应 FLF character.js:2519-2527 caught_release）
        /// 由抓取者在释放时调用
        /// </summary>
        public void caught_release()
        {
            Catching = null;
            TransitionToFrame(181, 22);
            Effect.Dvx = 3;
            Effect.Dvy = -3;
            Effect.TimeIn = -1;
            Effect.TimeOut = 0;
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
    }
}
