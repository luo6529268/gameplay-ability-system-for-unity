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
    /// 战斗行为以 C# authority 的实体、帧和输入模型为准；
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
            => base.SupportsFrameLogicBeforeAdvancePhase(frame);

        public override void RunFrameLogicBeforeAdvance()
            => base.RunFrameLogicBeforeAdvance();

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
            if (ShouldResolveCharacterLanding(stepResult))
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
            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                return LF2CharacterDatHitResolver.TryResolveHit(this, itr, attacker, attackerPos, vol);

            return base.Hit(itr, attacker, attackerPos, vol);
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
            return Runtime.KeyLeft != 0;
        }

        internal bool IsCurrentRightPressedInternal()
        {
            return Runtime.KeyRight != 0;
        }

        internal bool IsCurrentUpPressedInternal()
        {
            return Runtime.KeyUp != 0;
        }

        internal bool IsCurrentDownPressedInternal()
        {
            return Runtime.KeyDown != 0;
        }

        internal bool IsCurrentAttackPressedInternal()
        {
            return Runtime.KeyAttack != 0;
        }

        internal bool IsCurrentJumpPressedInternal()
        {
            return Runtime.KeyJump != 0;
        }

        internal bool IsCurrentDefendPressedInternal()
        {
            return Runtime.KeyDefend != 0;
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
        }

        internal void SetInputFrameDirectInternal(int frameId)
        {
            DirectWriteFramePreserveWaitCounter(frameId);
        }

        internal void UpdateLocalInputStateFromControllerBuffer(int tickIndex)
        {
            // Local callbacks enqueue edges only. Keep the held-state mirror across ticks;
            // frame advance clears Runtime keys after input has been applied, as in authority.
            // Cooldowns and combo progress may still be changed by later runtime passes.
            InputState?.SyncProgressFromRuntime(Runtime);
            InputState?.PollFromBuffer(Controller?.InputBuffer, tickIndex, this);
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

        /// <summary>
        /// 角色在 battle tick 中正式消费输入缓冲的入口。
        /// 顺序是先读取当前帧按键，再执行组合键与单键跳帧判定。
        /// </summary>
        internal override void RunHumanInputPollPhase(int tickIndex)
        {
            if (Runtime == null || AiControlled)
                return;

            // 第一步：把这一帧按键事件整理成“按住状态 + 新按下边沿 + 输入历史”。
            UpdateLocalInputStateFromControllerBuffer(tickIndex);
        }

        internal override void RunCharacterInputPhase(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;
            if (GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            RunCharacterInputPhaseForKnownCharacterDat(tickIndex);
        }

        internal override void RunCharacterInputPhaseForKnownCharacterDat(int tickIndex)
        {
            if (Runtime == null || Runtime.LinkState < 0)
                return;

            if (AiControlled)
            {
                BattleAiInputDetailDiagnostics diagnostics =
                    Match?.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
                diagnostics?.RecordAi();
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                try
                {
                    Match?.PrepareAiInputBasic(this, tickIndex);
                }
                finally
                {
                    diagnostics?.EndPhase(BattleAiInputDetailPhase.RemainingAiDecision);
                }
                diagnostics?.BeginPhase(BattleAiInputDetailPhase.InputStateSyncFromRuntime);
                InputState?.SyncFromRuntime(Runtime);
                diagnostics?.EndPhase(BattleAiInputDetailPhase.InputStateSyncFromRuntime);
            }

            BattleAiInputDetailDiagnostics comboDiagnostics =
                Match?.ActiveBattleAiInputDetailDiagnosticsForDiagnostics;
            comboDiagnostics?.BeginPhase(BattleAiInputDetailPhase.ComboUpdate);
            ComboUpdate();
            comboDiagnostics?.EndPhase(BattleAiInputDetailPhase.ComboUpdate);
            ApplyNonCharacterFrameVelocityForFrameAdvance();
        }

        internal override void ClearBattleEntryInputState()
        {
            base.ClearBattleEntryInputState();
            ResetLocalInputState();
        }

        internal override void RunPostCooldownInputPhase(int tickIndex)
        {
            if (!AiControlled)
                RunHumanInputPollPhase(tickIndex);
            RunCharacterInputPhase(tickIndex);
        }

        internal override void RunEarlyTeleportSpecialsPhase(List<LF2Entity> entities, bool frameToggleGate)
        {
            base.RunEarlyTeleportSpecialsPhase(entities, frameToggleGate);
        }

        private void InitializeFromOpoint(OPointCreateTask task)
        {
            ObjectId = task.opoint.oid;
            Runtime.SpawnSemantic = (int)task.releaseSpawnSemantic;
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

            ApplyInitialRuntimePosition(task);
            SetOpointVelocity(task);

            FrameDelay = task.frameDelay;
            AttackExempt = task.attackExempt;

            AiControlled = true;
            // opoint 生成的角色不能绑定玩家输入，但必须拥有独立的逻辑输入缓冲，
            // 否则 release 风格生成的 AI 角色会被标记为 AiControlled 却无法接收帧输入。
            Controller = new CharacterInputModule();
            _preserveOpointActionZero = task.preserveActionZero;
            _initializedFromOpoint = true;
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
            ResetPooledEntityState();
            ResetLocalInputState();
            _hitCounters?.Reset();
            ItrRest?.Reset();
            Runtime.Reset();
            FrameCache.Clear();
            Frame.N = 0;
            Frame.D = null;
            Frame.PN = 0;
            Frame.Prev = 0;
            Frame.Prev2 = 0;
            Frame.Prev2D = null;
            _heldWeapon = null;
            Catching = null;

            AttackingCounter = 0;
            FrameDelay = 0;
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
            base.UnregisterFromWorld();
        }

        public override void OnTransitDestroy()
        {
            DestroyEvent();

            if (Renderer != null)
            {
                LF2ObjectRenderer renderer = Renderer;
                Renderer = null;
                if (LF2ObjectPool.Instance != null)
                    LF2ObjectPool.Instance.Release(renderer);
                else
                    renderer.ResetState();
            }
            else
            {
                UnregisterFromWorld();
                Destroy();
            }

            LF2ReferencePool.Instance?.Release(this);
        }

        public override void DirectWriteFramePreserveWaitCounter(int frameId)
        {
            base.DirectWriteFramePreserveWaitCounter(frameId);
            if (Runtime != null)
                Runtime.FrameWaitCounter = 0;
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
                HolderCopySlot = 99;
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
            WeaponCount = 0;
            FallDamageDiv = 0;
            TrackerFlag = 0;
            TrackerParent = null;
            HolderCopySlot = 99;
            OwnerId = -1;
            RelationOwnerSlot = -1;
            OwnerEntityIndex = -1;
            SpawnerEntityIndex = -1;
        }

        public override void SimTransit(int tickIndex)
        {
            if (GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                RunReleaseFrameAdvance(tickIndex);
            else
                RunSharedNonCharacterDatFrameAdvance();
        }

        private bool RunReleaseFrameAdvance(int tickIndex)
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

        internal override void RunLateDeathOpointPreCleanupPhase()
        {
            base.RunLateDeathOpointPreCleanupPhase();
        }

        internal void ForceDropHeldWeaponForLateDeathInternal()
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
            victimEntity?.Runtime.SyncIntegerPosition();
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
            => base.RunPreCollisionRecoveryPhase(tickIndex);

        internal override bool IsStageBoundedCharacter() => base.IsStageBoundedCharacter();
        internal override bool ShouldContributeToReleaseCamera() => Health != null && Health.HP > 0;

        internal override void ApplyPreFrameZBounds(float zMin, float zMax)
        {
            base.ApplyPreFrameZBounds(zMin, zMax);
        }

        internal override void RunPreCollisionRecoveryPhase(int tickIndex)
        {
            base.RunPreCollisionRecoveryPhase(tickIndex);
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
            if (!UsesCharacterDatInteractionPhase())
                return;

            _interactionResolver.TryConsumeUnifiedStep7CandidateSequence();
        }

        public override void SimObjectInteraction(int tickIndex)
        {
            if (UsesCharacterDatInteractionPhase())
                return;

            // The C# authority uses the same candidate consumer in loop1/loop2 and
            // selects the loop only from the current DAT obj_type. A transformed
            // character shell therefore keeps the unified consumer but runs it in
            // the non-character object pass.
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
        }

    }
}
