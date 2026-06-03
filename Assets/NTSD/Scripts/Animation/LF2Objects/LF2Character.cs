using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Game;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using System;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// Character-specific battle logic layered on LF2LivingObject.
    /// Gameplay behavior should follow the C++ release project entity/frame/input model;
    /// Unity-specific code here is only for component wiring, pooling, and data adapters.
    /// 
    /// Inheritance: LF2LivingObject -> LF2Character
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

        // ========== 角色专用模块 ==========

        public NTSDInputStateModule InputState { get; private set; }

        /// <summary>
        /// OPoint module for release-style object creation requests.
        /// </summary>
        public LF2ObjectPointModule ObjectPointModule { get; private set; }

        /// <summary>
        /// WPoint module for release-style weapon hold/throw requests.
        /// </summary>
        public LF2WeaponPointModule WeaponPointModule { get; private set; }

        /// <summary>
        /// Hit counters for fall/bdefend-style damage accumulation.
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;

        /// <summary>
        /// 角色属性（HP/MP 等）
        /// </summary>
        private readonly NTSDCharacterStats _characterStats;
        public override NTSDCharacterStats CharacterStats => _characterStats;

        // ========== Weapon Holding ==========

        /// <summary>
        /// Currently held weapon.
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
        private Func<Vector2, float, bool> _cachedIsNearConcaveVertex;

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

        // ========== Death Blink Counter ==========
        // -1 = 不执行；0 = 开始；1~29 = 持续；>=30 = 结束销毁
        private int _deadBlinkCount = -1;

        // ========== C++ release character runtime aliases ==========
        private int MergeFlag { get => Runtime.MergeFlag; set => Runtime.MergeFlag = value; }
        private int MergePartnerSlotIndex { get => Runtime.MergePartnerSlotIndex; set => Runtime.MergePartnerSlotIndex = value; }
        private int MergeSelfObjectId { get => Runtime.MergeSelfObjectId; set => Runtime.MergeSelfObjectId = value; }
        private int MergePartnerObjectId { get => Runtime.MergePartnerObjectId; set => Runtime.MergePartnerObjectId = value; }
        private int MergeTimer { get => Runtime.MergeTimer; set => Runtime.MergeTimer = value; }
        private int RespawnCount { get => Runtime.RespawnCount; set => Runtime.RespawnCount = value; }
        private int RespawnCountdown { get => Runtime.RespawnCountdown; set => Runtime.RespawnCountdown = value; }

        // ========== Release Input / Transform Runtime ==========

        /// <summary>角色ID，ModuleBind 时赋值</summary>
        private int _characterId;

        private bool _catchingStateTU;
        private int _catchingCounter;
        private int _catchingAttacks;
        private bool _jumpFrameTU;
        private int _jumpAttackLock;

        // ========== 构造函数 ==========

        public LF2Character() : base()
        {
            AllocateStableId();

            // 创建角色专用模块
            InputState = new NTSDInputStateModule();
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

            // Character state dispatch is explicit; no runtime handler table is needed here.
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
            _cachedIsNearConcaveVertex = BoundaryWallManager.Instance != null
                ? BoundaryWallManager.Instance.IsNearConcaveVertex
                : null;

            PS.x = 0; PS.y = 0; PS.z = 0;
            PS.vx = 0; PS.vy = 0; PS.vz = 0;

        }

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            Renderer = renderer;
        }

        public override void Reset()
        {
            InputState?.Reset();
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
            MergeFlag = -1; MergePartnerSlotIndex = -1; MergeSelfObjectId = 0; MergePartnerObjectId = 0; MergeTimer = 0;
            RespawnCount = 0; RespawnCountdown = 0;
            System.Array.Clear(Runtime.InputHistory, 0, Runtime.InputHistory.Length);
            ResetStateRuntime();
        }

        public override void Destroy()
        {
            Reset();
        }

        public override void UnregisterFromWorld()
        {
            SimulationTickDriver.Instance?.World?.Unregister(this);
        }

        /// <summary>
        /// 角色不回收到对象池，只执行 destroy 逻辑
        /// </summary>
        public override void OnTransitDestroy()
        {
            DestroyEvent();
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
            InputState?.Reset();
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
            InputState.Reset();
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



        protected override void ResetStateRuntime()
        {
            _catchingStateTU = false;
            _catchingCounter = 0;
            _catchingAttacks = 0;
            _jumpFrameTU = false;
            _jumpAttackLock = 0;
        }



        protected override bool FrameEvent()
        {
            return Generic_Frame() || DispatchCurrentStateEvent("frame");
        }

        protected override bool TUEvent()
        {
            return Generic_TU() || DispatchCurrentStateEvent("TU");
        }

        protected override bool TransitEvent()
        {
            return Generic_Transit() || DispatchCurrentStateEvent("transit");
        }

        protected override bool StateExitEvent()
        {
            return Generic_StateExit() || DispatchCurrentStateEvent("state_exit");
        }

        private bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return (Frame.D?.state ?? -1) switch
            {
                LF2States.Standing => State_Standing(eventType, eventData),
                LF2States.Walking => State_Walking(eventType, eventData),
                LF2States.Running => State_Running(eventType, eventData),
                LF2States.Attack => State_Attack(eventType, eventData),
                LF2States.Jump => State_Jump(eventType, eventData),
                LF2States.Dash => State_Dash(eventType, eventData),
                LF2States.Rowing => State_Rowing(eventType, eventData),
                LF2States.Defending => State_Defending(eventType, eventData),
                LF2States.BrokenDefend => State_BrokenDefend(eventType, eventData),
                LF2States.Catching => State_Catching(eventType, eventData),
                LF2States.BeingCaught => State_BeingCaught(eventType, eventData),
                LF2States.Injured => State_Injured(eventType, eventData),
                LF2States.Falling => State_Falling(eventType, eventData),
                LF2States.Frozen => State_Frozen(eventType, eventData),
                LF2States.Lying => State_Lying(eventType, eventData),
                LF2States.StopRunning => State_StopRunning(eventType, eventData),
                LF2States.Injured2 => State_Injured2(eventType, eventData),
                LF2States.Charging => State_Charging(eventType, eventData),
                LF2States.Burning => State_Burning(eventType, eventData),
                _ => false,
            };
        }





        // ========== Core Simulation Lifecycle ==========

        /// <summary>
        /// Release input update. InputState owns cooldowns and combo-frame detection.
        /// </summary>
        protected override void ComboUpdate()

        {
            InputState?.ApplyFrameInput(this);
            PostComboEvent();
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            var ctx = new CharacterMechanicsContext(
                PS,
                Frame.D,
                GetSpriteWidthPxForCollision(),
                _mass,
                NTSDGlobal.Gameplay.MinSpeed,
                NTSDGlobal.Gameplay.Gravity,
                _cachedIsPointWalkable
            );

            _mech.Step(ctx);
        }

        /// <summary>
        /// 武器点更新
        /// Applies current frame weapon-point behavior.
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 方向控制 ==========

        // ========== 武器持有 ==========

        /// <summary>
        /// Holds a weapon and binds the release runtime relationship.
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
        /// Drops the currently held weapon.
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

            // 反汇编 0x41391A: [esi+0ECh] AttackExempt > 0 → dec
            if (AttackExempt > 0) AttackExempt--;

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

            // 反汇编 0x413D0C: [frame+7F0h]=hit_Uj < 0 && MPEnabled → cmp PP, hit_Uj; jl→frame=hit_a; else PP+=hit_Uj
            if (fD.hit_Uj < 0 && NTSDGlobal.MPEnabled && Health != null)
            {
                if (Health.PP < fD.hit_Uj)
                    Trans.Frame(fD.hit_a, 0);
                else
                    Health.PP += fD.hit_Uj;
            }

            // C++ release Entity::cd_defend_lock: frame 110/114 sets a 3-frame guard
            // that blocks defend-related direct actions while InputState decrements it.
            int frameN = Frame.N;
            if (frameN == 110 || frameN == 114)
                InputState?.SetDefendLock(3);

            // 反汇编 0x413DEB: cmp frame, 0xCA(202) → ShakeTimer=20
            if (frameN == 202)
                ShakeTimer = 20;
        }

        public override void SimTransit(int tickIndex)
        {
            InputState?.UpdateFromBuffer(Controller?.InputBuffer, tickIndex, this);
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

            // C++ release merge/split timer (Entity::unk_338).
            if (MergeTimer > 0) MergeTimer--;
            ApplyMergeLogic();

            // N-30 死亡复活 + 输入序列触发
            ApplyDeathRespawn();
            ApplyInputSequenceRespawn();
        }

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
