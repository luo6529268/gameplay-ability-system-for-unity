using UnityEngine;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2Character 受击系统。
    /// 当前逻辑应以 C++ release 工程的 Entity_AI_Update 命中处理为基准。
    ///
    /// 关键行为：
    ///   1. dvx/dvy 先累积到 KnockbackVx/KnockbackVy，再由帧后处理写入 PS.vx/PS.vy。
    ///   2. fall==80 时根据 KnockbackVx 与朝向选择正面倒地或背面倒地帧。
    ///   3. 倒地路径的 dvy 不直接写 PS.vy，而是累积到 KnockbackVy 并在帧后处理统一落地。
    ///   4. 攻击者处于 state 1002 时，命中后执行反弹速度。
    ///   5. 普通命中和击飞命中都会设置双方 FrameDelay。
    ///   6. fall 累积决定轻伤、中伤、重伤、击飞等帧切换。
    ///   7. kind=6/8/14/15/16 等特殊 itr 仍按正式命中分支处理。
    ///   8. HitStateCount 和 Fall 通过 HitCounters 代理到运行时状态。
    ///   9. 视觉 spark 只在正式命中路径触发。
    /// </summary>
    internal sealed class LF2CharacterHitResolver
    {
        private readonly LF2Character _character;
        private readonly InteractionArea _runtimeItr = new InteractionArea();

        public LF2CharacterHitResolver(LF2Character character)
        {
            _character = character;
        }

        // 主流程
        // ------------------------------------------------------------
        // 命中入口按 itr.kind 和当前 state 分流。

        public bool ResolveHit(InteractionArea itr, LF2Entity attacker,
                               UnityEngine.Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!_character.PassBaseHit(itr, attacker, attackerPos, vol)) return false;

            if (itr.kind == 4)
            {
                if (attacker.WeaponCount <= 0)
                    return false;

                _runtimeItr.CopyFrom(itr);
                itr = _runtimeItr;
                itr.kind = 0;
                if ((attacker.Runtime.Vx > 0.0 && attacker.Dirh() < 0) ||
                    (attacker.Runtime.Vx < 0.0 && attacker.Dirh() > 0))
                {
                    itr.dvx = -itr.dvx;
                }
            }

            // Kind 5：委托攻击，使用 TrackerParent 的 wpoint.attacking 替换伤害字段。
            // 条件：victim.GrabbedBy<0、TrackerParent.TrackerFlag==attacker.StableId、父对象不是自己。
            // 替换后走普通 kind=0 命中结算。
            if (itr.kind == 5 && _character.GrabbedBy < 0)
            {
                LF2Entity trackerParent = _character.ResolveTrackerParentFromRuntime();
                var tp = trackerParent;
                if (tp != null && tp.TrackerFlag == attacker.StableId && tp != _character)
                {
                    var tpFrame = tp.GetFrameDataById(tp.Frame.N);
                    var wp = (tpFrame?.wpoints?.Count > 0) ? tpFrame.wpoints[0] : null;
                    if (wp != null && wp.attacking > 0)
                    {
                        // 从 attacker 当前帧的 wpoints[wp.attacking] 取得实际伤害数据。
                        var attackerFrame = attacker.GetFrameDataById(attacker.Frame.N);
                        int wpIdx = wp.attacking;
                        var srcWp = (attackerFrame?.wpoints != null && wpIdx < attackerFrame.wpoints.Count)
                            ? attackerFrame.wpoints[wpIdx] : null;
                        if (srcWp != null)
                        {
                            // 保留原始 itr 的碰撞框，替换伤害字段，并将 kind 强制为普通攻击。
                            _runtimeItr.CopyFrom(itr);
                            itr = _runtimeItr;
                            itr.kind = 0;
                            itr.zwidth = srcWp.cover;
                            itr.dvx = srcWp.dvx;
                            itr.dvy = srcWp.dvy;
                            itr.dvz = srcWp.dvz;
                            itr.injury = srcWp.injury;
                            itr.fall = srcWp.fall;
                            itr.vaction = srcWp.vaction;
                            itr.arest = srcWp.arest;
                            itr.vrest = srcWp.vrest;
                            itr.effect = srcWp.effect;
                            itr.kill = srcWp.kill;
                            itr.bdefend = srcWp.bdefend;
                        }
                    }
                }
            }

            bool acceptHit  = false;
            bool isKnockdown = false;
            float efDvx      = 0f;
            float efDvy      = 0f;
            int inj          = 0;
            int effectNum    = 0;
            bool standardDamageApplied = false;

            int myState = _character.GetState();

            // State 10：被抓取。
            if (myState == LF2States.BeingCaught)
            {
                var catcherChar = _character.Catching as LF2Character;
                bool cHurtable  = catcherChar != null && catcherChar.caught_cpointhurtable();

                if (cHurtable)
                {
                    acceptHit = true;
                    isKnockdown |= HitFall(inj, ref efDvy, itr, attacker);
                }

                if (!cHurtable && _character.Catching != attacker)
                {
                    // skip
                }
                else
                {
                    acceptHit = true;
                    inj += Mathf.Abs(itr.injury);

                    if (itr.injury > 0)
                    {
                        int tar;
                        if (itr.vaction != 0)
                        {
                            tar = itr.vaction;
                        }
                        else
                        {
                            bool front  = (attackerPos.x > _character.PS.x) == (_character.PS.dir == "right");
                            var myCpoint = _character.CurrentFrame?.cpoint;
                            tar = front
                                ? (myCpoint?.fronthurtact ?? 0)
                                : (myCpoint?.backhurtact  ?? 0);
                        }
                        if (tar != 0) _character.ImmediateFrame(tar);
                    }
                }
            }

            // State 14：倒地期间无敌。
            else if (myState == LF2States.Lying)
            {
                // 倒地期间免疫所有伤害。
            }

            // State 19 + 攻击者 state 3000：fire-run 免疫。
            else if (myState == LF2States.FirenSpecific &&
                     ResolveAttackerState(attacker) == LF2States.ProjectileFlying)
            {
                return false;
            }

            // kind 5000-5999：直接扣 HP。
            else if (itr.kind >= 5000 && itr.kind < 6000)
            {
                acceptHit = true;
                int damage = itr.kind - 5000;
                _character.Health.HP = Mathf.Max(0, _character.Health.HP - damage);
            }

            // kind 6000-6999：直接跳转到指定帧。
            else if (itr.kind >= 6000 && itr.kind < 7000)
            {
                acceptHit = true;
                int targetFrame = itr.kind - 6000;
                if (_character.FrameCache.HasFrame(targetFrame))
                    _character.ImmediateFrame(targetFrame);
            }

            // 主命中流程：kind 0 / kind-4 / kind-9。
            else if (itr.kind == 0 || itr.kind == 9)
            {
                acceptHit = true;

                // 攻击方向取攻击者朝向。
                int attDir = attacker.Dirh();
                // C++ 当前基线：dvx 直接按攻击者朝向应用，不做额外反转。
                efDvx = (itr.dvx != 0) ? attDir * (float)itr.dvx : 0f;
                efDvy = (itr.dvy != 0) ? (float)itr.dvy : 0f;

                effectNum = itr.effect;

                if (itr.kind != 9 &&
                    LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, _character, itr))
                {
                    SimulationWorld world = _character.Match ?? attacker.Match;
                    world.DamageWriter.ApplyAlternateDamage(
                        world,
                        attacker,
                        _character,
                        _character.HitCounters,
                        itr);
                    _character.RecordKind0Hit(attacker, itr);
                    return true;
                }

                // C++ release collision.cpp:
                // victim.oid==300 时走专属受击跳转，不进入普通受击/击飞结算。
                // 当前帧 bdy.x>1000 时，将其解释为目标帧号并直接改写 frame，
                // 同时 attacker.frame_delay=3, victim.frame_delay=-3, victim.unk_364=1。
                int currentCharacterOid = _character.FrameCache?.Wrapper?.characterId ?? _character.ObjectId;
                if (currentCharacterOid == 300)
                {
                    LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
                    _character.HitCounters.SetHitStateCount(45);
                    var frameNow = _character.Frame?.D;
                    var futureFrame = _character.GetFrameDataById((_character.Frame?.N ?? 0) + 6);
                    int currentBdyX = (frameNow?.bodies != null && frameNow.bodies.Count > 0)
                        ? frameNow.bodies[0].x
                        : 0;

                    if (futureFrame?.bodies != null &&
                        futureFrame.bodies.Count > 0 &&
                        currentBdyX > 1000)
                    {
                        _character.RelationTeam = 1;
                        _character.DirectWriteFramePreserveWaitCounter(currentBdyX - 1000);
                        if (attacker != null)
                            attacker.FrameDelay = 3;
                        _character.FrameDelay = -3;
                    }

                    return true;
                }

                SimulationWorld standardWorld = _character.Match ?? attacker.Match;
                bool standardDamageSucceeded =
                    standardWorld?.DamageWriter.ApplyStandardCharacterDamage(
                        standardWorld,
                        attacker,
                        _character,
                        _character.HitCounters,
                        itr) == true;
                if (standardDamageSucceeded && itr.kind == 0)
                    _character.RecordKind0Hit(attacker, itr);
                return standardDamageSucceeded;
            }

            // Kind 6：命中确认标记。受击硬直帧会发出 kind=6，让原攻击者确认命中。
            // 当前对象作为攻击者 body 碰到该 itr 时，写入 HitConfirmEa。
            // 不走扣血、vrest、击退等结算。
            // 后续帧切换由状态逻辑处理。
            else if (itr.kind == 6)
            {
                _character.HitConfirmEa = 3;
                return true;   // 不扣血，不触发 vrest，直接返回。
            }

            // Kind 8：攻击者传送到受击者位置，受击者写回血计时。
            // C++ release apply_kind8：victim.heal_timer=itr.injury+1000，
            // attacker.frame=itr.dvx，attacker.x=victim.x，attacker.z=victim.z+1。
            else if (itr.kind == 8)
            {
                _character.HealTimer = itr.injury + 1000;
                if (attacker?.Runtime != null && _character.Runtime != null)
                {
                    attacker.DirectWriteRawFramePreserveWaitCounter(itr.dvx);
                    attacker.Runtime.X = _character.Runtime.X;
                    attacker.Runtime.Z = _character.Runtime.Z + 1f;
                    attacker.Runtime.XInt = _character.Runtime.XInt;
                    attacker.Runtime.ZInt = _character.Runtime.ZInt + 1;
                }
                return true;
            }

            // Kind 14：方向阻挡。
            // 根据 attacker 相对 victim 的位置设置本帧轴向阻挡标记。
            else if (itr.kind == 14)
            {
                if (_character.PS != null && attacker?.PS != null)
                {
                    double aix = attacker.PS.x;
                    double aiz = attacker.PS.z;
                    double vix = _character.PS.x;
                    double viz = _character.PS.z;

                    // C++ release apply_kind14 同时检查当前速度和击退速度。
                    if (aix > vix + 5f && (_character.PS.vx > 0f || _character.KnockbackVx > 0f)) _character.PS.xBoundPositive = true;
                    else if (aix < vix - 5f && (_character.PS.vx < 0f || _character.KnockbackVx < 0f)) _character.PS.xBoundNegative = true;

                    if (aiz > viz + 2f && (_character.PS.vz > 0f || _character.KnockbackVz > 0f)) _character.PS.zBoundPositive = true;
                    else if (aiz < viz - 2f && (_character.PS.vz < 0f || _character.KnockbackVz < 0f)) _character.PS.zBoundNegative = true;
                }
                return false;   // 不触发 vrest，每帧可持续生效。
            }

            // Kind 10/11：笛子效果。
            else if (itr.kind == 10 || itr.kind == 11)
            {
                if (itr.kind == 11 && _character.WeaponCount >= 0)
                    return false;

                _character.WeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                ApplyFluteCharacterForce();

                if (_character.KillCount == -1 &&
                    (_character.Match?.CurrentTickIndex ?? 0) % 12 == 0 &&
                    !LF2HitResolveRuntimeData.IsStepWaitGate(_character))
                {
                    LF2Entity holder = ResolveHolderCopyEntity(attacker);
                    if (holder != null)
                        holder.ComboCountAtk += 11;
                }

                SimulationWorld world = _character.Match ?? attacker?.Match;
                int damageStatIndex = _character.Unk344;
                if (world?.DamageStats != null &&
                    damageStatIndex > 0 &&
                    damageStatIndex < world.DamageStats.Length)
                {
                    world.DamageStats[damageStatIndex] += 11;
                }
                return true;
            }

            // Kind 15：旋风效果。
            // 按 attacker 与 victim 的相对位置推开 x/z 速度。
            else if (itr.kind == 15)
            {
                ApplyWhirlwindCharacterForce(attacker);
                return true;
            }

            // Kind 16：冰冻/扣蓝效果。
            else if (itr.kind == 16)
            {
                SimulationWorld world = _character.Match ?? attacker?.Match;
                return world?.DamageWriter.ApplyKind16(
                    world,
                    attacker,
                    _character,
                    itr) == true;
            }

            // 命中结算。
            if (acceptHit)
            {
                var attackerLiving = attacker as LF2LivingObject;
                if (attackerLiving != null) _character.Attacker = attackerLiving;

                // 击飞路径和普通路径使用不同 vrest 写入规则。
                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                if (attackerSlot >= 0)
                {
                    if (isKnockdown)
                        _character.ItrVrestUpdateKnockdown(attackerSlot, itr, true);
                    else
                        _character.ItrVrestUpdate(attackerSlot, itr, true);
                }

                // 攻击方碰撞豁免：arest 小于 4 且 vrest 为 0 时最少写 4，否则使用 itr.arest。
                // 该值写到攻击方的 HitCounters。
                int exemptVal = LF2Entity.ResolveArestCooldown(itr.arest, itr.vrest);
                attacker.AttackExempt = exemptVal;
                if (attacker.ItrRest != null)
                    attacker.ItrRest.Arest = exemptVal;

                // C++ release 中命中延迟与震动通道分离；Unity 侧只维护逻辑层 FrameDelay/ShakeTimer，
                // 具体视觉偏移由 SimulationTickDriver 在渲染层统一消费。

                // 所有命中路径都设置 FrameDelay。
                // 攻击方为 3，受击方击飞为 -3，普通受击为 -5。
                // 攻击方当前 FrameDelay 为负时不覆盖。
                // 受击方始终按当前命中类型覆盖。
                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                _character.FrameDelay = -3;

                // 攻击方未被抓取时，将 FrameDelay 传给 TrackerParent。
                LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);

                LF2HitResolveRuntimeData.ApplyCaughtVictimHurtFrame(
                    _character,
                    attacker,
                    _character.HitCounters.Fall);

                // 地面上 HitStateCount 足够高且 kind=7 时进入破防帧。
                if (!isKnockdown && _character.PS.vy == 0f &&
                    _character.HitCounters.HitStateCount >= 30 && itr.kind == 7)
                {
                    _character.ImmediateFrame(LF2StandardFrames.DefendBroken);
                }

                // 攻击者 state==1002 命中后反弹。
                // attacker.vx = -(victim.vx * 0.5)，attacker.vy = -3.5。
                if (ResolveAttackerState(attacker) == LF2States.WeaponThrowing) // 1002
                {
                    attacker.DirectWriteFramePreserveWaitCounter(attacker.BattleRandInt(0, 16));
                    attacker.Runtime.Vx = _character.KnockbackVx * -0.5f;
                    attacker.Runtime.Vy = -4.0f;
                    if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon &&
                        _character.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon)
                    {
                        attacker.KnockbackVx = -_character.KnockbackVx;
                    }
                }

                // 非击飞路径的 dvx 写入 KnockbackVx，不直接写 PS.vx。
                // PS.vx 由帧后处理根据 HitCount 统一写入。
                if (_character.HitCounters.Fall == 80)
                    _character.HitCounters.SetFall(0);
            }

            if (acceptHit && !standardDamageApplied)
                _character.ApplyHitInjury(inj);

            // 命中音效：击飞播放 006.wav，普通重击播放 001.wav。
            // 当前通过 SoundPlayer 播放。
            if (acceptHit && itr.kind == 0)
                _character.RecordKind0Hit(attacker, itr);

            return acceptHit;
        }

        private static int ResolveAttackerState(LF2Entity attacker)
        {
            return attacker?.GetState() ?? 0;
        }

        private LF2Entity ResolveHolderCopyEntity(LF2Entity attacker)
        {
            int holderSlot = attacker?.HolderCopySlot ?? -1;
            if (holderSlot < 0)
                return null;

            return _character.Match?.FindEntityByRuntimeSlotForQuery(holderSlot);
        }

        private void ApplyWhirlwindCharacterForce(LF2Entity attacker)
        {
            if (attacker == null || _character.Runtime == null)
                return;

            _character.KnockbackVx = _character.Runtime.Vx +
                (_character.Runtime.XInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            _character.Runtime.Vx = _character.KnockbackVx;
            _character.KnockbackVz = _character.Runtime.Vz +
                (_character.Runtime.ZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            _character.Runtime.Vz = _character.KnockbackVz;
            ApplyWhirlwindAirStep(3.0);
            _character.RefreshRuntimeSnapshot();
        }

        private void ApplyFluteCharacterForce()
        {
            const double factor = 0.9345794392523364;
            _character.KnockbackVx = _character.Runtime.Vx * factor;
            _character.Runtime.Vx = _character.KnockbackVx;
            _character.KnockbackVz = _character.Runtime.Vz * factor;
            _character.Runtime.Vz = _character.KnockbackVz;
            _character.DirectWriteRawFramePreserveWaitCounter(182);
            ApplyWhirlwindAirStep(3.0);
        }

        private void ApplyWhirlwindAirStep(double vyStep)
        {
            if (_character.GetRuntimeYInt() >= -2)
            {
                _character.Runtime.Y = -2f;
                _character.Runtime.YInt = -2;
                _character.Runtime.Vy = -6f;
                return;
            }

            if (_character.Runtime.Vy > -6f)
            {
                _character.Runtime.Vy -= vyStep;
                _character.KnockbackVy = _character.Runtime.Vy;
            }
        }

        // HitFall：fall 累积与受击帧选择
        // ------------------------------------------------------------
        // 根据 fall 档位选择轻伤、中伤、重伤或击飞。
        // 击飞时进入 HitFallDown。

        /// <summary>
        /// 处理 fall 累积和受击帧切换。
        /// 强制击飞条件必须在 AddFall 之前判断。
        /// fall > 60 直接击飞；fall > 40 进入重伤；fall > 20 进入中伤。
        /// 空中受击在中伤/重伤档位会升级为击飞。
        /// 每次命中后把 fall 钳制到当前档位上限，避免下一次命中跨档异常。
        /// </summary>
        private bool HitFall(int currentInj, ref float efDvx, ref float efDvy,
                             InteractionArea itr, LF2Entity attacker)
        {
            int fallInc = (itr.fall != 0) ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int prevState = _character.GetFrameDataById(_character.Frame?.Prev ?? 0)?.state ?? 0;
            int prev2State = _character.GetFrameDataById(
                    _character.Runtime?.PrevFrame2 ?? 0)?.state
                ?? _character.Frame?.Prev2D?.state
                ?? 0;

            bool forceKnockback = _character.Health.HP <= 0 ||
                                  itr.effect == 4 ||
                                  prevState == LF2States.Frozen ||
                                  prev2State == LF2States.Falling;

            if (forceKnockback)
            {
                _character.HitCounters.AddFall(fallInc);
                return HitFallDown(ref efDvx, ref efDvy, itr, default);
            }

            _character.HitCounters.AddFall(fallInc);
            int fall = _character.HitCounters.Fall;

            // fall > 60 时直接击飞。
            if (fall > 60)
                return HitFallDown(ref efDvx, ref efDvy, itr, default);

            // fall > 40 时进入重伤 frame 226；空中则升级为击飞。
            if (fall > 40)
            {
                _character.HitCounters.SetFall(60); // 钳制到重伤档位上限 60。
                _character.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.Injured6);
                if (_character.GetRuntimeYInt() < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, default);
                return false;
            }

            // fall > 20 时进入中伤 frame 222/224；空中则升级为击飞。
            if (fall > 20)
            {
                _character.HitCounters.SetFall(40); // 钳制到中伤档位上限 40。
                bool sameDir = attacker != null && attacker.Dirh() == _character.Dirh();
                _character.DirectWriteFramePreserveWaitCounter(
                    sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (_character.GetRuntimeYInt() < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, default);
                return false;
            }

            // fall > 0 时进入轻伤 frame 220；空中升级到中伤帧但不击飞。
            if (fall > 0)
            {
                _character.HitCounters.SetFall(20); // 钳制到轻伤档位上限 20。
                _character.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.Injured);
                if (_character.GetRuntimeYInt() < 0)
                {
                    bool sameDir = attacker != null && attacker.Dirh() == _character.Dirh();
                    _character.DirectWriteFramePreserveWaitCounter(
                        sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                }
            }
            return false;
        }

        // 被抓路径只需要传递 y 向击退，保留这个轻量重载。
        private bool HitFall(int currentInj, ref float efDvy,
                             InteractionArea itr, LF2Entity attacker)
        {
            float efDvxDummy = 0f;
            return HitFall(currentInj, ref efDvxDummy, ref efDvy, itr, attacker);
        }

        // HitFallDown：击飞倒地处理
        // ------------------------------------------------------------
        // fall==80 时选择倒地帧，并累积击退速度。
        // PS.vx/PS.vy 仍由帧后处理统一写入。

        /// <summary>
        /// fall==80 击飞倒地处理。
        /// 帧选择依据 KnockbackVx 和朝向，不直接使用 PS.vx。
        /// 朝右且 kb>0 或朝左且 kb<0 时使用背面倒地帧 186。
        /// 否则使用正面倒地帧 180。
        /// dvy 非零时累积到 KnockbackVy，并在 y+vy 越界时钳制为 -12。
        /// dvy 为 0 时默认 KnockbackVy -= 7。
        /// KnockbackVx 在本函数累积，实际速度由帧后处理写入。
        /// </summary>
        private bool HitFallDown(ref float efDvx, ref float efDvy,
                                 InteractionArea itr, Vector3 attackerPos)
        {
            _character.HitCounters.ResetFall();

            // vy 处理：写 KnockbackVy，不直接写 PS.vy。
            // PS.vy 由帧后处理统一写入。
            // 这样可保持同帧多次命中时的速度合并规则。
            if (itr.dvy != 0)
            {
                _character.KnockbackVy += itr.dvy;
                if ((int)(_character.KnockbackVy + _character.GetRuntimeYInt()) > 0)
                    _character.KnockbackVy = 12.0f;
                efDvy = itr.dvy;
            }
            else
            {
                _character.KnockbackVy -= 7.0f;
                efDvy = -7.0f;
            }

            return true;
        }

        // 只传 y 向击退的受击路径使用这个轻量重载。
        private bool HitFallDown(ref float efDvy, InteractionArea itr, Vector3 attackerPos)
        {
            float efDvxDummy = 0f;
            return HitFallDown(ref efDvxDummy, ref efDvy, itr, attackerPos);
        }

        // 辅助：方向判断
        // ------------------------------------------------------------
        // 判断攻击者方向与受击者朝向是否相同。

        private bool attacker_dir_matches_victim(Vector3 attackerPos)
        {
            // 当前 C# 逻辑用攻击者相对位置推断朝向关系。
            // victimFacingRight 来自当前 PS.dir。
            // 返回 true 时使用正面/同向受击帧。
            bool attackerFacingRight = attackerPos.x > _character.PS.x;
            bool victimFacingRight   = _character.PS.dir == "right";
            return attackerFacingRight == victimFacingRight;
        }

        // HitPostEffect：武器掉落与冰火效果
        // ------------------------------------------------------------
        // 这里处理命中后的视觉/状态副作用。

        private void HitPostEffect(int effectNum, PhysicsState.BattleVolume rect,
                                   float efDvx, float efDvy, bool defended,
                                   Vector3 attackerPos, int myState)
        {
            if (defended)
            {
                // 格挡后不额外切换火冰状态；震动只通过逻辑层 ShakeTimer 通道传递。
                return;
            }

            int nextFrame = _character.Trans.Next;

            switch (effectNum)
            {
                case 0:
                case 1:
                    if (nextFrame == LF2StandardFrames.FallingFront ||
                        nextFrame == LF2StandardFrames.FallingBack)
                    {
                        // 倒地时按当前速度脱手武器。
                        _character.DropWeapon((float)_character.PS.vx, (float)_character.PS.vy);
                    }
                    break;

                case 2:
                case 21:
                case 22:
                case 23:
                    _character.DropWeapon((float)_character.PS.vx, (float)_character.PS.vy);
                    goto case 20;

                case 20:
                    if (myState != LF2States.Burning && myState != LF2States.FirenSpecific)
                    {
                        _character.ImmediateFrame(LF2StandardFrames.Fire);
                    }
                    break;

                case 3:
                case 30:
                    _character.DropWeapon((float)_character.PS.vx, (float)_character.PS.vy);
                    if (myState != LF2States.Frozen)
                    {
                        _character.ImmediateFrame(LF2StandardFrames.MpDrain);
                        // 冰冻/扣蓝效果切到 MpDrain 帧。
                    }
                    else
                    {
                        _character.ImmediateFrame(LF2StandardFrames.FallingFront2);
                        // 已冻结状态下切到 FallingFront2。
                    }
                    break;

                case 4:
                    _character.DropWeapon((float)_character.PS.vx, (float)_character.PS.vy);
                    break;
            }
        }
    }
}
