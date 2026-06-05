using NTSD.Animation.LF2Tasks;
using NTSD.Game;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using System;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 角色专用战斗逻辑，基于 LF2LivingObject 分层实现。
    /// 战斗行为以 C++ release 的实体、帧和输入模型为准；
    /// Unity 专用代码只负责组件装配、对象池和数据适配。
    /// 
    /// 继承关系：LF2LivingObject -> LF2Character。
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;

        // ========== 角色专用模块 ==========

        public NTSDInputStateModule InputState { get; private set; }

        /// <summary>
        /// 处理正式流程中的 opoint 生成请求。
        /// </summary>
        public LF2ObjectPointModule ObjectPointModule { get; private set; }

        /// <summary>
        /// 处理正式流程中的 wpoint 持有、投掷和攻击请求。
        /// </summary>
        public LF2WeaponPointModule WeaponPointModule { get; private set; }

        /// <summary>
        /// 处理 fall / bdefend 等受击累计计数。
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;

        // ========== 武器持有 ==========

        /// <summary>当前持有的武器对象引用。正式持有关系字段同步到 Runtime。</summary>
        private ILF2Object _heldWeapon;

        // ========== Unity 组件引用 ==========
        public Transform EntityTransform { get; private set; }
        // ========== 物理计算 ==========

        private CharacterMechanics _mech;
        private float _mass = NTSDGlobal.Default.Machanics.Mass;
        private Func<Vector2, bool> _cachedIsPointWalkable;

        // ========== 抓取系统字段 ==========

        // 抓取持续计数。C++ release 抓取成功时写抓取者 caught_duration=300，
        // 后续由抓取者当前帧 cpoint.decrease 驱动递减或逃脱。
        protected int CaughtDuration { get => Runtime.CaughtDuration; set => Runtime.CaughtDuration = value; }
        // 被抓方向：true=正面，false=背面。
        protected bool CaughtFront { get => Runtime.CaughtFrontFlag != 0; set => Runtime.CaughtFrontFlag = value ? 1 : 0; }
        private bool CatchingStateTU { get => Runtime.CatchingStateTU != 0; set => Runtime.CatchingStateTU = value ? 1 : 0; }
        private int JumpAttackLock { get => Runtime.JumpAttackLock; set => Runtime.JumpAttackLock = value; }

        // ========== 死亡闪烁计数 ==========
        // -1 = 不执行；0 = 开始；1~29 = 持续；>=30 = 结束销毁
        private int _deadBlinkCount = -1;

        // ========== C++ release 角色运行时字段别名 ==========
        private int MergeFlag { get => Runtime.MergeFlag; set => Runtime.MergeFlag = value; }
        private int MergePartnerSlotIndex { get => Runtime.MergePartnerSlotIndex; set => Runtime.MergePartnerSlotIndex = value; }
        private int MergeSelfObjectId { get => Runtime.MergeSelfObjectId; set => Runtime.MergeSelfObjectId = value; }
        private int MergePartnerObjectId { get => Runtime.MergePartnerObjectId; set => Runtime.MergePartnerObjectId = value; }
        private int MergeTimer { get => Runtime.MergeTimer; set => Runtime.MergeTimer = value; }
        private int RespawnCount { get => Runtime.RespawnCount; set => Runtime.RespawnCount = value; }
        private int RespawnCountdown { get => Runtime.RespawnCountdown; set => Runtime.RespawnCountdown = value; }

        private bool _initializedFromOpoint;

        // ========== 构造函数 ==========

        public LF2Character() : base()
        {
            AllocateStableId();

            // 创建角色专用模块
            InputState = new NTSDInputStateModule();
            ObjectPointModule = new LF2ObjectPointModule();
            WeaponPointModule = new LF2WeaponPointModule();
            _hitCounters = new LF2HitCountersModule();

            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            Health.BindRuntime(Runtime);
            _hitCounters.BindRuntime(Runtime);
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);
            Controller = new CharacterInputModule();

            // 角色状态分发固定写在 switch 中，不再保留运行时 handler 表。
        }

        public void InjectDependencies(
            Transform entityTransform,
            Transform visualTransform,
            string name)
        {
            EntityTransform = entityTransform;
            Name = name;
        }

        /// <summary>
        /// 池化路径专用初始化（无参版本）。
        /// InjectDependencies 之后、ModuleBind 之前调用。
        /// 初始化物理和状态机运行所需的基础字段。
        /// </summary>
        public void ModuleInitialize()
        {
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = BoundaryWallManager.Instance != null
                ? BoundaryWallManager.Instance.IsPointWalkable: null;

            PS.x = 0; PS.y = 0; PS.z = 0;
            PS.vx = 0; PS.vy = 0; PS.vz = 0;

        }

        // ========== ILF2Object 抽象方法实现 ==========

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            Renderer = renderer;

            if (task is not OPointCreateTask opTask)
                return;

            InitializeFromOpoint(opTask);
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
            // C++ release 对齐 0x00421185/0x00421191：spawn/reset 时清 Entity::attacking。
            AttackingCounter = 0;
            FrameDelay = 10;
            ShotCount = 0;
            ResetSpark();
            MergeFlag = -1; MergePartnerSlotIndex = -1; MergeSelfObjectId = 0; MergePartnerObjectId = 0; MergeTimer = 0;
            RespawnCount = 0; RespawnCountdown = 0;
            _initializedFromOpoint = false;
            AiControlled = false;
            if (Controller is NullLF2Controller || Controller is OpointCloneController)
                Controller = new CharacterInputModule();
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

        // ========== 初始化（由角色装配流程调用）==========

        /// <summary>
        /// 模块绑定（对应 LF2CharacterAnimator.ModuleBind）
        /// </summary>
        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            // 加载帧数据
            FrameCache.Load(frameDataWrapper);

            if (!_initializedFromOpoint)
            {
                // 初始化帧信息
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
            }
            else
            {
                Frame.D = FrameCache.GetFrameDataById(Frame.N);
                if (Frame.D == null)
                {
                    Frame.N = 0;
                    Frame.PN = 0;
                    Frame.D = FrameCache.GetFrameDataById(0);
                }
            }

            if (Frame.D != null)
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);

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
            Health.HP = maxHp;
            Health.HPBound = maxHp;
            Health.MP = maxMp;
            Health.PP = maxMp;
            Health.MaxPP = maxMp;
            Health.PPBound = maxMp;
            // C++ release entity+340h = MaxMP，用于 kind=0/16 伤害 MP% 缩放
            Health.MaxMP = maxMp;
            InputState.Reset();
            HitCounters.Reset();
            ItrRest.Reset();
        }

        private void InitializeFromOpoint(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            Team = task.team;
            OwnerId = task.releaseOpointSpawn ? -1 : task.parent?.StableId ?? -1;
            KillCount = task.parent != null
                ? (task.parent.KillCount > -1 ? task.parent.KillCount : GetRuntimeSlotOrStableId(task.parent))
                : -1;
            HitStun = task.parent?.HitStun ?? 0;

            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(string.IsNullOrEmpty(dir) ? "right" : dir);

            int action = task.opoint.action;
            if (action == 0 && !task.preserveActionZero)
                action = 0;

            Frame.PN = action;
            Frame.N = action;
            Frame.D = null;

            SetOpointPosition(task);
            SetOpointVelocity(task);

            FrameDelay = task.frameDelay;
            AttackExempt = task.attackExempt;
            OwnerEntityIndex = task.ownerEntityIndex;

            AiControlled = task.releaseOpointSpawn && task.parent != null;
            Controller = AiControlled
                ? new OpointCloneController()
                : NullLF2Controller.Instance;
            _initializedFromOpoint = true;
        }

        private static int GetRuntimeSlotOrStableId(LF2Entity entity)
        {
            if (entity == null) return -1;
            return entity.Runtime.SlotIndex >= 0 ? entity.Runtime.SlotIndex : entity.StableId;
        }

        private void SetOpointPosition(OPointCreateTask task)
        {
            if (PS == null) return;

            PS.x = task.pos.x;
            PS.y = task.parent != null ? task.pos.y - task.z : task.pos.y;
            PS.z = task.z;
        }

        private void SetOpointVelocity(OPointCreateTask task)
        {
            if (PS == null) return;

            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;
            PS.vz = 0f;
        }

        // ========== 状态机初始化 ==========



        protected override void ResetStateRuntime()
        {
            CatchingStateTU = false;
            CaughtDuration = 0;
            CaughtFront = true;
            JumpAttackLock = 0;
            WeaponCount = 0;
            FallDamageDiv = 0;
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

        protected override bool StateEntryEvent()
        {
            return DispatchCurrentStateEvent("state_entry");
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
                LF2States.Catching => State_Catching(eventType, eventData),
                LF2States.BeingCaught => State_BeingCaught(eventType, eventData),
                LF2States.Injured => State_Injured(eventType, eventData),
                LF2States.Falling => State_Falling(eventType, eventData),
                LF2States.Frozen => State_Frozen(eventType, eventData),
                LF2States.Lying => State_Lying(eventType, eventData),
                LF2States.StopRunning => State_StopRunning(eventType, eventData),
                LF2States.Burning => State_Burning(eventType, eventData),
                _ => false,
            };
        }





        // ========== 核心模拟生命周期 ==========

        /// <summary>
        /// 正式输入更新。InputState 负责按键冷却和组合帧检测。
        /// </summary>
        protected override void ComboUpdate()

        {
            InputState?.ApplyFrameInput(this);
        }

        /// <summary>
        /// 角色输入会直接改写 walking/running 帧；这些帧不能在同一 tick 再按 DAT next 被推进回 idle。
        /// </summary>
        public override void Transit()
        {
            ComboUpdate();

            int prevDelay = FrameDelay;
            if (FrameDelay > 0) FrameDelay--;
            else if (FrameDelay < 0) FrameDelay++;

            if (prevDelay != 0)
                return;

            bool stuck = Effect.TimeIn < 0 && Effect.Stuck;
            if (!stuck)
            {
                Trans.Trans();
            }

            Effect.TimeIn--;

            stuck = Effect.TimeIn < 0 && Effect.Stuck;
            if (!stuck)
            {
                TransitEvent();
            }
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

            var stepResult = _mech.Step(ctx);
            if (stepResult.landed)
            {
                HandleLandingEvent(stepResult.verticalVelocityBeforeLanding);

                float spriteWidthPx = GetSpriteWidthPxForCollision();
                if (Frame?.D != null && spriteWidthPx > 0f)
                    PS.UpdateSpriteOrigin(Frame.D.centerx, Frame.D.centery, spriteWidthPx);
            }
        }

        /// <summary>
        /// 武器点更新，执行当前帧 wpoint 行为。
        /// </summary>
        public void WPointUpdate()
        {
            WeaponPointModule?.ProcessTransit(this);
        }

        // ========== 方向控制 ==========

        // ========== 武器持有 ==========

        /// <summary>
        /// 持有武器，并同步正式运行时的持有关系字段。
        /// </summary>
        public void HoldWeapon(ILF2Object weapon)
        {
            _heldWeapon = weapon;
            LF2Entity held = weapon as LF2Entity;
            Runtime.HeldWeaponStableId = held?.StableId ?? -1;
            Runtime.TargetSlotIndex = Runtime.HeldWeaponStableId;
            Runtime.LinkState = ResolveHeldWeaponLinkState(weapon);

            if (held != null)
            {
                held.Runtime.HolderStableId = StableId;
                held.Runtime.LinkState = -1;
                if (held.GrabbedBy == 0)
                    held.GrabbedBy = -1;
                held.Team = Team;
            }
        }

        /// <summary>
        /// opoint kind=2 生成后的持有绑定，对齐 C++ release spawn_from_opoint。
        /// </summary>
        public void AttachOpointHeldObject(LF2Entity held)
        {
            _heldWeapon = held;
            Runtime.LinkState = 1;
            Runtime.TargetSlotIndex = held?.StableId ?? -1;
            Runtime.HeldWeaponStableId = held?.StableId ?? -1;

            if (held == null)
                return;

            held.GrabbedBy = -1;
            held.Runtime.LinkState = -1;
            held.Runtime.HolderStableId = StableId;
            held.Runtime.TargetSlotIndex = -1;
            held.Team = Team;
            held.TrackerFlag = -1;
            held.TrackerParent = this;
            TrackerFlag = 1;
        }

        /// <summary>
        /// 获取当前持有的武器
        /// </summary>
        public ILF2Object GetHeldWeapon()
        {
            return _heldWeapon;
        }

        /// <summary>
        /// 按 C++ release AI_Process2 的 held-object pass 同步/释放当前持有对象。
        /// 武器仍复用 LF2WeaponBase.Act 的攻击细节；非武器 opoint kind=2 也会走同一释放规则。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(WeaponPoint holderWPoint, out WeaponActResult result)
        {
            return ReleaseHeldObjectByWPoint(_heldWeapon as LF2Entity, holderWPoint, out result);
        }

        /// <summary>
        /// C++ release AI_Process2 遍历 link_state&lt;0 对象后，按 holder 当前 wpoint 同步/释放。
        /// </summary>
        public bool ReleaseHeldObjectByWPoint(LF2Entity held, WeaponPoint holderWPoint, out WeaponActResult result)
        {
            result = new WeaponActResult();
            if (holderWPoint == null || held == null || held.PS == null)
                return false;

            if (!ReferenceEquals(_heldWeapon, held))
                _heldWeapon = held;

            Vector3 holdpoint = CalcHeldObjectPoint(holderWPoint);

            if (holderWPoint.kind == 3)
            {
                SyncHeldObjectFrameAndPosition(held, holderWPoint, holdpoint);
                DropHeldObjectRandomly(held);
                return true;
            }

            if (held is LF2WeaponBase weapon)
            {
                result = weapon.Act(this, holderWPoint, holdpoint);
                return true;
            }

            SyncHeldObjectFrameAndPosition(held, holderWPoint, holdpoint);

            LF2FrameData heldFrame = held.Frame?.D;
            if (heldFrame != null && (heldFrame.state == LF2States.Falling || heldFrame.state == LF2States.BeingCaught))
            {
                DropHeldObjectFromDamagedHolder(held);
                return true;
            }

            if (holderWPoint.dvx != 0)
            {
                int objType = held.ObjectType;
                if (objType == 1 || objType == 4 || objType == 6)
                {
                    held.ImmediateFrame(40);
                    ApplyHeldObjectThrowVelocity(held, holderWPoint);
                    ClearReleasedHeldObject(held, clearTeam: false);
                    result.Thrown = true;
                    return true;
                }

                if (objType == 2)
                {
                    held.ImmediateFrame(RandInt(0, 6));
                    ApplyHeldObjectThrowVelocity(held, holderWPoint);
                    ClearReleasedHeldObject(held, clearTeam: false);
                    result.Thrown = true;
                    return true;
                }
            }

            return true;
        }

        private Vector3 CalcHeldObjectPoint(WeaponPoint wpoint)
        {
            var frame = Frame?.D;
            if (PS == null || frame == null)
                return Vector3.zero;

            float x = PS.dir == "right"
                ? PS.x - frame.centerx + wpoint.x
                : PS.x + frame.centerx - wpoint.x;
            float y = PS.y - frame.centery + wpoint.y;
            return new Vector3(x, y, PS.z);
        }

        private void SyncHeldObjectFrameAndPosition(LF2Entity held, WeaponPoint holderWPoint, Vector3 holdpoint)
        {
            if (held == null || held.PS == null)
                return;

            held.ImmediateFrame(holderWPoint.weaponact);

            held.FrameDelay = FrameDelay;
            held.SwitchDir(PS?.dir ?? held.PS.dir);

            LF2FrameData heldFrame = held.Frame?.D;
            int heldCx = heldFrame?.centerx ?? 0;
            int heldCy = heldFrame?.centery ?? 0;
            WeaponPoint heldWPoint = heldFrame?.wpoints != null && heldFrame.wpoints.Count > 0
                ? heldFrame.wpoints[0]
                : null;
            int heldWpx = heldWPoint?.x ?? 0;
            int heldWpy = heldWPoint?.y ?? 0;

            held.PS.x = held.PS.dir == "right"
                ? holdpoint.x + heldCx - heldWpx
                : holdpoint.x + heldWpx - heldCx;
            held.PS.y = holdpoint.y + heldCy - heldWpy;
            held.PS.z = PS?.z ?? held.PS.z;

            if (holderWPoint.cover == 0)
            {
                held.PS.z += 1f;
                held.PS.y -= 1f;
            }
            else
            {
                held.PS.z -= 1f;
                held.PS.y += 1f;
            }
        }

        private void ApplyHeldObjectThrowVelocity(LF2Entity held, WeaponPoint holderWPoint)
        {
            held.PS.vx = PS?.dir == "left" ? -holderWPoint.dvx : holderWPoint.dvx;
            held.PS.vy = holderWPoint.dvy;

            bool up = InputState?.Up == true || Controller?.IsUp == true;
            bool down = InputState?.Down == true || Controller?.IsDown == true;
            if (up && !down)
                held.PS.vz = -holderWPoint.dvz;
            else if (!up && down)
                held.PS.vz = holderWPoint.dvz;

            held.PS.zz = 1f;
        }

        private void DropHeldObjectFromDamagedHolder(LF2Entity held)
        {
            held.ImmediateFrame(RandInt(0, 16));
            if (HitCount == 1)
            {
                held.PS.vx = KnockbackVx * (1f / 3f);
                held.PS.vy = KnockbackVy;
                held.PS.vz = KnockbackVz;
            }
            else
            {
                held.PS.vx = PS.vx * (1f / 3f);
                held.PS.vy = PS.vy;
                held.PS.vz = PS.vz;
            }

            if (held.PS.y > -2f)
                held.PS.y = -2f;

            ClearReleasedHeldObject(held, clearTeam: false);
        }

        private void DropHeldObjectRandomly(LF2Entity held)
        {
            held.ImmediateFrame(RandInt(0, 6));
            held.PS.vx = RandInt(0, 7) - 3f;
            held.PS.vy = -RandInt(0, 4);
            held.PS.vz = (RandInt(0, 5) - 2) * 0.2f;
            held.PS.zz = 0f;
            ClearReleasedHeldObject(held, clearTeam: true);
        }

        private void ClearReleasedHeldObject(LF2Entity held, bool clearTeam)
        {
            if (clearTeam)
                held.Team = 0;

            held.GrabbedBy = 0;
            held.Runtime.LinkState = 0;
            held.Runtime.HolderStableId = -1;
            if (held is LF2WeaponBase weapon)
                weapon.ForceClearHolder();

            _heldWeapon = null;
            GrabbedBy = 0;
            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
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
        /// 丢弃当前持有的武器。
        /// </summary>
        public void DropWeapon(float dvx = 0, float dvy = 0)
        {
            (_heldWeapon as LF2WeaponBase)?.Drop(dvx, dvy);

            _heldWeapon = null;
            Runtime.HeldWeaponStableId = -1;
            Runtime.TargetSlotIndex = -1;
            Runtime.LinkState = 0;
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
        /// C++ release sub_414C30：输入/连招命中字段触发的直接跳帧。
        /// 该路径使用目标帧的 mp 字段检查并扣除 PP/HP，成功后直接进入目标帧。
        /// </summary>
        internal bool TryInputFrameJump(int frameId)
        {
            bool flipFacing = false;
            if (frameId < 0)
            {
                frameId = -frameId;
                flipFacing = true;
            }

            if (frameId == 999)
                frameId = 0;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null || Health == null)
                return false;

            if (NTSDGlobal.MPEnabled)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;

                // C++ release 的负帧翻面只在 PP/HP 检查成功路径上执行。
                if (flipFacing)
                    SwitchDir(PS.dir == "right" ? "left" : "right");
            }

            OnFrameTransit(frameId, false, 0);
            return true;
        }

        /// <summary>
        /// C++ release 普通攻击类动作的 PP 消耗门控。
        /// 跑攻/冲刺攻要求 PP 足够；站立拳/空中拳按 C++ release 扣到 0 后仍允许进帧。
        /// </summary>
        internal bool TrySpendFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!NTSDGlobal.MPEnabled || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP < 0)
                Health.PP = 0;
            return true;
        }

        /// <summary>
        /// 设置等待时间
        /// </summary>
        public void SetWait(int value, int authority = 99)
        {
            Trans.SetWait(value, authority);
        }

        // ========== 当前帧信息 ==========

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;
        public int CurrentState => Frame.D?.state ?? 0;

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();

            Runtime.HeldWeaponStableId = (_heldWeapon as LF2Entity)?.StableId ?? -1;
            Runtime.TargetSlotIndex = Runtime.HeldWeaponStableId;
            if (_heldWeapon != null)
                Runtime.LinkState = ResolveHeldWeaponLinkState(_heldWeapon);
            Runtime.Blink = _deadBlinkCount;
        }

        private int ResolveHeldWeaponLinkState(ILF2Object weapon)
        {
            if (weapon is LF2Entity && weapon is not LF2WeaponBase)
                return 1;

            if (weapon is not LF2WeaponBase weaponBase)
                return 0;

            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(weaponBase.ObjectId);
            int typeSub = charData?.type_sub ?? 0;

            // C++ release 拾取路径：特殊 type_sub 优先，其次按武器 entity_type 写角色 link_state。
            if (typeSub == 0x78 || typeSub == 0x7C)
                return 101;
            if (weaponBase.IsHeavy)
                return 2;
            if (weaponBase.WeaponType == 4)
                return 4;
            if (weaponBase.WeaponType == 6)
                return weaponBase.Health?.HP > 0 ? 6 : 4;

            return 1;
        }

        // ========== 额外方法 ==========

        /// <summary>
        /// PreInteraction 全局 pass，对齐 C++ release 的早期抓取/拾取检测阶段。
        /// 由 SimulationWorld.PreInteractionTickAll 在所有对象 SerialTickAll 完成后统一调用。
        /// </summary>
        public override void SimPreInteraction(int tickIndex)
        {

            Generic_PreInteraction();
        }

        /// <summary>
        /// EntityCollision 阶段，对齐 C++ release 的角色专属碰撞后处理分支。
        /// C++ release frame_tick：
        /// cpoint.kind==2 直接返回；前一帧 state==14（被抓取）且当前帧 state!=13（非冰冻）时，
        /// 在游戏模式 1/4 下对非豁免实体设 ShakeTimer=15（[esi+8h]=15）
        /// 豁免条件：oid ∈ [30,39] 且 oid != 38
        /// 注：ShakeTimer 双向趋零已在 TUUpdate() 中处理
        /// </summary>
        public override void SimEntityCollision(int tickIndex)
        {
            var fD = Frame?.D;
            if (fD == null) return;

            // C++ release 0x41391A: [esi+0ECh] AttackExempt > 0 → dec
            if (AttackExempt > 0) AttackExempt--;

            // C++ release 0x413937: cpoint.kind == 2 → return
            if (fD.cpoint != null && fD.cpoint.kind == 2) return;

            // C++ release 0x413BC0-0x413C49：prev_frame.state==14 → ShakeTimer=15
            // 条件：prev_frame.state==14 && frame.state!=13 && oid 豁免
            var prevFD = FrameCache.GetFrameDataById(Frame.PN);
            if (prevFD != null && prevFD.state == LF2States.BeingCaught && fD.state != LF2States.Frozen)
            {
                int oid = FrameCache?.Wrapper?.characterId ?? -1;
                if (!(oid / 10 == 3 && oid != 38))
                    ShakeTimer = 15;
            }

            // C++ release 0x413D0C: [frame+7F0h]=hit_Uj < 0 && MPEnabled → cmp PP, hit_Uj; jl→frame=hit_a; else PP+=hit_Uj
            if (fD.hit_Uj < 0 && NTSDGlobal.MPEnabled && Health != null)
            {
                if (Health.PP < fD.hit_Uj)
                    ImmediateFrame(fD.hit_a);
                else
                    Health.PP += fD.hit_Uj;
            }

            // C++ release Entity::cd_defend_lock：frame 110/114 设置 3 帧防御锁，
            // InputState 递减期间会阻止防御相关的直接动作。
            int frameN = Frame.N;
            if (frameN == 110 || frameN == 114)
                InputState?.SetDefendLock(3);

            // C++ release 0x413DEB: cmp frame, 0xCA(202) → ShakeTimer=20
            if (frameN == 202)
                ShakeTimer = 20;
        }

        public override void SimTransit(int tickIndex)
        {
            // C++ release frame_advance：link_state < 0 的被持有对象不自行推进帧和物理。
            if (Runtime.LinkState < 0)
                return;

            if (AiControlled && Controller is OpointCloneController aiController)
                aiController.PrepareInput(this, tickIndex, Match);

            InputState?.UpdateFromBuffer(Controller?.InputBuffer, tickIndex, this);
            Transit();
        }

        public override void SimLateTick(int tickIndex)
        {
            base.SimLateTick(tickIndex);
            RunLateCharacterCleanup();
        }

        /// <summary>
        /// C++ release run_late_entity_update 开头的角色特殊状态处理。
        /// Unity 当前 frame_tick 仍在 SimTransit 中推进，本函数只承接已还原的特殊分支。
        /// </summary>
        internal void RunLateSpecialPreCollision()
        {
            // N-26 C++ release 0x004219F1 test edx,edx + jnz：仅 entity_type==0（角色）的 state==9995
            // data 替换为 oid=50, frame=0
            var fD = Frame?.D;
            if (fD != null && fD.state == 9995)
            {
                var wrapper50 = CharacterAnimtorManager.Instance?.GetCharacterConfig(50);
                if (wrapper50 != null)
                {
                    FrameCache.Load(wrapper50);
                    ImmediateFrame(0);
                }
            }

            // N-27：仅角色 state==9996 且 Entity::attacking == 1 时生成碎片。
            if (fD != null && fD.state == 9996 && AttackingCounter == 1)
                SpawnFragments9996Character();
        }

        /// <summary>
        /// C++ release run_late_entity_update 尾段的角色清理和状态转场副作用。
        /// </summary>
        internal void RunLateCharacterCleanup()
        {
            // C++ release merge/split timer (Entity::unk_338).
            if (MergeTimer > 0) MergeTimer--;
            ApplyMergeLogic();

            // N-30 死亡复活 + 输入序列触发
            ApplyDeathRespawn();
            ApplyInputSequenceRespawn();

            // N-31 C++ release spawn_state_transition_effects：
            // 进入 state=13(Frozen)/frame=200 时（上一帧不是该状态）播放 sound 15 + 生成 15 个 oid=999（frame 120/125/130/135）
            // 进入/持续 state=18(Burning)/19(FirenSpecific) 时：进入时 7 个，持续时 1/4 概率 1 个；frame=140
            ApplyFrozenBurningParticles();
        }

    }
}
