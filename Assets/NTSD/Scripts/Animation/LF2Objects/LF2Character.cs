using BeatEmUpTemplate2D;
using FairyGUI;
using MoreMountains.Tools;
using NTSD.Animation.LF2Tasks;
using NTSD.App;
using NTSD.Extensions;
using NTSD.Game;
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

        public ActionSequenceDetectorModule _ActionSequenceDetector { get; private set; }

        // ========== 武器持有（对应 FLF $.hold）==========

        /// <summary>
        /// 当前持有的武器（对应 FLF $.hold.obj）
        /// </summary>
        private ILF2Object _heldWeapon;

        // ========== Unity 组件引用 ==========
        public Transform EntityTransform { get; private set; }
        public Transform VisualTransform { get; private set; }
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
        // N-17: 对应反汇编 entity[+94h]（0x94h=148），被抓时由 Entity_AI_Update kind=3 命中设为 300（0x12C），每 tick 由 selfCpoint.decrease 驱动
        protected int _caughtDecayAccum = 0;
        // 被抓方向：true=正面(front,对应反汇编[+98h]==4)，false=背面(back,[+98h]==6)
        protected bool _caughtFront = true;

        // ========== 死亡闪烁计数器（对应 FLF $.counter.dead_blink_count）==========
        // -1 = 不执行；0 = 开始；1~29 = 持续；>=30 = 结束销毁
        private int _deadBlinkCount = -1;

        // ========== N-25 合体/拆分字段（对应反汇编 sub_402340）==========
        // [+328h] 合体标志：0=未合体，1=已合体，-1=拆分冷却
        private int _mergeFlag = 0;
        // [+32Ch] 伙伴 slot 索引
        private int _partnerSlotIndex = 0;
        // [+330h] 合体前自身 oid
        private int _savedOidSelf = 0;
        // [+334h] 合体前伙伴 oid
        private int _savedOidPartner = 0;
        // [+338h] 合体/拆分冷却计时器
        private int _mergeTimer = 0;

        // ========== N-30 复活触发字段（对应反汇编 0x421085）==========
        // [+314h] 复活触发计数（>0 时允许死亡复活）
        private int _respawnTriggerCount = 0;
        // [+30Ch] 复活倒计时（>= 2 时每帧递减，< 2 时触发复活）
        private int _respawnCountdown = 0;

        // ========== N-30 输入序列字段（对应反汇编 0x422FCC）==========
        // [+408h~418h] 输入历史（5 个 int，每帧移位）
        private readonly int[] _inputSeq = new int[5];

        // 反汇编 entity[+318h]：角色专属字段，用于 N-27/N-30 数据变换后的状态标记
        private int _field318 = 0;

        // ========== 调试 ==========

        private bool _debugCollisionLog = false;
        private bool _debugComboLog = false;

        // ========== 变换连招状态机（对应反汇编 sub_414AE0 / sub_4149F0）==========

        /// <summary>角色ID，ModuleBind 时赋值</summary>
        private int _characterId;

        // def-guard-cooldown (0xC1h): 反汇编 frame 110/114 时置3，每帧递减，阻止 def 技能触发
        internal byte _ks193;

        // ========== 构造函数 ==========

        public LF2Character() : base()
        {
            AllocateStableId();

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
            Controller = new CharacterInputModule();
            _ActionSequenceDetector = new ActionSequenceDetectorModule();

            // 初始化状态处理器
            InitializeStates();
        }

        public void InjectDependencies(
            Transform entityTransform,
            Transform visualTransform,
            string name)
        {
            EntityTransform = entityTransform;
            VisualTransform = visualTransform;
            Name = name;
        }

        /// <summary>
        /// 池化路径专用初始化（无参版本）。
        /// InjectDependencies 之后、ModuleBind 之前调用。
        /// 不依赖旧 Character Hub，只初始化物理和状态机运行所需的基础字段。
        /// </summary>
        public void ModuleInitialize()
        {
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null
                ? BoundaryWallManager.Instance.IsPointWalkable
                : null;

            PS.x = 0; PS.y = 0; PS.z = 0;
            PS.vx = 0; PS.vy = 0; PS.vz = 0;

            AllowSwitchDir = true;
        }

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            Renderer = renderer;
        }

        public override void Reset()
        {
            ComboBuffer?.Reset();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            PS?.Reset();
            ObjectPointModule?.Reset();
            WeaponPointModule?.Reset();
            _heldWeapon = null;
            // 对应反汇编 0x00421185/0x00421191：spawn/reset 时清零
            HitStun = 0;
            FrameDelay = 10;
            ShotCount = 0;
            ResetSpark();
            _ks193 = 0;
            // N-25/N-30/N-7 字段重置
            _mergeFlag = 0; _partnerSlotIndex = 0; _savedOidSelf = 0; _savedOidPartner = 0; _mergeTimer = 0;
            _respawnTriggerCount = 0; _respawnCountdown = 0;
            System.Array.Clear(_inputSeq, 0, _inputSeq.Length);
            _field318 = 0;
        }

        public override void Destroy()
        {
            Reset();
        }

        public override void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
            SimulationTickDriver.Instance?.World?.Unregister(_ActionSequenceDetector);
        }

        /// <summary>
        /// 角色不回收到对象池，只执行 destroy 逻辑
        /// </summary>
        public override void OnTransitDestroy()
        {
            StateUpdate("destroy");
            Destroy();
        }

        // ========== 初始化（由 Character Hub 调用）==========

        /// <summary>
        /// 模块绑定（对应 LF2CharacterAnimator.ModuleBind）
        /// </summary>
        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            _characterId = characterId;

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

            // 注册到 SimulationWorld（ModuleBind 时序确定，SimulationTickDriver 已完成 Awake）
            SimulationTickDriver.Instance?.World?.Register(this);

            // 绑定 WPoint Factory
            if (WeaponPointModule != null && WeaponPointModule.Factory == null && LF2WeaponPointFactory.Instance != null)
            {
                WeaponPointModule.SetFactory(LF2WeaponPointFactory.Instance);
            }

            // 初始化并注册 ActionSequenceDetector
            _ActionSequenceDetector.Initialize(Controller.InputBuffer, this);
            SimulationTickDriver.Instance?.World?.Register(_ActionSequenceDetector);
        }

        /// <summary>
        /// 初始化角色属性
        /// </summary>
        public void Initialize(int maxHp, int maxMp)
        {
            CharacterStats.Initialize(maxHp, maxMp);
            Health.HP = maxHp;
            Health.MP = maxMp;
            // 反汇编 entity+340h = MaxMP，用于 kind=0/16 伤害 MP% 缩放
            Health.MaxMP = maxMp;
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
                    // Generic_PreInteraction 已迁移至 SimPreInteraction（PreInteractionTickAll 全局 pass）

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
            if (!CurStateResult)
            {
                generalResult = OnGenericStateEvent("combo", K);
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

            if (boundsProvider != null && boundsProvider.TryGetStageBoundsPx(out stageBoundsPx))
            {
                hasStageBounds = true;
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

            return (_heldWeapon as LF2WeaponBase)?.IsHeavy == true;
        }

        /// <summary>
        /// 丢弃武器（对应 FLF character.prototype.drop_weapon）
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            (_heldWeapon as LF2WeaponBase)?.Drop(dvx, dvy);

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
                ImmediateFrame(cpoint.vaction);
            }
            else
            {
                ImmediateFrame(LF2StandardFrames.JumpingAir);
            }
            caught_throwz = vdir;
            // 对应反汇编 0x0042E2FD：被投掷时 FrameDelay=-5
            FrameDelay = -5;
        }

        /// <summary>
        /// 被释放时的处理（对应 FLF character.js:2519-2527 caught_release）
        /// 由抓取者在释放时调用
        /// </summary>
        public void caught_release()
        {
            Catching = null;
            ImmediateFrame(181);
            Effect.Dvx = 3;
            Effect.Dvy = -3;
            Effect.TimeIn = -1;
            Effect.TimeOut = 0;
            // 对应反汇编 0x0042D796：被释放时 FrameDelay=-3
            FrameDelay = -3;
        }


        // ========== 当前帧信息（兼容属性）==========

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;
        public int CurrentState => Frame.D?.state ?? 0;

        // ========== 额外方法 ==========

        /// <summary>
        /// PreInteraction 全局 pass（对应 NTSD 反汇编 GameMode_Process sub_41BDA0）
        /// 由 SimulationWorld.PreInteractionTickAll 在所有对象 SerialTickAll 完成后统一调用。
        /// </summary>
        public override void SimPreInteraction(int tickIndex)
        {
            Generic_PreInteraction();
        }

        /// <summary>
        /// EntityCollision 阶段 - 对应反汇编 Entity_Collision (sub_4138F0) 角色专属分支
        /// 反汇编 0x00413BC0-0x00413C49：
        /// 前一帧 state==14（被抓取）且当前帧 state!=13（非冰冻）时，
        /// 在游戏模式 1/4 下对非豁免实体设 ShakeTimer=15（[esi+8h]=15）
        /// 豁免条件：oid ∈ [30,39] 且 oid != 38
        /// 注：ShakeTimer 双向趋零已在 TUUpdate() 中处理
        /// </summary>
        public override void SimEntityCollision(int tickIndex)
        {
            var fD = Frame?.D;
            if (fD == null) return;

            // 反汇编 0x413957: cmp frame.state, 2; jz → return（Jumping 跳过）
            if (fD.state == 2) return;

            // 反汇编 0x413BC0-0x413C49：prev_frame.state==2（Jumping）→ ShakeTimer=15
            // 条件：prev_frame.state==2 && frame.state!=13 && gameMode==1||4 && oid 豁免
            var prevFD = FrameCache.GetFrameDataById(Frame.PN);
            if (prevFD != null && prevFD.state == 2 && fD.state != 13)
            {
                int oid = FrameCache?.Wrapper?.characterId ?? -1;
                if (!(oid / 10 == 3 && oid != 38))
                    ShakeTimer = 15;
            }

            // 反汇编 0x413D0C-0x413D69: frame.mp < 0 && dword_446970 → MP cost
            // +308h(PP) += mp (mp<0, so PP decreases); if PP < mp (signed, only if PP already negative) → frame=next
            if (fD.mp < 0 && NTSDGlobal.MPEnabled && Health != null)
            {
                Health.PP += fD.mp; // PP -= |mp|
                if (Health.PP < fD.mp) // signed: only triggers if PP was already negative
                    Trans.Frame(fD.next, 0);
            }

            // 反汇编 0x413DDA: cmp frame, 0x6E(110); 0x72(114) → [esi+0C1h]=3 (_ks193=3)
            // 阻止 def 相关技能触发 3 帧
            int frameN = Frame.N;
            if (frameN == 110 || frameN == 114)
                _ks193 = 3;

            // 反汇编 0x413DEB: cmp frame, 0xCA(202) → ShakeTimer=20
            if (frameN == 202)
                ShakeTimer = 20;
        }

        public override void SimTransit(int tickIndex)
        {
            Transit();

            // N-26 反汇编 0x004219F1 test edx,edx + jnz：仅 entity_type==0（角色）的 state==9995
            // data 替换为 oid=50, frame=0
            var fD = Frame?.D;
            if (fD != null && fD.state == 9995)
            {
                var wrapper50 = CharacterAnimtorManager.Instance?.GetCharacterConfig(50);
                if (wrapper50 != null)
                {
                    FrameCache.Load(wrapper50);
                    Trans.Frame(0, 0);
                }
            }

            // N-27 反汇编 0x421B05 test eax,eax + jnz：仅 entity_type==0（角色）的 state==9996
            // facing==1（right）时生成5个碎片（oid=217×4，oid=218×1），HitStun=6
            if (fD != null && fD.state == 9996 && PS.dir == "right")
                SpawnFragments9996Character();

            // N-31 反汇编 Game_FrameUpdate pseudoC pos~623462：
            // 进入 state=13(Frozen)/frame=200 时（上一帧不是该状态）播放 sound 15 + 生成 15 个 oid=999（frame 120/125/130/135）
            // 进入/持续 state=18(Burning)/19(FirenSpecific) 时：进入时 7 个，持续时 1/4 概率 1 个；frame=140
            ApplyFrozenBurningParticles();

            // N-25 合体/拆分（对应反汇编 sub_402340）
            if (_mergeTimer > 0) _mergeTimer--;
            ApplyMergeLogic();

            // N-30 死亡复活 + 输入序列触发
            ApplyDeathRespawn();
            ApplyInputSequenceRespawn();
        }

        private void ApplyFrozenBurningParticles()
        {
            var fD = Frame?.D;
            if (fD == null) return;
            var prevFD = FrameCache.GetFrameDataById(Frame.PN);
            int curState = fD.state;
            int prevState = prevFD?.state ?? curState;

            // N-31 Frozen/MpDrain transition: state==13 or frame==200, previous was not
            bool entersFrozenOrCut = (curState == LF2States.Frozen || Frame.N == 200)
                                   && !(prevState == LF2States.Frozen || Frame.PN == 200);
            if (entersFrozenOrCut)
            {
                PlaySound("15");
                SpawnOid999Particles(15, new int[] { 120, 125, 130, 135 });
            }

            // N-31 Burning/FirenSpecific: entering = 7 particles; ongoing = 25% chance 1 particle
            bool isBurning = (curState == LF2States.Burning || curState == LF2States.FirenSpecific);
            bool wasBurning = (prevState == LF2States.Burning || prevState == LF2States.FirenSpecific);
            if (isBurning)
            {
                int count = (!wasBurning) ? 7 : (UnityEngine.Random.Range(0, 4) == 0 ? 1 : 0);
                if (count > 0)
                    SpawnOid999Particles(count, new int[] { 140 });
            }
        }

        private void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            AppManager.Instance?.SoundPlayer?.PlaySfx(soundId);
        }

        private void SpawnOid999Particles(int count, int[] framePicks)
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;
            for (int i = 0; i < count; i++)
            {
                int frame = framePicks[i < framePicks.Length ? i : (i % framePicks.Length)];
                var op = new ObjectPoint
                {
                    oid = 999, kind = 0, action = frame,
                    dvx = UnityEngine.Random.Range(0, 11) - 5,
                    dvy = -(UnityEngine.Random.Range(0, 20) + 8),  // approx: -Random(29) offset from entity.z
                    dvz = UnityEngine.Random.Range(0, 11) - 5,
                    x = UnityEngine.Random.Range(0, 29) - 14,
                    y = -(UnityEngine.Random.Range(0, 39) - 19),
                    facing = UnityEngine.Random.Range(0, 2)
                };
                factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                {
                    opoint = op, parent = null, team = Team,
                    pos = new UnityEngine.Vector3(PS.x + op.x, PS.y + op.y, PS.z),
                    z = PS.z, dir = PS.dir, dvz = op.dvz
                });
            }
        }

        private void SpawnFragments9996Character()
        {
            var factory = LF2ObjectPointFactory.Instance;
            if (factory == null) return;
            for (int i = 0; i < 5; i++)
            {
                int oid = (i < 4) ? 217 : 218;
                float vy = -(UnityEngine.Random.Range(0, 15) / 2f + 5f);
                float vx, vz;
                int rnd2 = UnityEngine.Random.Range(0, 2);
                int rnd3 = UnityEngine.Random.Range(0, 3);
                if (i < 2)       { vx = (i == 0 ? 1 : -1) * (rnd3 + 10f); vz = (i == 0 ? 1 : -1) * (rnd2 + 3f); }
                else if (i < 4)  { vx = UnityEngine.Random.Range(0, 7) - 3f; vz = (i % 2 == 0 ? 1 : -1) * (rnd2 + 3f); }
                else             { vx = UnityEngine.Random.Range(0, 7) - 3f; vz = (UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1) * (rnd3 + 10f); }
                int frame = UnityEngine.Random.Range(0, 4);
                string dir = UnityEngine.Random.Range(0, 2) == 0 ? "left" : "right";
                var syntheticOpoint = new ObjectPoint
                {
                    oid = oid, kind = 0, action = frame,
                    dvx = (int)vx, dvy = (int)vy, dvz = (int)vz,
                    x = UnityEngine.Random.Range(0, 7) - 3,
                    y = UnityEngine.Random.Range(0, 7) - 9,
                    facing = (dir == "right") ? 0 : 1
                };
                factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                {
                    opoint = syntheticOpoint, parent = null, team = Team,
                    pos = new UnityEngine.Vector3(PS.x + syntheticOpoint.x, PS.y + syntheticOpoint.y, PS.z + 1),
                    z = PS.z + 1, dir = dir, dvz = vz
                });
            }
            HitStun = 6;
        }

        /// <summary>
        /// N-25: oid=7/8 合体 / oid=51 拆分逻辑（对应反汇编 sub_402340）
        /// 合体条件：oid=7/8, HP>0, _mergeTimer==0, frame.state==2, HP&lt;177 OR dword_44F224==1
        /// 拆分条件：oid=51, _mergeFlag==1, (frame&lt;9 OR frame>260), _mergeTimer&lt;=0
        /// </summary>
        private void ApplyMergeLogic()
        {
            if (FrameCache?.Wrapper == null) return;
            int oid = FrameCache.Wrapper.characterId;

            // ── 合体路径 ──
            if ((oid == 7 || oid == 8) && Health.HP > 0 && _mergeTimer == 0
                && Frame.D?.state == 2 && Health.HP < 177)
            {
                int partnerOid = 15 - oid; // oid=7→partner=8, oid=8→partner=7
                var allObjs = ListPool<LF2LivingObject>.Get();
                Match?.GetAllLivingObjects(allObjs);

                LF2Character partner = null;
                int candidateCount = 0;
                for (int i = 0; i < allObjs.Count && candidateCount < 10; i++)
                {
                    if (!(allObjs[i] is LF2Character ch)) continue;
                    if (ch == this) continue;
                    if (ch.FrameCache?.Wrapper?.characterId != partnerOid) continue;
                    if (ch.Team != Team) continue;
                    if (ch.Health.HP <= 0) continue;
                    if (ch._mergeTimer != 0) continue;
                    int pState = ch.Frame.D?.state ?? -1;
                    bool stateOk = pState == 2 || (pState != 0x0E && ch._mergeTimer == 0 && candidateCount > 9);
                    if (!stateOk) continue;
                    float dx = PS.x - ch.PS.x;
                    float dz = PS.z - ch.PS.z;
                    if (System.Math.Abs(dx) >= 50f || System.Math.Abs(dz) >= 8f) continue;
                    if (PS.x <= ch.PS.x && candidateCount <= 9) continue;
                    candidateCount++;
                    partner = ch;
                }
                ListPool<LF2LivingObject>.Release(allObjs);

                if (partner == null) return;

                // Find oid=0x33 (51) data
                var wrapper51 = CharacterAnimtorManager.Instance?.GetCharacterConfig(0x33);
                if (wrapper51 == null) return;

                // Sum HP/PP (clamp to max)
                int newHp = System.Math.Min(Health.HP + partner.Health.HP, Health.HPBound);
                int newPp = System.Math.Min(Health.PP + partner.Health.PP, Health.PPBound);

                // Save original oids
                _savedOidSelf = oid;
                _savedOidPartner = partnerOid;
                _partnerSlotIndex = partner.StableId;

                // Average position
                PS.x = (PS.x + partner.PS.x) * 0.5f;
                PS.z = (PS.z + partner.PS.z) * 0.5f;
                partner.PS.x = PS.x;
                partner.PS.z = PS.z;

                // Apply merge
                FrameCache.Load(wrapper51);
                Frame.N = 0x7A; // 122
                Frame.D = FrameCache.GetFrameDataById(0x7A);
                _mergeFlag = 1;
                PS.vx = 0; PS.vy = 0; PS.vz = 0;
                Health.HP = newHp;
                Health.PP = newPp;
                ShotCount = 500; // [+308h]=0x1F4
                _mergeTimer = 4500; // 0x1194

                // Deactivate partner
                partner._mergeFlag = -1;
                partner._mergeTimer = 4500;
                partner.Health.HP = 0;
                return;
            }

            // ── 拆分路径 ──
            if (oid == 0x33 && _mergeFlag == 1 && _mergeTimer <= 0
                && (Frame.N < 9 || Frame.N > 0x104))
            {
                // Restore self oid
                var wrapperSelf = CharacterAnimtorManager.Instance?.GetCharacterConfig(_savedOidSelf);
                if (wrapperSelf == null) return;

                int halfHp = Health.HP / 2;
                int halfPp = Health.PP / 2;

                FrameCache.Load(wrapperSelf);
                _mergeFlag = -1;
                _mergeTimer = 900; // 0x384
                Health.HP = halfHp;
                Health.PP = halfPp;
                PS.vx = 0; PS.vy = 0; PS.vz = 0;

                // Restore partner
                var allObjs = ListPool<LF2LivingObject>.Get();
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    if (!(allObjs[i] is LF2Character ch)) continue;
                    if (ch.StableId != _partnerSlotIndex) continue;
                    var wrapperPartner = CharacterAnimtorManager.Instance?.GetCharacterConfig(_savedOidPartner);
                    if (wrapperPartner == null) break;
                    ch.FrameCache.Load(wrapperPartner);
                    ch._mergeFlag = -1;
                    ch._mergeTimer = 900;
                    ch.Health.HP = halfHp;
                    ch.Health.PP = halfPp;
                    ch.PS.x = PS.x;
                    ch.PS.z = PS.z;
                    ch.Team = Team;
                    ch.Frame.N = 0x70; // 112
                    ch.Frame.D = ch.FrameCache.GetFrameDataById(0x70);
                    // facing = 1 - self_facing
                    ch.PS.dir = PS.dir == "right" ? "left" : "right";
                    // velocities from disasm: 0x40822000=4.066, 0xC0690000=-3.5625, 0x4072C000=3.671875
                    ch.PS.vx = 4.066f;
                    ch.PS.vy = -3.5625f;
                    ch.PS.vz = 3.671875f;
                    break;
                }
                ListPool<LF2LivingObject>.Release(allObjs);
            }
        }

        /// <summary>
        /// N-30 死亡时复活触发（对应反汇编 0x421085）
        /// 条件：frame.state==0x0E, HP&lt;=0, (_respawnTriggerCount>0 OR team==5), entity_type==0, [+8]∈(0,5)
        /// </summary>
        private void ApplyDeathRespawn()
        {
            if (Frame.D?.state != 0x0E) return;
            if (Health.HP > 0) return;
            // [+2F4h] < 0 OR team==5 check (OwnerEntityIndex < 0 OR Team==5)
            if (OwnerEntityIndex >= 0 && Team != 5) return;
            // esi >= 0x14 check: esi is slot index; we use StableId as proxy
            if (StableId < 0x14 && OwnerEntityIndex >= 0 && Team != 5) return;
            // [+8] > 0 && [+8] < 5: ShakeTimer in (0,5)
            if (ShakeTimer <= 0 || ShakeTimer >= 5) return;

            if (_respawnTriggerCount > 0)
            {
                // HP chain copy: [+310h]→[+30Ch], [+314h]→[+304h]→[+300h]→[+2FCh]
                _respawnCountdown = ShotCount; // [+30Ch] = [+310h] (ShotCount maps to +308h, use _respawnCountdown)
                // Actually: [+30Ch]=_respawnCountdown, [+310h]=ShotCount (per summary)
                // Disasm: [+30Ch]=[+310h], [+314h]→[+304h]→[+300h]→[+2FCh]
                // We map: _respawnCountdown=[+30Ch], ShotCount=[+308h], _respawnTriggerCount=[+314h]
                // [+304h] and [+300h] are intermediate; final HP=[+314h]
                Health.HP = _respawnTriggerCount;
                _respawnTriggerCount = 0;
                ShotCount = 0; // [+310h]=0

                Team = 1;
                Frame.N = 0xDB; // 219
                Frame.D = FrameCache.GetFrameDataById(0xDB);
                HitStun = 0;
                FrameDelay = 10;

                // oid in [0x1E, 0x24] → set [+318h]=0x8C
                int selfOid = FrameCache.Wrapper?.characterId ?? -1;
                if (selfOid >= 0x1E && selfOid <= 0x24)
                    _field318 = 0x8C;

                // Spawn oid=0x3E6 (998)
                {
                    var factory = LF2ObjectPointFactory.Instance;
                    if (factory != null)
                    {
                        var op = new ObjectPoint { oid = 0x3E6, kind = 0, action = 6, dvx = 0, dvy = 0, dvz = 0, x = 0, y = 0, facing = 0 };
                        factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                        {
                            opoint = op, parent = null, team = Team,
                            pos = new UnityEngine.Vector3(PS.x, PS.y, PS.z),
                            z = PS.z, dir = PS.dir, dvz = 0
                        });
                    }
                }

                // Post-spawn ally propagation: same team, HP>0, entity_type==0
                var allObjs = ListPool<LF2LivingObject>.Get();
                Match?.GetAllLivingObjects(allObjs);
                for (int i = 0; i < allObjs.Count; i++)
                {
                    if (!(allObjs[i] is LF2Character ch)) continue;
                    if (ch == this) continue;
                    if (ch.Team != Team) continue;
                    if (ch.Health.HP <= 0) continue;
                    ch.FrameCache.Load(FrameCache.Wrapper);
                    int chVy = (int)ch.PS.vy;
                    ch.Frame.N = chVy != 0 ? 0xD4 : 0;
                    ch.Frame.D = ch.FrameCache.GetFrameDataById(ch.Frame.N);
                }
                ListPool<LF2LivingObject>.Release(allObjs);
            }
            else
            {
                // [+314h]<=0: check [+30Ch]>=2 → dec; else deactivate + respawn
                if (_respawnCountdown >= 2)
                {
                    _respawnCountdown--;
                }
                else
                {
                    // Deactivate slot
                    Health.HP = 0;
                    // Respawn: average position of same-team allies
                    var allObjs = ListPool<LF2LivingObject>.Get();
                    Match?.GetAllLivingObjects(allObjs);
                    float sumX = 0, sumZ = 0;
                    int count = 0;
                    for (int i = 0; i < allObjs.Count; i++)
                    {
                        if (!(allObjs[i] is LF2Character ch)) continue;
                        if (ch == this) continue;
                        if (ch.Team != Team) continue;
                        if (ch.Health.HP <= 0) continue;
                        sumX += ch.PS.x;
                        sumZ += ch.PS.z;
                        count++;
                    }
                    ListPool<LF2LivingObject>.Release(allObjs);
                    if (count > 0)
                    {
                        PS.x = sumX / count + 31f;
                        PS.z = sumZ / count + 31f;
                        ShotCount = 500; // [+308h]=0x1F4
                        Health.HP = _respawnCountdown; // restore from [+304h]→[+300h]→[+2FCh]
                        Frame.N = 0xD4; // 212
                        Frame.D = FrameCache.GetFrameDataById(0xD4);
                        PS.vx = (int)PS.vx; // copy float→int
                        PS.vy = 0;
                        PS.vz = 0;
                    }
                }
            }
        }

        /// <summary>
        /// N-30 输入序列触发 oid=998 团队效果（对应反汇编 0x422FCC）
        /// 输入序列 9,0,9,0 → frame=0; 9,9,9,9 → frame=2; 9,5,9,5 → frame=4
        /// </summary>
        private void ApplyInputSequenceRespawn()
        {
            // Only low-index active characters trigger this
            if (StableId >= 0x32) return;
            if (Health.HP <= 0) return;

            // 反汇编 0x421085: v239[259]==9 (gate=att), check v239[260..262]
            // _inputSeq[0]=this[258](oldest), [1]=this[259](gate), [2]=this[260], [3]=this[261], [4]=this[262](newest)
            // key codes: 9=att, 6=jump, 5=down, 0=def, 8=left, 2=right, 4=up
            if (_inputSeq[1] != 9) return;
            int spawnFrame = -1;
            // (0,9,0) → frame 100
            if (_inputSeq[2] == 0 && _inputSeq[3] == 9 && _inputSeq[4] == 0) spawnFrame = 100;
            // (9,9,9) → frame 102
            else if (_inputSeq[2] == 9 && _inputSeq[3] == 9 && _inputSeq[4] == 9) spawnFrame = 102;
            // (5,9,5) → frame 104
            else if (_inputSeq[2] == 5 && _inputSeq[3] == 9 && _inputSeq[4] == 5) spawnFrame = 104;
            if (spawnFrame < 0) return;

            // Clear input history
            System.Array.Clear(_inputSeq, 0, _inputSeq.Length);

            // 反汇编: frame = v424 - 100 (v424=100→frame=0, v424=102→frame=2, v424=104→frame=4)
            {
                int spawnAction = spawnFrame - 100;
                var factory = LF2ObjectPointFactory.Instance;
                if (factory != null)
                {
                    var op = new ObjectPoint { oid = 0x3E6, kind = 0, action = spawnAction, dvx = 0, dvy = 0, dvz = 0, x = 0, y = 0, facing = 0 };
                    factory.EnqueueCreateObject(new LF2Tasks.OPointCreateTask
                    {
                        opoint = op, parent = null, team = Team,
                        pos = new UnityEngine.Vector3(PS.x, PS.y, PS.z),
                        z = PS.z, dir = PS.dir, dvz = 0
                    });
                }
            }

            // Post-spawn ally propagation
            var allObjs = ListPool<LF2LivingObject>.Get();
            Match?.GetAllLivingObjects(allObjs);
            for (int i = 0; i < allObjs.Count; i++)
            {
                if (!(allObjs[i] is LF2Character ch)) continue;
                if (ch == this) continue;
                if (ch.Team != Team) continue;
                if (ch.Health.HP <= 0) continue;
                ch.FrameCache.Load(FrameCache.Wrapper);
                int chVy = (int)ch.PS.vy;
                ch.Frame.N = chVy != 0 ? 0xD4 : 0;
                ch.Frame.D = ch.FrameCache.GetFrameDataById(ch.Frame.N);
            }
            ListPool<LF2LivingObject>.Release(allObjs);
        }

        /// <summary>
        /// 记录一次按键到输入序列（对应反汇编 sub_414D80 at 0x00414D80）
        /// 移位：[0]←[1]←[2]←[3]←[4]←ntsdCode
        /// key codes: 9=att, 6=jump, 5=down, 0=def, 8=left, 2=right, 4=up
        /// </summary>
        internal void RecordInputKey(int ntsdCode)
        {
            _inputSeq[0] = _inputSeq[1];
            _inputSeq[1] = _inputSeq[2];
            _inputSeq[2] = _inputSeq[3];
            _inputSeq[3] = _inputSeq[4];
            _inputSeq[4] = ntsdCode;
        }

        /// <summary>
        /// 重新加载角色帧数据
        /// </summary>
        public void ReloadCharacterFrameData()
        {
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
