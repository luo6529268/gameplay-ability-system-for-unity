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
                efDvx = (itr.dvx != 0) ? attDir * (float)itr.dvx : 0f;
                efDvy = (itr.dvy != 0) ? (float)itr.dvy : 0f;

                effectNum = itr.effect;

                if (myState == LF2States.Frozen && effectNum == 30) return false;
                if ((myState == LF2States.Burning || myState == LF2States.FirenSpecific) &&
                    (effectNum == 20 || effectNum == 21)) return false;

                // ── 防御分支 ──
                if (myState == LF2States.Defending &&
                    (attackerPos.x > PS.x) == (PS.dir == "right"))
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

                    if (itr.injury != 0) inj += itr.injury;

                    HitCounters.SetBdefend(45);

                    // 对应反汇编 entity+0B8h: add [eax+0B8h], ecx（累加，不是赋值）
                    HitCounters.AddHitStateCount(itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value);

                    isKnockdown |= HitFall(inj, ref efDvx, ref efDvy, itr, attackerPos);
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
            // victim 传送到 attacker 位置；victim.frame = itr.dvx；heal_timer = throwvz + 1000
            else if (itr.kind == 8)
            {
                if (PS != null && attacker?.PS != null)
                {
                    PS.x = attacker.PS.x;
                    PS.z = attacker.PS.z + 1f;
                }
                if (itr.dvx > 0)
                    ImmediateFrame(itr.dvx);
                // 用 Effect.Heal 承载回血总量（heal_timer = throwvz + 1000，每8帧回8HP）
                Effect.Heal = itr.throwvz + 1000;
                FrameDelay  = -3;
                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
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
            else if (itr.kind == 15)
            {
                WhirlwindForce(vol);
            }

            // ── Kind 16: 冰冻 ──
            else if (itr.kind == 16)
            {
                ImmediateFrame(LF2StandardFrames.MpDrain);
                inj = itr.injury;
                acceptHit = true;
            }

            // ── 结算 ──
            if (acceptHit)
            {
                var attackerLiving = attacker as LF2LivingObject;
                if (attackerLiving != null) Attacker = attackerLiving;
                ItrVrestUpdate(attacker.StableId, itr);

                // 攻击方碰撞豁免（对应反汇编 entity+0ECh）：命中后 6 帧内攻击方跳过碰撞检测
                attackerLiving?.HitCounters?.SetAttackExempt(6);

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
                                  || (state == LF2States.Frozen);

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
                ImmediateFrame(LF2StandardFrames.Injured6);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 20 → 中伤帧 222/224；空中（vy < 0）升级为击飞
            if (fall > 20)
            {
                bool sameDir = attacker_dir_matches_victim(attackerPos);
                ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (PS.vy < 0)
                    return HitFallDown(ref efDvx, ref efDvy, itr, attackerPos);
                return false;
            }

            // fall > 0 → 轻伤帧 220；空中（vy < 0）升级至中伤帧（不击飞）
            if (fall > 0)
            {
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
                // TODO: sound.play('1/002') — 格挡音效
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
                    // TODO: sound.play('1/070')
                    break;

                case 3:
                case 30:
                    DropWeapon(PS.vx, PS.vy);
                    if (myState != LF2States.Frozen)
                    {
                        ImmediateFrame(LF2StandardFrames.MpDrain);
                        // TODO: sound.play('1/065')
                    }
                    else
                    {
                        ImmediateFrame(LF2StandardFrames.FallingFront2);
                        // TODO: sound.play('1/066')
                    }
                    break;

                case 4:
                    DropWeapon(PS.vx, PS.vy);
                    break;
            }
        }
    }
}
