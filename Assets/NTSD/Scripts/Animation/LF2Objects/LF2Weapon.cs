using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 武器实体类（对应反汇编 weapon 结构体，[+6F8h] 字段区分类型）
    /// 
    /// 反汇编已确认的武器类型（ntsd24_full_disasm.txt ParseCharData 0x0040CBDA）：
    ///   0 = 轻武器 (light weapon, data\weapon0.txt)
    ///   1 = 重武器 (heavy weapon)
    ///   2 = 特殊武器
    ///   3 = 投掷武器 (data\weapon1.txt)
    ///   6 = 道具类 (data\weapon6.txt)
    /// 
    /// 框架说明：Unity 用一个类 + int 字段区分类型，与反汇编单结构体+类型字段完全等价。
    /// </summary>
    public class LF2Weapon : LF2WeaponBase
    {
        // 武器类型（对应反汇编 [+6F8h]，由 DAT 解析时写入）
        private int _weaponType;

        // 反汇编 [+31Ch]：飞行计数器，控制 type=1/2/4/6 的反弹/停止节奏
        // Entity_FrameAdvance 0x416A6C / 0x416B51 / 0x416C50 均读写此字段
        private int _flightCounter;

        // 反汇编 [+320h] (this+800)：笛子命中累积器
        // kind=10/11 命中时设为 -20；每帧末 frame.state!=12(Falling) 时清零
        // 用于 type=0 空中帧切换判断 和 CondB HP 扣减公式
        protected override int FluteWeight { get => _fluteWeight; set => _fluteWeight = value; }
        private int _fluteWeight;

        public override LF2ObjectType ObjectTypeEnum => _weaponType == 2
            ? LF2ObjectType.HeavyWeapon
            : (_weaponType == 4 ? LF2ObjectType.ThrowWeapon : LF2ObjectType.LightWeapon);
        public override bool IsLight => _weaponType != 2;
        public override bool IsHeavy => _weaponType == 2;
        public override int WeaponType => _weaponType;

        // ========== 初始化 ==========

        public void SetWeaponType(int weaponType)
        {
            _weaponType = weaponType;
        }

        public override void Reset()
        {
            base.Reset();
            _flightCounter = 0;
            _fluteWeight = 0;
        }

        protected override void OnHealthInitialized(LF2CharacterData charData)
        {
            // 反汇编 Entity_Spawn 0x402A74：_flightCounter 初始值 = weapon_hp
            _flightCounter = charData?.weapon_hp > 0 ? charData.weapon_hp : 100;
        }

        // 反汇编 0x004228A0: type=1/2/4/6 才检查 flightCounter
        protected override bool IsWeaponDestroyable()
        {
            int wt = WeaponType;
            return wt == 1 || wt == 2 || wt == 4 || wt == 6;
        }

        protected override int GetFlightCounter() => _flightCounter;

        protected override void OnDrinkConsumed()
        {
            // 反汇编 AI_Process2 0x41AD73: weapon.[+31Ch] = 0
            _flightCounter = 0;
        }

        protected override void InitializeStates()
        {
            base.InitializeStates();

            if (IsHeavy)
            {
                _states[LF2States.HeavyWeaponInSky] = State_HeavyInSky;
                _states[LF2States.HeavyWeaponOnGround] = State_HeavyOnGround;
            }
        }

        // ========== 状态处理 ==========

        // 重武器在空中（state 2000）
        private bool State_HeavyInSky(string eventType, object eventData)
        {
            if (eventType == "frame")
            {
                if (Frame.N == 21)
                {
                    Trans.SetNext(20);
                    var frame = Frame.D;
                    if (frame == null || string.IsNullOrEmpty(frame.sound))
                        PlaySound(WeaponDropSound);
                }
                return true;
            }
            return false;
        }

        // 重武器在地面（state 2004）
        private bool State_HeavyOnGround(string eventType, object eventData)
        {
            if (eventType == "frame")
            {
                if (Frame.N == 20)
                    Team = 0;
                return true;
            }
            return false;
        }

        // 轻武器刚落地（state 1003，基类 State_WeaponJustOnGround 已注册）
        protected override bool State_WeaponJustOnGround(string eventType, object eventData)
        {
            if (IsHeavy) return false;

            if (eventType == "frame")
            {
                if (Frame.N == 70)
                {
                    var frame = Frame.D;
                    if (frame == null || string.IsNullOrEmpty(frame.sound))
                        PlaySound(WeaponDropSound);
                }
                return true;
            }
            return false;
        }

        // 轻武器在地面（state 1004，基类 State_WeaponOnGround 已注册）
        protected override bool State_WeaponOnGround(string eventType, object eventData)
        {
            if (IsHeavy) return false;

            if (eventType == "frame")
            {
                if (Frame.N == 64)
                    Team = 0;
                return true;
            }
            return false;
        }

        // ========== 飞行物理 ==========

        /// <summary>
        /// 反汇编 Entity_Spawn 0x402A74：[entity+31Ch] = charData[+90h] = weapon_hp
        /// 投掷时 _flightCounter 初始化为武器的 weapon_hp（耐久/飞行计数）
        /// </summary>
        protected override void OnThrown()
        {
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            _flightCounter = charData?.weapon_hp > 0 ? charData.weapon_hp : 100;
        }

        /// <summary>
        /// 反汇编 Entity_FrameAdvance 0x4162EB-0x4164A2：
        /// 计算本帧的额外水平加速、重力、追踪 vz、回旋镖翻转。
        /// 重力值写入 _gravityToAdd，由 WeaponDynamics 在 y+=vy 后（新y<0时）应用，
        /// 严格对齐反汇编 0x4164BD 的执行顺序。
        /// </summary>
        protected override void WeaponFlightPhysics()
        {
            if (PS == null) return;

            int wt = WeaponType;
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            int typeSub = charData?.type_sub ?? 0;
            var fD = Frame.D;
            int frameState = fD?.state ?? -1;

            // ── 1. 额外水平速度加成（反汇编 0x4162EB-0x416327）──
            if (wt == 4 || typeSub == 0x78)
                PS.x += PS.vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;
            else if (typeSub == 0x65)
                PS.x -= PS.vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;

            // ── 2. 计算重力（写入 _gravityToAdd，由 WeaponDynamics 在 y+=vy 后应用）──
            if (wt == 6)
            {
                _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
            }
            else if (wt == 4)
            {
                _gravityToAdd = 0.85f;
            }
            else if (wt != 3)
            {
                if (frameState == LF2States.WeaponThrowing)
                {
                    switch (typeSub)
                    {
                        case 0x7C: _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityTypeSub7C;  break;
                        case 0x78: _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityTypeSub78;  break;
                        case 0x65: _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityTypeSub65;  break;
                        default:   _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityDefault1002; break;
                    }
                }
                else
                {
                    _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityDefault;
                }
            }

            // ── 3. type=3 追踪 vz（反汇编 0x41637D-0x4163A1）──
            if (wt == 3 && fD != null && fD.hit_j > 0)
                PS.vz += fD.hit_j - 50;

            // ── 4. type=4/6 回旋镖（反汇编 0x416466-0x4164A2）──
            if ((wt == 4 || wt == 6) && frameState == LF2States.WeaponInSky)
            {
                if (PS.vx > NTSDGlobal.Gameplay.WeaponBoomerangVxMax ||
                    PS.vx < NTSDGlobal.Gameplay.WeaponBoomerangVxMin)
                {
                    Trans.Frame(40, 0);
                }
            }
        }

        /// <summary>
        /// 反汇编 Entity_FrameAdvance 0x416577-0x41668A：
        /// type=0 空中（新y&lt;0）时，根据 frame.state 和 vy 动态切换帧号。
        /// Falling(state=12)：帧 180~189
        /// Burning(state=18)：帧 205
        /// </summary>
        protected override void OnInFlightFrameUpdate()
        {
            if (WeaponType != 0) return;

            double vy = PS.vy;
            int fnum = Frame?.N ?? -1;
            int fstate = Frame?.D?.state ?? -1;

            // 反汇编 0x416583~0x41662A：frame.state == 12 (Falling)
            // 反汇编用 fcomp（IEEE 比较语义），伪C已转写为 if/else 梯级
            if (fstate == LF2States.Falling)
            {
                if (fnum >= 185)
                {
                    // 0x41662A: fnum >= 0xBF(191) → 跳过
                    // 0x4165A1: fnum >= 0xB9(185) 进入此分支
                    // 0x4165A7: 条件 fnum > 185 && fnum < 191（伪C: v16 < 191 && v16 > 185）
                    if (fnum > 185 && fnum < 191)
                    {
                        // 与 fnum<185 分支相同的 vy 梯级，但帧号 +6
                        // 0x4165AA: fcomp dbl_443390(-8.0); test ah,1; jz→vy >= -8.0
                        // 0x4165CD: fcomp dbl_4432B0(1.0);  test ah,1; jz→vy >= 1.0
                        // 0x4165E2: fcomp dbl_443230(8.0);   test ah,1; jz→vy >= 8.0
                        if      (vy < -8.0) Trans.Frame(186, 0); // 0xB4 → 0xB4
                        else if (vy < 1.0)  Trans.Frame(187, 0); // 0xB5
                        else if (vy < 8.0)  Trans.Frame(188, 0); // 0xB6
                        else                Trans.Frame(189, 0); // 0xB7
                    }
                    // fnum==185 或 fnum>=191：不切换帧
                }
                else
                {
                    // fnum < 185
                    // 0x4165AA: fcomp dbl_443390(-8.0); test ah,1; jz→下级
                    //   test ah,1; jz = "跳转如果 C0==0" = "跳转如果 ST(0) >= dbl"
                    //   所以 jz(0x4165CA) 表示 vy >= -8.0
                    //   不跳转则 vy < -8.0 → frame=0xB4(180)
                    if      (vy < -8.0) Trans.Frame(180, 0);
                    else if (vy < 1.0)  Trans.Frame(181, 0);
                    else if (vy < 8.0)  Trans.Frame(182, 0);
                    else                Trans.Frame(183, 0);

                    // 反汇编 0x4165FB~0x416628：[esi+320h] < 0 时覆盖帧
                    if (_fluteWeight < 0)
                    {
                        // 0x416607: fcomp dbl_443388(12.0); test ah,1; jz→下级
                        // jz = vy >= 12.0 → 0x416617: cmp dword_449038, 6; jl→下级
                        // 两个条件都不满足 → frame=181(0xB5)
                        // 任一满足 → frame=182(0xB6)
                        if (vy >= 12.0 || HitStun < 6)
                            Trans.Frame(181, 0);
                        else
                            Trans.Frame(182, 0);
                    }
                }
            }

            // 反汇编 0x41668A~0x4166C9：
            // frame.state == 18(Burning) 且 fnum < 205(0xCD) 且 vy > 1.0 → frame=205
            int fnum2 = Frame?.N ?? fnum;
            int fstate2 = Frame?.D?.state ?? fstate;
            if (fstate2 == LF2States.Burning && fnum2 < 205 && vy > 1.0)
                Trans.Frame(205, 0);
        }

        /// <summary>
        /// 反汇编 Entity_FrameAdvance 0x4164A9-0x416DA4：按 type 分流落地逻辑
        /// </summary>
        protected override void OnLanded()
        {
            int wt = WeaponType;
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            int dropHurt = charData?.weapon_drop_hurt > 0 ? charData.weapon_drop_hurt : WeaponDropHurt;

            // ── type=3：落地时跳过弹射（反汇编 0x4164CE: jz loc_416577）──
            if (wt == 3) return;

            // ── type=1（轻武器，苦无）落地（反汇编 Entity_FrameAdvance case 1, 0x416A35）──
            if (wt == 1)
            {
                // 无论大弹/小弹都先扣耐久，vz 清零
                // 反汇编：v37 = this+796 - v3[37]（直接减 dropHurt，无 clamp）
                _flightCounter -= dropHurt;
                PS.vz = 0f;

                int frameState1 = Frame?.D?.state ?? -1;

                if (PS.vy > 9.9f)
                {
                    if (frameState1 == LF2States.WeaponThrowing) // 1002
                    {
                        // 大弹：vx*=0.5, 反转方向, frame=7, vy=-8
                        PS.vx *= NTSDGlobal.Gameplay.WeaponType1VxFactor; // 0.5
                        PS.dir = PS.dir == "right" ? "left" : "right";
                        Trans.Frame(7, 0);
                        PS.vy = NTSDGlobal.Gameplay.WeaponType1BigBounceVy; // -8
                        PlaySound(WeaponDropSound);
                    }
                    else
                    {
                        // 非1002大弹 → 小弹路径（frame=60，via LABEL_148）
                        PS.vy = 0f;
                        Trans.Frame(0x3C, 0); // frame=60
                        HitStun = 0;          // 反汇编 LABEL_148: this+136=0
                        PS.vx *= NTSDGlobal.Gameplay.WeaponType1VxFactor; // 0.5
                    }
                }
                else
                {
                    // vy <= 9.9 → 小弹（via LABEL_148）
                    PS.vy = 0f;
                    Trans.Frame(frameState1 == LF2States.WeaponThrowing ? 0x46 : 0x3C, 0);
                    HitStun = 0; // 反汇编 LABEL_148: this+136=0
                    PS.vx *= NTSDGlobal.Gameplay.WeaponType1VxFactor; // 0.5
                }
                // 反汇编 type=1 无 clamp，直接 return
                return;
            }

            // ── type=2（重武器，原木）落地（反汇编 0x416B34-0x416BDB）──
            if (wt == 2)
            {
                // 反汇编：进入 case 2 后立即统一 _flightCounter -= 1（v44 = v43 - 1）
                _flightCounter--;

                if (PS.vy > NTSDGlobal.Gameplay.WeaponType2BigBounceThreshold) // 9.0
                {
                    // 大弹：vx*=0.5，facing 反转，vy=-5，不设 frame
                    // 反汇编：_flightCounter 已在入口扣了 1，大弹不再额外扣
                    PS.vx *= NTSDGlobal.Gameplay.WeaponType2VxFactor; // 0.5
                    PS.dir = PS.dir == "right" ? "left" : "right";
                    PS.vy = NTSDGlobal.Gameplay.WeaponType2BigBounceVy; // -5
                }
                else
                {
                    // 小弹：再额外 -= dropHurt（clamp >=0），vy=0，frame=20，vx*=0.5（LABEL_148 一次）
                    // 反汇编 v48 = v44 - dropHurt; if v48<0: v48=0
                    _flightCounter -= dropHurt;
                    if (_flightCounter < 0) _flightCounter = 0;
                    PS.vy = 0f;
                    Trans.Frame(20, 0);
                    // 反汇编 LABEL_148：vx*=0.5（仅此一次）
                    PS.vx *= NTSDGlobal.Gameplay.WeaponType2VxFactor; // 0.5
                }
                return;
            }

            // ── type=4/6（特殊重/道具）落地（反汇编 0x416C25-0x416D0E）──
            if (wt == 4 || wt == 6)
            {
                // 耐久衰减（反汇编 0x416C4A-0x416C7B）
                _flightCounter -= dropHurt;
                // type=6 且 weapon.hp <= 0 时强制耐久=-1（反汇编 0x416C64）
                if (wt == 6 && Health.HP <= 0)
                    _flightCounter = -1;

                PS.vz = 0f;

                // 大弹/小弹判断（反汇编 0x416C7E-0x416CAF）：
                // 弹起条件：vy > 8.5 AND vx > -10.0 AND vx < 10.0（即 |vx| < 10）
                // 不弹起：vy <= 8.5 OR |vx| >= 10
                int fstate46 = Frame?.D?.state ?? -1;
                bool isFlyState46 = fstate46 == LF2States.WeaponThrowing || fstate46 == LF2States.WeaponInSky;
                bool bigBounce46 = PS.vy > 8.5f          // dbl_443358 = 8.5
                                   && PS.vx > -10f        // dbl_443350 = -10.0
                                   && PS.vx < 10f         // dbl_4432C0 = 10.0
                                   && isFlyState46;

                if (bigBounce46)
                {
                    // 大弹：vy*=-0.7，clamp vy >= -2.5（反汇编 0x416D11-0x416D2D）
                    // frame=0，vx*=0.7
                    PS.vy = PS.vy * -0.7f;
                    if (PS.vy < -2.5f) PS.vy = -2.5f;    // dbl_443348=-0.7, 0xC0240000=-2.5
                    Trans.Frame(0, 0);
                    PS.vx *= NTSDGlobal.Gameplay.WeaponType46VxFactor; // 0.7
                }
                else
                {
                    // 小弹：vy=0，frame=70(state==1002)或frame=60，vx*=0.7，zz=0
                    PS.vy = 0f;
                    PS.vx *= NTSDGlobal.Gameplay.WeaponType46VxFactor; // 0.7
                    PS.zz = 0;
                    Trans.Frame(fstate46 == LF2States.WeaponThrowing ? 0x46 : 0x3C, 0);
                    HitStun = 0; // 反汇编 type=4/6 小弹: this+136=0
                }
                // 反汇编 type=4/6 无 clamp
                return;
            }

            // ── type=0 落地（反汇编 0x416BE0-0x416D5A）──
            // entity_type=0 时：仅 type_sub=999(0x3E7) 有特殊处理，其余直接跳过（loc_416D5A）
            // 反汇编 0x416BE0: cmp [edi+6F4h], 3E7h; jnz loc_416D5A
            var cd = CharacterAnimtorManager.Instance?.GetCharacterData(_objectId);
            if (cd?.type_sub == 0x3E7)
            {
                // type_sub=999：立即静止 frame=101（反汇编 0x416C13: mov [esi+70h], 65h）
                PS.vx = 0f; PS.vy = 0f; PS.vz = 0f;
                Trans.Frame(0x65, 0); // 101
                HitStun = 0;
            }
            // 其他 type=0 武器落地：不做任何帧切换（反汇编 jnz loc_416D5A → fstp st → 末尾）
        }

        // ========== Interaction ==========

        /// <summary>
        /// 反汇编 sub_4063B0 (0x00407378)：
        ///   地面武器（state 1004/2004）才对角色造成碰撞伤害。
        ///   飞行武器（state 1002/2000）的碰撞在 Entity_FrameAdvance 另行处理（待实现）。
        /// </summary>
        public override void Interaction()
        {
            if (Team == 0) return;

            int state = GetState();

            bool canInteract = IsLight
                ? state == LF2States.WeaponThrowing   // 1002：飞行中的轻武器（Entity_FrameAdvance 路径）
                : state == LF2States.HeavyWeaponInSky; // 2000：飞行中的重武器

            // 地面武器攻击角色（sub_4063B0 路径）
            bool groundInteract = IsLight
                ? state == LF2States.WeaponOnGround    // 1004
                : state == LF2States.HeavyWeaponOnGround; // 2004

            if (canInteract || groundInteract)
                base.Interaction();
        }

        // ========== Hit ==========

        public override bool Hit(InteractionArea itr, LF2Entity attacker)
        {
            if (HoldObj != null) return false;
            if (IsVRest(attacker)) return false;

            if (itr.kind == 15) { WhirlwindForce(itr); return true; }
            if (itr.kind == 10 || itr.kind == 11) { FluteForce(); return true; }

            int state = GetState();
            bool accept = false;

            if (IsLight)
                accept = HitAsLight(itr, attacker, state);
            else
                accept = HitAsHeavy(itr, attacker, state);

            if (accept)
                ApplyHitEffects(itr, attacker);

            return accept;
        }

        private bool HitAsLight(InteractionArea itr, LF2Entity attacker, int state)
        {
            if (state == LF2States.WeaponThrowing) // 1002
            {
                // 反汇编：被角色打中时只设 fall=80（由 Entity_AI_Update LABEL_129 处理）
                // 无速度反转；vrest 由 ApplyHitEffects 统一处理
                return true;
            }

            if (state == LF2States.WeaponOnGround) // 1004
            {
                if (attacker is LF2Weapon)
                {
                    var aps = attacker.PS;
                    PS.vx = (aps.vx != 0 ? Mathf.Sign(aps.vx) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedX;
                    PS.vz = (aps.vz != 0 ? Mathf.Sign(aps.vz) : 0) * NTSDGlobal.Gameplay.WeaponBounceupSpeedZ;
                    return true;
                }
            }

            return false;
        }

        private bool HitAsHeavy(InteractionArea itr, LF2Entity attacker, int state)
        {
            // 反汇编 Entity_AI_Update 26344~26351：
            // type==1/2/4/6 被命中时 fall 强制=80，直接进飞出流程（LABEL_129→LABEL_130→LABEL_131）
            // 不做任何 vy/frame 设置——武器弹跳帧由 OnLanded() 分支处理
            if (state == LF2States.HeavyWeaponOnGround   // 2004
             || state == LF2States.HeavyWeaponInSky)     // 2000
            {
                return true;
            }

            return false;
        }

        private void ApplyHitEffects(InteractionArea itr, LF2Entity attacker)
        {
            if (itr.vrest > 0) SetVRest(attacker, itr.vrest);

            // 反汇编 Entity_AI_Update 26312：[+764h] -= injury（所有 type 均扣 HP）
            if (itr.injury > 0) Health.HP -= itr.injury;

            // 反汇编 0x0042E263：type==1/2/4/6 时 _flightCounter（耐久）-= itr.fall
            // itr.fall==100 时强制置 -1（秒毁）
            int wt = WeaponType;
            if (wt == 1 || wt == 2 || wt == 4 || wt == 6)
            {
                if (itr.kill == 100)
                    _flightCounter = -1;
                else
                    _flightCounter -= itr.fall;
            }

            PlaySound(WeaponHitSound);

            // 反汇编 0x42E5DE-0x42E6E8：飞行武器命中角色后的攻击者反弹三步骤
            // 步骤1: state=1002(WeaponThrowing) → 反弹
            // 步骤2: state=2000(HeavyWeaponInSky) → 飞向victim时减速
            // 步骤3: state=3000 → 停止
            // 三步骤非互斥，每步重新读 state
            ApplyAttackerResponse(attacker);
        }

        /// <summary>
        /// 反汇编 0x42E5DE-0x42E6E8：飞行武器命中角色后对武器自身的三步骤处理。
        /// </summary>
        private void ApplyAttackerResponse(LF2Entity victim)
        {
            // 步骤1: state=1002 → 反弹（0x42E5F1-0x42E63A）
            int curState = Frame?.D?.state ?? -1;
            if (curState == LF2States.WeaponThrowing) // 1002
            {
                Trans.Frame(UnityEngine.Random.Range(0, 16), 0);
                Trans.Trans();
                float knockbackVx = victim?.KnockbackVx ?? 0f;
                PS.vx = -(knockbackVx * 0.5f);
                PS.vy = -4f;
                PS.vz *= -0.6667f;
            }

            // 步骤2: state=2000 → 飞向victim时减速（0x42E65D-0x42E6A4）
            curState = Frame?.D?.state ?? -1;
            if (curState == LF2States.HeavyWeaponInSky) // 2000
            {
                bool flyingToward = (PS.x > victim.PS.x && PS.vx < 0f)
                                 || (PS.x < victim.PS.x && PS.vx > 0f);
                if (flyingToward)
                {
                    PS.vx *= 0.4f;
                    PS.vz *= 0.4f;
                }
            }

            // 步骤3: state=3000 → 停止（0x42E6A4-0x42E6E8）
            curState = Frame?.D?.state ?? -1;
            if (curState == 3000)
            {
                Trans.Frame(10, 0);
                Trans.Trans();
                PS.vx = 0f;
            }
        }

        // ========== ProcessAttack ==========

        /// <summary>
        /// 持有攻击处理（反汇编 Entity_AI_Update 0x42CAB1）
        /// wpoint.attacking 对应 weapon_strength_list 的 entry 编号。
        /// 用 entry 参数向周围角色发起 ITR 碰撞。
        /// </summary>
        protected override WeaponAttackResult ProcessAttack(LF2LivingObject holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            var result = new WeaponAttackResult();
            if (wpoint.attacking <= 0) return result;

            var entry = GetStrengthEntry(wpoint.attacking);
            if (entry == null) return result;

            var sceneQuery = Match?.SceneQuery;
            if (sceneQuery == null || PS == null || frame == null) return result;

            // 用武器 body 区域做碰撞查询（持有攻击命中体）
            float spriteW = GetSpriteWidthPxForCollision();
            if (spriteW <= 0f) return result;

            var bodyVols = PS.GetBodyVolumes(frame.bodies, frame.centerx, frame.centery, spriteW);
            if (bodyVols == null || bodyVols.Count == 0) return result;

            // 构造临时 ITR（entry 参数 → InteractionArea）
            var itr = new InteractionArea
            {
                kind = 0,
                dvx  = entry.dvx,
                dvy  = entry.dvy,
                fall = entry.fall,
                vrest = entry.vrest,
                arest = entry.arest,
                injury = entry.injury,
                effect = entry.effect,
            };

            // 持有攻击 dvx 方向跟随持有者
            if (holder != null && holder.PS.dir == "left")
                itr.dvx = -itr.dvx;

            for (int b = 0; b < bodyVols.Count; b++)
            {
                var vol = bodyVols[b];
                var candidates = sceneQuery.QueryBodies(vol, this);
                for (int c = 0; c < candidates.Count; c++)
                {
                    var target = candidates[c];
                    if (target == holder) continue;
                    if (target.Team == Team) continue;
                    if (target is not LF2Character character) continue;
                    if (IsVRest(target)) continue;

                    var attackerPos = new UnityEngine.Vector3(PS.x, PS.y, PS.z);
                    bool hit = character.Hit(itr, this, attackerPos, default);
                    if (hit)
                    {
                        result.HitUid = target.StableId;
                        result.VRest = itr.vrest;
                        result.ARest = itr.arest;
                        ItrArestUpdate(itr);
                        SetVRest(target, itr.vrest);
                        return result;
                    }
                }
            }

            return result;
        }
    }
}
