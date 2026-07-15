using UnityEngine;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Tasks;
using NTSD.Tools;
using NTSD.Simulation;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 具体武器实现。
    ///
    /// 基类 `LF2WeaponBase` 负责所有武器共有的生命周期，
    /// 这个类负责补上不同武器类型之间的差异：
    /// 例如飞行重力、落地反弹、被打反馈、以及手持武器命中角色时的处理。
    /// </summary>
    public class LF2Weapon : LF2WeaponBase
    {
        private int _poolWeaponType;

        // 这里复用 runtime 里的计数槽，保存武器飞行/耐久相关的即时状态。
        private int FlightCounter
        {
            get => Runtime.WeaponFlightCounter;
            set => Runtime.WeaponFlightCounter = value;
        }

        public override int FluteWeight { get => Runtime.WeaponCount; set => Runtime.WeaponCount = value; }

        public override LF2ObjectType ObjectTypeEnum => WeaponType == 2
            ? LF2ObjectType.HeavyWeapon
            : (WeaponType == 4
                ? LF2ObjectType.ThrowWeapon
                : (WeaponType == 6 ? LF2ObjectType.Drink : LF2ObjectType.LightWeapon));
        public override bool IsLight => WeaponType != 2;
        public override bool IsHeavy => WeaponType == 2;
        public override int WeaponType => Runtime.EntityType;

        protected override bool UsesNativeWeaponFrameAdvanceForCurrentData(int currentDataType)
        {
            return currentDataType == _poolWeaponType;
        }


        // 对象池复用时，需要恢复“这个实例当前代表哪一种武器”。
        public void SetWeaponType(int weaponType)
        {
            _poolWeaponType = weaponType;
            Runtime.EntityType = weaponType;
        }

        public override void Reset()
        {
            base.Reset();
            Runtime.EntityType = _poolWeaponType;
            FlightCounter = 0;
            FluteWeight = 0;
        }

        protected override void OnHealthInitialized(LF2CharacterData charData)
        {
            FlightCounter = charData?.weapon_hp ?? 0;
        }

        protected override bool IsWeaponDestroyable()
        {
            int wt = WeaponType;
            return wt == 1 || wt == 2 || wt == 4 || wt == 6;
        }

        protected override int GetFlightCounter() => FlightCounter;

        protected override void OnDrinkConsumed()
        {
            FlightCounter = 0;
        }

        protected override bool DispatchCurrentStateEvent(string eventType, object eventData = null)
        {
            if (IsHeavy)
            {
                switch (GetState())
                {
                    case LF2States.HeavyWeaponInSky:
                        return eventType == "frame" && ProcessHeavyInSkyFrame();
                    case LF2States.HeavyWeaponOnGround:
                        return eventType == "frame";
                }
            }

            return base.DispatchCurrentStateEvent(eventType, eventData);
        }

        // 重武器空中状态的帧事件比较简单，核心是必要时回到落地帧并补音效。
        private bool ProcessHeavyInSkyFrame()
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

        protected override bool State_WeaponOnGround(string eventType, object eventData)
        {
            if (IsHeavy) return false;

            if (eventType == "frame")
            {
                return true;
            }
            return false;
        }

        protected override bool ApplyObjectSpecificFrameTickBeforeWaitAdvance()
        {
            if (CurrentFrameState() == LF2States.HeavyWeaponOnGround)
                SwitchDir(Runtime.Vx > 0f ? "right" : "left");

            if (WeaponType == 2 &&
                CurrentFrameState() == LF2States.HeavyWeaponOnGround &&
                GetRuntimeYInt() == 0 &&
                System.Math.Abs(Runtime.Vx) < 0.1)
            {
                return false;
            }

            return base.ApplyObjectSpecificFrameTickBeforeWaitAdvance();
        }


        protected override void OnThrown()
        {
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            FlightCounter = charData?.weapon_hp ?? 0;
        }

        // 飞行中的类型差异都在这里收口处理：
        // 包括额外水平位移、重力、视觉 z 偏移，以及回旋类武器的切帧。
        protected override void WeaponFlightPhysics()
        {
            int wt = WeaponType;
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int typeSub = charData?.type_sub ?? 0;
            var fD = Frame.D;
            int frameState = fD?.state ?? -1;

            if (wt == 4 || typeSub == 0x78)
                Runtime.X += Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;
            else if (typeSub == 0x65)
                Runtime.X -= Runtime.Vx * NTSDGlobal.Gameplay.WeaponExtraVxFactor;

            if (wt == 6)
            {
                _gravityToAdd = NTSDGlobal.Gameplay.WeaponGravityTypeSub65;
            }
            else if (wt == 4)
            {
                _gravityToAdd = 0.85;   // baseline NtsdConstants.GravityType4 = 0.85 (double, 对齐 FlyingA)
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

            if (wt == 3 && fD != null && fD.hit_j > 0)
            {
                float visualZ = fD.hit_j - 50;
                Runtime.Z += visualZ;
                Runtime.Type3VisualZOffset += visualZ;
            }

        }

        protected override void OnInFlightFrameUpdate()
        {
            if (WeaponType != 0) return;

            double vy = Runtime.Vy;
            int fnum = Frame?.N ?? -1;
            int fstate = Frame?.D?.state ?? -1;

            if (fstate == LF2States.Falling)
            {
                if (fnum >= 185)
                {
                    if (fnum > 185 && fnum < 191)
                    {
                        if      (vy < -8.0) ImmediateFrame(186); // 0xBA
                        else if (vy < 1.0)  ImmediateFrame(187); // 0xBB
                        else if (vy < 8.0)  ImmediateFrame(188); // 0xBC
                        else                ImmediateFrame(189); // 0xBD
                    }
                }
                else
                {
                    if      (vy < -8.0) ImmediateFrame(180);
                    else if (vy < 1.0)  ImmediateFrame(181);
                    else if (vy < 8.0)  ImmediateFrame(182);
                    else                ImmediateFrame(183);

                    if (FluteWeight < 0)
                    {
                        int globalTick12 = (Match?.CurrentTickIndex ?? 0) % 12;
                        if (vy >= 12.0 || globalTick12 < 6)
                            ImmediateFrame(181);
                        else
                            ImmediateFrame(182);
                    }
                }
            }

            int fnum2 = Frame?.N ?? fnum;
            int fstate2 = Frame?.D?.state ?? fstate;
            if (fstate2 == LF2States.Burning && fnum2 < 205 && vy > 1.0)
                ImmediateFrame(205);
        }

        // 武器落地后的弹跳/停地规则很重类型分流，
        // 这里是理解轻武器、重武器、回旋类武器差异的关键入口。
        protected override void OnLanded()
        {
            int wt = WeaponType;
            double oldVy = _lastLandingVyBeforeClamp; // P0-f-2b B1: float→double (landing Vy, baseline oldVy is double)

            if (ApplyCurrentDatNonCharacterLanding(wt, Frame?.D, oldVy, crossedGround: true))
                return;

            var cd = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);

            int fstateLand = Frame?.D?.state ?? -1;
            int fnumLand = Frame?.N ?? 0;

            if (fstateLand == LF2States.Burning)
            {
                if (Runtime.Vy > 17.0 || Runtime.Vx > 9.0 || Runtime.Vx < -9.0) // P0-f-2b B1: f→double (chain)
                {
                    int ws = WeaponDropHurt;
                    Health.HP += ws != 0 ? (-1000 / ws) : -10;

                    Runtime.Y = 0f;
                    Runtime.Vy = -3.5;                       // P0-f-2b B1: -3.5f→-3.5 (baseline Vy=-3.5 double)
                    Runtime.Vz = 0f;
                    if (Runtime.Vx > 7.0) Runtime.Vx = 7.0;  // 7.0f→7.0 (chain double; baseline Vx clamp ±7.0)
                    else if (Runtime.Vx < -7.0) Runtime.Vx = -7.0;


                    var sceneQuery = Match?.SceneQuery;
                    var frameD = Frame?.D;
                    if (sceneQuery != null && frameD != null)
                    {
                        int hurt = ws != 0 ? (1000 / ws) : 10;
                        if (frameD.bodies != null)
                        {
                            var itr = new InteractionArea { kind = 0, injury = hurt, dvx = 3, dvy = 7, fall = 70, vrest = 10, arest = 0 };
                            for (int bodyIndex = 0; bodyIndex < frameD.bodies.Count; bodyIndex++)
                            {
                                if (!BruteForceSceneQuery.TryBuildBodyBattleVolume(
                                        this,
                                        frameD,
                                        frameD.bodies[bodyIndex],
                                        out PhysicsState.BattleVolume bvol))
                                {
                                    continue;
                                }

                                var hits = sceneQuery.QueryBodyHits(this, frameD, itr, bvol);
                                foreach (var hit in hits)
                                {
                                    var t = hit.Target;
                                    if (t == null || t.RelationTeam == RelationTeam) continue;

                                    int targetType = t.GetCurrentDataObjectTypeForSimulation();
                                    if (targetType != (int)LF2ObjectType.Character)
                                        continue;

                                    Vector3 attackerPos = new UnityEngine.Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                                    if (t is LF2Character character)
                                    {
                                        character.Hit(itr, this, attackerPos, bvol);
                                    }
                                    else if (LF2CharacterDatHitResolver.CanResolveTarget(t))
                                    {
                                        LF2CharacterDatHitResolver.TryResolveHit(t, itr, this, attackerPos, bvol);
                                    }
                                }
                            }
                        }
                    }
                    return;
                }
                else
                {
                    Runtime.Y = 0f;
                    Runtime.Vy = 0f;
                    return;
                }
            }

            if (fstateLand != LF2States.Falling && fstateLand != LF2States.Burning)
            {
                Runtime.Vx *= 0.3333333333333333; // P0-f-2b B1: VALUE-BUG 0.333f→0.3333333333333333 (baseline Physics.cs Vx*=0.3333333333333333)
                Runtime.Y = 0f;
                Runtime.Vy = 0f;

                if (fstateLand == 100)
                    ImmediateFrame(94);  // 0x5E
                else if (fnumLand == 212 || fstateLand == 6)
                    ImmediateFrame(215); // 0xD7
                else
                    ImmediateFrame(219); // 0xDB

                AttackingCounter = 0;           // Entity::attacking=0
                return;
            }

            if (cd?.type_sub == 0x3E7)
            {
                Runtime.Vx = 0f;
                Runtime.Vy = 0f;
                Runtime.Vz = 0f;
                ImmediateFrame(0x65); // 101
                AttackingCounter = 0;
            }

        }

        public override void Interaction()
        {
            base.Interaction();
        }

        // 这是“武器自己被打到”的入口。
        // 会先判断是否允许受击，再分流到对应的受击反馈逻辑。
        public override bool Hit(InteractionArea itr, LF2Entity attacker)
        {
            if (GetRuntimeHolderEntity() != null) return false;
            if (attacker != null && ItrRest != null)
            {
                int attackerKey = attacker.Runtime?.SlotIndex ?? -1;
                if (attackerKey >= 0 && ItrRest.HasVrest(attackerKey))
                    return false;
            }

            if (itr.kind == 14)
            {
                ApplyKind14DirectionalBlockFrom(attacker);
                return false;
            }

            if (itr.kind == 10 || itr.kind == 11)
            {
                if (itr.kind == 11 && FluteWeight >= 0)
                    return false;
                if (ObjectId == 201 || ObjectId == 202)
                    return false;

                const float kFluteVxzFactor = 0.93457943f;
                int curState = Frame?.D?.state ?? -1;
                bool isLight = WeaponType == 1 || WeaponType == 4 || WeaponType == 6;
                int inSkyState = isLight ? LF2States.WeaponInSky : LF2States.HeavyWeaponInSky;
                if (curState != inSkyState)
                    SetFrameDirect(0);
                KnockbackVx = (float)(Runtime.Vx * kFluteVxzFactor);
                Runtime.Vx = KnockbackVx;
                KnockbackVz = (float)(Runtime.Vz * kFluteVxzFactor);
                Runtime.Vz = KnockbackVz;
                ApplyWeaponAirStep(isLight ? 3f : 2.3f);
                FluteWeight = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                return true;
            }

            if (itr.kind == 15 || itr.kind == 16)
            {
                WhirlwindForce(itr, attacker);
                return true;
            }

            int state = Frame?.D?.state ?? -1;
            bool accept = IsLight
                ? ObjectId != 201 && ObjectId != 202
                : state == LF2States.HeavyWeaponOnGround || state == LF2States.HeavyWeaponInSky;

            if (accept)
                ApplyHitEffects(itr, attacker);

            return accept;
        }

        private void ApplyWeaponAirStep(float vyStep)
        {
            if (GetRuntimeYInt() >= -2)
            {
                Runtime.Y = -2f;
                Runtime.YInt = -2;
                Runtime.Vy = -6f;
                return;
            }

            if (Runtime.Vy > -6f)
            {
                Runtime.Vy -= vyStep;
                KnockbackVy = (float)Runtime.Vy;
            }
        }

        // 这里真正写入受击结果：
        // 扣耐久、设置 vrest、计算击退、切受击帧，并处理攻击者的后摇/僵直。
        private void ApplyHitEffects(InteractionArea itr, LF2Entity attacker)
        {
            int wt = WeaponType;
            bool lightThrow = wt == 1;
            bool heavyLike = wt == 2;
            bool specialLike = wt == 4 || wt == 6;
            int attackerKey = attacker?.Runtime?.SlotIndex ?? -1;

            if (attacker != null && attackerKey >= 0)
            {
                if (heavyLike)
                {
                    int vrest = (itr.fall <= 40 && itr.effect != 4) ? 3 : 19;
                    if (attacker.Runtime?.LinkState == -2 && attacker.Runtime.HolderStableId >= 0)
                    {
                        LF2Entity holder = Match?.FindEntityByRuntimeSlotForQuery(attacker.Runtime.HolderStableId);
                        holder?.ItrRest?.SetVrest(attackerKey, vrest);
                    }
                    else if (GetCurrentDataObjectType() != (int)LF2ObjectType.HeavyWeapon)
                    {
                        ItrRest?.SetVrest(attackerKey, vrest);
                    }
                }
                else if (specialLike)
                {
                    ItrRest?.SetVrest(attackerKey, 30);
                }
                else if (itr.vrest > 0)
                {
                    ItrRest?.SetVrest(attackerKey, itr.vrest);
                }
            }

            if (lightThrow || heavyLike || specialLike)
            {
                HitConfirm2 = 1;
                if (itr.bdefend == 100)
                    FlightCounter = -1;
                else
                    FlightCounter -= itr.injury;
            }

            if (attacker != null)
            {
                RelationTeam = attacker.RelationTeam;

                // ── C# baseline: ApplyObjectHurtTail + ApplyStandardDamageKnockbackX ──
                if (wt != 2 || itr.fall > 40)
                    HitCount++;

                FallCounter += itr.fall != 0 ? itr.fall : 20;

                if (lightThrow || heavyLike || specialLike)
                    FallCounter = 80;

                bool knockback = FallCounter > 60;

                // Step 2: ApplyStandardDamageKnockbackX —— 计算 victim.KnockbackVx
                bool attackerState2000 = (attacker.Frame?.D?.state ?? -1) == LF2States.HeavyWeaponInSky;
                float dvx = itr.dvx;

                // C# baseline: knockback 且 Vx 接近0 且 dvx==0 时用固定速度
                if (knockback && Runtime.Vx > -5f && Runtime.Vx < 5f && dvx == 0f)
                {
                    KnockbackVx += attackerState2000
                        ? 5f
                        : (attacker.Runtime.Dir == "right" ? 5f : -5f);
                }
                // C# baseline: state=2000 且有 dvx
                else if (attackerState2000 && dvx != 0f)
                {
                    KnockbackVx += attacker.Runtime.X < Runtime.X ? dvx : -dvx;
                }
                // C# baseline: FlyingA/FlyingB 特殊逻辑——根据当前 Vx 动态调整
                else if (specialLike)
                {
                    double scaled = System.Math.Abs(Runtime.Vx) * 0.55f;

                    if (dvx > scaled)
                    {
                        KnockbackVx += attacker.Runtime.Dir == "right" ? dvx : -dvx;
                    }
                    else if (attacker.Runtime.Dir == "right")
                    {
                        if (KnockbackVx > 0f)
                            KnockbackVx += dvx;
                        else if (Runtime.Vx < 0f)
                            KnockbackVx = (float)(-scaled);
                    }
                    else
                    {
                        if (KnockbackVx < 0f)
                            KnockbackVx -= dvx;
                        else if (Runtime.Vx > 0f)
                            KnockbackVx = (float)(-scaled);
                    }
                }
                // C# baseline: effect=22/23 特殊处理
                else if (itr.effect == 22 || itr.effect == 23)
                {
                    KnockbackVx += Runtime.X <= attacker.Runtime.X ? dvx : -dvx;
                }
                // C# baseline: 常规 dvx 处理
                else if (dvx != 0f)
                {
                    KnockbackVx += attacker.Runtime.Dir == "right" ? dvx : -dvx;
                }

                // Step 3: knockback 时设置击飞速度和帧；否则走原有轻/重击帧逻辑
                if (knockback)
                {
                    float kbVy = itr.dvy != 0 ? (float)itr.dvy : -7f;
                    KnockbackVy += kbVy;
                    if (KnockbackVy + Runtime.Y > 0f) KnockbackVy = 12f;

                    int facing = Runtime.Dir == "left" ? 1 : 0;
                    int hitFrame = facing == 0
                        ? (KnockbackVx <= 0f ? 180 : 186)
                        : (KnockbackVx >= 0f ? 180 : 186);
                    SetFrameDirect(hitFrame);

                    FallCounter = 0; // C# baseline: victim.Fall = 80 then victim.Fall = 0
                }
                else
                {
                    // 非 knockback：保留原有帧逻辑
                    if (heavyLike)
                    {
                        SwitchDir(attacker.Runtime.Dir ?? Runtime.Dir);
                        if (itr.fall <= 40 && (int)Runtime.Y >= 0 && itr.effect != 4)
                            ImmediateFrame(20);
                        else
                            ImmediateFrame(RandInt(0, 6));
                    }
                    else if (lightThrow || specialLike)
                    {
                        ImmediateFrame(RandInt(0, 16));
                    }
                }

                HitStateCount = 45;
                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                FrameDelay = -3;
                attacker.AttackExempt = itr.arest;
            }

            PlaySound(WeaponHitSound);

            ApplyAttackerResponse(attacker);
            if (attacker != null && itr.kind == 0)
            {
                ApplyKind0VictimObjectTail(itr, attacker);
                ApplyCommonEncodedHitEffectRange(itr.effect);
                RecordKind0Hit(attacker, itr);
            }
        }

        private void ApplyKind0VictimObjectTail(InteractionArea itr, LF2Entity attacker)
        {
            int wt = WeaponType;
            if (wt == 1)
            {
                HitConfirm2 = 1;
                SetFrameDirect(RandInt(0, 16));
                RelationTeam = attacker.RelationTeam;
                return;
            }

            if (wt == 4 || wt == 6)
            {
                HitConfirm2 = 1;
                SetFrameDirect(RandInt(0, 16));
                RelationTeam = attacker.RelationTeam;
                return;
            }

            if (wt == 2)
            {
                HitConfirm2 = 1;
                SwitchDir(attacker.Runtime.Dir);
                if (itr.fall <= 40 && (int)Runtime.Y >= 0 && itr.effect != 4)
                    SetFrameDirect(20);
                else
                    SetFrameDirect(RandInt(0, 6));
                RelationTeam = attacker.RelationTeam;
            }
        }

        // 武器受击后，攻击者自己也可能被反弹、减速或改帧。
        private void ApplyAttackerResponse(LF2Entity attacker)
        {
            if (attacker?.Runtime == null) return;

            int attackerState = attacker.Frame?.D?.state ?? -1;
            if (attackerState == LF2States.WeaponThrowing)
            {
                attacker.ImmediateFrame(RandInt(0, 16));

                // C# baseline: attacker.Vx = victim.KnockbackVx * -0.5
                // KnockbackVx 已在 ApplyHitEffects 里按 ApplyStandardDamageKnockbackX 正确累积
                attacker.Runtime.Vx = -(KnockbackVx * 0.5f);
                attacker.Runtime.Vy = -4f;

                int attackerWt = (attacker is LF2Weapon aw) ? aw.WeaponType : -1;
                if (attackerWt == 4 && WeaponType == 4)
                    attacker.KnockbackVx = -KnockbackVx;
            }

            attackerState = attacker.Frame?.D?.state ?? -1;
            if (attackerState == LF2States.HeavyWeaponInSky)
            {
                bool flyingToward = (attacker.Runtime.X > Runtime.X && attacker.Runtime.Vx < 0f)
                                 || (attacker.Runtime.X < Runtime.X && attacker.Runtime.Vx > 0f);
                if (flyingToward)
                {
                    attacker.Runtime.Vx *= 0.4f;
                    attacker.Runtime.Vz *= 0.4f;
                }
            }

            attackerState = attacker.Frame?.D?.state ?? -1;
            if (attackerState == LF2States.ProjectileFlying)
            {
                attacker.ImmediateFrame(10);
                attacker.AttackingCounter = 0;
                attacker.Runtime.Vx = 0f;
            }
        }

        // 这是“角色手里拿着武器去打别人”的命中流程。
        // wpoint 会先被翻译成 itr，再通过场景查询命中角色并调用角色 Hit。
        protected override WeaponAttackResult ProcessAttack(LF2Entity holder, WeaponPoint wpoint, LF2FrameData frame)
        {
            var result = new WeaponAttackResult();
            if (wpoint.attacking <= 0) return result;

            var entry = GetStrengthEntry(wpoint.attacking);
            if (entry == null) return result;

            var sceneQuery = Match?.SceneQuery;
            if (sceneQuery == null || frame == null) return result;
            if (frame.bodies == null || frame.bodies.Count == 0) return result;

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

            if (holder != null && holder.Runtime.Dir == "left")
                itr.dvx = -itr.dvx;

            for (int b = 0; b < frame.bodies.Count; b++)
            {
                if (!BruteForceSceneQuery.TryBuildBodyBattleVolume(
                        this,
                        frame,
                        frame.bodies[b],
                        out PhysicsState.BattleVolume vol))
                {
                    continue;
                }

                var hits = sceneQuery.QueryBodyHits(this, frame, itr, vol);
                for (int c = 0; c < hits.Count; c++)
                {
                    var target = hits[c].Target;
                    if (target == holder) continue;
                    if (target.RelationTeam == RelationTeam) continue;
                    if (ItrRest != null)
                    {
                        int targetKey = target.Runtime?.SlotIndex ?? -1;
                        if (targetKey >= 0 && ItrRest.HasVrest(targetKey)) continue;
                    }

                    var attackerPos = new UnityEngine.Vector3((float)Runtime.X, (float)Runtime.Y, (float)Runtime.Z);
                    bool hit = false;
                    if (target is LF2Character character)
                    {
                        hit = character.Hit(itr, this, attackerPos, vol);
                    }
                    else if (LF2CharacterDatHitResolver.CanResolveTarget(target))
                    {
                        hit = LF2CharacterDatHitResolver.TryResolveHit(target, itr, this, attackerPos, vol);
                    }

                    if (!hit)
                        continue;

                    result.HitUid = target.StableId;
                    result.VRest = itr.vrest;
                    result.ARest = itr.arest;
                    ItrArestUpdate(itr);
                    int hitTargetKey = target.Runtime?.SlotIndex ?? -1;
                    if (hitTargetKey >= 0)
                        ItrRest?.SetVrest(hitTargetKey, itr.vrest);
                    return result;
                }
            }

            return result;
        }

    }
}

