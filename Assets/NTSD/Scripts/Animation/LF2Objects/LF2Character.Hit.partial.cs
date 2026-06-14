using UnityEngine;
using NTSD.App;
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
    public partial class LF2Character : LF2LivingObject
    {
        // 主流程
        // ------------------------------------------------------------
        // 命中入口按 itr.kind 和当前 state 分流。

        public override bool Hit(InteractionArea itr, LF2Entity attacker,
                                 UnityEngine.Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!base.Hit(itr, attacker, attackerPos, vol)) return false;

            // Kind 5：委托攻击，使用 TrackerParent 的 wpoint.attacking 替换伤害字段。
            // 条件：victim.GrabbedBy<0、TrackerParent.TrackerFlag==attacker.StableId、父对象不是自己。
            // 替换后走普通 kind=0 命中结算。
            if (itr.kind == 5 && GrabbedBy < 0 && TrackerParent != null)
            {
                var tp = TrackerParent as LF2Entity;
                if (tp != null && tp.TrackerFlag == attacker.StableId && tp != this)
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
                            itr = new InteractionArea
                            {
                                kind    = 0,
                                x       = itr.x,
                                y       = itr.y,
                                w       = itr.w,
                                h       = itr.h,
                                zwidth  = srcWp.cover,
                                dvx     = srcWp.dvx,
                                dvy     = srcWp.dvy,
                                dvz     = srcWp.dvz,
                                injury  = srcWp.injury,
                                fall    = srcWp.fall,
                                vaction = srcWp.vaction,
                                arest   = srcWp.arest,
                                vrest   = srcWp.vrest,
                                effect  = srcWp.effect,
                                kill    = srcWp.kill,
                                bdefend = srcWp.bdefend,
                            };
                        }
                    }
                }
            }

            bool acceptHit  = false;
            bool defended    = false;
            bool isKnockdown = false;
            float efDvx      = 0f;
            float efDvy      = 0f;
            int inj          = 0;
            int effectNum    = 0;

            int myState = GetState();

            // State 10：被抓取。
            if (myState == LF2States.BeingCaught)
            {
                var catcherChar = Catching as LF2Character;
                bool cHurtable  = catcherChar != null && catcherChar.caught_cpointhurtable();

                if (cHurtable)
                {
                    acceptHit = true;
                    isKnockdown |= HitFall(inj, ref efDvy, itr, attackerPos);
                }

                if (!cHurtable && Catching != attacker)
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
                            bool front  = (attackerPos.x > PS.x) == (PS.dir == "right");
                            var myCpoint = CurrentFrame?.cpoint;
                            tar = front
                                ? (myCpoint?.fronthurtact ?? 0)
                                : (myCpoint?.backhurtact  ?? 0);
                        }
                        if (tar != 0) ImmediateFrame(tar);
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
                Health.HP = Mathf.Max(0, Health.HP - damage);
            }

            // kind 6000-6999：直接跳转到指定帧。
            else if (itr.kind >= 6000 && itr.kind < 7000)
            {
                acceptHit = true;
                int targetFrame = itr.kind - 6000;
                if (FrameCache.GetFrameDataById(targetFrame) != null)
                    ImmediateFrame(targetFrame);
            }

            // 主命中流程：kind 0 / kind-4 / kind-9。
            else if (itr.kind == 0 ||
                     itr.kind == 4 ||
                     itr.kind == 9)
            {
                acceptHit = true;

                // 攻击方向取攻击者朝向。
                int attDir = attacker.Dirh();
                // C++ 当前基线：dvx 直接按攻击者朝向应用，不做额外反转。
                efDvx = (itr.dvx != 0) ? attDir * (float)itr.dvx : 0f;
                efDvy = (itr.dvy != 0) ? (float)itr.dvy : 0f;

                // 重武器命中时，dvx/dvy/injury/fall 减半。
                // injury/fall 使用整数右移，与 C++ release 的整型语义一致。
                if (IsHeavyWeaponAttacker(attacker))
                {
                    efDvx *= 0.5f;
                    efDvy *= 0.5f;
                    itr = itr.ShallowCopy();
                    itr.injury >>= 1;
                    itr.fall >>= 1;
                }

                effectNum = itr.effect;

                if (myState == LF2States.Frozen && effectNum == 30) return false;
                if ((myState == LF2States.Burning || myState == LF2States.FirenSpecific) &&
                    (effectNum == 20 || effectNum == 21)) return false;

                // 防御分支。
                // 条件：防御状态、伤害不超过 60、面向攻击者。
                // 特殊防御角色直接成功；普通角色要求攻击者仍存活。
                // 防御成功后按防御吸收表削减 dvx。
                // 防御破坏由 Bdefend 累积超过阈值触发。
                // 防御失败会进入普通受击逻辑。
                int victimOid = _FrameDataWrapper?.characterData?.type_sub ?? 0;
                bool isSpecialDefendOid = (victimOid == 124 || victimOid == 220 || victimOid == 221 || victimOid == 222);
                bool canDefend = myState == LF2States.Defending
                                 && itr.injury <= 60
                                 && (attackerPos.x > PS.x) == (PS.dir == "right");
                var attackerLivingForDefend = attacker as LF2LivingObject;
                bool defenderWins = canDefend
                                    && (isSpecialDefendOid || (attackerLivingForDefend?.Health?.HP ?? 1) > 0);

                if (defenderWins)
                {
                    if (itr.injury != 0)
                        inj += Mathf.RoundToInt(NTSDGlobal.Gameplay.DefendInjuryFactor * itr.injury);

                    if (itr.bdefend != 0) HitCounters.AddBdefend(itr.bdefend);

                    if (HitCounters.Bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                        ImmediateFrame(LF2StandardFrames.DefendBroken);
                    else
                        ImmediateFrame(LF2StandardFrames.Defend1);

                    if (efDvx != 0f)
                    {
                        float absorbed = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.DefendAbsorb, efDvx);
                        efDvx += (efDvx > 0f ? -1f : 1f) * absorbed;
                    }
                    efDvy = 0f;

                    if (Health.HP - inj <= 0)
                        isKnockdown |= HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                    else
                        defended = true;
                }
                // 非防御分支。
                else
                {
                    if (IsHeldHeavyWeapon())
                        DropWeapon(0f, 0f);

                    // type_sub 免疫规则。
                    int victimTypeSub = victimOid; // victimOid 已在防御判定处计算。
                    int attackerTypeSub = (attacker as LF2LivingObject)?._FrameDataWrapper?.characterData?.type_sub ?? 0;
                    int effectDiv10 = itr.effect / 10;
                    bool attackerIsSpecial = (attackerTypeSub == 0xD6 || attackerTypeSub == 0xD0);
                    bool effectAllowed = (effectDiv10 == 2 || effectDiv10 == 3);

                    if (victimTypeSub == 0x25 && HitCounters.HitStateCount <= 15) // type_sub=37
                    {
                        if (!effectAllowed && !attackerIsSpecial) return false;
                    }
                    else if (victimTypeSub == 6 && HitCounters.HitStateCount <= 1) // type_sub=6 (Sasuke)
                    {
                        if (!effectAllowed && !attackerIsSpecial)
                        {
                            // Sasuke 特判：前 20 帧或特定状态下跳过普通命中。
                            if (Frame.N < 20) return false;
                            int fstate = Frame?.D?.state ?? -1;
                            if (fstate == 5 || fstate == 4 || fstate == 7) return false;
                        }
                    }
                    else if (victimTypeSub == 0x34 && HitCounters.HitStateCount <= 15) // type_sub=52
                    {
                        if (!attackerIsSpecial) return false;
                    }

                    // 伤害按 MaxMP 缩放。
                    // itr.injury 是缩放输入；MaxMP 为 0 时直接使用 injury。
                    int mpDmg = (Health.MaxMP > 0) ? itr.injury * 100 / Health.MaxMP : itr.injury;
                    if (mpDmg != 0) inj += mpDmg;

                    // 普通受击先把 HitStateCount 设为 45。
                    HitCounters.SetHitStateCount(45);

                    // C++ release collision.cpp:
                    // victim.oid==300 时走专属受击跳转，不进入普通受击/击飞结算。
                    // 当前帧 bdy.x>1000 时，将其解释为目标帧号并直接改写 frame，
                    // 同时 attacker.frame_delay=3, victim.frame_delay=-3, victim.unk_364=1。
                    if (ObjectId == 300)
                    {
                        var frameNow = Frame?.D;
                        var futureFrame = GetFrameDataById((Frame?.N ?? 0) + 6);
                        int currentBdyX = (frameNow?.bodies != null && frameNow.bodies.Count > 0)
                            ? frameNow.bodies[0].x
                            : 0;

                        if (futureFrame?.bodies != null &&
                            futureFrame.bodies.Count > 0 &&
                            currentBdyX > 1000)
                        {
                            RelationTeam = 1;
                            DirectWriteFramePreserveWaitCounter(currentBdyX - 1000);
                            if (attacker != null)
                                attacker.FrameDelay = 3;
                            FrameDelay = -3;
                        }

                        return true;
                    }

                    isKnockdown |= HitFall(inj, ref efDvx, ref efDvy, itr, attackerPos);

                    // 非击飞路径才追加 bdefend 到 HitStateCount。
                    if (!isKnockdown)
                        HitCounters.AddHitStateCount(itr.bdefend);
                }
            }

            // Kind 6：命中确认标记。受击硬直帧会发出 kind=6，让原攻击者确认命中。
            // 当前对象作为攻击者 body 碰到该 itr 时，写入 HitConfirmEa。
            // 不走扣血、vrest、击退等结算。
            // 后续帧切换由状态逻辑处理。
            else if (itr.kind == 6)
            {
                HitConfirmEa = 3;
                return true;   // 不扣血，不触发 vrest，直接返回。
            }

            // Kind 8：攻击者传送到受击者位置，受击者写回血计时。
            // C++ release apply_kind8：victim.heal_timer=itr.injury+1000，
            // attacker.frame=itr.dvx，attacker.x=victim.x，attacker.z=victim.z+1。
            else if (itr.kind == 8)
            {
                HealTimer = itr.injury + 1000;
                if (attacker?.PS != null && PS != null)
                {
                    attacker.PS.x = PS.x;
                    attacker.PS.z = PS.z + 1f;
                }
                if (itr.dvx > 0)
                    attacker?.ImmediateFrame(itr.dvx);
                return true;
            }

            // Kind 14：方向阻挡。
            // 根据 attacker 相对 victim 的位置设置本帧轴向阻挡标记。
            else if (itr.kind == 14)
            {
                if (PS != null && attacker?.PS != null)
                {
                    float aix = attacker.PS.x;
                    float aiz = attacker.PS.z;
                    float vix = PS.x;
                    float viz = PS.z;

                    // C++ release apply_kind14 同时检查当前速度和击退速度。
                    if (aix > vix + 5f && (PS.vx > 0f || KnockbackVx > 0f)) PS.xBoundPositive = true;
                    else if (aix < vix - 5f && (PS.vx < 0f || KnockbackVx < 0f)) PS.xBoundNegative = true;

                    if (aiz > viz + 2f && (PS.vz > 0f || KnockbackVz > 0f)) PS.zBoundPositive = true;
                    else if (aiz < viz - 2f && (PS.vz < 0f || KnockbackVz < 0f)) PS.zBoundNegative = true;
                }
                return false;   // 不触发 vrest，每帧可持续生效。
            }

            // Kind 10/11：笛子效果。
            else if (itr.kind == 10 || itr.kind == 11)
            {
                if (itr.kind == 11 && WeaponCount >= 0)
                    return false;

                WeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                FluteForce();
                if (myState == LF2States.Falling)
                {
                    inj = itr.injury * 2;
                    acceptHit = true;
                }
            }

            // Kind 15：旋风效果。
            // 按 attacker 与 victim 的相对位置推开 x/z 速度。
            else if (itr.kind == 15)
            {
                if (PS != null && attacker?.PS != null)
                {
                    PS.vx += (attacker.PS.x >= PS.x) ? 1f : -1f;
                    PS.vz += (attacker.PS.z >= PS.z) ? 0.5f : -0.5f;
                }
            }

            // Kind 16：冰冻/扣蓝效果。
            else if (itr.kind == 16)
            {
                ImmediateFrame(LF2StandardFrames.MpDrain);
                // 伤害使用与 kind=0 相同的 MaxMP 缩放公式。
                // 这里不播放独立命中音效；震动由 ShakeTimer 写入后交给渲染层消费。
                inj = (Health.MaxMP > 0) ? itr.injury * 100 / Health.MaxMP : itr.injury;
                acceptHit = true;
            }

            // 命中结算。
            if (acceptHit)
            {
                var attackerLiving = attacker as LF2LivingObject;
                if (attackerLiving != null) Attacker = attackerLiving;

                // 击飞路径和普通路径使用不同 vrest 写入规则。
                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                if (attackerSlot >= 0)
                {
                    if (isKnockdown)
                        ItrVrestUpdateKnockdown(attackerSlot, itr, true);
                    else
                        ItrVrestUpdate(attackerSlot, itr, true);
                }

                // 攻击方碰撞豁免：vrest 足够大或 arest 非零时使用 itr.vrest，否则最少 4。
                // 该值写到攻击方的 HitCounters。
                int exemptVal = (itr.vrest >= 4 || itr.arest != 0) ? itr.vrest : 4;
                attackerLiving?.HitCounters?.SetAttackExempt(exemptVal);

                // C++ release 中命中延迟与震动通道分离；Unity 侧只维护逻辑层 FrameDelay/ShakeTimer，
                // 具体视觉偏移由 SimulationTickDriver 在渲染层统一消费。

                // 所有命中路径都设置 FrameDelay。
                // 攻击方为 3，受击方击飞为 -3，普通受击为 -5。
                // 攻击方当前 FrameDelay 为负时不覆盖。
                // 受击方始终按当前命中类型覆盖。
                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                FrameDelay = isKnockdown ? -3 : -5;

                // 攻击方未被抓取时，将 FrameDelay 传给 TrackerParent。
                if (attacker.GrabbedBy < 0 && attacker.TrackerParent != null)
                    attacker.TrackerParent.FrameDelay = FrameDelay;

                // 非击飞路径清空 C++ release Entity::attacking。
                if (!isKnockdown)
                    AttackingCounter = 0;

                // 地面上 HitStateCount 足够高且 kind=7 时进入破防帧。
                if (!isKnockdown && PS.vy == 0f &&
                    HitCounters.HitStateCount >= 30 && itr.kind == 7)
                {
                    ImmediateFrame(LF2StandardFrames.DefendBroken);
                }

                // 攻击者 state==1002 命中后反弹。
                // attacker.vx = -(victim.vx * 0.5)，attacker.vy = -3.5。
                if (ResolveAttackerState(attacker) == LF2States.WeaponThrowing) // 1002
                {
                    var aps = attacker.PS;
                    if (aps != null)
                    {
                        aps.vx = -(PS.vx * 0.5f);
                        aps.vy = -3.5f; // 0xC00C000000000000 = -3.5
                    }
                }

                // 非击飞路径的 dvx 写入 KnockbackVx，不直接写 PS.vx。
                // PS.vx 由帧后处理根据 HitCount 统一写入。
                if (!defended && efDvx != 0f)
                {
                    KnockbackVx += efDvx;
                    HitCount++;
                }
            }

            if (acceptHit)
                Injury(inj);

            if (acceptHit && itr.kind == 0)
            {
                HitPostEffect(effectNum, vol, efDvx, efDvy, defended, attackerPos, myState);
            }

            // 命中音效：击飞播放 006.wav，普通重击播放 001.wav。
            // 当前通过 SoundPlayer 播放。
            if (acceptHit && itr.kind == 0)
            {
                string hitSfx = isKnockdown
                    ? NTSDGlobal.Sound.HitKnockdown
                    : NTSDGlobal.Sound.HitNormal;
                AppManager.Instance?.SoundPlayer?.PlaySfx(hitSfx);
            }

            // kind=0 且 effect 不为 6/23 时生成命中 spark。
            if (acceptHit && itr.kind == 0 && effectNum != 6 && effectNum != 23)
            {
                SpawnSpark(itr, attacker, attackerPos, vol);
            }

            return acceptHit;
        }

        private static int ResolveAttackerState(LF2Entity attacker)
        {
            if (attacker is LF2WeaponBase weapon)
                return weapon.Runtime?.WeaponState is int state and not 0 ? state : weapon.GetState();

            return attacker?.GetState() ?? 0;
        }

        // 受伤数值结算
        // ------------------------------------------------------------
        // 扣血由基类处理，这里只追加正式逻辑中的 PP 回复。

        protected override void Injury(int inj)
        {
            base.Injury(inj);
            // 受击后 PP 按 injury/3 回复，并限制到 MaxPP。
            if (inj > 0)
                Health.PP = System.Math.Min(Health.PP + inj / 3, Health.MaxPP);
            // C++ release 的攻击/受击计数由命中结算路径维护；这里不追加额外统计回调。
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
                             InteractionArea itr, Vector3 attackerPos)
        {
            int fallInc = (itr.fall != 0) ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int state   = GetState();

            // 强制击飞判定在 fall 累积之前执行。
            bool forceKnockback = (Health.HP - currentInj <= 0)
                                  || (state == LF2States.Falling)
                                  || (state == LF2States.Frozen)
                                  || (itr.fall == 100); // itr.fall==100 强制击飞。

            if (forceKnockback)
            {
                HitCounters.AddFall(fallInc);
                return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
            }

            HitCounters.AddFall(fallInc);
            int fall = HitCounters.Fall;

            // fall > 60 时直接击飞。
            if (fall > 60)
                return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);

            // fall > 40 时进入重伤 frame 226；空中则升级为击飞。
            if (fall > 40)
            {
                HitCounters.SetFall(60); // 钳制到重伤档位上限 60。
                ImmediateFrame(LF2StandardFrames.Injured6);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 20 时进入中伤 frame 222/224；空中则升级为击飞。
            if (fall > 20)
            {
                HitCounters.SetFall(40); // 钳制到中伤档位上限 40。
                bool sameDir = attacker_dir_matches_victim(attackerPos);
                ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 0 时进入轻伤 frame 220；空中升级到中伤帧但不击飞。
            if (fall > 0)
            {
                HitCounters.SetFall(20); // 钳制到轻伤档位上限 20。
                ImmediateFrame(LF2StandardFrames.Injured);
                if (PS.vy < 0)
                {
                    bool sameDir = attacker_dir_matches_victim(attackerPos);
                    ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                }
            }
            return false;
        }

        // 被抓路径只需要传递 y 向击退，保留这个轻量重载。
        private bool HitFall(int currentInj, ref float efDvy,
                             InteractionArea itr, Vector3 attackerPos)
        {
            float efDvxDummy = 0f;
            return HitFall(currentInj, ref efDvxDummy, ref efDvy, itr, attackerPos);
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
            HitCounters.ResetFall();

            // 倒地帧由 KnockbackVx 与当前朝向决定。
            bool facingRight = PS.dir == "right";
            float kb = KnockbackVx;
            bool flyingBack = (facingRight && kb > 0f) || (!facingRight && kb < 0f);
            int fallFrame = flyingBack ? LF2StandardFrames.FallingBack   // 186
                                       : LF2StandardFrames.FallingFront; // 180
            ImmediateFrame(fallFrame);

            // vy 处理：写 KnockbackVy，不直接写 PS.vy。
            // PS.vy 由帧后处理统一写入。
            // 这样可保持同帧多次命中时的速度合并规则。
            if (itr.dvy != 0)
            {
                KnockbackVy += itr.dvy;
                // y + KnockbackVy > 0 时钳制为 -12。
                if ((int)PS.y + (int)KnockbackVy > 0)
                    KnockbackVy = -12.0f;
                efDvy = itr.dvy;
            }
            else
            {
                KnockbackVy -= 7.0f;
                efDvy = -7.0f;
            }

            // KnockbackVx/Vy 是帧内累积值，PS.vx/vy 由帧后处理根据 HitCount 写入。
            KnockbackVx += efDvx;
            HitCount++;

            efDvx = 0f;
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
            bool attackerFacingRight = attackerPos.x > PS.x;
            bool victimFacingRight   = PS.dir == "right";
            return attackerFacingRight == victimFacingRight;
        }

        // 视觉效果
        // ------------------------------------------------------------
        // 命中 spark 由 SpawnSpark 生成。

        public override void VisualEffectCreate(int num, PhysicsState.BattleVolume rect,
                                                bool righttip = false, int variant = 0,
                                                bool withSound = false)
        {
            // 保留基类虚方法；当前 spark 生成集中在 SpawnSpark。
        }

        /// <summary>
        /// 生成命中 spark。
        /// timer 初始值：fall > 60 时使用 itrIndex*20，否则 itrIndex*4+10。
        /// 坐标基于攻击者 itr 框、受击者位置和随机偏移计算。
        /// </summary>
        private void SpawnSpark(InteractionArea itr, LF2Entity attacker,
                                Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            // timer 初始值按当前 itr index 和 fall 档位计算。
            // v_5C 是当前 itr 下标。
            // fall > 60：大 spark，计时为 v_5C * 20。
            // fall <= 60：小 spark，计时为 v_5C * 4 + 10。
            // 渲染层会根据 timer 推进 spark 动画。
            // 这里只负责写入 spark slot。
            int fall = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int sparkPhase = itr.effect == 1 ? 1 : 0;
            int timerInitial = fall > 60
                ? sparkPhase * 20
                : sparkPhase * 20 + 10;

            // spark_x 按攻击者朝向和 itr 横向范围计算。
            // spark_y 先在角色高度范围内夹取，再叠加随机偏移。
            // spark_y_stored = attacker.z + edi + Random(9) - 4。
            // edi 是基于 itr.y、itr.h、attacker.y 和 centery 得到的高度偏移。
            // Unity worldY 最终由 SparkRenderer 使用 (sz + sy) / 100。
            // 因此 sz 保存深度 z，sy 保存跳跃高度相关偏移。
            // SparkRenderer 负责把两者合成实际屏幕 Y。
            float sx;
            float sy;
            float sz;

            if (attacker?.PS != null)
            {
                var atk     = attacker.PS;
                int centerx = attacker.Frame.D?.centerx ?? 0;
                int centery = attacker.Frame.D?.centery ?? 0;
                float itrW  = vol.w;
                float itrX  = itr.x;   // dat 原始 itr.x。
                float itrY  = itr.y;   // dat 原始 itr.y。

                // spark_x 方向公式：朝右 => atk.x - centerx + itr.w + itr.x。
                //                    朝左 => atk.x + centerx - itr.w - itr.x。
                if (attacker.Dirh() > 0) // 朝右
                {
                    sx = atk.x - centerx + itrW + itrX;
                    if (sx > PS.x) sx = PS.x;
                }
                else
                {
                    sx = atk.x + centerx - itrW - itrX;
                    if (sx < PS.x) sx = PS.x;
                }

                // spark_y 基于 itr.y、itr.h 和角色中心高度计算。
                float baseY = atk.y + (vol.h * 0.5f) + itrY - centery;
                float lower = PS.y - centery;
                float upper = PS.y;
                if (baseY < lower)       baseY = (lower + baseY) * 0.5f;
                else if (baseY > upper)  baseY = (upper + baseY) * 0.5f;

                // 随机偏移：两次 Random(9)，范围 0-8，再各自减 4。
                float rand1 = RandInt(0, 9);
                float rand2 = RandInt(0, 9);
                sy = atk.z + baseY + rand1 - 4f;   // C++ hit_record_z：深度 z + 高度偏移 + 随机偏移。
                sx += rand2 - 4f;
                sz = atk.z;                // attacker.PS.z，表示深度。
            }
            else
            {
                sx = PS.x;
                sy = PS.z + PS.y - 4f;
                sz = PS.z;
            }

            LF2Entity sparkOwner = SelectSparkOwner(attacker);
            int currentRenderFrame = Match?.SparkRenderFrame ?? -1;
            sparkOwner.AddSparkSlot(timerInitial, sx, sy, sz, currentRenderFrame);
        }

        private LF2Entity SelectSparkOwner(LF2Entity attacker)
        {
            if (attacker?.PS == null || PS == null)
                return this;

            if (attacker.PS.z > PS.z)
                return attacker;

            if (Mathf.Approximately(attacker.PS.z, PS.z))
            {
                int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
                int victimSlot = Runtime?.SlotIndex ?? -1;
                if (attackerSlot > victimSlot)
                    return attacker;
            }

            return this;
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

            int nextFrame = Trans.Next;

            switch (effectNum)
            {
                case 0:
                case 1:
                    if (nextFrame == LF2StandardFrames.FallingFront ||
                        nextFrame == LF2StandardFrames.FallingBack)
                    {
                        // 倒地时按当前速度脱手武器。
                        DropWeapon(PS.vx, PS.vy);
                    }
                    break;

                case 2:
                case 21:
                case 22:
                case 23:
                    DropWeapon(PS.vx, PS.vy);
                    goto case 20;

                case 20:
                    if (myState != LF2States.Burning && myState != LF2States.FirenSpecific)
                    {
                        ImmediateFrame(LF2StandardFrames.Fire);
                    }
                    break;

                case 3:
                case 30:
                    DropWeapon(PS.vx, PS.vy);
                    if (myState != LF2States.Frozen)
                    {
                        ImmediateFrame(LF2StandardFrames.MpDrain);
                        // 冰冻/扣蓝效果切到 MpDrain 帧。
                    }
                    else
                    {
                        ImmediateFrame(LF2StandardFrames.FallingFront2);
                        // 已冻结状态下切到 FallingFront2。
                    }
                    break;

                case 4:
                    DropWeapon(PS.vx, PS.vy);
                    break;
            }
        }
    }
}
