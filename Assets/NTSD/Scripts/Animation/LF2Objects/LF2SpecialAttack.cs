using UnityEngine;
using NTSD.App;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Extensions;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 特殊攻击对象（投射物、能量球、技能生成物等）。
    /// 复刻基准是 C++ release 工程中的 Entity_FrameLogic、Entity_AI_Update 和 opoint 生成流程；
    /// Unity 这里只保留对象池、渲染引用和分层调度适配。
    /// </summary>
    public class LF2SpecialAttack : LF2Entity
    {
        // ========== 技能专属字段（不在 LF2Entity 的） ==========

        public override LF2ItrRestTracker ItrRest { get; protected set; }

        /// <summary>生命值（技能耐久/存活帧数等）</summary>
        public override LF2Health Health { get; protected set; } = new LF2Health();
        // ========== 配置字段 ==========
        private LF2LivingObject _parent;
        private int _lastState = -1;

        // ========== 状态机字段 ==========
        public bool NoBounce { get; set; }



        // ========== ILF2Object 实现 ==========
        public override LF2ObjectType ObjectTypeEnum => LF2ObjectType.SpecialAttack;
        internal override bool UsesDynamicRuntimeSlot() => true;

        public override void RunFrameLogicBeforeAdvance()
            => base.RunFrameLogicBeforeAdvance();

        public override void Init(LF2TaskBase taskBase, LF2ObjectRenderer renderer)
        {
            AllocateStableId();

            PS = new PhysicsState();
            PS.BindRuntime(Runtime);
            Health.BindRuntime(Runtime);
            Trans = new FrameTransistor(this);
            Frame = new LF2FrameInfo();
            Effect = new LF2EffectState();
            ItrRest = new LF2ItrRestTracker();
            Sprite = new LF2Sprite();

            if (!(taskBase is OPointCreateTask task))
            {
                Log.Error("[LF2SpecialAttack] Invalid task type");
                return;
            }

            Runtime.SpawnSemantic = (int)task.releaseSpawnSemantic;

            InitializeParent(task);
            InitializeDirection(task);
            InitializeFrame(task);
            InitializePosition(task);
            InitializeVelocity(task);
            InitializeHealth();

            Renderer = renderer;
            SimulationTickDriver.Instance?.World?.Register(this);
        }

        protected override bool StateEntryEvent() => DispatchCurrentStateEvent("state_entry");

        protected override bool FrameEvent()
        {
            Generic_Frame();
            return DispatchCurrentStateEvent("frame");
        }

        protected override bool TUEvent()
        {
            return DispatchCurrentStateEvent("TU");
        }

        protected override bool DieEvent()
        {
            Generic_Die();
            return true;
        }

        #region 通用状态处理

        private void Generic_Frame()
        {
            var frame = Frame.D;
            if (frame == null) return;

            if (Frame.N == 15)
            {
                SetFrameDirect(1000);
            }
        }

        private void Generic_Die()
        {
            var frame = Frame.D;
            if (frame != null && frame.hit_d != 0)
            {
                SetFrameDirect(frame.hit_d);
            }
        }

        #endregion

        #region 特定状态处理

        private bool State_15(string eventType, object eventData)
        {
            if (eventType == "TU")
                return ProcessState15TU();

            return false;
        }

        private bool State_3000(string eventType, object eventData)
        {
            return false;
        }

        private bool State_3001(string eventType, object eventData)
        {
            return false;
        }

        private bool State_3003(string eventType, object eventData)
        {
            return false;
        }

        private bool State_3005(string eventType, object eventData)
        {
            return false;
        }

        private bool State_3006(string eventType, object eventData)
        {
            return false;
        }

        private bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            return GetState() switch
            {
                15 => State_15(eventType, eventData),
                LF2States.ProjectileFlying => State_3000(eventType, eventData),
                LF2States.ProjectileHiting => State_3001(eventType, eventData),
                LF2States.ProjectileTeleport => State_3003(eventType, eventData),
                LF2States.ObjectFlying => State_3005(eventType, eventData),
                LF2States.ObjectExpanding => State_3006(eventType, eventData),
                _ => false,
            };
        }

        private bool ProcessState15TU()
        {
            var frame = Frame.D;
            if (frame != null && frame.dvx != 0)
            {
                PS.vx = Dirh() * frame.dvx;
            }

            return true;
        }





        #endregion

        public override void Reset()
        {
            ResetPooledEntityState();
            _parent = null;
            TrackerParent = null;
            Runtime.Reset();
            ObjectId = 0;
            Team = 0;
            Health.HP = 0;
            _lastState = -1;
            NoBounce = false;
            ShotCount = 0;
            ResetSpark();
            ResetStableId();
        }

        public override void Destroy()
        {
            CreateBrokenEffect();
        }

        // ========== ISimObject 生命周期 ==========

        public override void OnFrameTransit(int targetFrameId, bool switchDirAfterTrans, int oldLock)
        {
            Frame.PN = Frame.N;
            Frame.N = targetFrameId;

            LF2FrameData targetFrame = FrameCache.GetFrameDataById(targetFrameId);
            if (targetFrame == null) return;

            bool isStateTrans = Frame.D?.state != targetFrame.state;
            if (isStateTrans)
                StateExitEvent();

            Frame.D = targetFrame;

            if (isStateTrans)
            {
                AttackingCounter = 0;
                StateEntryEvent();
                _lastState = Frame.D.state;
            }

            Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next);
            FrameEvent();

            QueueCurrentFrameSound();
        }

        public override void OnFrameTickFrameChangedFromWaitCounter()
        {
            base.OnFrameTickFrameChangedFromWaitCounter();
        }

        public override void SimFrameTick(int tickIndex)
        {
            RunCommonFrameTick();
        }

        /// <summary>
        /// 直接写入当前帧，用于 C++ release Entity_Collision / frame_tick 中不触发帧事件的跳帧。
        /// </summary>
        private void SetFrameDirect(int frameId, int waitCounter = int.MinValue)
        {
            if (frameId >= 0 && FrameCache?.HasFrame(frameId) != true)
                return;

            Frame.N = frameId;
            Frame.D = FrameCache.GetFrameDataById(frameId);
            AttackingCounter = 0;
            if (Frame.D != null && Trans != null)
            {
                Trans.SyncDirectFrameData(Frame.D.wait, Frame.D.next, waitCounter);
            }
        }

        /// <summary>
        /// TU 阶段：执行技能对象的逐 tick 逻辑、物理和状态分支。
        /// </summary>
        public override void SimTU(int tickIndex)
        {
            int dataType = GetCurrentDataObjectTypeForSimulation();
            if (dataType == (int)LF2ObjectType.Character)
            {
                RunSharedCharacterDatFrameAdvanceAsCharacter(tickIndex);
                return;
            }
            if (!RunSharedNonCharacterDatFrameAdvance())
                return;

            if (dataType != (int)LF2ObjectType.SpecialAttack)
                return;

            int currentState = GetState();

            if (currentState != _lastState)
            {
                StateEntryEvent();
                _lastState = currentState;
            }

            if (currentState == 15)
                ProcessState15TU();

            if (Health.HP <= 0)
            {
                DieEvent();
            }
        }

        public override void SimObjectInteraction(int tickIndex)
        {
            if (UsesCharacterDatInteractionPhase())
                return;

            Interaction();
        }

        // ========== 交互方法 ==========

        /// <summary>
        /// 技能对象命中检测入口，对齐 C++ release 的 Entity_AI_Update 技能/投射物交互流程。
        /// </summary>
        public void Interaction()
        {
            LF2FrameData frame = GetCollisionFrameData();
            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (frame?.itrs == null || sceneQuery == null || kindService == null) return;
            if (!sceneQuery.TryGetCollisionCandidateSequence(this, out var candidates) || candidates == null)
                return;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                SceneQueryHit candidate = candidates[candidateIndex];
                int itrIndex = candidate.ItrIndex;
                if (itrIndex < 0 || itrIndex >= frame.itrs.Count)
                    continue;

                LF2Entity target = candidate.ResolveCurrentTarget(Match);
                if (target == null)
                    continue;
                InteractionArea runtimeItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    this,
                    target,
                    frame,
                    frame.itrs[itrIndex],
                    out bool zeroAttackerHpOnConsume,
                    out bool releaseHeavyHeldTargetOnConsume);
                if (runtimeItr == null || !CanConsumeRecordedCandidate(target))
                    continue;

                var hitInfo = new SceneQueryHit(
                    target,
                    candidate.BodyX,
                    itrIndex,
                    runtimeItr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);

                ApplyReleaseSceneQueryConsumeEffects(hitInfo);
                bool abortAfterSuccessfulHit = LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect(
                    target,
                    runtimeItr);
                if (!DispatchInteractionByKind(kindService, runtimeItr, target))
                    continue;

                if (abortAfterSuccessfulHit)
                    return;
            }
        }

        private bool CanConsumeRecordedCandidate(LF2Entity target)
        {
            if (target == null || target == this || target.Runtime == null)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int selfSlot = Runtime?.SlotIndex ?? -1;
            return selfSlot < 0 || target.ItrVrestTest(selfSlot, true);
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 8)
            {
                if (target?.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                    return false;
                if (DeferState3005Kind8LeadIn()) return false;
                return TryApplyHit(itr, target);
            }

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                case 1:
                    return LF2CharacterInteractionResolver.TryApplyKind1Grab(this, target, itr);
                case 2:
                case 7:
                    return TryApplyPickupCandidate(itr, target);
                case 3:
                    return LF2CharacterInteractionResolver.TryApplyKind3Grab(this, target, itr);
                default:
                    return false;
            }
        }

        private bool DeferState3005Kind8LeadIn()
        {
            var activeFrame = Frame?.D;
            if (activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
            {
                return false;
            }

            // C++ release defer_state3005_kind8_lead_in：
            // state=3005 且当前/下一帧带 hit_Fa 或 opoint 时，延后 kind=8 命中。
            if (activeFrame.hit_Fa > 0 || (activeFrame.opoints != null && activeFrame.opoints.Count > 0))
            {
                return true;
            }

            if (activeFrame.next <= 0 || activeFrame.next == Frame.N)
            {
                return false;
            }

            var nextFrame = GetFrameDataById(activeFrame.next);
            return nextFrame != null
                && (nextFrame.hit_Fa > 0 || (nextFrame.opoints != null && nextFrame.opoints.Count > 0));
        }

        private bool TryApplyPickupCandidate(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target?.Runtime == null || Runtime == null)
                return false;
            if (itr.kind == 7 && Runtime.LinkState != 0)
                return false;

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            int selfSlot = Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (selfSlot < 0 || targetSlot < 0)
                return false;

            if (itr.kind == 7)
            {
                int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
                Runtime.LinkState = 1;
                target.Runtime.LinkState = -1;
                if (targetOid == 0x78 || targetOid == 0x7C)
                {
                    Runtime.LinkState = 101;
                    target.Runtime.LinkState = -1;
                }
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    Runtime.LinkState = 4;
                    target.Runtime.LinkState = -4;
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    Runtime.LinkState = target.Health?.HP > 0 ? 6 : 4;
                    target.Runtime.LinkState = -Runtime.LinkState;
                    if ((target.Health?.HP ?? 0) <= 0)
                        target.Runtime.WeaponFlightCounter = 0;
                }
            }
            else if (itr.kind == 2)
            {
                int pickupFrame;
                if (targetType == (int)LF2ObjectType.LightWeapon)
                {
                    pickupFrame = 115;
                    Runtime.LinkState = 1;
                    target.Runtime.LinkState = -1;
                }
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    pickupFrame = 115;
                    Runtime.LinkState = 4;
                    target.Runtime.LinkState = -4;
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    pickupFrame = 115;
                    Runtime.LinkState = target.Health?.HP > 0 ? 6 : 4;
                    target.Runtime.LinkState = -Runtime.LinkState;
                    if ((target.Health?.HP ?? 0) <= 0)
                        target.Runtime.WeaponFlightCounter = 0;
                }
                else if (targetType == (int)LF2ObjectType.HeavyWeapon)
                {
                    pickupFrame = 116;
                    Runtime.LinkState = 2;
                    target.Runtime.LinkState = -2;
                }
                else
                {
                    return false;
                }

                DirectWriteRawFramePreserveWaitCounter(pickupFrame);
                AttackingCounter = 0;
            }
            else
            {
                return false;
            }

            target.RelationTeam = RelationTeam;
            Runtime.TargetSlotIndex = targetSlot;
            Runtime.HeldWeaponStableId = targetSlot;
            target.Runtime.HolderStableId = selfSlot;
            target.HolderCopySlot = selfSlot;
            Runtime.PickupCount++;
            RefreshRuntimeSnapshot();
            target.RefreshRuntimeSnapshot();
            return true;
        }

        private bool TryApplyHit(InteractionArea itr, LF2Entity target)
        {
            bool applied = false;

            int targetDataType = target?.GetCurrentDataObjectTypeForSimulation() ?? -1;
            if (targetDataType == (int)LF2ObjectType.Character && PS != null)
            {
                var attackerPos = new Vector3((float)PS.x, (float)PS.y, (float)PS.z);
                if (target is LF2Character character)
                    applied = character.Hit(itr, this, attackerPos, default);
                else if (LF2CharacterDatHitResolver.CanResolveTarget(target))
                    applied = LF2CharacterDatHitResolver.TryResolveHit(target, itr, this, attackerPos, default);
            }
            else if (target is LF2WeaponBase weapon)
            {
                applied = weapon.Hit(itr, this);
            }
            else if (target is LF2SpecialAttack specialAttack)
            {
                applied = specialAttack.Hit(itr, this);
            }

            if (applied && itr.kind == 0)
                ApplyPostHitSelfDestruct(target);

            return applied;
        }

        /// <summary>
        /// 技能对象被命中入口，对齐 C++ release 的 Entity_AI_Update hurt pipeline。
        /// </summary>
        public bool Hit(InteractionArea itr, LF2Entity attacker)
        {
            if (itr == null || attacker?.Runtime == null || Runtime == null)
                return false;

            if (itr.kind == 9)
            {
                LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
                LF2CharacterData charData = FrameCache?.Wrapper?.characterData;
                if (!string.IsNullOrEmpty(charData?.weapon_broken_sound))
                    PlaySound(charData.weapon_broken_sound);

                attacker.FrameDelay = -3;

                int curState = GetState();
                if (curState == LF2States.ObjectFlying) // 3005
                {
                    HitConfirm2 = 1;
                    DirectWriteRawFramePreserveWaitCounter(40);
                }
                else
                {
                    RelationTeam = attacker.RelationTeam;
                    HolderCopySlot = attacker.HolderCopySlot;
                    HitConfirm2 = 1;
                    DirectWriteRawFramePreserveWaitCounter(30);
                    AttackingCounter = 0;
                    KnockbackVx = 0.0;
                    KnockbackVy = 0.0;
                    KnockbackVz = 0.0;
                    Runtime.Vx = 0.0;
                    Runtime.Vy = 0.0;
                    Runtime.Vz = 0.0;
                    Runtime.AnimCounter = attacker.Runtime.SlotIndex;
                }
                return true;
            }

            if (itr.kind == 14)
            {
                ApplySpecialKind14DirectionalBlockFrom(attacker);
                return true;
            }

            if (itr.kind != 0)
                return false;

            LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
            ApplyObjectHurtTail(attacker, itr);
            ApplyKind0Type3Tail(attacker, itr);
            RecordKind0Hit(attacker, itr);
            return true;
        }

        private void ApplyObjectHurtTail(LF2Entity attacker, InteractionArea itr)
        {
            int victimType = GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            int itrArest = itr.arest < 4 && itr.vrest == 0 ? 4 : itr.arest;

            if ((Health?.HP ?? 0) <= 0 || itr.effect == 4)
                FallCounter = 80;

            if (victimType != (int)LF2ObjectType.HeavyWeapon || itr.fall > 40)
                HitCount++;

            FallCounter += itr.fall != 0 ? itr.fall : 20;
            if (ShouldForceFall80())
                FallCounter = 80;

            bool knockback = false;
            if (FallCounter > 60 && victimType != (int)LF2ObjectType.SpecialAttack)
            {
                FallCounter = 80;
                knockback = true;
            }
            else if (victimType != (int)LF2ObjectType.SpecialAttack)
            {
                if (FallCounter > 50)
                {
                    FallCounter = 60;
                    DirectWriteRawFramePreserveWaitCounter(226);
                    if (GetRuntimeYInt() < 0)
                    {
                        FallCounter = 80;
                        knockback = true;
                    }
                }
                else if (FallCounter > 30)
                {
                    FallCounter = 40;
                    DirectWriteRawFramePreserveWaitCounter(Dirh() != attacker.Dirh() ? 222 : 224);
                    if (GetRuntimeYInt() < 0)
                    {
                        FallCounter = 80;
                        knockback = true;
                    }
                }
                else if (FallCounter > 10)
                {
                    FallCounter = 20;
                    DirectWriteRawFramePreserveWaitCounter(220);
                    if (GetRuntimeYInt() < 0)
                        DirectWriteRawFramePreserveWaitCounter(Dirh() != attacker.Dirh() ? 222 : 224);
                }
            }

            LF2HitResolveRuntimeData.RecordStandardHurtSounds(attacker, this, itr, knockback);
            float defaultDvx = itr.dvx != 0 ? attacker.Dirh() * itr.dvx : 0f;
            float resolvedDvx = LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(
                attacker,
                this,
                itr,
                knockback,
                defaultDvx);
            if (resolvedDvx != 0f)
            {
                KnockbackVx += resolvedDvx;
                LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(this);
            }

            ApplyState3000ObjectHurtTail(attacker);

            if (knockback)
            {
                if ((victimType != (int)LF2ObjectType.HeavyWeapon &&
                     victimType != (int)LF2ObjectType.SpecialAttack) ||
                    itr.fall > 40)
                {
                    KnockbackVy += itr.dvy != 0 ? itr.dvy : -7.0;
                }

                if ((int)(KnockbackVy + GetRuntimeYInt()) > 0)
                    KnockbackVy = 12.0;

                int fallFrame = Dirh() > 0
                    ? (KnockbackVx <= 0.0 ? 180 : 186)
                    : (KnockbackVx >= 0.0 ? 180 : 186);
                DirectWriteRawFramePreserveWaitCounter(fallFrame);
                ApplyKnockdownHeldPairVrest(attacker);
            }

            HitStateCount = 45;
            if (attacker.FrameDelay >= 0)
                attacker.FrameDelay = 3;
            FrameDelay = -3;
            attacker.AttackExempt = itrArest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = itrArest;
            if (attackerSlot >= 0 && itr.vrest > 0)
                ItrRest?.SetVrest(attackerSlot, itr.vrest);

            ApplyCaughtVictimHurtFrame(attacker);

            if (FallCounter == 80)
                FallCounter = 0;

            if (attacker.Runtime.LinkState < 0)
            {
                int holderSlot = attacker.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                if (holder != null)
                    holder.FrameDelay = attacker.FrameDelay;
            }

            if (attacker.GetState() == LF2States.WeaponThrowing)
            {
                attacker.DirectWriteRawFramePreserveWaitCounter(attacker.BattleRandInt(0, 16));
                attacker.Runtime.Vx = KnockbackVx * -0.5;
                attacker.Runtime.Vy = -4.0;
                if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon &&
                    victimType == (int)LF2ObjectType.ThrowWeapon)
                {
                    attacker.KnockbackVx = -KnockbackVx;
                }
            }
        }

        private bool ShouldForceFall80()
        {
            LF2FrameData previousFrame = GetFrameDataById(Frame?.Prev ?? 0);
            if (previousFrame?.state == 13)
                return true;

            LF2FrameData previousFrame2 = GetFrameDataById(Runtime.PrevFrame2);
            if (previousFrame2?.state == 12)
                return true;

            int victimType = GetCurrentDataObjectTypeForSimulation();
            return victimType == (int)LF2ObjectType.LightWeapon ||
                   victimType == (int)LF2ObjectType.HeavyWeapon ||
                   victimType == (int)LF2ObjectType.ThrowWeapon ||
                   victimType == (int)LF2ObjectType.Drink;
        }

        private void ApplyState3000ObjectHurtTail(LF2Entity attacker)
        {
            if (attacker == null || attacker.GetState() != LF2States.ProjectileFlying)
                return;

            int attackerOid = attacker.FrameCache?.Wrapper?.characterId ?? attacker.ObjectId;
            int victimOid = FrameCache?.Wrapper?.characterId ?? ObjectId;
            int victimType = GetCurrentDataObjectTypeForSimulation();
            bool nonCharacterVictim = victimType != (int)LF2ObjectType.Character;
            bool skipReset = nonCharacterVictim && attackerOid == 209 &&
                (victimOid == 200 ||
                 victimOid == 203 ||
                 victimOid == 205 ||
                 victimOid == 206 ||
                 victimOid == 207 ||
                 victimOid == 215 ||
                 victimOid == 216 ||
                 (victimOid == 209 && Frame.N == 40));
            if (skipReset)
                return;

            attacker.DirectWriteRawFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
            LF2FrameData frame10 = attacker.GetFrameDataById(10);
            if (frame10 != null)
                attacker.Runtime.Vz = frame10.dvz;
        }

        private void ApplyKnockdownHeldPairVrest(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || Runtime.LinkState <= 0)
                return;

            int attackerSlot = attacker.Runtime.SlotIndex;
            int victimSlot = Runtime.SlotIndex;
            int heldTargetSlot = Runtime.ResolveActiveHeldSlotIndex();
            if (attackerSlot < 0 || victimSlot < 0 || heldTargetSlot < 0)
                return;

            LF2Entity heldTarget = Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
            if (heldTarget?.Runtime == null || !heldTarget.Runtime.IsActivelyHeldBySlot(victimSlot))
                return;

            heldTarget.ItrRest?.SetVrest(attackerSlot, 45);
            ItrRest?.SetVrest(heldTargetSlot, 30);
        }

        private void ApplyCaughtVictimHurtFrame(LF2Entity attacker)
        {
            if (FallCounter == 80 || Runtime == null || attacker == null)
                return;

            LF2FrameData previousFrame2 = GetFrameDataById(Runtime.PrevFrame2);
            CatchPoint cpoint = previousFrame2?.cpoint;
            if (cpoint == null || cpoint.kind != 2)
                return;

            int catcherSlot = CatcherSlotIndex;
            int victimSlot = Runtime.SlotIndex;
            LF2Entity catcher = catcherSlot >= 0
                ? Match?.FindEntityByRuntimeSlotForQuery(catcherSlot)
                : null;
            if (catcher == null || catcher.CaughtSlotIndex != victimSlot)
                return;

            int hurtFrame = Dirh() != attacker.Dirh()
                ? cpoint.fronthurtact
                : cpoint.backhurtact;
            if (hurtFrame != 0)
                DirectWriteRawFramePreserveWaitCounter(hurtFrame);
        }

        private void ApplySpecialKind14DirectionalBlockFrom(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || Runtime == null)
                return;

            int attackerX = attacker.Runtime.XInt;
            int attackerZ = attacker.Runtime.ZInt;
            int victimX = Runtime.XInt;
            int victimZ = Runtime.ZInt;

            if (attackerX > victimX + 5 && (Runtime.Vx > 0.0 || KnockbackVx > 0.0))
                Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5 && (Runtime.Vx < 0.0 || KnockbackVx < 0.0))
                Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2 && (Runtime.Vz > 0.0 || KnockbackVz > 0.0))
                Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2 && (Runtime.Vz < 0.0 || KnockbackVz < 0.0))
                Runtime.ZBoundNegative = true;
        }

        /// <summary>
        /// entity_type==0 对应 attacker 是非武器（角色）目标
        /// </summary>
        private void ApplyPostHitSelfDestruct(LF2Entity victim)
        {
            // attacker 的 entity_type：武器类型为 1/2/3/4/6，角色为 0。
            if (victim == null ||
                victim.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return;

            int currentOid = FrameCache?.Wrapper?.characterId ?? ObjectId;
            if (currentOid == 201)
                FreeEntityLikeExe();
            else if (currentOid == 214 && Health != null)
                Health.HP = 0;
        }

        private void ApplyKind0Type3Tail(LF2Entity attacker, InteractionArea itr)
        {
            int victimState = GetState();
            int attackerState = attacker.GetState();
            bool skipToStateSync = victimState == LF2States.ObjectFlying ||
                                   (victimState == LF2States.ObjectExpanding &&
                                    attackerState == LF2States.ObjectFlying);

            if (!skipToStateSync)
            {
                LF2Entity attackerHolder = ResolveActiveHolder(attacker);
                CopyRelation(attackerHolder ?? attacker, this);
                HitConfirm2 = 1;
                ResetType3HitMotion(this);

                if (attacker.ObjectId == 209 && IsKarasuOid(ObjectId))
                {
                    CopyRelation(attacker, this);
                    if (TryApplyRuntimeIdentity(attacker.ObjectId, 40, false, out _))
                    {
                        Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, 40);
                        Frame.Prev = 40;
                    }
                    skipToStateSync = true;
                }
                else
                {
                    bool frame20 = false;
                    bool checkEffectGate;
                    if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
                    {
                        checkEffectGate = true;
                    }
                    else if (attacker.Runtime.LinkState >= 0)
                    {
                        frame20 = true;
                        checkEffectGate = false;
                    }
                    else
                    {
                        checkEffectGate = true;
                    }

                    if (checkEffectGate && (itr.effect == 2 || itr.effect == 20))
                        frame20 = true;

                    DirectWriteHeldFramePreserveWaitCounter(frame20 ? 20 : 30);
                    if (frame20)
                    {
                        if (attacker.Runtime.LinkState < 0)
                        {
                            if (attacker.ObjectId == 213 && IsKarasuOid(ObjectId))
                                ReplaceWithActiveKarasuData();
                            CopyRelation(attackerHolder, this);
                        }
                    }
                    else
                    {
                        if (attacker.ObjectId == 8 && IsKarasuOid(ObjectId))
                            ReplaceWithActiveKarasuData();

                        if (attacker.Runtime.LinkState < 0)
                        {
                            if (attacker.ObjectId == 213 && IsKarasuOid(ObjectId))
                                ReplaceWithActiveKarasuData();
                            CopyRelation(attackerHolder, this);
                        }
                    }
                }
            }

            victimState = GetState();
            attackerState = attacker.GetState();
            if ((victimState == LF2States.ObjectFlying && attackerState == LF2States.ObjectFlying) ||
                (victimState == LF2States.ObjectExpanding && attackerState == LF2States.ObjectExpanding))
            {
                DirectWriteHeldFramePreserveWaitCounter(20);
                ResetType3HitMotion(this);
                attacker.DirectWriteHeldFramePreserveWaitCounter(20);
                ResetType3HitMotion(attacker);

                if (attacker.Runtime.LinkState < 0)
                {
                    LF2Entity holder = ResolveActiveHolder(attacker);
                    if (holder != null && holder.FrameDelay > 0)
                        holder.FrameDelay = -holder.FrameDelay;
                }
                else if (attacker.FrameDelay > 0)
                {
                    attacker.FrameDelay = -attacker.FrameDelay;
                }
            }

            ApplyType3EffectTail(itr.effect);
        }

        private void ApplyType3EffectTail(int effect)
        {
            LF2FrameData prevFrame = GetFrameDataById(Frame?.Prev ?? 0);
            int prevState = prevFrame?.state ?? 0;
            bool characterDat = GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;

            if (effect == 3 || effect == 30)
            {
                if (characterDat && prevState != 13)
                {
                    DirectWriteHeldFramePreserveWaitCounter(200);
                    AttackingCounter = 0;
                    Match?.QueueSound("SFX_065", Runtime.XInt);
                }
            }
            else if (effect >= 5000 && effect < 6000)
            {
                int nextPp = (Health?.PP ?? 0) - (effect - 5000);
                if (Health != null)
                    Health.PP = nextPp < 0 ? 0 : nextPp;
            }
            else if (effect >= 6000 && effect < 7000)
            {
                DirectWriteHeldFramePreserveWaitCounter(effect - 6000);
            }
            else if (effect == 2 || effect == 21 || effect == 22)
            {
                if (characterDat)
                    ApplyType3BurningEffect();
            }
            else if (effect == 20)
            {
                if (characterDat && prevState != 18)
                    ApplyType3BurningEffect();
            }
            else if (effect == 23)
            {
                Match?.QueueSound("SFX_068", Runtime.XInt);
            }
        }

        private void ApplyType3BurningEffect()
        {
            DirectWriteHeldFramePreserveWaitCounter(203);
            AttackingCounter = 0;
            SwitchDir(KnockbackVx < 0.0 ? "right" : "left");
            Match?.QueueSound("SFX_068", Runtime.XInt);
        }

        private static void ResetType3HitMotion(LF2Entity entity)
        {
            entity.AttackingCounter = 0;
            entity.KnockbackVx = 0.0;
            entity.KnockbackVy = 0.0;
            entity.KnockbackVz = 0.0;
            entity.Runtime.Vx = 0.0;
            entity.Runtime.Vy = 0.0;
            entity.Runtime.Vz = 0.0;
        }

        private static void CopyRelation(LF2Entity source, LF2Entity target)
        {
            if (source == null || target == null)
                return;

            target.RelationTeam = source.RelationTeam;
            target.HolderCopySlot = source.HolderCopySlot;
        }

        private static LF2Entity ResolveActiveHolder(LF2Entity entity)
        {
            if (entity?.Match == null)
                return null;

            int holderSlot = entity.Runtime.HolderStableId;
            if (holderSlot < 0 || holderSlot >= entity.Match.MaxRuntimeSlotsForServices)
                return null;

            return entity.Match.FindEntityByRuntimeSlotForQuery(holderSlot);
        }

        private static bool IsKarasuOid(int oid)
        {
            return oid == 200 || oid == 203 || oid == 205 || oid == 206 ||
                   oid == 207 || oid == 215 || oid == 216;
        }

        private void ReplaceWithActiveKarasuData()
        {
            if (Match == null)
                return;

            for (int slot = 0; slot < Match.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = Match.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate == null || candidate.ObjectId != 209)
                    continue;

                int frameId = Frame?.N ?? 0;
                if (TryApplyRuntimeIdentity(209, frameId, false, out _) && Frame?.D != null)
                {
                    Trans?.SyncDirectFrameData(Frame.D.wait, Frame.D.next, frameId);
                    Frame.Prev = frameId;
                }
                return;
            }
        }

        private bool Hit_State3000(LF2Entity attacker, InteractionArea itr)
        {
            var frame = Frame.D;

            if (itr.kind == 14)
            {
                Trans.SetWait(0, 20);
                return true;
            }

            if (attacker != null)
            {
                if (Team == attacker.Team && PS.dir == attacker.PS?.dir)
                {
                    return false;
                }
            }

            var frameItr = GetFirstItr(frame);
            if (frameItr != null && frameItr.effect == 3)
            {
                var attackerSA = attacker as LF2SpecialAttack;
                if (attackerSA != null && attackerSA.GetState() == LF2States.ProjectileFlying &&
                    itr.effect != 3 && itr.effect != 2)
                {
                    return true;
                }
            }

            var attackerSpecial = attacker as LF2SpecialAttack;
            if (attackerSpecial != null)
            {
                if (frameItr != null && frameItr.effect != 3 && frameItr.effect != 2 && itr.effect == 3)
                {
                    PS.vx = 0;
                    // 忍偶系变身逻辑
                    int selfOid = ObjectId;
                    bool isValidTarget = selfOid == 200 || selfOid == 203 || selfOid == 205
                        || selfOid == 206 || selfOid == 207 || selfOid == 215 || selfOid == 216;

                    if (attackerSpecial.ObjectId == 209 && isValidTarget)
                    {
                        // 0x0042DC73: target.data=attacker.data
                        // 0x0042DC80: target.[+70h/74h/78h]=40
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                            FrameCache.Load(karasuWrapper);
                            SetFrameDirect(40, 40);
                            Frame.PN = 40;
                        }
                        return true;
                    }

                    if (attackerSpecial.ObjectId == 213 && isValidTarget)
                    {
                        // target.team 和 target.[+354h] 继承 attacker.TrackerParent 的对应值。
                        var karasuWrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(209);
                        if (karasuWrapper != null)
                        {
                            int savedFrame = Frame.N;
                            FrameCache.Load(karasuWrapper);
                            SetFrameDirect(savedFrame);
                            Frame.PN = savedFrame;
                        }
                        var parent = attackerSpecial.ResolveTrackerParentFromRuntime() as LF2SpecialAttack;
                        if (parent != null)
                        {
                            Team = parent.Team;
                            OwnerId = parent.OwnerId;
                        }
                        else
                        {
                            Team = attackerSpecial.Team;
                            OwnerId = attackerSpecial.OwnerId;
                        }
                        return true;
                    }

                    SetFrameDirect(1000);
                    CreateObjectAt(209, attackerSpecial);
                    return true;
                }

                if (itr.kind == 0)
                {
                    PS.vx = 0;
                    SetFrameDirect(20);
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = 0;
                Team = attacker?.Team ?? 0;
                SetFrameDirect(30);
                return true;
            }

            return false;
        }

        private bool Hit_State3006(LF2Entity attacker, InteractionArea itr)
        {
            var attackerSA = attacker as LF2SpecialAttack;
            if (attackerSA != null)
            {
                int attackerState = attackerSA.GetState();

                if (attackerState == LF2States.ObjectFlying || attackerState == LF2States.ObjectExpanding)
                {
                    SetFrameDirect(20);
                    PS.vx = 0;
                    PS.vz = 0;
                    return true;
                }

                if (attackerState == LF2States.ProjectileFlying)
                {
                    PS.vx = (PS.vx > 0 ? -1 : 1) * 7;
                    return true;
                }
            }

            if (itr.kind == 0)
            {
                PS.vx = (PS.vx > 0 ? -1 : 1) * 1;
                if (itr.bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                {
                    Health.HP = 0;
                }
                return true;
            }

            return false;
        }

        private static InteractionArea GetFirstItr(LF2FrameData frame)
        {
            if (frame?.itrs == null || frame.itrs.Count == 0) return null;
            return frame.itrs[0];
        }

        // ========== 辅助方法 ==========

        public void CreateBrokenEffect()
        {
            // C++ release 中 broken/effect 生成由 GameMode 层处理；这里先保留统一入口。
            BrokenEffectCreate(ObjectId);
        }

        public void CreateObject(ObjectPoint op)
        {
            if (op.oid <= 0) return;
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = this;
            task.team = Team;
            task.pos = MakeObjectPoint(op);
            task.z = (float)PS.z;
            task.dir = PS.dir;
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        public void CreateObjectAt(int oid, LF2SpecialAttack source)
        {
            var op = new ObjectPoint { oid = oid, action = 0, facing = 0 };
            var task = LF2ReferencePool.Instance.Fetch<OPointCreateTask>();
            task.opoint = op;
            task.parent = source;
            task.team = source?.Team ?? 0;
            task.pos = new Vector3((float)(source?.PS?.x ?? 0), (float)(source?.PS?.y ?? 0), (float)(source?.PS?.z ?? 0));
            task.z = (float)(source?.PS?.z ?? 0);
            task.dir = source?.PS?.dir ?? "right";
            task.dvz = 0;
            LF2ObjectPointFactory.Instance?.EnqueueCreateObject(task);
        }

        private Vector3 MakeObjectPoint(ObjectPoint op)
        {
            var frame = Frame?.D;
            if (PS == null || frame == null)
                return new Vector3((float)(PS?.x ?? 0f), (float)(PS?.y ?? 0f), (float)(PS?.z ?? 0f));

            double x = PS.dir == "right"
                ? PS.x - frame.centerx + op.x
                : PS.x + frame.centerx - op.x;
            double y = PS.y + PS.z - frame.centery + op.y;
            double z = PS.z + op.y;
            return new Vector3((float)x, (float)y, (float)z);
        }

        public void PlaySound(string soundId)
        {
            if (string.IsNullOrEmpty(soundId)) return;
            QueueBattleSound(soundId);
        }

        private void QueueCurrentFrameSound()
        {
            int frameId = Frame?.N ?? -1;
            string soundId = Frame?.D?.sound;
            if (frameId < 0 || frameId >= LF2FrameCache.MaxFrameIdExclusive || string.IsNullOrWhiteSpace(soundId))
                return;

            Match?.QueueSound(soundId, Runtime.XInt);
        }

        // ========== 初始化子步骤 ==========

        private void InitializeParent(OPointCreateTask task)
        {
            _parent = task.parent as LF2LivingObject;
            ObjectId = task.opoint.oid;
            Team = task.team;
        }

        private void InitializePosition(OPointCreateTask task)
        {
            if (task.useDirectRuntimePosition)
            {
                PS.x = task.directX;
                PS.y = task.directY;
                PS.z = task.directZ;
                return;
            }

            PS.x = task.pos.x;
            PS.y = task.pos.y;
            PS.z = task.z;
        }

        private void InitializeDirection(OPointCreateTask task)
        {
            string dir = CalculateDirection(task.opoint.facing, task.dir);
            SwitchDir(dir);
        }

        private void InitializeFrame(OPointCreateTask task)
        {
            var wrapper = CharacterAnimtorManager.Instance?.GetCharacterConfig(ObjectId);
            FrameCache.Load(wrapper);
            int action = task.opoint.action;
            if (action == 0 && !FrameCache.HasFrame(0))
                action = 999;
            Frame.D = FrameCache.GetFrameDataById(action);
            SetFrameDirect(action);
        }

        private void InitializeVelocity(OPointCreateTask task)
        {
            if (task.useDirectVelocity)
            {
                PS.vx = task.directVx;
                PS.vy = task.directVy;
                PS.vz = task.directVz;
                return;
            }

            PS.vx = Dirh() * task.opoint.dvx;
            PS.vy = task.opoint.dvy;

            bool hasFrameDvx = (Frame.D != null && Frame.D.dvx != 0);
            PS.vz = hasFrameDvx ? task.dvz : 0f;
        }

        private void InitializeHealth()
        {
            Health.HP = NTSDGlobal.Default.Health.HpFull;
        }

        private Vector3 MakePointCenter(LF2FrameData frame)
        {
            float spriteWidth = Sprite?.GetWidthPx() ?? 0;

            int centerx = frame?.centerx ?? 0;
            int centery = frame?.centery ?? 0;

            float x = (PS.dir == "right")
                ? PS.sx + centerx
                : PS.sx + spriteWidth - centerx;

            float y = PS.sy + centery;
            float z = PS.sz + centery;

            return new Vector3(x, y, z);
        }

        private void CoincideXYForInit(Vector3 targetPos, Vector3 selfPoint)
        {
            float vx = targetPos.x - selfPoint.x;
            float vz = targetPos.z - selfPoint.z;
            PS.x += vx;
            PS.z += vz;
        }
    }
}
