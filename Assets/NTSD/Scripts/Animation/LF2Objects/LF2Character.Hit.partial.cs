using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// LF2Character 受击系统
    /// 对应 FLF character.prototype.hit (character.js:1893-2130)
    /// 以及 character.prototype.injury (character.js:2136-2145)
    ///
    /// 调用链（对齐 FLF）：
    ///   target.Attacked(target.Hit(itr, attacker, pos, vol))
    ///
    /// Hit() 是 prototype 方法，与 state_update 完全无关。
    /// 内部局部辅助（对应 JS 闭包局部函数）：
    ///   HitFall()       → fall()
    ///   HitFallDown()   → falldown()
    ///   HitPostEffect() → posteffect()
    /// </summary>
    public partial class LF2Character : LF2LivingObject
    {
        // ─────────────────────────────────────────────────────────────────
        // 主方法：character.prototype.hit
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 受击处理（完整实现）
        /// 对应 FLF character.js:1893-2130 character.prototype.hit
        /// </summary>
        public override bool Hit(InteractionArea itr, LF2LivingObject attacker,
                                 UnityEngine.Vector3 attackerPos, PhysicsState.FlfVolume vol)
        {
            // FLF:1896 — vrest 冷却检查（基类已实现）
            if (!base.Hit(itr, attacker, attackerPos, vol)) return false;

            bool acceptHit = false;
            bool defended  = false;
            float efDvx    = 0f;
            float efDvy    = 0f;
            int inj        = 0;

            int myState = GetState();

            // ──────────────────────────────────────────
            // FLF:1900-1938  State 10: being caught
            // ──────────────────────────────────────────
            if (myState == LF2States.BeingCaught)
            {
                // FLF 保证 state 10 时 $.catching 非空，C# 中若为 null 视为不可伤（无效状态）
                var catcherChar = Catching as LF2Character;
                bool cHurtable  = catcherChar != null && catcherChar.caught_cpointhurtable();

                // FLF:1901-1904  catcher cpoint 允许被伤：触发 fall()
                if (cHurtable)
                {
                    acceptHit = true;
                    HitFall(inj, ref efDvy, itr, attackerPos);
                }

                // FLF:1905-1938  伤害应用判断
                if (!cHurtable && Catching != attacker)
                {
                    // 不可伤且攻击者不是抓取者 → 跳过
                }
                else
                {
                    acceptHit = true;
                    inj += Mathf.Abs(itr.injury);

                    if (itr.injury > 0)
                    {
                        // FLF:1912  创建受击效果
                        EffectCreate(0, NTSDGlobal.Gameplay.EffectDuration);

                        // FLF:1913-1920  目标帧：vaction 优先，否则正面/背面 cpoint hurtact
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
                        if (tar != 0) Trans.Frame(tar, 20);
                    }
                }
            }

            // ──────────────────────────────────────────
            // FLF:1940  State 14: lying — 躺地无敌
            // ──────────────────────────────────────────
            else if (myState == LF2States.Lying)
            {
                // 躺地期间免疫所有伤害，直接不处理
            }

            // ──────────────────────────────────────────
            // FLF:1941  State 19 + 攻击者 state 3000 → fire-run 免疫
            // ──────────────────────────────────────────
            else if (myState == LF2States.FirenSpecific &&
                     attacker.GetState() == LF2States.ProjectileFlying)
            {
                return false;
            }

            // ──────────────────────────────────────────
            // FLF:1943-1949  NTSD M01: kind 5000-5999 直接扣 HP
            // ──────────────────────────────────────────
            else if (itr.kind >= 5000 && itr.kind < 6000)
            {
                acceptHit = true;
                int damage = itr.kind - 5000;
                Health.HP = Mathf.Max(0, Health.HP - damage);
            }

            // ──────────────────────────────────────────
            // FLF:1950-1955  NTSD M02: kind 6000-6999 帧跳转
            // ──────────────────────────────────────────
            else if (itr.kind >= 6000 && itr.kind < 7000)
            {
                acceptHit = true;
                int targetFrame = itr.kind - 6000;
                if (FrameCache.GetFrameDataById(targetFrame) != null)
                    Trans.Frame(targetFrame, 0);
            }

            // ──────────────────────────────────────────
            // FLF:1956-2038  主流程：kind 0 / kind-4系 / kind-9系
            // 等价：ITR.kind === undefined || GC.match_itr_kind(kind, 0|4|9)
            // ──────────────────────────────────────────
            else if (itr.kind == 0 ||
                     MatchItrKind(itr.kind, 4) ||
                     MatchItrKind(itr.kind, 9))
            {
                acceptHit = true;

                // FLF:1961  地面补偿（站地面 compen=1，空中 compen=0）
                int compen = (PS.y == 0) ? 1 : 0;

                // FLF:1962  攻击方向：优先取攻击者 vx 符号，否则取朝向
                float attVx = attacker.PS?.vx ?? 0f;
                int attDir  = (attVx == 0f) ? attacker.Dirh() : (attVx > 0f ? 1 : -1);

                efDvx = (itr.dvx != 0) ? attDir * (float)(itr.dvx - compen) : 0f;
                efDvy = (itr.dvy != 0) ? (float)itr.dvy : 0f;

                // FLF:1964  效果编号（0 为默认，GC.default.effect.num=0）
                int effectNum = itr.effect;

                // FLF:1966  冰冻状态免疫弱冰效果
                if (myState == LF2States.Frozen && effectNum == 30) return false;

                // FLF:1967  燃烧状态免疫弱火效果
                if ((myState == LF2States.Burning || myState == LF2States.FirenSpecific) &&
                    (effectNum == 20 || effectNum == 21)) return false;

                // ── 防御分支 ──
                // FLF:1969  state 7 且正面受击
                if (myState == LF2States.Defending &&
                    (attackerPos.x > PS.x) == (PS.dir == "right"))
                {
                    // FLF:1971  防御减伤
                    if (itr.injury != 0)
                        inj += Mathf.RoundToInt(NTSDGlobal.Gameplay.DefendInjuryFactor * itr.injury);

                    // FLF:1972  bdefend 累加 → 可能破防
                    if (itr.bdefend != 0) HitCounters.AddBdefend(itr.bdefend);

                    if (HitCounters.Bdefend > NTSDGlobal.Gameplay.DefendBreakLimit)
                        Trans.Frame(LF2StandardFrames.DefendBroken, 20);
                    else
                        Trans.Frame(LF2StandardFrames.Defend1, 20);

                    // FLF:1978  防御吸收击退速度
                    if (efDvx != 0f)
                    {
                        float absorbed = NTSDGlobal.LookupAbs(NTSDGlobal.Gameplay.DefendAbsorb, efDvx);
                        efDvx += (efDvx > 0f ? -1f : 1f) * absorbed;
                    }
                    efDvy = 0f;

                    // FLF:1980  HP 耗尽 → 强制倒地；否则标记防御成功
                    if (Health.HP - inj <= 0)
                        HitFallDown(ref efDvy, itr, attackerPos);
                    else
                        defended = true;
                }
                // ── 非防御分支 ──
                else
                {
                    // FLF:1983  持重武器则丢弃
                    if (GetHeldWeapon() is LF2HeavyWeapon)
                        DropWeapon(0f, 0f);

                    // FLF:1984  伤害累加
                    if (itr.injury != 0) inj += itr.injury;

                    // FLF:1985  重置防御值（立即失去防御资格）
                    HitCounters.SetBdefend(45);

                    // FLF:1986  fall() 局部函数
                    HitFall(inj, ref efDvy, itr, attackerPos);
                }

                // FLF:1989-1991  计算效果持续帧数（防御时缩短）
                int vanish    = NTSDGlobal.Gameplay.EffectDuration - 1;
                int nextFrame = Trans.Next;
                if      (nextFrame == LF2StandardFrames.Defend1)      vanish = 3;
                else if (nextFrame == LF2StandardFrames.DefendBroken) vanish = 4;

                EffectCreate(effectNum, vanish, efDvx, efDvy);
                HitPostEffect(effectNum, vol, efDvx, efDvy, defended, attackerPos, myState);
            }

            // ──────────────────────────────────────────
            // FLF:2039-2043  Kind 10/11: 笛子效果
            // GC.match_itr_kind(kind, 10) → {10, 1}
            // ──────────────────────────────────────────
            else if (MatchItrKind(itr.kind, 10) || itr.kind == 11)
            {
                FluteForce();
                // FLF:2041  倒地时笛子伤害翻倍
                if (myState == LF2States.Falling)
                {
                    inj = itr.injury * 2;
                    acceptHit = true;
                }
            }

            // ──────────────────────────────────────────
            // FLF:2044-2046  Kind 15: 旋风效果
            // ──────────────────────────────────────────
            else if (itr.kind == 15)
            {
                WhirlwindForce(vol);
            }

            // ──────────────────────────────────────────
            // FLF:2047-2050  Kind 16: 冰冻
            // ──────────────────────────────────────────
            else if (itr.kind == 16)
            {
                Trans.Frame(LF2StandardFrames.MpDrain, 38);   // FLF: frame 200 (frozen pose)
                inj = itr.injury;
                acceptHit = true;
            }

            // ──────────────────────────────────────────
            // FLF:2122-2126  结算
            // ──────────────────────────────────────────
            if (acceptHit)
            {
                Attacker = attacker;                            // FLF: $.itr.attacker = att
                ItrVrestUpdate(attacker.StableId, itr);        // FLF: itr_vrest_update(att.uid, ITR)
            }

            Injury(inj);   // FLF: $.injury(inj) — 无论 acceptHit 与否都调用（inj 可能为 0）
            return acceptHit;
        }

        // ─────────────────────────────────────────────────────────────────
        // character.prototype.injury
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 伤害结算（override，NPC 扩展入口）
        /// 对应 FLF character.js:2136-2145 character.prototype.injury
        /// </summary>
        protected override void Injury(int inj)
        {
            base.Injury(inj);
            // FLF:2143  if (this.is_npc && this.itr.attacker) this.itr.attacker.offset_attack(inj)
            // TODO: NPC offset_attack 回调（需要 IsNpc 标志位与 offset_attack 接口）
        }

        // ─────────────────────────────────────────────────────────────────
        // 局部辅助方法（对应 hit() 内的 JS 闭包局部函数）
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// 倒地积累计数，决定受伤帧或 falldown
        /// 对应 FLF character.js:2024-2040 局部函数 fall()
        /// 注意：currentInj 只读（对应 JS 闭包读取外部 inj 变量）
        /// </summary>
        private void HitFall(int currentInj, ref float efDvy, InteractionArea itr, Vector3 attackerPos)
        {
            // FLF:2025  fall 值累加
            int fallInc = (itr.fall != 0) ? itr.fall : NTSDGlobal.Default.Fall.Value;
            HitCounters.AddFall(fallInc);
            int fall = HitCounters.Fall;

            int state = GetState();
            if      (state == LF2States.Frozen)                               HitFallDown(ref efDvy, itr, attackerPos);
            else if (PS.y < 0f || PS.vy < 0f)                                HitFallDown(ref efDvy, itr, attackerPos);
            else if (Health.HP - currentInj <= 0)                            HitFallDown(ref efDvy, itr, attackerPos);
            else if (fall > 0  && fall <= 20) Trans.Frame(LF2StandardFrames.Injured,  20);
            else if (fall > 20 && fall <= 30) Trans.Frame(LF2StandardFrames.Injured2, 20);
            else if (fall > 30 && fall <= 40) Trans.Frame(LF2StandardFrames.Injured4, 20);
            else if (fall > 40 && fall <= 60) Trans.Frame(LF2StandardFrames.Injured6, 20);
            else if (NTSDGlobal.Gameplay.FallKO < fall)                      HitFallDown(ref efDvy, itr, attackerPos);
        }

        /// <summary>
        /// 进入倒地状态
        /// 对应 FLF character.js:2042-2050 局部函数 falldown()
        /// </summary>
        private void HitFallDown(ref float efDvy, InteractionArea itr, Vector3 attackerPos)
        {
            // FLF:2043  未定义 dvy 时使用默认下落速度（itr.dvy==0 视为"未设置"）
            if (itr.dvy == 0) efDvy = NTSDGlobal.Default.Fall.Dvy;

            HitCounters.ResetFall();
            PS.vy = 0f;

            bool front = (attackerPos.x > PS.x) == (PS.dir == "right");

            // FLF:2048-2050  正面特例：dvx<0 且 bdefend>=60 → 强制背面倒地
            if (front && itr.dvx < 0 && itr.bdefend >= 60)
                Trans.Frame(LF2StandardFrames.FallingBack,  21);
            else if (front)
                Trans.Frame(LF2StandardFrames.FallingFront, 21);
            else
                Trans.Frame(LF2StandardFrames.FallingBack,  21);
        }

        /// <summary>
        /// 效果后处理（音效、特效、武器掉落、状态帧切换）
        /// 对应 FLF character.js:2052-2120 局部函数 posteffect()
        /// </summary>
        private void HitPostEffect(int effectNum, PhysicsState.FlfVolume rect,
                                   float efDvx, float efDvy, bool defended,
                                   Vector3 attackerPos, int myState)
        {
            // FLF:2053-2059  防御时仅播放格挡音效，不触发后续效果
            if (defended)
            {
                if (effectNum == 0 || effectNum == 1)
                {
                    // TODO: sound.play('1/002') — 格挡音效
                }
                return;
            }

            int nextFrame = Trans.Next;

            switch (effectNum)
            {
                // FLF:2060-2064  普通/强力击中
                case 0:
                case 1:
                    if (nextFrame == LF2StandardFrames.FallingFront ||
                        nextFrame == LF2StandardFrames.FallingBack)
                    {
                        DropWeapon(efDvx, efDvy);
                    }
                    // righttip: 攻击者在右侧时视觉特效显示在右端
                    VisualEffectCreate(effectNum, rect,
                        attackerPos.x < PS.x,
                        HitCounters.Fall > 0 ? 0 : 1,
                        true);
                    break;

                // FLF:2065-2069  火系效果（带 fallthrough 到 case 20）
                case 2:
                case 21:
                case 22:
                case 23:
                    DropWeapon(efDvx, efDvy);
                    goto case 20;

                // FLF:2070-2071  燃烧
                case 20:
                    Trans.Frame(LF2StandardFrames.Fire, 36);  // FLF: frame 203
                    // TODO: sound.play('1/070') — 燃烧音效
                    break;

                // FLF:2072-2077  冰系效果
                case 3:
                case 30:
                    DropWeapon(efDvx, efDvy);
                    if (myState != LF2States.Frozen)
                    {
                        Trans.Frame(LF2StandardFrames.MpDrain, 38);       // FLF: frame 200 (冰冻)
                        // TODO: sound.play('1/065') — 冰冻音效
                    }
                    else
                    {
                        Trans.Frame(LF2StandardFrames.FallingFront2, 21); // FLF: frame 182 (冰块碎裂)
                        // TODO: sound.play('1/066') — 碎裂音效
                    }
                    break;

                // FLF:2078-2079  击退（仅掉武器）
                case 4:
                    DropWeapon(efDvx, efDvy);
                    break;
            }
        }
    }
}
