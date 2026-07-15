using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Game;
using NTSD.Input;
using NTSD.LevelEditor;
using NTSD.Simulation;
using NTSD.Tools;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /*
    /// <summary>
    /// 角色专用战斗逻辑，基于 LF2LivingObject 分层实现。
    /// 战斗行为以 C++ release 的实体、帧和输入模型为准；
    /// Unity 专用代码只负责组件装配、对象池和数据适配。
    /// 
    /// 继承关系：LF2Li
    vingObject -> LF2Character。
    /// </summary>
    */
    /// <summary>
    /// 角色实体主类。
    /// 
    /// 读这个项目时，如果你想先搞清楚“一个角色到底由哪些部分组成”，
    /// 这个类就是最重要的入口之一。
    /// 
    /// 可以把它理解成角色的总控制器：
    /// 1. 自己持有角色运行时数据。
    /// 2. 把输入、状态机、受击、抓取、武器链接等模块装配起来。
    /// 3. 在每个战斗 tick 中，对外暴露角色该执行的主要入口。
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ========== ILF2Object 实现 ==========

        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.Character;
        internal override bool UsesDynamicRuntimeSlot() => _initializedFromOpoint;
        internal override bool SupportsFrameLogicBeforeAdvancePhase(LF2FrameData frame)
            => false;

        // ========== 角色专用模块 ==========

        public NTSDInputStateModule InputState { get; private set; }

        /// <summary>
        /// 处理 fall / bdefend 等受击累计计数。
        /// </summary>
        private readonly LF2HitCountersModule _hitCounters;
        public override LF2HitCountersModule HitCounters => _hitCounters;
        private readonly LF2CharacterHitResolver _hitResolver;
        private readonly LF2CharacterActionResolver _actionResolver;
        private readonly LF2CharacterCatchResolver _catchResolver;
        private readonly LF2CharacterInteractionResolver _interactionResolver;
        private readonly LF2CharacterStateResolver _stateResolver;
        private readonly LF2CharacterDamageStateResolver _damageStateResolver;
        private readonly LF2CharacterWeaponLinkResolver _weaponLinkResolver;

        // ========== 武器持有 ==========

        /// <summary>当前持有的武器对象引用。正式持有关系字段同步到 Runtime。</summary>
        private ILF2Object _heldWeapon;
        private int _heldDiagLastPrintedSlot = -1;

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

        // ========== 死亡闪烁计数 ==========
        // -1 = 不执行；0 = 开始；1~29 = 持续；>=30 = 结束销毁
        private int _deadBlinkCount = -1;

        private bool _initializedFromOpoint;
        private bool _preserveOpointActionZero;

        // ========== 构造函数 ==========

        public LF2Character() : base()
        {
            AllocateStableId();

            // 创建角色专用模块
            InputState = new NTSDInputStateModule();
            _hitCounters = new LF2HitCountersModule();
            _hitResolver = new LF2CharacterHitResolver(this);
            _actionResolver = new LF2CharacterActionResolver(this);
            _catchResolver = new LF2CharacterCatchResolver(this);
            _interactionResolver = new LF2CharacterInteractionResolver(this);
            _stateResolver = new LF2CharacterStateResolver(this);
            _damageStateResolver = new LF2CharacterDamageStateResolver(this);
            _weaponLinkResolver = new LF2CharacterWeaponLinkResolver(this);

            // 基类字段初始化
            ItrRest = new LF2ItrRestTracker();
            
            
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            Health = new LF2Health();
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            _hitCounters.BindRuntime(Runtime);
            Sprite = new LF2Sprite();
            Trans = new FrameTransistor(this);
            Controller = new CharacterInputModule();

            // 角色状态分发固定写在 switch 中，不再保留运行时 handler 表。
        }

        /// <summary>
        /// 应用物理动力学
        /// </summary>
        public void ApplyDynamics()
        {
            var ctx = new CharacterMechanicsContext(
                Runtime,
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
                    Runtime.UpdateSpriteOrigin(Frame.D.centerx, Frame.D.centery, spriteWidthPx);
            }

            Runtime.SyncIntegerPosition();
        }

        public override bool Hit(InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            return _hitResolver.ResolveHit(itr, attacker, attackerPos, vol);
        }

        internal bool PassBaseHit(InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            return base.Hit(itr, attacker, attackerPos, vol);
        }

        internal bool IsHeavyWeaponAttackerForHit(LF2Entity attacker)
        {
            return attacker is LF2WeaponBase weapon && weapon.WeaponType == 2;
        }

        internal void ApplyKind14DirectionalBlockFromHit(LF2Entity attacker)
        {
            ApplyKind14DirectionalBlockFrom(attacker);
        }

        internal void ApplyHitInjury(int injury)
        {
            Injury(injury);
        }

        internal bool ProcessReleaseInput()
        {
            return _actionResolver.ProcessReleaseInput();
        }

        /// <summary>
        /// 共享输入快照读取入口。
        /// 当前阶段仍保留 `Controller` 作为当前按住态的兼容兜底，
        /// 但 previous/cooldown/defend-lock 已经统一从 Runtime 读取，
        /// 这样后续把输入链从 `LF2Character.InputState` 拆出来时，resolver 不需要再逐个改。
        /// </summary>
        internal bool IsCurrentLeftPressedInternal()
        {
            return Runtime.KeyLeft != 0 || Controller?.IsLeft == true;
        }

        internal bool IsCurrentRightPressedInternal()
        {
            return Runtime.KeyRight != 0 || Controller?.IsRight == true;
        }

        internal bool IsCurrentUpPressedInternal()
        {
            return Runtime.KeyUp != 0 || Controller?.IsUp == true;
        }

        internal bool IsCurrentDownPressedInternal()
        {
            return Runtime.KeyDown != 0 || Controller?.IsDown == true;
        }

        internal bool IsCurrentAttackPressedInternal()
        {
            return Runtime.KeyAttack != 0 || Controller?.IsAttack == true;
        }

        internal bool IsCurrentJumpPressedInternal()
        {
            return Runtime.KeyJump != 0 || Controller?.IsJump == true;
        }

        internal bool IsCurrentDefendPressedInternal()
        {
            return Runtime.KeyDefend != 0 || Controller?.IsDefend == true;
        }

        internal bool WasLeftPressedPreviousFrameInternal()
        {
            return Runtime.PrevLeft != 0;
        }

        internal bool WasRightPressedPreviousFrameInternal()
        {
            return Runtime.PrevRight != 0;
        }

        internal bool WasUpPressedPreviousFrameInternal()
        {
            return Runtime.PrevUp != 0;
        }

        internal bool WasDownPressedPreviousFrameInternal()
        {
            return Runtime.PrevDown != 0;
        }

        internal bool WasAttackPressedPreviousFrameInternal()
        {
            return Runtime.PrevAttack != 0;
        }

        internal bool WasJumpPressedPreviousFrameInternal()
        {
            return Runtime.PrevJump != 0;
        }

        internal bool WasDefendPressedPreviousFrameInternal()
        {
            return Runtime.PrevDefend != 0;
        }

        internal int ReadAttackCooldownInternal()
        {
            return Runtime.CdAttack;
        }

        internal int ReadJumpCooldownInternal()
        {
            return Runtime.CdJump;
        }

        internal int ReadDefendCooldownInternal()
        {
            return Runtime.CdDefend;
        }

        /// <summary>
        /// 当前 Unity 角色动作层的“攻击动作输入成立”入口。
        ///
        /// 这里故意把“按钮 + cooldown”打包成语义 helper，
        /// 这样后续如果要把 Unity 从当前直连 cooldown 语义迁到参考 C# 的交叉 cooldown 语义，
        /// 就不需要在每个 resolver 里重新逐点改判断条件。
        /// </summary>
        /// <summary>
        /// 当前角色动作层的 attack 输入成立入口。
        /// 这里已经对齐参考 C# 的交叉 cooldown 语义：
        /// `Jump 键当前按住 + CdAttack > 0` 才表示这一拍要走 attack 输入分支。
        /// </summary>
        internal bool IsAttackActionInputReadyInternal()
        {
            return IsCurrentJumpPressedInternal() && ReadAttackCooldownInternal() > 0;
        }

        /// <summary>
        /// 当前 Unity 角色动作层的“跳跃动作输入成立”入口。
        /// 当前仍保持现有直连语义，不在这里提前切到参考 C# 的交叉 cooldown 映射。
        /// </summary>
        /// <summary>
        /// 当前角色动作层的 jump 输入成立入口。
        /// 这里同样使用参考 C# 的交叉 cooldown 语义：
        /// `Defend 键当前按住 + CdJump > 0` 才表示 jump 输入分支。
        /// </summary>
        internal bool IsJumpActionInputReadyInternal()
        {
            return IsCurrentDefendPressedInternal() && ReadJumpCooldownInternal() > 0;
        }

        /// <summary>
        /// 当前 Unity 角色动作层的“防御动作输入成立”入口。
        /// `requireDefendLockOpen=true` 时，会额外要求当前不处于 defend lock 短窗口。
        /// </summary>
        internal bool IsDefendActionInputReadyInternal(bool requireDefendLockOpen = false)
        {
            if (!IsCurrentAttackPressedInternal() || ReadDefendCooldownInternal() <= 0)
                return false;

            return !requireDefendLockOpen || !IsDefendLockActiveInternal();
        }

        internal bool IsDefendLockActiveInternal()
        {
            return Runtime.CdDefendLock > 0;
        }

        internal void SetDefendLockInternal(byte value)
        {
            Runtime.CdDefendLock = value;
            InputState?.SetDefendLock(value);
        }

        internal void UpdateLocalInputStateFromControllerBuffer(int tickIndex)
        {
            InputState?.UpdateFromBuffer(Controller?.InputBuffer, tickIndex, this);
        }

        internal void ApplyFrameInputFromLocalState()
        {
            InputState?.ApplyFrameInput(this);
        }

        internal void ResetLocalInputState()
        {
            InputState?.Reset();
        }

        internal void OnLocalInputStateExit()
        {
            InputState?.OnStateExit();
        }

        internal bool HasBoundControllerInternal()
        {
            return Controller != null;
        }

        internal bool HasAnyDirectionInputInternal()
        {
            return IsCurrentLeftPressedInternal() ||
                   IsCurrentRightPressedInternal() ||
                   IsCurrentUpPressedInternal() ||
                   IsCurrentDownPressedInternal();
        }

        internal bool ShouldHoldNarutoDjaInputGuard(int targetFrame)
        {
            if (ObjectId != 6 || targetFrame != 300 || Health == null || Health.HP <= 177)
                return false;

            return Match?.Runtime?.Flow?.DjaGuardGlobal44F224 == 0;
        }

        internal bool CanEnterDjaInputFrameJump()
        {
            return TransformOriginalObjectId == -1 && Runtime.LinkState != 2;
        }

        internal bool ProcessCatchingInputInternal()
        {
            return _catchResolver.ProcessCatchingInput();
        }

        internal void SetAnimSubInternal(int value)
        {
            Runtime.AnimSub = value;
        }

        internal int GetAnimCounterInternal()
        {
            return Runtime.AnimCounter;
        }

        internal void SetAnimCounterInternal(int value)
        {
            Runtime.AnimCounter = value;
        }

        internal bool HasHeldObjectInternal()
        {
            return _weaponLinkResolver.HasHeldObject();
        }

        internal bool IsHeldHeavyWeaponInternal()
        {
            return _weaponLinkResolver.IsHeldHeavyWeapon();
        }

        internal bool IsHeldObjectAttackableInternal()
        {
            return _weaponLinkResolver.IsHeldObjectAttackable();
        }

        internal bool CanHeldObjectStandThrowInternal()
        {
            return _weaponLinkResolver.CanHeldObjectStandThrow();
        }

        internal bool CanHeldObjectRunThrowInternal()
        {
            return _weaponLinkResolver.CanHeldObjectRunThrow();
        }

        internal LF2WeaponBase GetHeldWeaponBaseInternal()
        {
            return _weaponLinkResolver.GetHeldWeaponBase();
        }

        public void DropWeapon(float dvx = 0f, float dvy = 0f)
        {
            LF2WeaponBase weapon = GetHeldWeaponBaseInternal();
            weapon?.Drop(dvx, dvy);
            _weaponLinkResolver.HoldWeapon(null);
        }

        internal void SetMoveFrameDirectInternal(int frameId)
        {
            _stateResolver.SetMoveFrameDirect(frameId);
        }

        internal void ApplyWalkRunFrameInternal(bool heavy)
        {
            _stateResolver.ApplyWalkRunFrame(heavy);
        }

        internal void ApplyRunLaneInternal(float speedZ)
        {
            _stateResolver.ApplyRunLane(speedZ);
        }

        internal void ApplyDashStartVelocityInternal(bool forward)
        {
            _stateResolver.ApplyDashStartVelocity(forward);
        }

        internal void ApplyDashFrameInternal()
        {
            _stateResolver.ApplyDashFrame();
        }

        internal bool TrySpendFramePpCostInternal(int frameId, bool clampOnOverdraw = false)
        {
            return TrySpendFramePpCost(frameId, clampOnOverdraw);
        }

        internal bool TrySpendFramePpCost(int frameId, bool clampOnOverdraw = false)
        {
            if (!IsPpModeEnabled() || Health == null)
                return true;

            LF2FrameData targetFrame = FrameCache?.GetFrameDataById(frameId);
            if (targetFrame == null)
                return false;

            int ppCost = targetFrame.mp;
            if (!clampOnOverdraw && Health.PP < ppCost)
                return false;

            Health.PP -= ppCost;
            if (Health.PP >= 0)
            {
                SpendPpDisplay(ppCost);
            }
            else
            {
                Health.PP = 0;
            }

            return true;
        }

        protected override void Injury(int injury)
        {
            base.Injury(injury);
            if (injury > 0)
                Health.PP = System.Math.Min(Health.PP + injury / 3, Health.MaxPP);
        }

        public override void VisualEffectCreate(int num, PhysicsState.BattleVolume rect, bool righttip = false, int variant = 0, bool withSound = false)
        {
        }

        public bool IsHeavyWeapon()
        {
            return _weaponLinkResolver.IsHeldHeavyWeapon();
        }

        internal void ForceReleaseHeldObjectReference(LF2Entity held)
        {
            _weaponLinkResolver.ForceReleaseHeldObjectReference(held);
        }

        internal void ClearHolderLinkRuntimeOnlyInternal()
        {
            _weaponLinkResolver.ClearHolderLinkRuntimeOnly();
        }

        internal ILF2Object HeldWeaponReferenceInternal
        {
            get => _heldWeapon;
            set => _heldWeapon = value;
        }

        internal int HeldDiagLastPrintedSlotInternal
        {
            get => _heldDiagLastPrintedSlot;
            set => _heldDiagLastPrintedSlot = value;
        }

        internal int DeadBlinkCountInternal
        {
            get => _deadBlinkCount;
            set => _deadBlinkCount = value;
        }

        internal bool HasHeldObjectInternalForInteraction()
        {
            return _weaponLinkResolver.HasHeldObject();
        }

        internal bool TryApplyKind6HitConfirmInternal(InteractionArea itr, LF2Entity target)
        {
            return TryApplyKind6HitConfirm(itr, target);
        }

        internal void ApplyReleaseSceneQueryConsumeEffectsInternal(SceneQueryHit hitInfo)
        {
            ApplyReleaseSceneQueryConsumeEffects(hitInfo);
        }

        internal void SetCaughtDurationInternal(int value)
        {
            CaughtDuration = value;
        }

        internal void SetCaughtFrontInternal(bool value)
        {
            CaughtFront = value;
        }

        internal void ApplyCpointThrowStep10BaseInternal(CatchPoint cpoint, LF2Entity victimEntity)
        {
            base.ApplyCpointThrowStep10(cpoint, victimEntity);
        }

        internal void ApplyCpointThrowStep10BaseInternal(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            base.ApplyCpointThrowStep10(cpoint, victimEntity, throwFrameSnapshot);
        }

        internal int RandIntInternal(int minInclusive, int maxExclusive)
        {
            return RandInt(minInclusive, maxExclusive);
        }

        /// <summary>
        /// 角色的正式切帧入口。
        /// 和 ImmediateFrame 不同，这里会交给 FrameTransistor 按 wait/next 规则处理。
        /// </summary>
        public override void TransitionToFrame(int frameId)
        {
            Trans.Frame(frameId);
        }

        /// <summary>
        /// 输入触发的直接跳帧尝试。
        /// 常见于按键出招：先校验目标帧、资源消耗和转向，再真正切过去。
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

            if (FrameCache?.HasFrame(frameId) != true || Health == null)
                return false;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(frameId);
            bool ppMode = IsPpModeEnabled();
            if (ppMode)
            {
                int ppCost = targetFrame.mp % 1000;
                int hpCost = (targetFrame.mp / 1000) * 10;
                if (Health.PP < ppCost || Health.HP <= hpCost)
                    return false;

                Health.HP -= hpCost;
                Health.PP -= ppCost;
                ComboCountVic += hpCost;
                SpendPpDisplay(ppCost);

                if (flipFacing)
                    SwitchDir(Runtime.Dir == "right" ? "left" : "right");
            }

            OnFrameTransit(frameId, false);
            return true;
        }

        public int CurrentFrameId => Frame.N;
        public LF2FrameData CurrentFrame => Frame.D;
        public int PreviousFrameId => Frame.PN;

        /// <summary>
        /// 当前帧的 frame 事件入口。
        /// 先跑角色通用阶段，再把事件分发给当前 state。
        /// </summary>
        protected override bool FrameEvent()
        {
            return RunFramePhase() || DispatchCurrentStateEvent("frame");
        }

        /// <summary>
        /// 当前 tick 的 TU 事件入口。
        /// TU 可以理解为“这一帧内的持续逻辑”，例如运动、受击状态维持等。
        /// </summary>
        protected override bool TUEvent()
        {
            return RunTUPhase() || DispatchCurrentStateEvent("TU");
        }

        /// <summary>
        /// 帧切换时的 transit 事件入口。
        /// 用来处理那些只在“刚切帧时”需要执行一次的逻辑。
        /// </summary>
        protected override bool TransitEvent()
        {
            return RunTransitPhase() || DispatchCurrentStateEvent("transit");
        }

        /// <summary>
        /// 状态退出事件入口。
        /// 先跑角色通用退出处理，再转发给具体状态。
        /// </summary>
        protected override bool StateExitEvent()
        {
            return RunStateExitPhase() || DispatchCurrentStateEvent("state_exit");
        }

        /// <summary>
        /// 状态进入事件入口。
        /// 这里主要负责把切帧后的 state_entry 转给当前状态机。
        /// </summary>
        protected override bool StateEntryEvent()
        {
            return DispatchCurrentStateEvent("state_entry");
        }

        /// <summary>
        /// 角色动作解析入口。
        /// 这里会把 `NTSDInputStateModule` 已经整理好的输入状态，进一步转换成技能或动作跳帧请求。
        /// </summary>
        protected override void ComboUpdate()
        {
            ApplyFrameInputFromLocalState();
        }

        internal void RunTuCoreForSelfCheck()
        {
            RunTUCore();
        }

        /// <summary>
        /// 角色状态机总分发器。
        /// 读角色行为时，可以把这里当成“当前 state 会转去哪个处理函数”的目录。
        /// </summary>
        /// <summary>
        /// 角色当前 DAT state 的本地事件分发入口。
        /// 这里只负责把 `frame / TU / transit / state_entry / state_exit`
        /// 这类“当前角色当前 state”的事件转发给对应 `State_*` 处理函数。
        ///
        /// 它不是整个角色战斗链路的总调度器。
        /// 像 early state 500/501、step10 cpoint、pre-collision state transform、
        /// late cleanup / death tail 这类逻辑，属于 battle pass 级特判，
        /// 入口在 `SimulationWorldPassRunner`，不走这里。
        /// </summary>
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

        private bool State_Attack(string eventType, object eventData)
        {
            switch (eventType)
            {
                case "frame":
                    var frameData = Frame.D;
                    if (frameData.next == LF2StandardFrames.LoopToStart && Runtime.Vy < 0)
                    {
                        NTSD.Tools.Log.Info("[State {0}:{1}] -> TransitionTo: Frame {2} ({3})", 3, "Attack", LF2StandardFrames.JumpingAir, "air attack return");
                        Trans.SetNext(LF2StandardFrames.JumpingAir);
                    }
                    return false;

                default:
                    return false;
            }
        }

        public override void RunStateSpecialPreCollision()
        {
            // 这里属于 late entity update 里的 pre-collision 特判阶段，
            // 不是 `DispatchCurrentStateEvent(...)` 的本地 state 事件分发。
            RunState9996SpecialPreCollision();

            base.RunStateSpecialPreCollision();
        }

        /// <summary>
        /// 角色在 battle tick 中正式消费输入缓冲的入口。
        /// 顺序是先读取当前帧按键，再执行组合键与单键跳帧判定。
        /// </summary>
        internal override void RunPostCooldownInputPhase(int tickIndex)
        {
            if (Runtime.LinkState < 0)
                return;

            if (AiControlled)
            {
                Match?.PrepareAiInputBasic(this, tickIndex);
                InputState?.SyncFromRuntime(Runtime);
                ComboUpdate();
                return;
            }

            // 第一步：把这一帧按键事件整理成“按住状态 + 新按下边沿 + 输入历史”。
            UpdateLocalInputStateFromControllerBuffer(tickIndex);
            // 第二步：根据当前帧 DAT 的 hit_* 配置，决定是否切到技能/动作帧。
            ComboUpdate();
        }

        internal override void RunCharacterInputPhase(int tickIndex)
        {
            if (Runtime.LinkState < 0)
                return;

            UpdateLocalInputStateFromControllerBuffer(tickIndex);
            ComboUpdate();
            // BMD-065: baseline InputRuntime.ApplyCharacterInput L737 applies frame dvx/dvy/dvz
            // at end of character input phase. Character frames have dvy=0 for jump/attack states,
            // so this primarily affects dvx momentum on certain attack frames (e.g., frame 71 dvx=4).
            ApplyNonCharacterFrameVelocityForFrameAdvance();
        }

        internal override void RunEarlyTeleportSpecialsPhase(List<LF2Entity> entities, bool frameToggleGate)
        {
            base.RunEarlyTeleportSpecialsPhase(entities, frameToggleGate);
        }

        private void InitializeFromOpoint(OPointCreateTask task)
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

            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
            KillCount = -1;
            HitStun = 0;

            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(string.IsNullOrEmpty(dir) ? "right" : dir);

            int action = task.opoint.action;
            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = action;
            Frame.N = action;
            Frame.D = FrameCache?.GetFrameDataById(action);
            Frame.Prev2D = Frame.D;
            if (Frame.D != null)
                Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, 0);

            SetOpointPosition(task);
            SetOpointVelocity(task);
            InitializeRuntimeIntPosition(task);

            FrameDelay = task.frameDelay;
            AttackExempt = task.attackExempt;

            AiControlled = true;
            // opoint 生成的角色不能绑定玩家输入，但必须拥有独立的逻辑输入缓冲，
            // 否则 release 风格生成的 AI 角色会被标记为 AiControlled 却无法接收帧输入。
            Controller = new CharacterInputModule();
            _preserveOpointActionZero = task.preserveActionZero;
            _initializedFromOpoint = true;
        }

        private void SetOpointPosition(OPointCreateTask task)
        {
            SetPos(task.pos.x, task.pos.y, task.z);
        }

        private void SetOpointVelocity(OPointCreateTask task)
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
            Runtime.Vz = 0f;
        }

        private void InitializeRuntimeIntPosition(OPointCreateTask task)
        {
            if (task == null)
                return;

            if (task.useInitialRuntimeIntPosition)
            {
                ApplyForcedRuntimeIntPosition(task.initialRuntimeX, task.initialRuntimeY, task.initialRuntimeZ);
                return;
            }

            ClearForcedRuntimeIntPosition();
        }

        public void InjectDependencies(Transform entityTransform, Transform visualTransform, string name)
        {
            EntityTransform = entityTransform;
            Name = name;
        }

        public void ModuleInitialize()
        {
            _mech = new CharacterMechanics();
            _cachedIsPointWalkable = point =>
            {
                SimulationWorld world = Match;
                return world == null || world.IsGroundPointWalkable(point);
            };

            Runtime.X = 0f;
            Runtime.Y = 0f;
            Runtime.Z = 0f;
            Runtime.Vx = 0f;
            Runtime.Vy = 0f;
            Runtime.Vz = 0f;
        }

        public override void Init(LF2TaskBase task, LF2ObjectRenderer renderer)
        {
            Renderer = renderer;

            if (task is not OPointCreateTask opTask)
                return;

            InitializeFromOpoint(opTask);
        }

        public override void Reset()
        {
            ResetLocalInputState();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            Runtime.Reset();
            _heldWeapon = null;

            AttackingCounter = 0;
            FrameDelay = 10;
            ShotCount = 0;
            ResetSpark();
            _initializedFromOpoint = false;
            _preserveOpointActionZero = false;
            AiControlled = false;
            if (Controller is CharacterInputModule inputModule)
                inputModule.ModuleUnbind();

            Controller = new CharacterInputModule();
            ResetStateRuntime();
        }

        public override void Destroy()
        {
            Reset();
        }

        public override void UnregisterFromWorld()
        {
            Match?.Unregister(this);
        }

        public override void OnTransitDestroy()
        {
            DestroyEvent();
            Destroy();

            if (Renderer != null)
            {
                LF2ObjectPool.Instance?.Release(Renderer);
                Renderer = null;
            }
            else
            {
                UnregisterFromWorld();
            }

            LF2ReferencePool.Instance?.Release(this);
        }

        public void ModuleBind(LF2CharacterDataWrapper frameDataWrapper, int characterId)
        {
            FrameCache.Load(frameDataWrapper);

            if (!_initializedFromOpoint)
            {
                Frame.D = FrameCache.GetFrameDataById(0);
                Frame.PN = 0;
                Frame.N = 0;
            }
            else
            {
                if (Frame.N == 0 && !_preserveOpointActionZero && !FrameCache.HasFrame(0))
                    Frame.N = 999;

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

            InputState?.Reset();
            ItrRest?.Reset();
            _hitCounters?.Reset();

            _mass = NTSDSpec.GetMassOrDefault(characterId);

            SimulationTickDriver.Instance?.World?.Register(this);

            if (!_initializedFromOpoint)
                HolderCopySlot = Runtime?.SlotIndex ?? -1;
        }

        public void Initialize(int maxHp, int maxMp)
        {
            Health.HP = maxHp;
            Health.HPBound = maxHp;
            Health.HP3 = maxHp;
            Health.MP = maxMp;
            Health.PP = maxMp;
            Health.MaxPP = maxMp;
            Health.PPBound = maxMp;
            Health.MaxMP = maxMp;
            ResetLocalInputState();
            HitCounters.Reset();
            ItrRest.Reset();
        }

        protected override void ResetStateRuntime()
        {
            CaughtDuration = 0;
            CaughtFront = true;
            WeaponCount = 0;
            FallDamageDiv = 0;
            GrabbedBy = 0;
            CaughtSlotIndex = -1;
            CatcherSlotIndex = -1;
            TrackerFlag = 0;
            TrackerParent = null;
            HolderCopySlot = -1;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
        }

        public override void SimTransit(int tickIndex)
        {
            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                RunReleaseFrameAdvance(tickIndex, consumeInitialRuntimePosition: true);
            else
                RunSharedNonCharacterDatFrameAdvance();
        }

        private bool RunReleaseFrameAdvance(int tickIndex, bool consumeInitialRuntimePosition)
        {
            if (!TryEnterReleaseFrameAdvanceAfterDelay())
                return false;

            if (IsBlockedByReleaseLinkOrCaughtCpoint())
                return false;

            if (Frame?.D?.cpoint != null && Frame.D.cpoint.kind == 2)
                return false;

            ApplyDynamics();
            PromoteState12AirborneFrameIfNeeded(tickIndex);
            PromoteBurningAirborneFrame205IfNeeded();
            ResetWeaponCountOutsideState12FrameAdvanceTail();

            if (consumeInitialRuntimePosition)
                ConsumeForcedRuntimeIntPosition();

            return true;
        }

        private void PromoteState12AirborneFrameIfNeeded(int tickIndex)
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return;

            if (frame.state != LF2States.Falling)
                return;

            if (Runtime.Y >= 0f)
                return;

            int frameId = Frame.N;
            double vy = Runtime.Vy;

            if (frameId < LF2StandardFrames.FallingFront5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingFront2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingFront3);

                if (WeaponCount < 0)
                {
                    int cadencePhase = (tickIndex - 1) % 12;
                    if (cadencePhase < 0)
                        cadencePhase += 12;

                    if (vy < 12.0f && cadencePhase >= 6)
                        SetFrameTickDirect(LF2StandardFrames.FallingFront2);
                    else
                        SetFrameTickDirect(LF2StandardFrames.FallingFront1);
                }
            }
            else if (frameId > LF2StandardFrames.FallingFront5 && frameId < LF2StandardFrames.FallingBack5)
            {
                if (vy < -8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack);
                else if (vy < 1.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack1);
                else if (vy < 8.0f)
                    SetFrameTickDirect(LF2StandardFrames.FallingBack2);
                else
                    SetFrameTickDirect(LF2StandardFrames.FallingBack3);
            }
        }

        private void PromoteBurningAirborneFrame205IfNeeded()
        {
            LF2FrameData frame = Frame?.D;
            if (frame == null)
                return;

            if (frame.state != LF2States.Burning)
                return;

            if (Frame.N >= LF2StandardFrames.Fire2)
                return;

            if (Runtime.Y >= 0f || Runtime.Vy <= 1.0f)
                return;

            SetFrameTickDirect(LF2StandardFrames.Fire2);
        }

        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();
        }

        public override void RunFrameLogicBeforeAdvance()
        {
            int hitFa = Frame?.D?.hit_Fa ?? 0;
            if (Runtime == null || (hitFa != 1 && hitFa != 2 && hitFa != 3 && hitFa != 4 && hitFa != 5 && hitFa != 6 && hitFa != 7 && hitFa != 8 && hitFa != 9 && hitFa != 10 && hitFa != 11 && hitFa != 12 && hitFa != 13 && hitFa != 14))
                return;

            if (hitFa == 1)
            {
                RunHitFa1FrameLogic();
                return;
            }

            if (hitFa == 3)
            {
                RunHitFa3FrameLogic();
                return;
            }

            if (hitFa == 2 || hitFa == 4 || hitFa == 12 || hitFa == 14)
            {
                RunHitFa2Or4Or12Or14FrameLogic(hitFa);
                return;
            }

            if (hitFa == 10)
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 1.1f;
                else
                    Runtime.Vx += 1.1f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -30.0, 30.0);
                if (Runtime.Y > 3f)
                    Runtime.Y = 3f;

                SwitchDir(Runtime.Vx > 0f ? "right" : "left");
                Runtime.YInt = (int)Runtime.Y;
                return;
            }

            if (hitFa == 6 || hitFa == 9)
            {
                RunHitFa6Or9FrameLogic(hitFa);
                return;
            }

            if (hitFa == 8)
            {
                RunHitFa8FrameLogic();
                return;
            }

            if (hitFa == 11)
            {
                RunHitFa11FrameLogic();
                return;
            }

            if (hitFa == 13)
            {
                RunHitFa13FrameLogic();
                return;
            }

            if (hitFa == 5)
            {
                RunHitFa5FrameLogic();
                return;
            }

            RunHitFa7FrameLogic();
        }

        private void RunHitFa1FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(1);
            if (target == null || target.Health == null || target.Health.HP <= 0)
            {
                if (Health != null)
                    Health.HP = 0;
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 1);
            int selfZ = GetFrameLogicTargetZInt(this, 1);

            if (targetX > selfX)
                Runtime.Vx += 0.85f;
            if (targetX < selfX)
                Runtime.Vx -= 0.85f;
            if (targetZ > selfZ + 7)
                Runtime.Vz += 0.3f;
            if (targetZ < selfZ - 7)
                Runtime.Vz -= 0.3f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f→0.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 10f < target.Runtime.Y)
                    Runtime.Y += 1.2f;
                if (Runtime.Y + 10f > target.Runtime.Y)
                    Runtime.Y -= 1.2f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -13.0, 13.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.0, 2.0);
            if (Runtime.Y > 1f)
                Runtime.Y = 1f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private void RunHitFa3FrameLogic()
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(3);
            if (target == null)
            {
                if (Health != null)
                    Health.HP = 0;

                return;
            }

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa3NoTargetDrift();
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, 3);
            int selfZ = GetFrameLogicTargetZInt(this, 3);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 10)
                Runtime.Vz += 0.17f;
            if (targetZ < selfZ - 10)
                Runtime.Vz -= 0.17f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -16.0, 16.0);
            Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.4, 2.4);
        }

        private void RunHitFa8FrameLogic()
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            var enemies = new List<int>(8);
            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity obj = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(obj))
                    continue;
                if (!IsCharacterFrameLogicTarget(obj))
                    continue;
                if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                    continue;

                int enemySlot = GetRuntimeSlotOrNegative(obj);
                if (enemySlot < 0)
                    continue;

                enemies.Add(enemySlot);
            }

            int count = 3;
            if (enemies.Count > 4)
                count = (enemies.Count - 3) / 2 + 3;

            int facing = Runtime.Dir == "right" ? 0 : 1;
            for (int i = 0; i < count; i++)
            {
                int ownerSlot = enemies.Count > 0
                    ? enemies[RandInt(0, enemies.Count)]
                    : GetRuntimeSlotOrNegative(this);

                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 225,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = facing,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = Runtime.Dir;
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = RandInt(0, 21) - 11;
                task.directVy = 3.0f - RandInt(0, 24) * 0.25f;
                task.directVz = 3.0f - RandInt(0, 24) * 0.25f;
                task.ownerEntityIndex = ownerSlot;
                FillHitFa8SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa6Or9FrameLogic(int hitFa)
        {
            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int max = hitFa == 9 ? 10 : 7;
            int maxPerLaterPass = hitFa == 9 ? 4 : 0;
            int spawnCount = 0;
            int loopCount = 0;
            bool spawnedThisLoop;

            do
            {
                spawnedThisLoop = false;
                for (int i = 0; i < allObjects.Count && spawnCount < max; i++)
                {
                    LF2Entity obj = allObjects[i];
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;
                    if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                        continue;
                    if (!(spawnCount < maxPerLaterPass || loopCount == 0))
                        continue;

                    int enemySlot = GetRuntimeSlotOrNegative(obj);
                    if (enemySlot < 0)
                        continue;

                    int oid;
                    float vx;
                    float vy;
                    if (hitFa == 6)
                    {
                        oid = 220;
                        vx = (float)((obj.Runtime.X - Runtime.X) / 50.0f);
                        vy = -(4 + RandInt(0, 4));
                    }
                    else
                    {
                        oid = RandInt(0, 2) + 221;
                        vx = RandInt(0, 21) - 11;
                        vy = -2.0f - RandInt(0, 40) * (1.0f / 6.0f);
                    }

                    OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                    task.opoint = new ObjectPoint
                    {
                        oid = oid,
                        kind = 0,
                        action = 0,
                        dvx = 0,
                        dvy = 0,
                        dvz = 0,
                        facing = Runtime.Dir == "right" ? 0 : 1,
                    };
                    task.parent = this;
                    task.team = Team;
                    task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                    task.z = (float)Runtime.Z;
                    task.dir = Runtime.Dir;
                    task.dvz = 0f;
                    task.useDirectVelocity = true;
                    task.directVx = vx;
                    task.directVy = vy;
                    task.directVz = 0f;
                    task.ownerEntityIndex = enemySlot;
                    FillHitFa8SpawnTask(task);
                    factory.EnqueueCreateObject(task);

                    spawnCount++;
                    spawnedThisLoop = true;
                }

                loopCount++;
            } while (hitFa == 9 &&
                     spawnCount < maxPerLaterPass &&
                     spawnCount > 0 &&
                     spawnedThisLoop &&
                     spawnCount < max);

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa2Or4Or12Or14FrameLogic(int hitFa)
        {
            LF2Entity target = ResolveFrameLogicTargetByHitFa(hitFa);
            if (hitFa == 4 && target == null && Match != null && OwnerEntityIndex >= 0)
            {
                target = Match.FindEntityByRuntimeSlotForQuery(OwnerEntityIndex) ??
                         Match.FindEntityByRuntimeSlotIncludingPending(OwnerEntityIndex);
            }

            if (Health == null || Health.HP <= 0)
            {
                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            if (hitFa == 4 && target != null && target.Health != null && target.Health.HP > 0)
            {
                int dx = target.GetRuntimeXInt() - GetRuntimeXInt();
                int dy = target.GetRuntimeYInt() - GetRuntimeYInt();
                int dz = GetFrameLogicZInt(target) - GetFrameLogicZInt(this);
                if (dx > -30 && dx < 30 && dy > 0 && dy < 80 && dz > -10 && dz < 10)
                {
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                    SetFrameTickDirect(60);
                    target.CatchTimer = 100;
                    return;
                }
            }

            if (target == null)
            {
                if (hitFa != 4 && Health != null)
                {
                    Health.HP = 0;
                    return;
                }

                ApplyHitFa2Or4Or12Or14NoTargetCatch(hitFa);
                return;
            }

            int targetX = target.GetRuntimeXInt();
            int selfX = GetRuntimeXInt();
            int targetZ = GetFrameLogicTargetZInt(target, hitFa);
            int selfZ = GetFrameLogicTargetZInt(this, hitFa);

            if (targetX > selfX)
                Runtime.Vx += 0.7f;
            if (targetX < selfX)
                Runtime.Vx -= 0.7f;
            if (targetZ > selfZ + 5)
                Runtime.Vz += 0.4f;
            if (targetZ < selfZ - 5)
                Runtime.Vz -= 0.4f;

            Runtime.Vy *= 0.7142857142857143; // P0-f-2b B2-3a: VALUE-BUG 5f/7f→0.7142857142857143 (baseline FrameAdvance.cs Vy*=0.7142857142857143)

            if (IsCharacterFrameLogicTarget(target))
            {
                if (Runtime.Y + 40f < target.Runtime.Y)
                    Runtime.Y += 1f;
                if (Runtime.Y + 40f > target.Runtime.Y)
                    Runtime.Y -= 1f;
            }
            else if (Runtime.Y > 0f)
            {
                Runtime.Y += 1f;
            }

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            if (hitFa == 14)
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -1.5, 1.5);
            else
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();

            if (hitFa == 14)
            {
                double absVx = System.Math.Abs(Runtime.Vx);
                int curFrame = Frame?.N ?? -1;
                if (absVx >= 8f)
                {
                    if (curFrame > 40)
                        SetFrameTickDirect(curFrame - 50);
                }
                else if (curFrame < 10)
                {
                    SetFrameTickDirect(curFrame + 50);
                }
            }
        }

        private void RunHitFa7FrameLogic()
        {
            if (Match != null)
                SpawnHitFa7Clone();

            LF2Entity target = null;
            int targetSlot = Runtime.OwnerSlotIndex;
            if (Match != null && targetSlot >= 0)
                target = Match.FindEntityByRuntimeSlotForQuery(targetSlot) ??
                         Match.FindEntityByRuntimeSlotIncludingPending(targetSlot);

            bool valid = target != null && Health != null && Health.HP > 0;
            if (valid)
            {
                if (target.GetRuntimeXInt() > GetRuntimeXInt())
                {
                    Runtime.Vx += 0.7f;
                    Runtime.Vx += 0.7f;
                }
                else if (target.GetRuntimeXInt() < GetRuntimeXInt())
                {
                    Runtime.Vx -= 0.7f;
                    Runtime.Vx -= 0.7f;
                }

                int targetZ = target.GetRenderZInt();
                int selfZ = GetRenderZInt();
                if (targetZ > selfZ + 5)
                    Runtime.Vz += 0.4f;
                if (targetZ < selfZ - 5)
                    Runtime.Vz -= 0.4f;

                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -14.0, 14.0);
                if (Runtime.Y > 1.4f)
                    Runtime.Y = 1.4f;
                Runtime.Vz = System.Math.Clamp(Runtime.Vz, -2.2, 2.2);
            }
            else
            {
                if (Runtime.Vx < 0f)
                    Runtime.Vx -= 2f;
                else
                    Runtime.Vx += 2f;

                Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
                if (Runtime.Vy < 4f)
                    Runtime.Vy += 0.4f;

                Runtime.Y += Runtime.Vy;
                if (Runtime.YInt > -25)
                {
                    SetFrameTickDirect(60);
                    Runtime.YInt = -25;
                    Runtime.Vx = 0f;
                    Runtime.Vy = 0f;
                    Runtime.Vz = 0f;
                }
            }

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;
        }

        private void RunHitFa13FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            var allObjects = new List<LF2Entity>(16);
            Match?.GetAllEntities(allObjects);

            var enemies = new List<int>(8);
            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity obj = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(obj))
                    continue;
                if (!IsCharacterFrameLogicTarget(obj))
                    continue;
                if (ResolveFrameLogicRelationIdentity(obj) == selfTeam)
                    continue;

                int enemySlot = GetRuntimeSlotOrNegative(obj);
                if (enemySlot < 0)
                    continue;

                enemies.Add(enemySlot);
            }

            int freeSlot = -1;
            for (int slot = 50; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null &&
                    Match.FindEntityByRuntimeSlotIncludingPending(slot) == null)
                {
                    freeSlot = slot;
                    break;
                }
            }

            if (freeSlot < 0)
            {
                Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int spawnOid = 228;
            if (CharacterAnimtorManager.Instance?.GetCharacterData(spawnOid) == null)
            {
                Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            int chosenTarget = enemies.Count == 0
                ? GetRuntimeSlotOrNegative(this)
                : enemies[RandInt(0, enemies.Count)];

            float spawnY = (float)(Runtime.Y + RandInt(0, 7) - 3);
            OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = new ObjectPoint
            {
                oid = spawnOid,
                kind = 0,
                action = 0,
                dvx = 0,
                dvy = 0,
                dvz = 0,
                facing = Runtime.Dir == "right" ? 0 : 1,
            };
            task.parent = this;
            task.team = Team;
            task.pos = new Vector3((float)Runtime.X, spawnY, (float)Runtime.Z);
            task.z = (float)Runtime.Z;
            task.dir = Runtime.Dir;
            task.dvz = 0f;
            task.useDirectVelocity = true;
            task.directVx = (float)Runtime.Vx;
            task.directVy = 0.1f;
            task.directVz = (float)(3.0f - RandInt(0, 24) * 0.25f + Runtime.Vz);
            task.ownerEntityIndex = chosenTarget;
            FillHitFa13SpawnTask(task);
            factory.EnqueueCreateObject(task);

            Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa5FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            if (CharacterAnimtorManager.Instance?.GetCharacterConfig(219) == null)
            {
                if (Health != null)
                    Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            var allObjects = new List<LF2Entity>(16);
            Match.GetAllEntities(allObjects);

            int selfTeam = ResolveFrameLogicRelationIdentity();
            for (int i = 0; i < allObjects.Count; i++)
            {
                LF2Entity ally = allObjects[i];
                if (IsDeadLikeFrameLogicTarget(ally))
                    continue;
                if (!IsCharacterFrameLogicTarget(ally))
                    continue;
                if (ResolveFrameLogicRelationIdentity(ally) != selfTeam)
                    continue;

                int allySlot = GetRuntimeSlotOrNegative(ally);
                if (allySlot < 0)
                    continue;

                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = 219,
                    kind = 0,
                    action = 0,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = 0,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                task.z = (float)Runtime.Z;
                task.dir = "right";
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = (float)((ally.Runtime.X - Runtime.X) / 50.0f);
                task.directVy = 0f;
                task.directVz = 0f;
                task.ownerEntityIndex = allySlot;
                FillHitFa13SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void RunHitFa11FrameLogic()
        {
            if (Match == null)
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int[] spawnOids =
            {
                211, 221, 212, 212, 212, 212, 212, 212, 211, 211, 211, 211, 211, 211,
            };

            for (int i = 0; i < spawnOids.Length; i++)
            {
                if (CharacterAnimtorManager.Instance?.GetCharacterData(spawnOids[i]) == null)
                {
                    if (Health != null)
                        Health.HP = 0;
                    Runtime.PendingFlushDestroy = true;
                    return;
                }
            }

            (int oid, int frameId, float xOff, float yOff, float zOff, float vzDelta, string dir)[] spawns =
            {
                (211, 109,    0f,    0f,  0f,  0f, Runtime.Dir),
                (221,  81,    0f, -100f,  0f,  0f, Runtime.Dir),
                (212, 100,   80f,   -3f,  0f, -7f, "right"),
                (212, 100,  100f,   -3f,  0f,  0f, "right"),
                (212, 100,   80f,   -3f,  0f,  7f, "right"),
                (212, 100,  -80f,   -3f,  0f, -7f, "left"),
                (212, 100, -100f,   -3f,  0f,  0f, "left"),
                (212, 100,  -80f,   -3f,  0f,  7f, "left"),
                (211,  50,  -30f,   -1f, -5f,  0f, "left"),
                (211,  50,   30f,   -1f, -5f,  0f, "left"),
                (211,  50,  -30f,   -1f,  2f,  0f, "right"),
                (211,  50,   30f,   -1f,  2f,  0f, "right"),
                (211,  50,    0f,   -1f, -9f,  0f, "left"),
                (211,  50,    0f,   -1f,  6f,  0f, "right"),
            };

            for (int i = 0; i < spawns.Length; i++)
            {
                var spawn = spawns[i];
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint
                {
                    oid = spawn.oid,
                    kind = 0,
                    action = spawn.frameId,
                    dvx = 0,
                    dvy = 0,
                    dvz = 0,
                    facing = spawn.dir == "right" ? 0 : 1,
                };
                task.parent = this;
                task.team = Team;
                task.pos = new Vector3((float)(Runtime.X + spawn.xOff), (float)(Runtime.Y + spawn.yOff), (float)(Runtime.Z + spawn.zOff));
                task.z = (float)(Runtime.Z + spawn.zOff);
                task.dir = spawn.dir;
                task.dvz = 0f;
                task.useDirectVelocity = true;
                task.directVx = (float)Runtime.Vx;
                task.directVy = (float)Runtime.Vy;
                task.directVz = (float)(Runtime.Vz + spawn.vzDelta);
                FillHitFa13SpawnTask(task);
                factory.EnqueueCreateObject(task);
            }

            ResolveFrameLogicTargetByHitFa(11);

            if (OwnerEntityIndex < 0)
            {
                if (Health != null)
                    Health.HP = 0;
                Runtime.PendingFlushDestroy = true;
                return;
            }

            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            if (Health != null)
                Health.HP = 0;
            Runtime.PendingFlushDestroy = true;
        }

        private void SpawnHitFa7Clone()
        {
            if (Match == null || FrameCache?.Wrapper?.characterData == null)
                return;

            int freeSlot = -1;
            for (int slot = Match.DynamicRuntimeSlotStartForServices; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                if (Match.FindEntityByRuntimeSlotForQuery(slot) == null &&
                    Match.FindEntityByRuntimeSlotIncludingPending(slot) == null)
                {
                    freeSlot = slot;
                    break;
                }
            }

            if (freeSlot < 0)
                return;

            var clone = new LF2Character();
            clone.ModuleInitialize();
            clone.Name = $"{Name}_HitFa7Clone";
            clone.ObjectId = ObjectId;
            clone.Controller = new CharacterInputModule();
            clone.FrameCache.Load(FrameCache.Wrapper);
            clone.Frame.D = clone.FrameCache.GetFrameDataById(0);
            clone.Frame.PN = 0;
            clone.Frame.N = 0;
            clone.Initialize(500, 500);
            clone.FrameDelay = 0;
            clone.Team = Team;
            clone.RelationTeam = RelationTeam;
            clone.HolderCopySlot = HolderCopySlot;
            clone.SpawnerEntityIndex = -1;
            clone.Runtime.SetPosition(Runtime.X, Runtime.Y, Runtime.Z);
            clone.Runtime.SetVelocity(0f, 0f, 0f);
            clone.Runtime.SyncIntegerPosition();
            clone.ImmediateFrame(40);
            clone.SwitchDir("right");
            clone.SetRuntimeSlotIndex(freeSlot);
            clone.RefreshRuntimeSnapshot();
            Match.Register(clone);
        }

        private void FillHitFa13SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private void FillHitFa8SpawnTask(OPointCreateTask task)
        {
            if (task == null)
                return;

            task.parent = this;
            task.releaseOpointSpawn = true;
            task.spawnerEntityIndex = -1;
            task.useExplicitRelationIdentity = true;
            task.relationTeam = ResolveFrameLogicRelationIdentity();
            task.holderCopySlot = HolderCopySlot;
            task.skipPostInitZOffset = true;
            task.useInitialRuntimeIntPosition = true;
            task.initialRuntimeX = (int)task.pos.x;
            task.initialRuntimeY = (int)task.pos.y;
            task.initialRuntimeZ = (int)task.pos.z;
            task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
        }

        private LF2Entity ResolveFrameLogicTargetByHitFa(int hitFa)
        {
            if (Match == null)
                return null;

            if (hitFa == 4)
            {
                return OwnerEntityIndex >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(OwnerEntityIndex) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(OwnerEntityIndex)
                    : null;
            }

            int selfTeam = ResolveFrameLogicRelationIdentity();
            int holderTeam = -1;
            if (SpawnerEntityIndex >= 0)
            {
                LF2Entity spawner = Match.FindEntityByRuntimeSlotForQuery(SpawnerEntityIndex) ??
                                    Match.FindEntityByRuntimeSlotIncludingPending(SpawnerEntityIndex);
                if (spawner != null)
                    holderTeam = ResolveFrameLogicRelationIdentity(spawner);
            }

            int currentTargetSlot = OwnerEntityIndex;
            bool needScan = true;
            LF2Entity target = currentTargetSlot >= 0
                ? Match.FindEntityByRuntimeSlotForQuery(currentTargetSlot) ??
                  Match.FindEntityByRuntimeSlotIncludingPending(currentTargetSlot)
                : null;

            if (target != null)
            {
                bool valid = !IsDeadLikeFrameLogicTarget(target) &&
                             IsCharacterFrameLogicTarget(target) &&
                             target.GetState() != LF2States.Lying &&
                             Mathf.Abs(target.HitStun) <= 2f &&
                             ResolveFrameLogicRelationIdentity(target) != selfTeam;
                if (valid && holderTeam != ResolveFrameLogicRelationIdentity(target))
                    needScan = false;
                if (!valid)
                    target = null;
            }

            if (needScan)
            {
                var allObjects = new List<LF2Entity>(16);
                Match.GetAllEntities(allObjects);

                int bestDist = 10000;
                int bestSlot = -1;
                for (int i = 0; i < allObjects.Count; i++)
                {
                    LF2Entity obj = allObjects[i];
                    if (obj == null || ReferenceEquals(obj, this))
                        continue;
                    if (IsDeadLikeFrameLogicTarget(obj))
                        continue;
                    if (!IsCharacterFrameLogicTarget(obj))
                        continue;

                    int objTeam = ResolveFrameLogicRelationIdentity(obj);
                    if (objTeam == selfTeam)
                        continue;
                    if (holderTeam >= 0 && objTeam == holderTeam)
                        continue;
                    if ((obj.GetState() == LF2States.Lying || Mathf.Abs(obj.HitStun) > 2f) && currentTargetSlot != -1)
                        continue;

                    int dist = Mathf.Abs(obj.GetRuntimeXInt() - GetRuntimeXInt()) +
                               Mathf.Abs(GetFrameLogicTargetZInt(obj, hitFa) - GetFrameLogicTargetZInt(this, hitFa));
                    if (dist < bestDist)
                    {
                        bestDist = dist;
                        bestSlot = GetRuntimeSlotOrNegative(obj);
                    }
                }

                OwnerEntityIndex = bestSlot;
                target = bestSlot >= 0
                    ? Match.FindEntityByRuntimeSlotForQuery(bestSlot) ??
                      Match.FindEntityByRuntimeSlotIncludingPending(bestSlot)
                    : null;
            }

            return target;
        }

        private int ResolveFrameLogicRelationIdentity()
        {
            return ResolveFrameLogicRelationIdentity(this);
        }

        private static int ResolveFrameLogicRelationIdentity(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            return entity.RelationTeam != 0 ? entity.RelationTeam : entity.Team;
        }

        private static bool IsCharacterFrameLogicTarget(LF2Entity entity)
        {
            return entity?.GetCurrentDataObjectType() == (int)LF2ObjectType.Character;
        }

        private static bool IsDeadLikeFrameLogicTarget(LF2Entity entity)
        {
            if (entity == null)
                return true;
            if (entity is LF2LivingObject living && living.Dead)
                return true;

            return entity.Health == null || entity.Health.HP <= 0;
        }

        private static int GetRuntimeSlotOrNegative(LF2Entity entity)
        {
            return entity?.Runtime?.SlotIndex ?? -1;
        }

        private void ApplyHitFa2Or4Or12Or14NoTargetCatch(int hitFa)
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            if (Runtime.Y > 1.4f)
                Runtime.Y = 1.4f;

            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
            Runtime.YInt = (int)Runtime.Y;

            if (hitFa == 2)
                ApplyHitFa2FrameSelection();
        }

        private void ApplyHitFa3NoTargetDrift()
        {
            if (Runtime.Vx < 0f)
                Runtime.Vx -= 2f;
            else
                Runtime.Vx += 2f;

            Runtime.Vx = System.Math.Clamp(Runtime.Vx, -17.0, 17.0);
            SwitchDir(Runtime.Vx > 0f ? "right" : "left");
        }

        private void ApplyHitFa2FrameSelection()
        {
            double absVx = System.Math.Abs(Runtime.Vx);
            int curFrame = Frame?.N ?? -1;
            if (absVx > 14f)
            {
                if (curFrame != 5 && curFrame != 6)
                    SetFrameTickDirect(5);
            }
            else if (absVx > 7f)
            {
                if (curFrame != 3 && curFrame != 4)
                    SetFrameTickDirect(3);
            }
            else
            {
                if (curFrame != 1 && curFrame != 2)
                    SetFrameTickDirect(1);
            }
        }

        private static int GetFrameLogicZInt(LF2Entity entity)
        {
            if (entity == null)
                return 0;

            if (entity.GetCurrentDataObjectType() == (int)LF2ObjectType.SpecialAttack &&
                entity.Runtime != null &&
                System.Math.Abs(entity.Runtime.Type3VisualZOffset) > 0.0001)
            {
                return (int)(entity.Runtime.Z - entity.Runtime.Type3VisualZOffset);
            }

            return entity.GetRenderZInt();
        }

        private static int GetFrameLogicTargetZInt(LF2Entity entity, int hitFa)
        {
            if (hitFa == 12 || hitFa == 14)
                return entity?.GetRenderZInt() ?? 0;

            return GetFrameLogicZInt(entity);
        }

        protected override void ApplyCommonCaughtExitHitStop(int previousFrameId)
        {
            base.ApplyCommonCaughtExitHitStop(previousFrameId);
        }

        protected override bool IsFrameTickLeftPressed()
        {
            return IsCurrentLeftPressedInternal();
        }

        protected override bool IsFrameTickRightPressed()
        {
            return IsCurrentRightPressedInternal();
        }

        protected override bool IsFrameTickUpPressed()
        {
            return IsCurrentUpPressedInternal();
        }

        protected override bool IsFrameTickDownPressed()
        {
            return IsCurrentDownPressedInternal();
        }

        /// <summary>
        /// 帧推进前的角色专属补丁。
        /// 当前主要处理“站立 state 但角色已经在空中”时，强制切回 212 空中跳跃帧。
        /// </summary>
        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            if ((Frame?.D?.state ?? -1) == 0 && GetRuntimeYInt() < 0)
                // BMD-023-extended: standing-state-but-below-ground branch must mirror
                // baseline FrameTick.cs:67-76 (SetFrameImmediate(entity, 212)), which writes
                // Frame + FrameWaitCounter but does NOT touch Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                // DirectWriteFramePreserveWaitCounter routes through SetFrameTickDirect,
                // preserving AttackingCounter while mirroring baseline parity.
                DirectWriteFramePreserveWaitCounter(212);

            if ((Frame?.D?.state ?? -1) == LF2States.Lying &&
                Health != null &&
                Health.HP <= 0)
            {
                bool allowHitStopArm =
                    KillCount >= 0 ||
                    ResolveRespawnRelationIdentity() == 5 ||
                    (Runtime != null && Runtime.SlotIndex >= 20);
                if (allowHitStopArm && HitStun <= 0)
                    HitStun = 30;

                AttackingCounter = 0;
            }

            return true;
        }

        private int ResolveRespawnRelationIdentity()
        {
            return RelationTeam != 0 ? RelationTeam : Team;
        }

        /// <summary>
        /// wait/next 真正推进完成后的角色补充处理。
        /// 目前最关键的是 212 帧起跳初始化。
        /// </summary>
        public override void OnFrameTickAfterWaitAdvance(int previousFrame, bool allowJumpInit)
        {
            if (allowJumpInit && (Frame?.N ?? -1) == 212)
                ApplyFrame212JumpInit();

            base.OnFrameTickAfterWaitAdvance(previousFrame, allowJumpInit);
        }

        // 角色跳跃起始帧 212 的附加速度初始化在这里完成。
        protected override void ApplyFrame212JumpInit()
        {
            if ((Frame?.N ?? -1) != 212)
                return;

            var characterData = _FrameDataWrapper?.characterData;
            if (characterData == null)
                return;

            Runtime.Vy = characterData.jump_height;

            bool right = IsFrameTickRightPressed();
            bool left = IsFrameTickLeftPressed();
            bool up = IsFrameTickUpPressed();
            bool down = IsFrameTickDownPressed();

            if (right && !left)
                Runtime.Vx = characterData.jump_distance;
            else if (left && !right)
                Runtime.Vx = -characterData.jump_distance;

            if (up && !down)
                Runtime.Vz = -characterData.jump_distancez;
            else if (down && !up)
                Runtime.Vz = characterData.jump_distancez;
        }

        // 角色的 next=999：空中回到 212，地面回到 0。
        public override int ResolveFrameTickNext999Target(out bool allowJumpInit)
        {
            allowJumpInit = false;
            return GetRuntimeYInt() != 0 ? 212 : 0;
        }

        /// <summary>
        /// 角色晚阶段清理入口。
        /// 用来放那些不属于普通 frame/TU，但又必须在本 tick 尾部处理的角色特例。
        /// </summary>
        /// <summary>
        /// state 9996 的 pre-collision 特例。
        /// 对齐正式 C++ release：这条分支属于 `run_state_special_pre_collision`，
        /// 触发条件是当前 state=9996 且 `attacking == 1`，不是看朝向。
        /// </summary>
        private void RunState9996SpecialPreCollision()
        {
            if (!ShouldRunState9996SpecialPreCollision())
                return;

            LF2ObjectPointFactory factory = LF2ObjectPointFactory.Instance;
            if (factory == null)
                return;

            int baseX = GetRuntimeXInt();
            int baseY = GetRuntimeYInt();
            int baseZ = GetRenderZInt();

            for (int i = 0; i < 5; i++)
            {
                int spawnX = baseX + RandInt(0, 7) - 3;
                int spawnY = baseY + RandInt(0, 7) - 9;
                int spawnZ = baseZ + 1;
                int spawnOid = i == 4 ? 218 : 217;
                OPointCreateTask task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
                task.opoint = new ObjectPoint { oid = spawnOid, kind = 0, action = RandInt(0, 4), facing = RandInt(0, 2) };
                task.parent = null;
                task.team = Team;
                task.pos = new Vector3(spawnX, spawnY, spawnZ);
                task.z = spawnZ;
                task.dir = task.opoint.facing == 0 ? "right" : "left";
                task.useDirectVelocity = true;
                task.directVx = 0f;
                task.directVy = -(RandInt(0, 15) / 2f) - 5f;
                task.directVz = 0f;
                task.spawnerEntityIndex = Runtime?.SlotIndex ?? -1;
                task.attackExempt = 6;
                task.releaseSpawnSemantic = ReleaseSpawnSemantic.ImmediateEffect;
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                task.initialRuntimeHoldMode = InitialRuntimeIntPositionHoldMode.UntilCurrentTickTu;
                task.deferPresentationToNextTick = false;
                task.suppressLateFrameTickThisTick = false;
                task.deferFrameTickToNextTick = false;

                if (i == 1 || i == 3)
                    task.directVz = -3f - RandInt(0, 2);
                else if (i == 4)
                    task.directVz = 1f;
                else
                    task.directVz = RandInt(0, 2) + 3f;

                if (i >= 4)
                    task.directVx = RandInt(0, 7) - 3f;
                else if (i >= 2)
                    task.directVx = RandInt(0, 3) + 10f;
                else
                    task.directVx = -10f - RandInt(0, 3);

                factory.CreateObjectImmediate(task);
            }
        }

        internal bool ShouldRunState9996SpecialPreCollision()
        {
            LF2FrameData frame = Frame?.D;
            return frame != null && frame.state == 9996 && AttackingCounter == 1;
        }

        internal override void RunLateDeathOpointPreCleanupPhase()
        {
            if (Frame?.D?.state != LF2States.Lying)
                return;
            if (Health == null || Health.HP > 0)
                return;

            ForceDropHeldWeaponForLateDeath();

            int frameId = Frame.N;
            if (frameId < 12 || frameId == 110 || frameId == 111)
                EnterLateDeathLaunchFrame();

            if (GetRuntimeYInt() == 0 &&
                Runtime.Y == 0f &&
                Runtime.Vy == 0f &&
                KnockbackVy == 0f)
            {
                int currentFrame = Frame.N;
                bool groundDeathFrame = (currentFrame >= 180 && currentFrame <= 189 && currentFrame != 184) ||
                                        (currentFrame >= 212 && currentFrame <= 214);
                if (groundDeathFrame)
                    EnterLateDeathLaunchFrame();
            }
        }

        private void EnterLateDeathLaunchFrame()
        {
            ImmediateFrame(186);
            Runtime.Vy = -3f;
            KnockbackVy = -3f;
            Runtime.Y = -1f;
            Runtime.YInt = -1;
        }

        private void ForceDropHeldWeaponForLateDeath()
        {
            LF2WeaponBase weapon = GetHeldWeaponBaseInternal();
            if (weapon == null)
                return;

            ForceReleaseHeldObjectReference(weapon);
        }

        public int caught_cpointkind()
        {
            var cpoint = CurrentFrame?.cpoint;
            return cpoint?.kind ?? 0;
        }

        public bool caught_cpointhurtable()
        {
            var cpoint = CurrentFrame?.cpoint;
            if (cpoint == null)
                return true;

            return cpoint.hurtable != 0;
        }

        private bool State_Catching(string eventType, object eventData)
        {
            return _catchResolver.StateCatching(eventType, eventData);
        }

        private bool State_BeingCaught(string eventType, object eventData)
        {
            return _catchResolver.StateBeingCaught(eventType, eventData);
        }

        protected override void RunCpointActionSelectionStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            _catchResolver.RunCpointActionSelectionStep10(cpoint, victimEntity);
        }

        protected override void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity)
        {
            _catchResolver.ApplyCpointThrowStep10(cpoint, victimEntity);
        }

        protected override void ApplyCpointThrowStep10(CatchPoint cpoint, LF2Entity victimEntity, LF2FrameData throwFrameSnapshot)
        {
            _catchResolver.ApplyCpointThrowStep10(cpoint, victimEntity, throwFrameSnapshot);
        }

        protected override void SetVictimThrowVzStep10(CatchPoint cpoint, LF2Entity victim)
        {
            _catchResolver.SetVictimThrowVzStep10(cpoint, victim);
        }

        protected override void ApplyCpointDirControlStep10(CatchPoint cpoint)
        {
            _catchResolver.ApplyCpointDirControlStep10(cpoint);
        }

        protected override void ApplyCpointHeldInjuryStep10(LF2Entity victimEntity, int injury)
        {
            _catchResolver.ApplyCpointHeldInjuryStep10(victimEntity, injury);
        }

        protected override void SyncCpointHeldPositionStep10(LF2Entity victimEntity, LF2FrameData catcherFrame, CatchPoint catcherCpoint)
        {
            _catchResolver.SyncCpointHeldPositionStep10(victimEntity, catcherFrame, catcherCpoint);
        }

        private bool State_Standing(string eventType, object eventData)
        {
            return _stateResolver.StateStanding(eventType, eventData);
        }

        private bool State_Walking(string eventType, object eventData)
        {
            return _stateResolver.StateWalking(eventType, eventData);
        }

        private bool State_Running(string eventType, object eventData)
        {
            return _stateResolver.StateRunning(eventType, eventData);
        }

        private bool State_Jump(string eventType, object eventData)
        {
            return _stateResolver.StateJump(eventType, eventData);
        }

        private bool State_Dash(string eventType, object eventData)
        {
            return _stateResolver.StateDash(eventType, eventData);
        }

        private bool State_StopRunning(string eventType, object eventData)
        {
            return _stateResolver.StateStopRunning(eventType, eventData);
        }

        private bool RunTUPhase()
        {
            ApplyLegacySpecialStateVzInput();
            UpdateLegacyDeathBlinkLifecycle();
            RecoverLegacyHitCounters();

            return false;
        }

        private void ApplyLegacySpecialStateVzInput()
        {
            int curStateForVz = Frame.D?.state ?? -1;
            if (curStateForVz != LF2States.DeepSpecific && curStateForVz != LF2States.FirenSpecific)
                return;

            bool isLeft = IsCurrentLeftPressedInternal();
            bool isRight = IsCurrentRightPressedInternal();
            float dvz = Frame.D?.dvz ?? 0f;
            if (dvz == 0f || (int)Runtime.Z != 0)
                return;

            if (isLeft && !isRight)
                Runtime.Vz = -dvz;
            else if (isRight && !isLeft)
                Runtime.Vz = dvz;
        }

        private void UpdateLegacyDeathBlinkLifecycle()
        {
            if (_deadBlinkCount == 0)
            {
                Effect.Blink = true;
                _deadBlinkCount = 1;
                return;
            }

            if (_deadBlinkCount > 0 && _deadBlinkCount < 30)
            {
                _deadBlinkCount++;
                return;
            }

            if (_deadBlinkCount < 30)
                return;

            Effect.Blink = false;
            Sprite?.Hide();
            _deadBlinkCount = -1;
            Match?.Unregister(this);
        }

        private void RecoverLegacyHitCounters()
        {
            HitCounters.RecoverFall(NTSDGlobal.Gameplay.RecoverFall);
            HitCounters.RecoverBdefend(NTSDGlobal.Gameplay.RecoverBdefend);
        }

        public void RegeneratePreCollisionStats(int tickIndex)
        {
            if (Health == null)
                return;

            BattleFlowRuntimeState flow = Match?.Runtime?.Flow;
            bool stepWaitGate = flow != null && flow.BattleStepMode == 1 && flow.BattleStepGate != 1;

            bool period12 = tickIndex % NTSDGlobal.Gameplay.HpRecoverPeriod == 0;
            if (Health.HP > 0 && Health.HP < Health.HPBound && period12 && !stepWaitGate)
                Health.HP++;

            if (WeaponCount < 0 && period12 && !stepWaitGate)
            {
                int injury = NTSDGlobal.Gameplay.NegativeWeaponCountInjury;
                if (FallDamageDiv > 0)
                    injury = NTSDGlobal.Gameplay.NegativeWeaponCountScaledInjury / FallDamageDiv;

                Health.HP -= injury;
                Health.HPBound -= injury / NTSDGlobal.Gameplay.NegativeWeaponCountHpBoundDivisor;
                if (Health.HP < 0)
                    Health.HP = 0;
                if (Health.HPBound < 0)
                    Health.HPBound = 0;
                ComboCountVic += 9;
            }

            bool period3 = tickIndex % NTSDGlobal.Gameplay.PpRecoverPeriod == 0;
            if (!period3)
                return;
            if (Health.PP >= NTSDGlobal.Gameplay.PpRecoverCap)
                return;
            if (KillCount != -1 && Health.PP >= NTSDGlobal.Gameplay.PpRecoverLowLimit)
                return;
            if (HitStun < 0)
                return;
            if (stepWaitGate)
                return;

            int hpForRate = Health.HP;
            if (hpForRate > NTSDGlobal.Gameplay.PpRecoverCap)
                hpForRate = NTSDGlobal.Gameplay.PpRecoverCap;

            int oid = ObjectId;
            if (oid == 51 || oid == 52)
                hpForRate /= 2;

            int ppGain = ((NTSDGlobal.Gameplay.PpRecoverCap - hpForRate) /
                          NTSDGlobal.Gameplay.PpRecoverHpRateDivisor) + 1;
            Health.PP = Math.Min(Health.PP + ppGain, NTSDGlobal.Gameplay.PpRecoverCap);
        }

        internal override bool SupportsPostInteractionPhase() => true;
        internal override bool IsStageBoundedCharacter() => true;
        internal override bool ShouldContributeToReleaseCamera() => Health != null && Health.HP > 0;

        internal override void ApplyPreFrameZBounds(float zMin, float zMax)
        {
            if (Runtime.Z < zMin)
                Runtime.Z = zMin;
            if (Runtime.Z > zMax)
                Runtime.Z = zMax;
        }

        internal override void RunPreCollisionRecoveryPhase(int tickIndex)
        {
            RegeneratePreCollisionStats(tickIndex);
        }

        private bool RunTransitPhase()
        {
            if (FrameDelay != 0)
                return false;

            ApplyDynamics();
            return false;
        }

        private bool RunFramePhase()
        {
            return false;
        }

        private bool RunStateExitPhase()
        {
            OnLocalInputStateExit();
            return false;
        }

        public string CaughtA(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos)
        {
            return TryCaughtAInternal(itr, attacker, attackerPos, out string catchSide)
                ? catchSide
                : null;
        }

        internal bool TryCaughtAInternal(InteractionArea itr, LF2LivingObject attacker, Vector3 attackerPos, out string catchSide)
        {
            return _interactionResolver.TryCaughtA(itr, attacker, attackerPos, out catchSide);
        }

        /// <summary>
        /// 角色在交互结算后的收尾入口。
        /// 这里主要消费 step7 阶段累计下来的抓取/交互候选。
        /// </summary>
        public override void SimPostInteraction(int tickIndex)
        {
            _interactionResolver.TryConsumeUnifiedStep7CandidateSequence();
        }

        /// <summary>
        /// 跑动受击/翻滚类状态转发到 DamageStateResolver。
        /// LF2Character 本身只保留路由职责，具体规则集中在 resolver 里。
        /// </summary>
        private bool State_Rowing(string eventType, object eventData)
        {
            return _damageStateResolver.StateRowing(eventType, eventData);
        }

        /// <summary>
        /// 轻伤状态转发入口。
        /// </summary>
        private bool State_Injured(string eventType, object eventData)
        {
            return _damageStateResolver.StateInjured(eventType, eventData);
        }

        /// <summary>
        /// 角色落地入口。
        /// 真正的落地分流在 DamageStateResolver 中处理，这里只负责把事件交过去。
        /// </summary>
        private void HandleLandingEvent(double vyBeforeLand) // P0-f-2b B2-1: float→double
        {
            _damageStateResolver.HandleLandingEvent(vyBeforeLand);
        }

        /// <summary>
        /// 摔落状态转发入口。
        /// </summary>
        private bool State_Falling(string eventType, object eventData)
        {
            return _damageStateResolver.StateFalling(eventType, eventData);
        }

        /// <summary>
        /// 冻结状态转发入口。
        /// </summary>
        private bool State_Frozen(string eventType, object eventData)
        {
            return _damageStateResolver.StateFrozen(eventType, eventData);
        }

        /// <summary>
        /// 躺地状态转发入口。
        /// </summary>
        private bool State_Lying(string eventType, object eventData)
        {
            return _damageStateResolver.StateLying(eventType, eventData);
        }

        /// <summary>
        /// 燃烧状态转发入口。
        /// </summary>
        private bool State_Burning(string eventType, object eventData)
        {
            return _damageStateResolver.StateBurning(eventType, eventData);
        }

        internal void ClearConsumedHeldWeaponReference(LF2Entity held)
        {
            _weaponLinkResolver.ClearConsumedHeldWeaponReference(held);
        }

        internal void ClearReleasedHeldWeaponReferenceInternal(LF2Entity held)
        {
            _weaponLinkResolver.ClearReleasedHeldWeaponReference(held);
        }

        private static bool IsHeavyWeaponAttacker(LF2Entity attacker)
        {
            return attacker is LF2WeaponBase weapon && weapon.WeaponType == 2;
        }

        public void HoldWeapon(ILF2Object weapon)
        {
            _weaponLinkResolver.HoldWeapon(weapon);
        }

        public void AttachOpointHeldObject(LF2Entity held)
        {
            _weaponLinkResolver.AttachOpointHeldObject(held);
        }

        public ILF2Object GetHeldWeapon()
        {
            return _weaponLinkResolver.GetHeldWeapon();
        }

        public override void RunWeaponSyncHeldStep10()
        {
            base.RunWeaponSyncHeldStep10();
            _weaponLinkResolver.RunWeaponSyncHeldStep10();
        }

        public bool ReleaseHeldObjectByWPoint(WeaponPoint holderWPoint, out WeaponActResult result)
        {
            return _weaponLinkResolver.ReleaseHeldObjectByWPoint(holderWPoint, out result);
        }

        public bool ReleaseHeldObjectByWPoint(LF2Entity held, WeaponPoint holderWPoint, out WeaponActResult result)
        {
            return _weaponLinkResolver.ReleaseHeldObjectByWPoint(held, holderWPoint, out result);
        }

        internal bool TryDropHeldWeaponFallbackRandomly()
        {
            return _weaponLinkResolver.TryDropHeldWeaponFallbackRandomly();
        }

        protected override void RefreshRuntimeFromEntity()
        {
            base.RefreshRuntimeFromEntity();
            Runtime.Blink = _deadBlinkCount;
        }

    }
}
