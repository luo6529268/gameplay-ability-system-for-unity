using UnityEngine;
using NTSD.App;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2Character 受击系统
    /// 权威来源：反汇编 Entity_AI_Update (sub_42C8C0) + FLF character.prototype.hit
    ///
    /// 关键对齐点（反汇编 vs 旧C#的差距修复）：
    ///   1. efDvx/efDvy 现在真正写入 PS.vx/PS.vy
    ///   2. fall==80 时倒地方向判断：基于被击者自身 vx 方向（反汇编逻辑）
    ///   3. fall==80 时 vy 处理：dvy!=0 则 vy+=dvy 再钳制；dvy==0 则 vy-=7
    ///   4. 攻击者 state==1002 命中后：攻击者速度反弹（vx=-victim.vx*0.5, vy=-3.5）
    ///   5. FrameDelay：所有命中路径都设 attacker=3/victim=-3；击飞路径 victim=-5
    ///   6. dvx 默认兜底：fall==80 且 dvx==0 时，被击者 vx += ±5.0
    ///   7. 空中升级条件：vy < 0（速度向上），非 y < 0（高度）
    ///   8. HitStateCount：+= itr.fall 累加，非直接赋 45
    ///   9. HitStateCount >= 30 + kind=7 + 地面 → frame 112（防御破碎）
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ─────────────────────────────────────────────────────────────────
        // 主方法
        // ─────────────────────────────────────────────────────────────────

        public override bool Hit(InteractionArea itr, LF2Entity attacker,
                                 UnityEngine.Vector3 attackerPos, PhysicsState.FlfVolume vol)
        {
            if (!base.Hit(itr, attacker, attackerPos, vol)) return false;

            // ── Kind 5: 委托攻击 itr 替换（反汇编 0x0042CA30~0x0042CADC）──
            // 条件：itr.kind==5 AND victim.GrabbedBy<0 AND TrackerParent.TrackerFlag==attacker.StableId
            //       AND TrackerParent.wpoint.attacking>0 AND TrackerParent!=this
            if (itr.kind == 5 && GrabbedBy < 0 && TrackerParent != null)
            {
                var tp = TrackerParent as LF2Entity;
                if (tp != null && tp.TrackerFlag == attacker.StableId && tp != this)
                {
                    var tpFrame = tp.GetFrameDataById(tp.Frame.N);
                    var wp = (tpFrame?.wpoints?.Count > 0) ? tpFrame.wpoints[0] : null;
                    if (wp != null && wp.attacking > 0)
                    {
                        // 从 attacker 的 wpoints[wp.attacking] 获取伤害数据
                        var attackerFrame = attacker.GetFrameDataById(attacker.Frame.N);
                        int wpIdx = wp.attacking;
                        var srcWp = (attackerFrame?.wpoints != null && wpIdx < attackerFrame.wpoints.Count)
                            ? attackerFrame.wpoints[wpIdx] : null;
                        if (srcWp != null)
                        {
                            // 保留原始 itr 的碰撞框（x/y/w/h），替换伤害字段，kind 强制为 0
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

            // ── State 10: being caught ──
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
                        EffectCreate(0, NTSDGlobal.Gameplay.EffectDuration);

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

            // ── State 14: lying — 躺地无敌 ──
            else if (myState == LF2States.Lying)
            {
                // 躺地期间免疫所有伤害
            }

            // ── State 19 + 攻击者 state 3000 → fire-run 免疫 ──
            else if (myState == LF2States.FirenSpecific &&
                     attacker.GetState() == LF2States.ProjectileFlying)
            {
                return false;
            }

            // ── kind 5000-5999 直接扣 HP ──
            else if (itr.kind >= 5000 && itr.kind < 6000)
            {
                acceptHit = true;
                int damage = itr.kind - 5000;
                Health.HP = Mathf.Max(0, Health.HP - damage);
            }

            // ── kind 6000-6999 帧跳转 ──
            else if (itr.kind >= 6000 && itr.kind < 7000)
            {
                acceptHit = true;
                int targetFrame = itr.kind - 6000;
                if (FrameCache.GetFrameDataById(targetFrame) != null)
                    ImmediateFrame(targetFrame);
            }

            // ── 主流程：kind 0 / kind-4系 / kind-9系 ──
            else if (itr.kind == 0 ||
                     MatchItrKind(itr.kind, 4) ||
                     MatchItrKind(itr.kind, 9))
            {
                acceptHit = true;

                // 攻击方向：取攻击者朝向（反汇编 0x42D35B: [eax+80h] = facing byte → 1-2*facing = Dirh()）
                int attDir = attacker.Dirh();
                // 注：反汇编 0x42D384-0x42D42A dvx 直接应用，无 -1 补偿，compen 已删除
                // 反汇编 0x0042CAEC：attacker.NoBounce > 0 AND itr.kind == 4 → dvx 取反
                bool attackerNoBounce = (attacker as LF2SpecialAttack)?.NoBounce == true;
                int dvxSign = (attackerNoBounce && itr.kind == 4) ? -1 : 1;
                efDvx = (itr.dvx != 0) ? attDir * dvxSign * (float)itr.dvx : 0f;
                efDvy = (itr.dvy != 0) ? (float)itr.dvy : 0f;

                // 反汇编 0x0042CBF8~0x0042CC1D：重武器（entity_type==2）被命中时 dvx/dvy/injury/fall 减半
                // 0x42CC01: sar [itr.injury], 1；0x42CC12: sar [itr.fall], 1
                if (attacker is LF2WeaponBase wb && wb.WeaponType == 2)
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

                // ── 防御分支 ──
                // 反汇编 0x42CE30: frame.state==7（防御状态）
                // 反汇编 0x42CE36: itr.injury <= 60 才能防御（超过60伤害无法防御）
                // 反汇编 0x42CE45: attacker.facing != victim.facing（面向攻击者）
                // 反汇编 0x42CE5A: victim.oid in {124,220,221,222} → 直接防御成功
                // 反汇编 0x42CE80: attacker.HP > 0 → 防御成功；attacker.HP <= 0 → 正常命中
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
                // ── 非防御分支 ──
                else
                {
                    if ((GetHeldWeapon() as LF2WeaponBase)?.IsHeavy == true)
                        DropWeapon(0f, 0f);

                    // 反汇编 0x0042CCDE~0x0042CE28：type_sub 免疫系统
                    int victimTypeSub = victimOid; // victimOid 已在防御判定处计算
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
                            // 反汇编 0x0042CD56：victim.frame < 20 → skip；frame.state ∈ {5,4,7} → skip
                            if (Frame.N < 20) return false;
                            int fstate = Frame?.D?.state ?? -1;
                            if (fstate == 5 || fstate == 4 || fstate == 7) return false;
                        }
                    }
                    else if (victimTypeSub == 0x34 && HitCounters.HitStateCount <= 15) // type_sub=52
                    {
                        if (!attackerIsSpecial) return false;
                    }

                    // 反汇编 0x0042E12F–0x0042E148：damage = injury * 100 / MaxMP（MaxMP > 0 时）
                    // entity[0x340] = MaxMP；itr[0x44] = injury
                    int mpDmg = (Health.MaxMP > 0) ? itr.injury * 100 / Health.MaxMP : itr.injury;
                    if (mpDmg != 0) inj += mpDmg;

                    // 反汇编 0x0042CE90: mov [ecx+0B8h], 2Dh → HitStateCount = 45（固定赋值）
                    HitCounters.SetHitStateCount(45);

                    isKnockdown |= HitFall(inj, ref efDvx, ref efDvy, itr, attackerPos);

                    // 反汇编 0x0042E2D1~0x0042E2DC：非击飞路径才执行 HitStateCount += bdefend
                    if (!isKnockdown)
                        HitCounters.AddHitStateCount(itr.bdefend);
                }
            }

            // ── Kind 6: 命中确认标记（EXE 0x0042E6F4）──
            // victim 受伤硬直帧（226~229）携带 kind=6 itr 向外发出
            // 攻击者 body 碰到该 itr：this=攻击者，attacker=受伤的 victim
            // this.HitConfirmEa=3，攻击者回到 standing 按 att 跳 frame 70
            else if (itr.kind == 6)
            {
                HitConfirmEa = 3;
                return true;   // 不扣血，不触发 vrest，直接返回
            }

            // ── Kind 8: 传送 + 回血（EXE 0x0042EC85–0x0042ECC7）──
            // victim 传送到 attacker 位置；victim.frame = itr.dvx
            // 反汇编 0x0042EC85：heal_timer 写入 attacker（[esi+0E0h]），无 HP 扣减，无 FrameDelay
            else if (itr.kind == 8)
            {
                if (PS != null && attacker?.PS != null)
                {
                    PS.x = attacker.PS.x;
                    PS.z = attacker.PS.z + 1f;
                    // 反汇编 0x0042ECBE: fadd dbl_4432B0(1.0) → victim.y += 1.0
                    PS.y += 1f;
                }
                if (itr.dvx > 0)
                    ImmediateFrame(itr.dvx);
                attacker.HealTimer = itr.throwvz + 1000;
                return true;
            }

            // ── Kind 14: 方向阻挡（EXE 0x0042F079–0x0042F16A）──
            // 根据 attacker 相对 victim 位置设置方向阻挡标志，当帧物理层阻止对应轴移动
            else if (itr.kind == 14)
            {
                if (PS != null && attacker?.PS != null)
                {
                    float aix = attacker.PS.x;
                    float aiz = attacker.PS.z;
                    float vix = PS.x;
                    float viz = PS.z;

                    if (aix > vix + 5f && PS.vx > 0f) PS.xBoundPositive = true;
                    else if (aix < vix - 5f && PS.vx < 0f) PS.xBoundNegative = true;

                    if (aiz > viz + 2f && PS.vz > 0f) PS.zBoundPositive = true;
                    else if (aiz < viz - 2f && PS.vz < 0f) PS.zBoundNegative = true;
                }
                return false;   // 不触发 vrest，每帧持续可激活
            }

            // ── Kind 10/11: 笛子效果 ──
            else if (MatchItrKind(itr.kind, 10) || itr.kind == 11)
            {
                FluteForce();
                if (myState == LF2States.Falling)
                {
                    inj = itr.injury * 2;
                    acceptHit = true;
                }
            }

            // ── Kind 15: 旋风效果 ──
            // 反汇编 0x0042F38F~0x0042F3FB：vx ±1.0（基于 x 位置差），vz ±0.5（基于 z 位置差）
            else if (itr.kind == 15)
            {
                if (PS != null && attacker?.PS != null)
                {
                    PS.vx += (attacker.PS.x >= PS.x) ? 1f : -1f;
                    PS.vz += (attacker.PS.z >= PS.z) ? 0.5f : -0.5f;
                }
            }

            // ── Kind 16: 冰冻 ──
            else if (itr.kind == 16)
            {
                ImmediateFrame(LF2StandardFrames.MpDrain);
                // 反汇编 0x0042E12F：damage = injury * 100 / MaxMP（同 kind=0 公式）
                // entity[0x340] = MaxMP；itr[0x44] = injury
                inj = (Health.MaxMP > 0) ? itr.injury * 100 / Health.MaxMP : itr.injury;
                // 反汇编 0x0042E12F 后无音效调用；sub_419C40(x,0x0E) 为屏幕震动，暂不实现
                acceptHit = true;
            }

            // ── 结算 ──
            if (acceptHit)
            {
                var attackerLiving = attacker as LF2LivingObject;
                if (attackerLiving != null) Attacker = attackerLiving;

                // 反汇编 0x0042D762/0x0042D7C5：击飞路径写 45，非击飞路径写 itr.arest（若>0）
                if (isKnockdown)
                    ItrVrestUpdateKnockdown(attacker.StableId, itr);
                else
                    ItrVrestUpdate(attacker.StableId, itr);

                // 攻击方碰撞豁免（反汇编 0x0042D7A0~0x0042D7BF）：
                // (itr.vrest >= 4 OR itr.arest != 0) ? itr.vrest : 4
                int exemptVal = (itr.vrest >= 4 || itr.arest != 0) ? itr.vrest : 4;
                attackerLiving?.HitCounters?.SetAttackExempt(exemptVal);

                // 反汇编 0x0042D218/0x0042D17A/0x0042D0B6：entity+0B0h 值决定屏幕震动强度
                // sub_419C40 写入 slot channel，渲染层消费产生视觉抖动
                // TODO: 实现屏幕震动（sub_419C40），当前暂不实现

                // 反汇编 0x0042D676：cmp [eax+0B0h], 50h; jnz loc_42D77A
                // fall != 80 同样跳到 loc_42D77A（FrameDelay 设置处），所有命中路径都设 FrameDelay
                // loc_42D77A: attacker.FrameDelay=3（若当前>=0）; victim.FrameDelay=-3（击飞）或-5（普通受伤）
                // 反汇编 0x42D796: fall>60路径 victim=-3；0x42E2FD: fall<=60路径 victim=-5
                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                FrameDelay = isKnockdown ? -3 : -5;

                // 反汇编 0x0042D864~0x0042D88B：attacker.GrabbedBy < 0 时传播 FrameDelay 给 TrackerParent
                if (attacker.GrabbedBy < 0 && attacker.TrackerParent != null)
                    attacker.TrackerParent.FrameDelay = FrameDelay;

                // 反汇编 0x0042E2C5：非击飞路径 HitStun = 0
                if (!isKnockdown)
                    HitStun = 0;

                // 反汇编 0x0042E314–0x0042E328：地面上 HitStateCount >= 30 且 itr.kind == 7 → frame 112
                if (!isKnockdown && PS.vy == 0f &&
                    HitCounters.HitStateCount >= 30 && itr.kind == 7)
                {
                    ImmediateFrame(LF2StandardFrames.DefendBroken);
                }

                // 反汇编 sub_42C8C0：attacker state==1002 命中后的速度反弹
                // attacker.vx = -(victim.vx * 0.5)；attacker.vy = -3.5（向上弹起）
                if (attacker.GetState() == LF2States.WeaponThrowing) // 1002
                {
                    var aps = attacker.PS;
                    if (aps != null)
                    {
                        aps.vx = -(PS.vx * 0.5f);
                        aps.vy = -3.5f; // 0xC00C000000000000 = -3.5
                    }
                }

                // 反汇编：非击飞路径的 dvx 写入 knockback_vx（entity+28h），不直接写 PS.vx
                // PS.vx 由 FramePostProcessAll 统一写入；HitCount++ 使写入条件成立
                if (!defended && efDvx != 0f)
                {
                    KnockbackVx += efDvx;
                    HitCount++;
                }
            }

            if (acceptHit)
                Injury(inj);

            // 反汇编 sub_42C8C0 loc_42D11A：击中音效
            // slot 2=006.wav(knockdown), slot 0=001.wav(普通/重击) → PlaySfx
            if (acceptHit && itr.kind == 0)
            {
                string hitSfx = isKnockdown
                    ? NTSDGlobal.Sound.HitKnockdown
                    : NTSDGlobal.Sound.HitNormal;
                AppManager.Instance?.SoundPlayer?.PlaySfx(hitSfx);
            }

            // 反汇编 0x0042F7A8–0x0042F9A1：kind=0 且 effect ∉ {6,23} 时生成 spark
            if (acceptHit && itr.kind == 0 && effectNum != 6 && effectNum != 23)
            {
                SpawnSpark(itr, attacker, attackerPos, vol);
            }

            return acceptHit;
        }

        // ─────────────────────────────────────────────────────────────────
        // character.prototype.injury
        // ─────────────────────────────────────────────────────────────────

        protected override void Injury(int inj)
        {
            base.Injury(inj);
            // 反汇编 0x0042CF3F~0x0042CF6B：victim.PP += injury/3（钳制到 MaxPP）
            if (inj > 0)
                Health.PP = System.Math.Min(Health.PP + inj / 3, Health.MaxPP);
            // TODO: NPC offset_attack 回调
        }

        // ─────────────────────────────────────────────────────────────────
        // 局部辅助：HitFall
        // 对应反汇编 Entity_AI_Update 中 fall 累加 + 帧切换逻辑
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 反汇编 0x0042C8C0 LABEL_79：fall 累加 + 帧选择。
        ///   强制击飞条件（HP耗尽 / Falling / Frozen）必须在 AddFall 之前判断。
        ///   fall > 60 → HitFallDown；fall > 40 → frame 226；fall > 20 → frame 222/224。
        ///   空中（PS.y < 0）在中/重伤档时升级为击飞。
        ///   每档命中后将 fall 钳制到对应档位上限，防止下次命中从累加值越界。
        /// </summary>
        private bool HitFall(int currentInj, ref float efDvx, ref float efDvy,
                             InteractionArea itr, Vector3 attackerPos)
        {
            int fallInc = (itr.fall != 0) ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int state   = GetState();

            // 反汇编 0x0042C8C0 LABEL_79：强制击飞判断在 fall 累加之前
            bool forceKnockback = (Health.HP - currentInj <= 0)
                                  || (state == LF2States.Falling)
                                  || (state == LF2States.Frozen)
                                  || (itr.fall == 100); // 反汇编 0x0042CC88：itr.fall==100 强制击飞

            if (forceKnockback)
            {
                HitCounters.AddFall(fallInc);
                return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
            }

            HitCounters.AddFall(fallInc);
            int fall = HitCounters.Fall;

            // fall > 60 → 直接击飞（反汇编 50113: cmp ecx,3Ch; jle→轻/中/重伤分支）
            if (fall > 60)
                return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);

            // fall > 40 → 重伤帧 226；空中（vy < 0，速度向上）升级为击飞
            if (fall > 40)
            {
                HitCounters.SetFall(60); // 反汇编 0x0042D159：钳制到档位上限 60
                ImmediateFrame(LF2StandardFrames.Injured6);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 20 → 中伤帧 222/224；空中（vy < 0）升级为击飞
            if (fall > 20)
            {
                HitCounters.SetFall(40); // 反汇编 0x0042D218：钳制到档位上限 40
                bool sameDir = attacker_dir_matches_victim(attackerPos);
                ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 0 → 轻伤帧 220；空中（vy < 0）升级至中伤帧（不击飞）
            if (fall > 0)
            {
                HitCounters.SetFall(20); // 反汇编 0x0042D218：钳制到档位上限 20
                ImmediateFrame(LF2StandardFrames.Injured);
                if (PS.vy < 0)
                {
                    bool sameDir = attacker_dir_matches_victim(attackerPos);
                    ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                }
            }
            return false;
        }

        // 兼容签名：BeingCaught 路径只传 efDvy
        private bool HitFall(int currentInj, ref float efDvy,
                             InteractionArea itr, Vector3 attackerPos)
        {
            float efDvxDummy = 0f;
            return HitFall(currentInj, ref efDvxDummy, ref efDvy, itr, attackerPos);
        }

        // ─────────────────────────────────────────────────────────────────
        // 局部辅助：HitFallDown
        // 对应反汇编 fall==80 时的倒地处理
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 反汇编 0x0042D730 fall==80 倒地处理：
        ///   帧选择依据 knockback_vx（+0x28h）和 facing（+0x80h），不用 PS.vx。
        ///     (facing==right && kb>0) || (facing==left && kb<0) → 186（背面飞）
        ///     否则 → 180（正面飞）
        ///   vy 处理（反汇编 0x0042D6B0–0x0042D6D2）：
        ///     dvy != 0 → vy += dvy；若 y+vy > 0 则钳制 vy = -12.0（反汇编值）
        ///     dvy == 0 → vy -= 7.0
        ///   KnockbackVx 累加，PS.vx 由 Generic_Transit 每帧重算，此处不写。
        /// </summary>
        private bool HitFallDown(ref float efDvx, ref float efDvy,
                                 InteractionArea itr, Vector3 attackerPos)
        {
            HitCounters.ResetFall();

            // 反汇编 0x0042D730：帧号由 knockback_vx（+0x28h）和 facing（+0x80h）决定
            bool facingRight = PS.dir == "right";
            float kb = KnockbackVx;
            bool flyingBack = (facingRight && kb > 0f) || (!facingRight && kb < 0f);
            int fallFrame = flyingBack ? LF2StandardFrames.FallingBack   // 186
                                       : LF2StandardFrames.FallingFront; // 180
            ImmediateFrame(fallFrame);

            // dvx 兜底：dvx==0 时补充 ±5.0（按攻击者方向）
            if (itr.dvx == 0 && efDvx == 0f)
            {
                int attackerDir = (attackerPos.x > PS.x) ? 1 : -1;
                efDvx = attackerDir * 5.0f;
            }

            // vy 处理（反汇编 0x0042D6B0–0x0042D6D2）
            // 反汇编：写 KnockbackVy(entity+30h)，不直接写 PS.vy(entity+48h)
            // PS.vy 由 Frame_PostProcess 统一写入
            if (itr.dvy != 0)
            {
                KnockbackVy += itr.dvy;
                // y + KnockbackVy > 0 → 钳制为 -12.0（反汇编 40280000h = 12.0f，存负号）
                if ((int)PS.y + (int)KnockbackVy > 0)
                    KnockbackVy = -12.0f;
                efDvy = itr.dvy;
            }
            else
            {
                KnockbackVy -= 7.0f;
                efDvy = -7.0f;
            }

            // KnockbackVx/Vy 累加（反汇编 entity+28h/30h）；PS.vx/vy 由 FramePostProcessAll 写入
            KnockbackVx += efDvx;
            HitCount++;

            efDvx = 0f;
            return true;
        }

        // 兼容签名：防御路径只有 efDvy
        private bool HitFallDown(ref float efDvy, InteractionArea itr, Vector3 attackerPos)
        {
            float efDvxDummy = 0f;
            return HitFallDown(ref efDvxDummy, ref efDvy, itr, attackerPos);
        }

        // ─────────────────────────────────────────────────────────────────
        // 辅助：判断攻击者方向与被击者朝向是否相同
        // ─────────────────────────────────────────────────────────────────

        private bool attacker_dir_matches_victim(Vector3 attackerPos)
        {
            // 反汇编：*(_BYTE *)(v61 + 128) == *(_BYTE *)(v50 + 128) → dir 字节相等
            // v61 = victim, v50 = attacker entity
            // C# 中：攻击者相对位置判断
            bool attackerFacingRight = attackerPos.x > PS.x;
            bool victimFacingRight   = PS.dir == "right";
            return attackerFacingRight == victimFacingRight;
        }

        // ─────────────────────────────────────────────────────────────────
        // 视觉效果
        // ─────────────────────────────────────────────────────────────────

        public override void VisualEffectCreate(int num, PhysicsState.FlfVolume rect,
                                                bool righttip = false, int variant = 0,
                                                bool withSound = false)
        {
            // 保留基类虚方法，spark 生成已移至 SpawnSpark
        }

        /// <summary>
        /// 生成命中 spark（对应反汇编 0x0042F7A8–0x0042F9A1）。
        /// timer 初始值：itr.fall > 60 → attacking*20（大spark）；否则 → attacking*4+10（小spark）
        /// 坐标：基于攻击者 itr box 与被击者位置计算，含随机偏移
        /// </summary>
        private void SpawnSpark(InteractionArea itr, LF2Entity attacker,
                                Vector3 attackerPos, PhysicsState.FlfVolume vol)
        {
            // timer 初始值（反汇编 0x0042F81B–0x0042F837）
            // var_5C = itr index（0-based），对应反汇编 [edx+esi+2D0h]
            // fall > 60 路径：
            //   lea edx,[edx+edx*4] → var_5C*5；shl edx,2 → var_5C*20
            // fall <= 60 路径：
            //   lea edx,ds:0Ah[edx*4] → var_5C*4 + 10
            int fall   = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int v_5C   = attacker?.CurrentItrIndex ?? 0;
            int timerInitial = fall > 60
                ? v_5C * 20
                : v_5C * 4 + 10;

            // spark_x（反汇编 0x0042F84F–0x0042F8EF）
            // spark_y（反汇编 0x0042F8F7–0x0042F954）
            // 原版：spark_y_stored = attacker.z + edi + Random(9) - 4
            //       edi = clamp((itr.y - centery)/2 + attacker.y + itr.h - centery, ...)
            //       worldY_unity = spark_y_stored / 100 = (attacker.z + edi) / 100
            // 故存储：sz = attacker.z（深度），sy = edi（跳跃高度域偏移）
            // SparkRenderer 用 (sz + sy) / 100 作为 Unity worldY
            float sx;
            float sy;
            float sz;

            if (attacker?.PS != null)
            {
                var atk     = attacker.PS;
                int centerx = attacker.Frame.D?.centerx ?? 0;
                int centery = attacker.Frame.D?.centery ?? 0;
                float itrW  = vol.w;
                float itrX  = itr.x;   // dat 原始 itr.x（反汇编 [idi+4]）
                float itrY  = itr.y;   // dat 原始 itr.y（反汇编 [idi+10h]）

                // spark_x（反汇编：facing right → atk.x - centerx + itr.w + itr.x）
                //                  facing left  → atk.x + centerx - itr.w - itr.x）
                if (attacker.Dirh() > 0) // facing right
                {
                    sx = atk.x - centerx + itrW + itrX;
                    if (sx > PS.x) sx = PS.x;
                }
                else
                {
                    sx = atk.x + centerx - itrW - itrX;
                    if (sx < PS.x) sx = PS.x;
                }

                // spark_y（反汇编 [idi+10h] = itr.y，[idi+8] = itr.h）
                float baseY = atk.y + (vol.h * 0.5f) + itrY - centery;
                float lower = PS.y - centery;
                float upper = PS.y;
                if (baseY < lower)       baseY = (lower + baseY) * 0.5f;
                else if (baseY > upper)  baseY = (upper + baseY) * 0.5f;

                // 随机偏移（反汇编 Random_Int(9) 两次，各 0-8，偏移 -4）
                float rand1 = UnityEngine.Random.Range(0, 9);
                float rand2 = UnityEngine.Random.Range(0, 9);
                sy = baseY + rand1 - 4f;   // edi（跳跃高度偏移，PS.y 域）
                sx += rand2 - 4f;
                sz = atk.z;                // attacker.PS.z（深度）
            }
            else
            {
                sx = PS.x;
                sy = PS.y - 4f;
                sz = PS.z;
            }

            int currentRenderFrame = SimulationTickDriver.Instance?.SparkRenderFrame ?? -1;
            AddSparkSlot(timerInitial, sx, sy, sz, currentRenderFrame);
        }

        // ─────────────────────────────────────────────────────────────────
        // 局部辅助：HitPostEffect（武器掉落 + 冰火效果）
        // ─────────────────────────────────────────────────────────────────

        private void HitPostEffect(int effectNum, PhysicsState.FlfVolume rect,
                                   float efDvx, float efDvy, bool defended,
                                   Vector3 attackerPos, int myState)
        {
            if (defended)
            {
                // 反汇编 0x0042D20F：格挡后 frame=0xDC，无独立音效调用（sub_419C40 为屏幕震动）
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
                        // 反汇编 AI_Process2 0x41B035: vx=holder.vx*1/3, vy=holder.vy
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
                    ImmediateFrame(LF2StandardFrames.Fire);
                    // 反汇编 0x0042DCC3：frame=0x1E，无音效调用
                    break;

                case 3:
                case 30:
                    DropWeapon(PS.vx, PS.vy);
                    if (myState != LF2States.Frozen)
                    {
                        ImmediateFrame(LF2StandardFrames.MpDrain);
                        // 反汇编 0x0042DFCA：frame=0xC8，sub_419C40(x,0x0E) 为屏幕震动，无独立音效
                    }
                    else
                    {
                        ImmediateFrame(LF2StandardFrames.FallingFront2);
                        // 反汇编 0x0042E033：frame=0xCB，sub_419C40(x,0x10) 为屏幕震动，无独立音效
                    }
                    break;

                case 4:
                    DropWeapon(PS.vx, PS.vy);
                    break;
            }
        }
    }
}
