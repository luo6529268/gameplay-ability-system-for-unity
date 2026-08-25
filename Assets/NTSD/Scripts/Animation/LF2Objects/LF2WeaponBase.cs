using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Tools;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 对齐 C++ release 的武器基类。
    /// </summary>
    /// <summary>
    /// 武器对象公共基类。
    /// 
    /// 这个类负责武器在三种典型场景下的公共行为：
    /// 1. 被角色拿在手里时怎么同步。
    /// 2. 被扔出去后怎么飞行、落地。
    /// 3. 落在地上后怎么等待再次拾取或被破坏。
    /// </summary>
    public abstract class LF2WeaponBase : LF2Entity
    {
        // ========== 武器专属字段（不在 LF2Entity 的） ==========

        /// <summary>交互冷却（武器也有 itr 碰撞冷却）</summary>
        public override LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>生命值（武器耐久度等）</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();

        /// <summary>控制器（武器由持有者间接控制）</summary>
        public ILF2Controller Controller { get; set; }
        private readonly LF2WeaponInteractionResolver _interactionResolver;
        private readonly LF2WeaponHeldStateResolver _heldStateResolver;
        private readonly LF2WeaponReleaseFlowResolver _releaseFlowResolver;
        private readonly LF2WeaponFrameLogicResolver _frameLogicResolver;

        // ========== 配置字段 ==========

        // ========== 持有者信息 ==========

        // C++ release [entity+3F8h]：picker_idx，保存的是运行时槽位。
        public int PickerStableId
        {
            get => Runtime.PickerStableId;
            set => Runtime.PickerStableId = value;
        }

        // 持有诊断：只在每次新持有时打印一次赋值前后快照，之后不再重复。
        private bool _heldDiagPrinted;
        private bool _lateBreakEffectsHandled;
        public long InvalidInitTaskTypeCountForDiagnostics { get; private set; }

        // 本帧重力累加量，由 WeaponFlightPhysics 计算，WeaponDynamics 在 y+=vy 后使用
        // 对齐 C++ release 0x4164BD：gravity 在 y 更新后、新 y<0 时才加入 vy
        protected double _gravityToAdd; // P0-f-2a: double sim gravity accumulator
        protected double _lastLandingVyBeforeClamp; // P0-f-2b B1: float→double (landing Vy snapshot, no truncation)

        internal bool LateBreakEffectsHandledForSnapshot =>
            _lateBreakEffectsHandled;
        internal double GravityToAddForSnapshot => _gravityToAdd;
        internal double LastLandingVyBeforeClampForSnapshot =>
            _lastLandingVyBeforeClamp;

        internal bool TryRestoreWeaponShellForSnapshot(
            in BattleWeaponShellSnapshot state)
        {
            if (state.HasPoolWeaponType != (this is LF2Weapon))
                return false;

            _lateBreakEffectsHandled = state.LateBreakEffectsHandled;
            InvalidInitTaskTypeCountForDiagnostics =
                state.InvalidInitTaskTypeCount;
            _gravityToAdd = state.GravityToAdd;
            _lastLandingVyBeforeClamp = state.LastLandingVyBeforeClamp;
            if (this is LF2Weapon concrete)
                concrete.RestorePoolWeaponTypeForSnapshot(state.PoolWeaponType);
            return true;
        }

        // ========== 武器数据 ==========
        public int WeaponDropHurt
        {
            get => Runtime.WeaponDropHurt > 0 ? Runtime.WeaponDropHurt : 10;
            set => Runtime.WeaponDropHurt = value;
        }

        // weapon_strength_list（由 CharacterAnimtorManager 在加载时注入）
        protected List<WeaponStrengthEntry> _weaponStrengthList;
        public string WeaponDropSound { get; set; } = "";
        public string WeaponBrokenSound { get; set; } = "";
        public string WeaponHitSound { get; set; } = "";

        // ========== 公开属性 ==========
        public LF2LivingObject HoldObj => GetRuntimeHolder();
        public override float GetSpriteWidthPxForCollision() => ResolveCurrentSpriteFileWidthPx();
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.LightWeapon;

        public abstract bool IsLight { get; }
        public abstract bool IsHeavy { get; }
        // C++ release [weapon+368h+6F8h]：0=普通轻武器, 1=重武器, 2=轻特殊, 4=特殊重武器, 6=饮料类
        public abstract int WeaponType { get; }
        public override int ReleaseEntityType => WeaponType;
        internal override bool UsesDynamicRuntimeSlot() => true;
        // C++ release weapon_count：笛子命中累积器，子类实现存储。
        public virtual int FluteWeight { get => 0; set { } }

        /// <summary>C++ release 0x004228A0: type=1/2/4/6 才检查 flightCounter</summary>
        protected virtual bool IsWeaponDestroyable() => false;

        /// <summary>供基类 SimTU 读取 WeaponFlightCounter。</summary>
        protected virtual int GetFlightCounter() => 0;

        protected LF2WeaponBase()
        {
            _interactionResolver = new LF2WeaponInteractionResolver(this);
            _heldStateResolver = new LF2WeaponHeldStateResolver(this);
            _releaseFlowResolver = new LF2WeaponReleaseFlowResolver(this);
            _frameLogicResolver = new LF2WeaponFrameLogicResolver(this);
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
        }

        protected LF2Entity GetRuntimeHolderEntity()
        {
            if (Runtime?.PendingFlushDestroy == true)
                return null;

            if ((Runtime?.LinkState ?? 0) >= 0)
                return null;

            int runtimeSlot = Runtime?.ResolveActiveHolderSlotIndex() ?? -1;
            if (runtimeSlot < 0)
                return null;

            return Match?.FindEntityByRuntimeSlotForQuery(runtimeSlot);
        }

        protected LF2LivingObject GetRuntimeHolder()
        {
            return GetRuntimeHolderEntity() as LF2LivingObject;
        }

        internal LF2Entity ResolveRuntimeHolderEntityForOwnedModule()
        {
            return GetRuntimeHolderEntity();
        }

        public virtual void Drop(double dvx, double dvy)
        {
            _heldStateResolver.Drop(dvx, dvy);
        }

        protected virtual void OnHealthInitialized(LF2CharacterData charData) { }

        protected virtual void OnInFlightFrameUpdate() { }

        /// <summary>
        /// 飞行武器落地后的弹射与停止处理
        /// C++ release 对齐 Entity_FrameAdvance 0x4164A9-0x416577（y>=0 路径）
        /// 子类按 WeaponType 重写以实现差异化落地行为
        /// </summary>
        // 武器落地后的分流点。不同武器类型会在这里决定落地后的状态。
        protected virtual void OnLanded()
        {
            // 基类不做任何清零——所有 type 分支由 LF2Weapon.OnLanded() 完整覆盖并 return。
        }

        /// <summary>
        /// 飞行武器每帧的特化物理（在 Dynamics 之前执行）
        /// C++ release 对齐 Entity_FrameAdvance 0x416240-0x416577（在空中时的 type 分流）
        /// 子类按 WeaponType 重写
        /// </summary>
        // 武器飞行中的特殊物理规则扩展点。
        protected virtual void WeaponFlightPhysics() { }

        /// <summary>
        /// 投掷成功后的初始化回调（子类用于初始化 WeaponFlightCounter 等）。
        /// </summary>
        protected virtual void OnThrown() { }

        protected virtual bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return GetState() switch
            {
                LF2States.WeaponJustOnGround => State_WeaponJustOnGround(eventType, eventData),
                LF2States.WeaponOnGround => State_WeaponOnGround(eventType, eventData),
                _ => false,
            };
        }

        protected virtual bool State_WeaponJustOnGround(string eventType, object eventData)
        {
            return false;
        }

        protected virtual bool State_WeaponOnGround(string eventType, object eventData)
        {
            return false;
        }

        protected int ResolveRuntimeWeaponState()
        {
            int runtimeState = Runtime?.WeaponState ?? 0;
            return runtimeState != 0 ? runtimeState : GetState();
        }

        internal int GetRuntimeWeaponState()
        {
            return ResolveRuntimeWeaponState();
        }

        protected int CurrentFrameState()
        {
            return Frame?.D?.state ?? GetState();
        }

        internal int GetResolvedWeaponStateForExternalUse()
        {
            return CurrentFrameState();
        }

        internal bool HeldDiagPrinted
        {
            get => _heldDiagPrinted;
            set => _heldDiagPrinted = value;
        }

        internal void ApplyHeldWPointSync(LF2Entity holder, WeaponPoint holderWPoint, Vector3 holdpoint, WeaponPoint heldWPoint)
        {
            _heldStateResolver.ApplyHeldWPointSync(holder, holderWPoint, holdpoint, heldWPoint);
        }

        internal void ReleaseHeldWeaponRuntimeInternal(LF2Entity holder, bool stampReleaseTick = false)
        {
            _releaseFlowResolver.ReleaseHeldWeaponRuntime(holder, stampReleaseTick);
        }

        internal void ReleaseHeldWeaponForConsumeInternal(LF2Entity holder)
        {
            _releaseFlowResolver.ReleaseHeldWeaponForConsume(holder);
        }

        internal WeaponAttackResult ProcessAttackInternal(LF2Entity holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            return ProcessAttack(holder, wpoint, frame);
        }

        internal bool TryApplyKind6HitConfirmInternal(InteractionArea itr, LF2Entity target)
        {
            return TryApplyKind6HitConfirm(itr, target);
        }

        internal bool CanInteractTargetInternal(InteractionArea itr, LF2Entity target)
        {
            return CanInteractTarget(itr, target);
        }

        // 暴露给武器交互解析器使用的内部转发入口。
        internal void ApplyReleaseSceneQueryConsumeEffectsInternal(SceneQueryHit hitInfo)
        {
            ApplyReleaseSceneQueryConsumeEffects(hitInfo);
        }

        internal void OnDrinkConsumedInternal()
        {
            OnDrinkConsumed();
        }

        internal void OnThrownInternal()
        {
            OnThrown();
        }

        /// <summary>
        /// C++ release 语义下的武器受击入口。
        /// </summary>
        public abstract bool Hit(InteractionArea itr, LF2Entity attacker);

        // 武器候选在真正消费前的通用目标过滤。
        protected virtual bool CanInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null) return false;
            if (target == this) return false;
            if (target.Frame?.D == null) return false;
            if (target.Health != null && target.Health.HP <= 0) return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr)) return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(this, target)) return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(this, itr, target)) return false;
            int selfSlot = Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true)) return false;

            return true;
        }

        /// <summary>
        /// C++ release 语义下的武器拾取入口。
        /// </summary>
        // 武器被角色拿起时的入口。
        public virtual bool Pick(LF2LivingObject holder)
        {
            if (GetRuntimeHolderEntity() != null) return false;

            var holderEntity = holder as LF2Entity;
            Runtime.HolderStableId = holderEntity?.Runtime?.SlotIndex ?? -1;
            HolderCopySlot = holderEntity?.Runtime?.SlotIndex ?? -1;
            Team = holder.Team;
            RelationTeam = holderEntity?.RelationTeam ?? holder.Team;
            GrabbedBy = 0;

            return true;
        }

        /// <summary>
        /// 饮料消耗完毕后的子类钩子，用于重置 WeaponFlightCounter 等字段。
        /// C++ release 0x41AD73: weapon.[+31Ch] = 0
        /// </summary>
        protected virtual void OnDrinkConsumed() { }

        protected virtual WeaponAttackResult ProcessAttack(LF2Entity holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            return default;
        }

        // 武器处于持有状态时，每帧同步和动作分发的主入口。
        public virtual WeaponActResult Act(LF2Entity holder, WeaponPoint wpoint, Vector3 holdpoint)
        {
            return _heldStateResolver.Act(holder, wpoint, holdpoint);
        }

        public void WhirlwindForce(InteractionArea itr, LF2Entity attacker)
        {
            if (attacker == null)
                return;

            int state = CurrentFrameState();
            bool lightLike = WeaponType == 1 || WeaponType == 4 || WeaponType == 6;
            bool heavyLike = WeaponType == 2;

            if (lightLike)
            {
                if (ObjectId == 201 || ObjectId == 202)
                    return;
                if (state != LF2States.WeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 3.0);
            }
            else if (heavyLike)
            {
                if (state != LF2States.HeavyWeaponInSky)
                    SetFrameDirect(0);
                ApplyWhirlwindVelocity(attacker, 2.3);
            }
        }

        public override void FluteForce()
        {
        }

        protected void CoincideXYWithWPoint(Vector3 holdpoint, WeaponPoint heldFrameWpoint)
        {
            var weaponFrame = Frame?.D;
            int wcx = weaponFrame?.centerx ?? 0;
            int wcy = weaponFrame?.centery ?? 0;
            int wpx = heldFrameWpoint?.x ?? 0;
            int wpy = heldFrameWpoint?.y ?? 0;

            if (Runtime.Dir == "right")
                Runtime.X = holdpoint.x + wcx - wpx;
            else
                Runtime.X = holdpoint.x + wpx - wcx;

            Runtime.Y = holdpoint.y + wcy - wpy;
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId))
                return;

            QueueBattleSound(soundId);
        }

        public void CreateBrokenEffect()
        {
            SpawnBrokenWeaponFragments(ObjectId);
        }

        protected Vector3 MakePointCenter(LF2FrameData frame)
        {
            float x = (float)Runtime.X;
            float y = (float)Runtime.Y;
            float z = GetDisplayZ();
            return new Vector3(x, y, z);
        }

        protected void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            Runtime.X += vx;
            Runtime.Z += vz;
        }

        public virtual void Interaction()
        {
            _interactionResolver.RunInteraction();
        }

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);

            if (taskBase is not OPointCreateTask task)
            {
                InvalidInitTaskTypeCountForDiagnostics++;
                if (Match?.RuntimeCapacity.IsSealed != true)
                    Log.Error($"[{GetType().Name}] Invalid task type");
                return;
            }

            Runtime.SpawnSemantic = (int)task.releaseSpawnSemantic;

            InitializeParent(task);
            ApplyInitialRuntimePosition(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializeVelocity(task);
            InitializeHealth();

            Renderer = renderer;
            SimulationWorld world = task.targetWorld ??
                                    task.parent?.Match ??
                                    SimulationTickDriver.Instance?.World;
            world?.Register(this);
        }

        public override void Reset()
        {
            FrameCache.Clear();
            ResetPooledEntityState();
            Runtime.Reset();
            ResetReusableRuntimeComponents();
            _lateBreakEffectsHandled = false;
            ObjectId = 0;
            Team = 0;
            RelationTeam = 0;
            Health.HP = 0;
            Health.HPBound = 0;
            Health.HP3 = 0;
            Health.MP = 0;
            Health.PP = 0;
            Health.MaxPP = 0;
            Health.PPBound = 0;
            Health.MaxMP = 0;
            ShotCount = 0;
            PickerStableId = -1;
            GrabbedBy = 0;
            HolderCopySlot = -1;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
            TrackerFlag = 0;
            TrackerParent = null;
            Runtime.LinkState = 0;
            Runtime.TargetSlotIndex = -1;
            Runtime.HeldWeaponStableId = -1;
            Runtime.HolderStableId = -1;
            Runtime.WeaponState = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            if (_lateBreakEffectsHandled)
                return;

            RunDiePhase();
        }

        /// <summary>
        /// 武器正式切帧入口。
        /// 这里负责同步当前帧数据，并在 state 变化时触发进入/退出事件。
        /// </summary>
        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans)
        {
            Frame.PN = Frame.N;
            WriteCurrentFrameId(targetFrameId);

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null)
                return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateExitEvent();

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                AttackingCounter = 0;
                StateEntryEvent();
            }

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            if (!string.IsNullOrEmpty(Frame.D.sound))
                PlaySound(Frame.D.sound);
        }

        // 武器也复用统一的 common frame tick，只是在更外层额外补武器专属行为。
        /// <summary>
        /// 武器的通用 frame_tick 入口。
        /// 武器也复用 common frame tick，只是在别的阶段再补武器专属物理和交互。
        /// </summary>
        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();
        }

        public override void RunFrameLogicBeforeAdvance()
        {
            RunWeaponFrameLogicBeforeAdvance();
            int hitFa = Frame?.D?.hit_Fa ?? 0;
            if (hitFa != 4 && hitFa != 12)
                base.RunFrameLogicBeforeAdvance();
        }

        internal override bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
        {
            return frame != null &&
                   GetRuntimeHolderEntity() == null &&
                   frame.hit_Fa > 0;
        }

        /// <summary>
        /// 武器 TU 入口。
        /// 这里主要负责飞行物理、落地分流，以及把结果刷新回运行时快照。
        /// </summary>
        public override void SimTU(int tickIndex)
        {
            int currentDataType = GetCurrentDataObjectTypeForSimulation();
            if (currentDataType == (int)LF2ObjectType.Character)
            {
                RunSharedCharacterDatFrameAdvanceAsCharacter(tickIndex);
                return;
            }
            if (!UsesNativeWeaponFrameAdvanceForCurrentData(currentDataType))
            {
                RunSharedNonCharacterDatFrameAdvance();
                return;
            }

            RunFrameAdvancePhysics();
            Runtime.SyncIntegerPosition();
            RefreshRuntimeSnapshot();
        }

        protected abstract bool UsesNativeWeaponFrameAdvanceForCurrentData(int currentDataType);

        /// <summary>
        /// 武器晚阶段销毁检查。
        /// 主要处理“飞行寿命结束后，在本 tick 尾部标记销毁”的那条路径。
        /// </summary>
        internal override bool TryRunLatePostOpointCleanupPhase()
        {
            bool completed = base.TryRunLatePostOpointCleanupPhase();
            if (completed)
            {
                // The C# authority frees the depleted slot after sound only; suppress Destroy's
                // generic weapon effect path so this cleanup does not manufacture fragments.
                _lateBreakEffectsHandled = true;
            }

            return completed;
        }

        protected override bool StateEntryEvent() => DispatchCurrentStateEvent("state_entry");
        protected override bool FrameEvent() => DispatchCurrentStateEvent("frame");

        protected override bool DieEvent()
        {
            RunDiePhase();
            return true;
        }

        public void ForceClearHolder()
        {
            _releaseFlowResolver.ForceClearHolder();
        }

        public void ForceClearHolder(bool preserveRuntimeOwnerFields)
        {
            _releaseFlowResolver.ForceClearHolder(preserveRuntimeOwnerFields);
        }

        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            return _frameLogicResolver.ApplyBeforeWaitAdvance();
        }

        internal void SetFrameTickDirectForOwnedModule(int frameId)
        {
            SetFrameTickDirect(frameId);
        }

        // 供 LF2WeaponHeldStateResolver 调用受保护的 CoincideXYWithWPoint（:332）
        internal void CoincideXYWithWPointInternal(Vector3 holdpoint, WeaponPoint heldFrameWpoint)
            => CoincideXYWithWPoint(holdpoint, heldFrameWpoint);

        #region 交互分发（从 8101df55 恢复）

        internal bool HandleWeaponKind3Stick(InteractionArea itr, LF2Entity target)
        {
            if (target is LF2WeaponBase) return false;
            if (!ItrArestTest()) return false;

            int catchingFrame = itr.catchingact != null && itr.catchingact.Length > 0 ? itr.catchingact[0] : 0;
            int caughtFrame   = itr.caughtact   != null && itr.caughtact.Length   > 0 ? itr.caughtact[0]   : 0;
            if (catchingFrame <= 0 && caughtFrame <= 0)
                return HandlePreInteractionKind3(itr, target); // 无粘附帧 → 普通攻击

            if (catchingFrame > 0) SetFrameDirect(catchingFrame);
            if (caughtFrame > 0 && target is LF2Character ch)
            {
                ch.ImmediateFrame(caughtFrame);
            }
            return true;
        }

        internal bool TryApplyHit(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null || PS == null)
                return false;

            SimulationWorld world = Match ?? target.Match;
            var attackerPos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z);
            return world?.DamageWriter.TryApplyCurrentDatTargetHit(
                world,
                this,
                target,
                itr,
                attackerPos) == true;
        }

        internal bool HandlePreInteractionKind1(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null)
            {
                return false;
            }
            if (!ItrArestTest())
            {
                return false;
            }
            if (Renderer == null)
            {
                return false;
            }
            if (target is not LF2Character character)
            {
                return false;
            }
            if (character.GetHeldWeapon() != null)
            {
                return false;
            }

            // 只有地面武器才能被拾取（C++ release 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround)
            {
                return false;
            }

            bool pickOk = Pick(character);
            if (!pickOk)
            {
                return false;
            }
            character.HoldWeapon(this);
            _interactionResolver.ApplyPickupGrabbedBy(character);
            return true;
        }

        internal bool HandlePreInteractionKind2(InteractionArea itr, LF2Entity target)
        {
            if (HoldObj != null)
            {
                return false;
            }
            if (!ItrArestTest())
            {
                return false;
            }
            if (Renderer == null)
            {
                return false;
            }
            if (target is not LF2Character character)
            {
                return false;
            }
            if (character.GetHeldWeapon() != null)
            {
                return false;
            }

            // 只有地面武器才能被拾取（C++ release 0x00407378：仅检查 state=1004 和 2004）
            int wstate = GetState();
            bool isOnGround = wstate == LF2States.WeaponOnGround
                           || wstate == LF2States.HeavyWeaponOnGround;
            if (!isOnGround)
            {
                return false;
            }

            bool pickOk = Pick(character);
            if (!pickOk)
            {
                return false;
            }
            character.HoldWeapon(this);
            _interactionResolver.ApplyPickupGrabbedBy(character);
            // C++ release 0x42EA9C/0x42EC29：kind=2 拾取后跳转 frame=115/116
            _interactionResolver.ApplyPickupFrameJump(character);
            return true;
        }

        internal bool HandlePreInteractionKind3(InteractionArea itr, LF2Entity target)
        {
            // C++ release sub_419F80：kind=3 时若 target.charData.type != 0（即目标是武器）则跳过
            // 否则走普通命中路径，与 kind=0 相同
            if (target is LF2WeaponBase) return false;
            return TryApplyHit(itr, target);
        }

        internal bool HandlePreInteractionKind7(InteractionArea itr, LF2Entity target)
        {
            // C++ release 0x42E97B/0x42E984：kind=7 近身拾取，与 kind=1 相同但无帧跳转
            return HandlePreInteractionKind1(itr, target);
        }

        #endregion

        public void SetWeaponStrengthList(List<WeaponStrengthEntry> list)
        {
            _weaponStrengthList = list;
        }

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();
            Runtime.PickerStableId = PickerStableId;
            Runtime.WeaponDropHurt = WeaponDropHurt;
        }

        protected virtual void ProcessDrinkConsumption(
            LF2Entity holder,
            ref WeaponActResult result)
        {
            _heldStateResolver.ProcessDrinkConsumption(holder, ref result);
        }

        protected WeaponStrengthEntry GetStrengthEntry(int attackingIndex)
        {
            if (_weaponStrengthList == null || attackingIndex <= 0)
                return null;

            for (int index = 0; index < _weaponStrengthList.Count; index++)
            {
                WeaponStrengthEntry entry = _weaponStrengthList[index];
                if (entry != null && entry.index == attackingIndex)
                    return entry;
            }

            return null;
        }

        protected void InitializeParent(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            bool inheritParentRelation = task.inheritParentRelation;
            if (task.useExplicitRelationIdentity)
            {
                Team = task.team;
                RelationTeam = task.relationTeam;
                HolderCopySlot = task.holderCopySlot;
            }
            else if (task.parent != null && inheritParentRelation)
            {
                Team = task.parent.Team;
                RelationTeam = task.parent.RelationTeam;
                HolderCopySlot = task.parent.HolderCopySlot;
            }
            else
            {
                Team = task.team;
                RelationTeam = task.relationTeam;
                if (RelationTeam == 0)
                    RelationTeam = task.team;
                HolderCopySlot = task.holderCopySlot;
            }

            Runtime.OwnerStableId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
        }

        protected void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        protected void InitializeFrame(OPointCreateTask task)
        {
            int action = task.opoint.action;
            LF2CharacterDataWrapper wrapper = ResolveRuntimeCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);
            if (action == 0 && !task.preserveActionZero && !FrameCache.HasFrame(0))
                action = 999;
            Frame.D = FrameCache.GetFrameDataById(action);
            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = 0;
            Frame.Prev2D = FrameCache.GetFrameDataById(0);
            SetFrameDirect(action, 0);
        }

        protected void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                Runtime.Vx = task.directVx;
                Runtime.Vy = task.directVy;
                Runtime.Vz = task.directVz;
                return;
            }

            Runtime.Vx = Dirh() * task.opoint.dvx;
            Runtime.Vy = task.opoint.dvy;
            Runtime.Vz = task.releaseSpawnSemantic == ReleaseSpawnSemantic.LateOpoint ? 0f : task.dvz;
        }

        protected void InitializeHealth()
        {
            LF2CharacterData charData = ResolveRuntimeCharacterData(ObjectId);
            Health.HP = 500;
            Health.HPBound = 500;
            Health.HP3 = 500;
            Health.PP = 500;

            if (charData != null)
            {
                WeaponDropHurt = charData.weapon_drop_hurt > 0 ? charData.weapon_drop_hurt : WeaponDropHurt;
            }

            OnHealthInitialized(charData);
        }

        private void ApplyWhirlwindVelocity(LF2Entity attacker, double vyDelta)
        {
            KnockbackVx = Runtime.Vx +
                (Runtime.XInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            Runtime.Vx = KnockbackVx;

            KnockbackVz = Runtime.Vz +
                (Runtime.ZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            Runtime.Vz = KnockbackVz;

            if (GetRuntimeYInt() >= -2)
            {
                Runtime.Y = -2f;
                Runtime.YInt = -2;
                Runtime.Vy = -6f;
            }

            if (Runtime.Vy > -6f)
            {
                Runtime.Vy -= vyDelta;
                KnockbackVy = Runtime.Vy;
            }
        }

        protected static bool ShouldAbortAfterSuccessfulReleaseHit(InteractionArea itr, LF2Entity target)
        {
            return itr != null &&
                   target != null &&
                   itr.kind == 0 &&
                   target.ObjectId == 300;
        }

        private void RunWeaponFrameLogicBeforeAdvance()
        {
            _frameLogicResolver.RunWeaponFrameLogicBeforeAdvance();
        }

        protected internal void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            if (frameId >= 0 && FrameCache?.HasFrame(frameId) != true)
                return;

            WriteCurrentFrameId(frameId);
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;
            if (Frame.D != null && Trans != null)
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
        }

        // 这里是真正的武器飞行动力学入口：
        // 先看是否允许推进，再做速度应用、飞行物理、空中更新和落地分流。
        private void RunFrameAdvancePhysics()
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return;
            if (GetRuntimeHolderEntity() != null)
                return;
            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return;

            ApplyNonCharacterFrameVelocityForFrameAdvance();

            int state = CurrentFrameState();
            switch (state)
            {
                case LF2States.WeaponOnHand:
                case 2001:
                    break;
                default:
                    _gravityToAdd = 0f;
                    WeaponFlightPhysics();
                    bool landed = CharacterMechanics.WeaponDynamics(Runtime, _gravityToAdd, out _lastLandingVyBeforeClamp);
                    RegisteredWorldForSimulation?.BoundaryWriter.SyncConsumedFlags(Runtime);

                    if (Runtime.Y < -0.0001)
                        OnInFlightFrameUpdate();

                    if (landed)
                        OnLanded();
                    break;
            }

            if ((Frame?.D?.state ?? -1) != LF2States.Falling)
                FluteWeight = 0;
        }

        /// <summary>
        /// 武器交互阶段入口。
        /// 只有未被持有、未冻结在特殊 link 中、且当前可交互时才会真正执行命中检测。
        /// </summary>
        public override void SimObjectInteraction(int tickIndex)
        {
            if (UsesCharacterDatInteractionPhase())
                return;

            if (Runtime?.PendingFlushDestroy == true)
                return;

            // Step 9 consumes the candidates frozen by step 6. Held/link state,
            // frame delay, and attack-exempt gates belong to collection, not here.
            Interaction();
        }

        internal override bool TryGetBattleHitCandidateConsumer(
            BattleHitExecutionPass pass,
            out IBattleHitCandidateConsumer consumer)
        {
            if (pass == BattleHitExecutionPass.Object &&
                !UsesCharacterDatInteractionPhase())
            {
                consumer = _interactionResolver;
                return true;
            }

            return base.TryGetBattleHitCandidateConsumer(pass, out consumer);
        }

        private void RunDiePhase()
        {
            PlaySound(WeaponBrokenSound);
        }

    }

    public struct WeaponActResult
    {
        public bool Thrown;
        public bool ForceDrop;
        public bool NeedsKind3Drop;
        public WeaponAttackResult AttackResult;
    }

    public struct WeaponAttackResult
    {
        public int VRest;
        public int ARest;
        public int HitUid;
    }
}
