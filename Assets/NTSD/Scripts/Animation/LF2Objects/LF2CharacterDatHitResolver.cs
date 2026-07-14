using NTSD.App;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal static class LF2HitResolveRuntimeData
    {
        internal static int ResolveAttackerTypeSub(LF2Entity attacker)
        {
            return (attacker as LF2LivingObject)?._FrameDataWrapper?.characterData?.type_sub
                ?? attacker?.FrameCache?.Wrapper?.characterData?.type_sub
                ?? 0;
        }

        internal static bool IsAlternateEffectAllowed(int effectNum)
        {
            return (effectNum / 3) == 2 ||
                   (effectNum / 3) == 3 ||
                   effectNum == 2 ||
                   effectNum == 3;
        }

        internal static bool IsSpecialDefendAttacker(int attackerTypeSub)
        {
            return attackerTypeSub == 124 ||
                   attackerTypeSub == 220 ||
                   attackerTypeSub == 221 ||
                   attackerTypeSub == 222;
        }

        internal static bool IsStepWaitGate(LF2Entity entity)
        {
            var flow = entity?.Match?.Runtime?.Flow;
            return flow != null && flow.BattleStepMode == 1 && flow.BattleStepGate != 1;
        }

        internal static LF2Entity ResolveHolderCopyEntity(LF2Entity attacker)
        {
            int holderSlot = attacker?.HolderCopySlot ?? -1;
            return holderSlot >= 0 ? attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot) : null;
        }

        internal static float ResolveStandardDamageKnockbackX(LF2Entity attacker, LF2Entity victim, InteractionArea itr, bool knockback, float defaultDvx)
        {
            if (attacker == null || victim == null || itr == null)
                return defaultDvx;

            bool attackerState2000 = ResolveAttackerState(attacker) == LF2States.WeaponThrowing;
            if (knockback && victim.Runtime.Vx > -5f && victim.Runtime.Vx < 5f && itr.dvx == 0)
            {
                if (attackerState2000)
                    return 5f;

                return attacker.Dirh() > 0 ? 5f : -5f;
            }

            if (attackerState2000 && itr.dvx != 0)
                return attacker.GetRuntimeXInt() < victim.GetRuntimeXInt() ? itr.dvx : -itr.dvx;

            if (itr.effect == 22 || itr.effect == 23)
                return victim.GetRuntimeXInt() <= attacker.GetRuntimeXInt() ? itr.dvx : -itr.dvx;

            return defaultDvx;
        }

        internal static void ApplyOid100KnockbackTail(LF2Entity victim)
        {
            if (victim?.ObjectId != 100 || victim.Runtime == null || victim.Runtime.LinkState >= 0)
                return;

            victim.KnockbackVx *= 2.5f;
            victim.QueueBattleSound("SFX_039");
            if (victim.KnockbackVx > 0f && victim.KnockbackVx < 10f)
                victim.KnockbackVx = 10f;
            else if (victim.KnockbackVx < 0f && victim.KnockbackVx > -10f)
                victim.KnockbackVx = -10f;
        }

        private static int ResolveAttackerState(LF2Entity attacker)
        {
            if (attacker is LF2WeaponBase weapon)
                return weapon.Frame?.D?.state ?? 0;

            return attacker?.GetState() ?? 0;
        }
    }

    internal static class LF2AlternateDamageResolver
    {
        internal static bool ShouldUseAlternateHurt(LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (attacker == null || victim == null || victim.Health == null || itr == null)
                return false;

            LF2CharacterData attackerData = ResolveCharacterData(attacker);
            LF2CharacterData victimData = ResolveCharacterData(victim);
            if (attackerData == null ||
                victimData == null ||
                victim.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
            {
                return false;
            }

            int victimOid = victimData.type_sub;
            int attackerOid = attackerData.type_sub;
            int victimState = victim.Frame?.D?.state ?? 0;
            int victimPrev2Frame = victim.Runtime?.PrevFrame2 ?? 0;
            int victimPrev2State = victim.GetFrameDataById(victimPrev2Frame)?.state ?? 0;
            bool heavyEffect =
                (itr.effect / 3 == 2) ||
                (itr.effect / 3 == 3) ||
                itr.effect == 2 ||
                itr.effect == 3 ||
                attackerOid == 214 ||
                attackerOid == 208;

            if (victimOid == 37 && victim.HitStateCount <= 15 && !heavyEffect)
                return true;

            if (victimOid == 6 && victim.HitStateCount <= 1 && !heavyEffect)
            {
                if ((victim.Frame?.N ?? 0) < 20)
                    return true;

                if (victimState == 5 || victimState == 4 || victimState == 7)
                    return true;
            }

            if (victimOid == 52 &&
                victim.HitStateCount <= 15 &&
                attackerOid != 214 &&
                attackerOid != 208)
            {
                return true;
            }

            if (victimPrev2State == LF2States.Defending && itr.bdefend <= 60 && victim.Health.HP > 0)
            {
                return attacker.Dirh() != victim.Dirh() ||
                       itr.dvx < 0 ||
                       LF2HitResolveRuntimeData.IsSpecialDefendAttacker(attackerOid);
            }

            return false;
        }

        internal static void ApplyAlternateDamage(
            LF2Entity attacker,
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr)
        {
            if (attacker == null ||
                victim == null ||
                victim.Health == null ||
                itr == null ||
                ResolveCharacterData(victim) == null ||
                victim.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
            {
                return;
            }

            SimulationWorld world = victim.Match ?? attacker.Match;
            RecordLeadSound(attacker, victim);

            int injury = itr.injury;
            if (victim.FallDamageDiv > 0)
                injury = injury * 100 / victim.FallDamageDiv;

            int reducedInjury = injury / 10;
            if (victim.Health.HP > 0 && reducedInjury >= victim.Health.HP && victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(attacker);
                if (holder != null)
                    holder.KillStat++;

                int killStatIndex = victim.Unk344;
                if (world?.KillStats != null && killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= reducedInjury;
            victim.Health.HPBound -= reducedInjury / 3;
            victim.ComboCountVic += reducedInjury;
            if (victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(attacker);
                if (holder != null)
                    holder.ComboCountAtk += reducedInjury;
            }

            int damageStatIndex = victim.Unk344;
            if (world?.DamageStats != null && damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
                world.DamageStats[damageStatIndex] += reducedInjury;

            if (victim.Health.HP <= 0)
                victim.FallCounter = 80;

            victim.AttackingCounter = 0;
            if (victimHitCounters != null)
                victimHitCounters.AddHitStateCount(itr.bdefend);
            else
                victim.HitStateCount += itr.bdefend;
            victim.HitCount++;
            attacker.FrameDelay = 3;
            victim.FrameDelay = -5;

            int victimPrev2Frame = victim.Runtime?.PrevFrame2 ?? 0;
            int victimPrev2State = victim.GetFrameDataById(victimPrev2Frame)?.state ?? 0;
            if (victim.GetRuntimeYInt() == 0)
            {
                int hitStateCount = victimHitCounters?.HitStateCount ?? victim.HitStateCount;
                if (hitStateCount > 30 && victimPrev2State == LF2States.Defending)
                    victim.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.DefendBroken);
                else if ((victim.Frame?.N ?? 0) == LF2StandardFrames.Defend)
                    victim.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.Defend1);

                ApplyGroundKnockback(attacker, victim, itr);
            }
            else
            {
                ApplyAirKnockback(attacker, victim, itr);
            }

            attacker.Runtime.AttackExempt = itr.arest < 4 && itr.vrest == 0
                ? 4
                : Mathf.Min(itr.arest, 12);
            if (itr.vrest > 0)
            {
                int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
                if (attackerSlot >= 0)
                {
                    int vrest = itr.vrest > 4 ? Mathf.Min(itr.vrest, 12) : 4;
                    victim.ItrRest?.SetVrest(attackerSlot, vrest);
                }
            }

            if ((attacker.Runtime?.LinkState ?? 0) < 0)
            {
                int holderSlot = attacker.Runtime.ResolveActiveHolderSlotIndex();
                LF2Entity holder = holderSlot >= 0
                    ? attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot)
                    : null;
                if (holder != null)
                    holder.FrameDelay = attacker.FrameDelay;
            }

            if (FrameStateIs(attacker, LF2States.WeaponThrowing))
            {
                attacker.DirectWriteFramePreserveWaitCounter(attacker.BattleRandInt(0, 16));
                attacker.Runtime.Vx = victim.KnockbackVx * -0.5;
                attacker.Runtime.Vy = -4.0;
                attacker.Runtime.Vz *= -0.6666666666666666;
            }

            DampenState2000Attacker(attacker, victim);
            ApplyState3000Tail(attacker);
        }

        private static LF2CharacterData ResolveCharacterData(LF2Entity entity)
        {
            return (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity?.FrameCache?.Wrapper?.characterData;
        }

        private static void RecordLeadSound(LF2Entity attacker, LF2Entity victim)
        {
            LF2CharacterData attackerData = ResolveCharacterData(attacker);
            LF2CharacterData victimData = ResolveCharacterData(victim);
            if (attackerData == null || victimData == null)
                return;

            if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.SpecialAttack)
            {
                attacker.QueueBattleSound(attackerData.weapon_broken_sound);
                return;
            }

            victim.QueueBattleSound(victimData.type_sub == 37 || victimData.type_sub == 6
                ? "SFX_017"
                : "SFX_002");
        }

        private static bool FrameStateIs(LF2Entity entity, int state)
        {
            return entity?.Frame?.D?.state == state;
        }

        private static void ApplyGroundKnockback(LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (victim.FallCounter == 80 &&
                victim.Runtime.Vx < 3.0 &&
                victim.Runtime.Vx > -3.0 &&
                itr.dvx == 0)
            {
                if (FrameStateIs(attacker, LF2States.HeavyWeaponInSky))
                    victim.KnockbackVx += attacker.GetRuntimeXInt() < victim.GetRuntimeXInt() ? 6.0 : -6.0;
                else
                    victim.KnockbackVx += attacker.Dirh() > 0 ? 3.0 : -3.0;
                return;
            }

            if (FrameStateIs(attacker, LF2States.HeavyWeaponInSky))
            {
                victim.KnockbackVx += attacker.GetRuntimeXInt() < victim.GetRuntimeXInt()
                    ? itr.dvx
                    : -itr.dvx;
            }
            else if (itr.effect == 22 || itr.effect == 23)
            {
                victim.KnockbackVx += victim.GetRuntimeXInt() <= attacker.GetRuntimeXInt()
                    ? itr.dvx
                    : -itr.dvx;
            }
            else
            {
                int halfDvx = itr.dvx / 2;
                victim.KnockbackVx += attacker.Dirh() > 0 ? halfDvx : -halfDvx;
            }
        }

        private static void ApplyAirKnockback(LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (victim.FallCounter == 80 &&
                victim.Runtime.Vx < 6.0 &&
                victim.Runtime.Vx > -6.0 &&
                itr.dvx < 6)
            {
                victim.KnockbackVx += attacker.Dirh() > 0 ? 6.0 : -6.0;
            }
            else if (itr.effect == 22 || itr.effect == 23)
            {
                victim.KnockbackVx += victim.GetRuntimeXInt() <= attacker.GetRuntimeXInt()
                    ? itr.dvx
                    : -itr.dvx;
            }
            else
            {
                victim.KnockbackVx += attacker.Dirh() > 0 ? itr.dvx : -itr.dvx;
            }
        }

        private static void DampenState2000Attacker(LF2Entity attacker, LF2Entity victim)
        {
            if (!FrameStateIs(attacker, LF2States.HeavyWeaponInSky))
                return;

            bool movingTowardVictim = false;
            if (attacker.GetRuntimeXInt() > victim.GetRuntimeXInt())
                movingTowardVictim = attacker.Runtime.Vx < 0.0;
            else if (attacker.GetRuntimeXInt() < victim.GetRuntimeXInt())
                movingTowardVictim = attacker.Runtime.Vx > 0.0;

            if (!movingTowardVictim)
                return;

            attacker.Runtime.Vx *= 0.4;
            attacker.Runtime.Vz *= 0.4;
        }

        private static void ApplyState3000Tail(LF2Entity attacker)
        {
            if (!FrameStateIs(attacker, LF2States.ProjectileFlying))
                return;

            attacker.DirectWriteFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
        }
    }

    internal sealed class LF2CharacterDatHitResolver
    {
        private readonly LF2Entity _victim;
        private readonly LF2LivingObject _livingVictim;
        private readonly LF2HitCountersModule _hitCounters;

        public LF2CharacterDatHitResolver(LF2Entity victim)
        {
            _victim = victim;
            _livingVictim = victim as LF2LivingObject;
            _hitCounters = ResolveHitCounters(victim);
        }

        internal static bool CanResolveTarget(LF2Entity target)
        {
            return target != null &&
                   target is not LF2Character &&
                   target.Health != null &&
                   target.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        internal static bool TryResolveHit(LF2Entity target, InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!CanResolveTarget(target))
                return false;

            return new LF2CharacterDatHitResolver(target).ResolveHit(itr, attacker, attackerPos, vol);
        }

        public bool ResolveHit(InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!PassBaseHit(attacker))
                return false;

            if (itr.kind == 5 && _victim.GrabbedBy < 0 && _victim.TrackerParent != null)
            {
                LF2Entity trackerParent = _victim.TrackerParent;
                if (trackerParent != null && trackerParent.TrackerFlag == attacker.StableId && trackerParent != _victim)
                {
                    LF2FrameData trackerFrame = trackerParent.GetFrameDataById(trackerParent.Frame.N);
                    WeaponPoint trackerWPoint = (trackerFrame?.wpoints?.Count > 0) ? trackerFrame.wpoints[0] : null;
                    if (trackerWPoint != null && trackerWPoint.attacking > 0)
                    {
                        LF2FrameData attackerFrame = attacker.GetFrameDataById(attacker.Frame.N);
                        int wpointIndex = trackerWPoint.attacking;
                        WeaponPoint sourceWPoint = (attackerFrame?.wpoints != null && wpointIndex < attackerFrame.wpoints.Count)
                            ? attackerFrame.wpoints[wpointIndex]
                            : null;
                        if (sourceWPoint != null)
                        {
                            itr = new InteractionArea
                            {
                                kind = 0,
                                x = itr.x,
                                y = itr.y,
                                w = itr.w,
                                h = itr.h,
                                zwidth = sourceWPoint.cover,
                                dvx = sourceWPoint.dvx,
                                dvy = sourceWPoint.dvy,
                                dvz = sourceWPoint.dvz,
                                injury = sourceWPoint.injury,
                                fall = sourceWPoint.fall,
                                vaction = sourceWPoint.vaction,
                                arest = sourceWPoint.arest,
                                vrest = sourceWPoint.vrest,
                                effect = sourceWPoint.effect,
                                kill = sourceWPoint.kill,
                                bdefend = sourceWPoint.bdefend,
                            };
                        }
                    }
                }
            }

            bool acceptHit = false;
            bool defended = false;
            bool isKnockdown = false;
            float effectDvx = 0f;
            float effectDvy = 0f;
            float baseKnockbackDvx = 0f;
            int injury = 0;
            int effectNum = 0;
            bool hitCountAlreadyRecorded = false;

            int victimState = _victim.GetState();

            if (victimState == LF2States.BeingCaught && _livingVictim != null)
            {
                LF2Entity catcherEntity = ResolveActiveCatcherEntity(_livingVictim, _livingVictim.Catching);
                bool cpointHurtable = ResolveCatchHurtable(catcherEntity);

                if (cpointHurtable)
                {
                    acceptHit = true;
                    isKnockdown |= HitFall(Mathf.Abs(itr.injury), ref effectDvy, itr, attackerPos);
                }

                if (cpointHurtable || IsSameCatchPair(catcherEntity, attacker, _livingVictim))
                {
                    acceptHit = true;
                    injury += Mathf.Abs(itr.injury);

                    if (itr.injury > 0)
                    {
                        int targetFrame = itr.vaction;
                        if (targetFrame == 0)
                        {
                            bool front = (attackerPos.x > _victim.Runtime.X) == (_victim.Runtime.Dir == "right");
                            CatchPoint victimCpoint = _victim.Frame?.D?.cpoint;
                            targetFrame = front
                                ? (victimCpoint?.fronthurtact ?? 0)
                                : (victimCpoint?.backhurtact ?? 0);
                        }

                        if (targetFrame != 0)
                            _victim.ImmediateFrame(targetFrame);
                    }
                }
            }
            else if (victimState == LF2States.Lying)
            {
            }
            else if (victimState == LF2States.FirenSpecific && ResolveAttackerState(attacker) == LF2States.ProjectileFlying)
            {
                return false;
            }
            else if (itr.kind == 0 || itr.kind == 4 || itr.kind == 9)
            {
                acceptHit = true;

                int attackerDir = attacker.Dirh();
                effectDvx = itr.dvx != 0 ? attackerDir * itr.dvx : 0f;
                effectDvy = itr.dvy != 0 ? itr.dvy : 0f;

                effectNum = itr.effect;

                if (victimState == LF2States.Frozen && effectNum == 30)
                    return false;

                if ((victimState == LF2States.Burning || victimState == LF2States.FirenSpecific) &&
                    (effectNum == 20 || effectNum == 21))
                    return false;

                if (itr.kind != 9 && LF2AlternateDamageResolver.ShouldUseAlternateHurt(attacker, _victim, itr))
                {
                    LF2AlternateDamageResolver.ApplyAlternateDamage(attacker, _victim, _hitCounters, itr);
                    _victim.RecordKind0Hit(attacker, itr);
                    return true;
                }

                if (IsHeavyWeaponAttacker(attacker))
                {
                    effectDvx *= 0.5f;
                    effectDvy *= 0.5f;
                    itr = itr.ShallowCopy();
                    itr.injury >>= 1;
                    itr.fall >>= 1;
                }

                injury += itr.injury;
                _hitCounters.SetHitStateCount(45);

                if (_victim.ObjectId == 300)
                {
                    LF2FrameData frameNow = _victim.Frame?.D;
                    LF2FrameData futureFrame = _victim.GetFrameDataById((_victim.Frame?.N ?? 0) + 6);
                    int currentBodyX = (frameNow?.bodies != null && frameNow.bodies.Count > 0)
                        ? frameNow.bodies[0].x
                        : 0;

                    if (futureFrame?.bodies != null &&
                        futureFrame.bodies.Count > 0 &&
                        currentBodyX > 1000)
                    {
                        _victim.RelationTeam = 1;
                        _victim.DirectWriteFramePreserveWaitCounter(currentBodyX - 1000);
                        if (attacker != null)
                            attacker.FrameDelay = 3;
                        _victim.FrameDelay = -3;
                    }

                    return true;
                }

                baseKnockbackDvx = effectDvx;
                int hitCountBeforeFall = _victim.HitCount;
                isKnockdown |= HitFall(injury, ref effectDvx, ref effectDvy, itr, attackerPos);
                if (_victim.HitCount == hitCountBeforeFall)
                    _victim.HitCount++;
                hitCountAlreadyRecorded = true;

                if (!isKnockdown)
                    _hitCounters.AddHitStateCount(itr.bdefend);
            }

            // kind 7: 正常状态下的抓取发起——只建立双方抓取关系，不写帧，不扣血。
            // 基线 Entity_AI_Update ~L27224: guard victim.state152==0，写 state152=1/-1，复制 RelationTeam，建立 slot 引用。
            else if (itr.kind == 7)
            {
                if (_livingVictim == null || _victim.CatcherSlotIndex >= 0)
                    return false;

                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                int victimSlot = _victim.Runtime?.SlotIndex ?? -1;
                _victim.CatcherSlotIndex = attackerSlot;
                if (attacker != null)
                {
                    attacker.CaughtSlotIndex = victimSlot;
                    _victim.RelationTeam = attacker.RelationTeam;
                }
                if (attacker is LF2LivingObject living)
                    _livingVictim.Catching = living;
                return true;
            }
            else if (itr.kind == 6)
            {
                _victim.HitConfirmCounter = 3;
                return true;
            }
            else if (itr.kind == 8)
            {
                _victim.HealTimer = itr.injury + 1000;
                if (attacker != null)
                {
                    attacker.Runtime.X = _victim.Runtime.X;
                    attacker.Runtime.Z = _victim.Runtime.Z + 1f;
                }

                attacker?.ImmediateFrame(itr.dvx);
                return true;
            }
            else if (itr.kind == 14)
            {
                ApplyKind14DirectionalBlockFrom(attacker);
                return false;
            }
            else if (itr.kind == 10 || itr.kind == 11)
            {
                if (itr.kind == 11 && _victim.WeaponCount >= 0)
                    return false;

                ApplyFluteCharacterForce();
                if (_victim.KillCount == -1 &&
                    (_victim.Match?.CurrentTickIndex ?? 0) % 12 == 0 &&
                    !LF2HitResolveRuntimeData.IsStepWaitGate(_victim))
                {
                    LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(attacker);
                    if (holder != null)
                        holder.ComboCountAtk += 11;
                }
                return true;
            }
            else if (itr.kind == 15)
            {
                if (attacker != null)
                    ApplyWhirlwindCharacterForce(attacker);
                return true;
            }
            else if (itr.kind == 16)
            {
                int adjustedInjury = itr.injury;
                if (_victim.FallDamageDiv > 0)
                    adjustedInjury = itr.injury * 100 / _victim.FallDamageDiv;

                if (_victim.Health.HP > 0 && adjustedInjury >= _victim.Health.HP && _victim.KillCount == -1)
                {
                    LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(attacker);
                    if (holder != null)
                        holder.KillStat++;
                }

                _victim.Health.HP -= adjustedInjury;
                _victim.Health.HPBound -= adjustedInjury / 3;
                _victim.ComboCountVic += adjustedInjury;
                if (_victim.KillCount == -1)
                {
                    LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(attacker);
                    if (holder != null)
                        holder.ComboCountAtk += adjustedInjury;
                }
                _victim.ImmediateFrame(LF2StandardFrames.MpDrain);
                _victim.AttackingCounter = 0;

                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                if (attackerSlot >= 0)
                    _victim.ItrVrestUpdate(attackerSlot, itr, true);

                ReleaseHeldTargetOnKind16(attackerSlot);
                _victim.QueueBattleSound("SFX_065");
                return true;
            }

            if (acceptHit)
            {
                LF2LivingObject attackerLiving = attacker as LF2LivingObject;
                if (_livingVictim != null && attackerLiving != null)
                    _livingVictim.Attacker = attackerLiving;

                int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
                if (attackerSlot >= 0)
                {
                    if (isKnockdown)
                        _victim.ItrVrestUpdateKnockdown(attackerSlot, itr, true);
                    else
                        _victim.ItrVrestUpdate(attackerSlot, itr, true);
                }

                int itrArest = (itr.arest < 4 && itr.vrest == 0) ? 4 : itr.arest;
                attacker.AttackExempt = itrArest;

                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                _victim.FrameDelay = -3;

                if (attacker.GrabbedBy < 0 && attacker.TrackerParent != null)
                    attacker.TrackerParent.FrameDelay = _victim.FrameDelay;

                if (!isKnockdown)
                    _victim.AttackingCounter = 0;

                if (!isKnockdown && _victim.Runtime.Vy == 0f &&
                    _hitCounters.HitStateCount >= 30 && itr.kind == 7)
                {
                    _victim.ImmediateFrame(LF2StandardFrames.DefendBroken);
                }

                bool addedNonKnockdownDvx = false;
                if (!defended)
                {
                    float resolvedDvx = LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(attacker, _victim, itr, isKnockdown, effectDvx);
                    if (isKnockdown)
                    {
                        _victim.KnockbackVx += resolvedDvx - baseKnockbackDvx;
                        LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(_victim);
                    }
                    else if (resolvedDvx != 0f)
                    {
                        _victim.KnockbackVx += resolvedDvx;
                        LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(_victim);
                        if (!hitCountAlreadyRecorded)
                            _victim.HitCount++;
                        addedNonKnockdownDvx = true;
                    }
                }

                if (ResolveAttackerState(attacker) == LF2States.WeaponThrowing)
                {
                    NTSDEntityRuntime attackerRuntime = attacker.Runtime;
                    if (attackerRuntime != null)
                    {
                        attacker.ImmediateFrame(attacker.BattleRandInt(0, 16));
                        if (attacker is LF2WeaponBase)
                            attacker.Runtime.WeaponState = attacker.Frame?.D?.state ?? 0;
                        attackerRuntime.Vx = _victim.KnockbackVx * -0.5f;
                        attackerRuntime.Vy = -4.0f;
                    }
                }

                if (ResolveAttackerState(attacker) == LF2States.ProjectileFlying)
                {
                    attacker.ImmediateFrame(10);
                    attacker.AttackingCounter = 0;
                    attacker.Runtime.Vx = 0f;
                    LF2FrameData frame10 = attacker.GetFrameDataById(10);
                    if (frame10 != null)
                        attacker.Runtime.Vz = frame10.dvz;
                }

                if (!isKnockdown && !addedNonKnockdownDvx && !defended && effectDvx != 0f)
                {
                    _victim.KnockbackVx += effectDvx;
                    if (!hitCountAlreadyRecorded)
                        _victim.HitCount++;
                }
            }

            if (acceptHit)
                ApplyHitInjury(injury);

            if (acceptHit && itr.kind == 0)
            {
                HitPostEffect(effectNum, vol, effectDvx, effectDvy, defended, attackerPos, victimState);
            }

            if (acceptHit && itr.kind == 0)
            {
                string hitSfx = isKnockdown
                    ? NTSDGlobal.Sound.HitKnockdown
                    : NTSDGlobal.Sound.HitNormal;
                _victim.QueueBattleSound(hitSfx);
            }

            if (acceptHit && itr.kind == 0 && effectNum != 6 && effectNum != 23)
            {
                SpawnSpark(itr, attacker, attackerPos, vol);
            }

            // BMD-058: oid 5/52 victim vitals reset (baseline ~L20069)
            if (acceptHit && (_victim.ObjectId == 5 || _victim.ObjectId == 52) && _victim.Health != null)
            {
                _victim.Health.HP = 10;
                _victim.Health.HP3 = 10;
                _victim.Health.HPBound = 10;
                _victim.Health.PP = 5;
            }

            return acceptHit;
        }

        private static LF2HitCountersModule ResolveHitCounters(LF2Entity victim)
        {
            if (victim is LF2Character character)
                return character.HitCounters;

            var counters = new LF2HitCountersModule();
            counters.BindRuntime(victim.Runtime);
            return counters;
        }

        private bool PassBaseHit(LF2Entity attacker)
        {
            int attackerSlot = attacker?.Runtime?.SlotIndex ?? -1;
            return attackerSlot < 0 || _victim.ItrVrestTest(attackerSlot, true);
        }

        private static bool IsHeavyWeaponAttacker(LF2Entity attacker)
        {
            return attacker is LF2WeaponBase weapon && weapon.WeaponType == 2;
        }

        private void ApplyHitInjury(int injury)
        {
            if (injury <= 0 || _victim.Health == null)
                return;

            if (_livingVictim != null)
            {
                _livingVictim.ApplyDirectInjury(injury);
                return;
            }

            _victim.Health.HP -= injury;
            _victim.Health.HPLost += injury;
            _victim.Health.HPBound -= injury / 3;
        }

        private void ApplyKind14DirectionalBlockFrom(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || _victim.Runtime == null)
                return;

            double attackerX = attacker.Runtime.X;
            double attackerZ = attacker.Runtime.Z;
            double victimX = _victim.Runtime.X;
            double victimZ = _victim.Runtime.Z;

            if (attackerX > victimX + 5f && (_victim.Runtime.Vx > 0f || _victim.KnockbackVx > 0f))
                _victim.Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5f && (_victim.Runtime.Vx < 0f || _victim.KnockbackVx < 0f))
                _victim.Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2f && (_victim.Runtime.Vz > 0f || _victim.KnockbackVz > 0f))
                _victim.Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2f && (_victim.Runtime.Vz < 0f || _victim.KnockbackVz < 0f))
                _victim.Runtime.ZBoundNegative = true;
        }

        private void ApplyFluteCharacterForce()
        {
            const float factor = 0.93457943f;
            _victim.WeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
            _victim.KnockbackVx = (float)(_victim.Runtime.Vx * factor);
            _victim.Runtime.Vx = _victim.KnockbackVx;
            _victim.KnockbackVz = (float)(_victim.Runtime.Vz * factor);
            _victim.Runtime.Vz = _victim.KnockbackVz;
            _victim.ImmediateFrame(182);
            ApplyAirStep(3.0f);
        }

        private void ApplyWhirlwindCharacterForce(LF2Entity attacker)
        {
            _victim.KnockbackVx = (float)(_victim.Runtime.Vx + (_victim.GetRuntimeXInt() > attacker.GetRuntimeXInt() ? -1f : 1f));
            _victim.Runtime.Vx = _victim.KnockbackVx;
            _victim.KnockbackVz = (float)(_victim.Runtime.Vz + (_victim.GetRenderZInt() > attacker.GetRenderZInt() ? -0.5f : 0.5f));
            _victim.Runtime.Vz = _victim.KnockbackVz;
            ApplyAirStep(3.0f);
            _victim.RefreshRuntimeSnapshot();
        }

        private void ReleaseHeldTargetOnKind16(int attackerSlot)
        {
            int heldTargetSlot = _victim.Runtime?.ResolveActiveHeldSlotIndex() ?? -1;
            if (_victim.Runtime == null || _victim.Runtime.LinkState != 2 || heldTargetSlot < 0)
                return;

            LF2Entity heldTarget = _victim.Match?.FindEntityByRuntimeSlotForQuery(heldTargetSlot);
            int holderSlot = _victim.Runtime.SlotIndex;
            if (heldTarget?.Runtime == null ||
                heldTarget.Runtime.LinkState != -2 ||
                !heldTarget.Runtime.IsActivelyHeldBySlot(holderSlot))
            {
                _victim.Runtime.LinkState = 0;
                return;
            }

            if (attackerSlot >= 0)
                _victim.ItrRest?.SetVrest(attackerSlot, 45);

            _victim.ItrRest?.SetVrest(heldTargetSlot, 30);
            _victim.Runtime.LinkState = 0;
            heldTarget.Runtime.LinkState = 0;
            heldTarget.ImmediateFrame(heldTarget.BattleRandInt(0, 6));
            heldTarget.Runtime.Vy = -1f;
            heldTarget.RefreshRuntimeSnapshot();
            _victim.RefreshRuntimeSnapshot();
        }

        private void ApplyAirStep(float vyStep)
        {
            if (_victim.GetRuntimeYInt() >= -2)
            {
                _victim.Runtime.Y = -2f;
                _victim.Runtime.YInt = -2;
                _victim.Runtime.Vy = -6f;
                return;
            }

            if (_victim.Runtime.Vy > -6f)
            {
                _victim.Runtime.Vy -= vyStep;
                _victim.KnockbackVy = (float)_victim.Runtime.Vy;
            }
        }

        private static int ResolveAttackerState(LF2Entity attacker)
        {
            if (attacker is LF2WeaponBase weapon)
                return weapon.Frame?.D?.state ?? 0;

            return attacker?.GetState() ?? 0;
        }

        private bool HitFall(int currentInjury, ref float effectDvx, ref float effectDvy, InteractionArea itr, Vector3 attackerPos)
        {
            int fallInc = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int state = _victim.GetState();

            bool forceKnockback = (_victim.Health.HP - currentInjury <= 0) ||
                                  state == LF2States.Falling ||
                                  state == LF2States.Frozen ||
                                  itr.fall == 100;

            if (forceKnockback)
            {
                _hitCounters.AddFall(fallInc);
                return HitFallDown(ref effectDvx, ref effectDvy, itr, attackerPos);
            }

            _hitCounters.AddFall(fallInc);
            int fall = _hitCounters.Fall;

            if (fall > 60)
                return HitFallDown(ref effectDvx, ref effectDvy, itr, attackerPos);

            if (fall > 40)
            {
                _hitCounters.SetFall(60);
                _victim.ImmediateFrame(LF2StandardFrames.Injured6);
                if (_victim.GetRuntimeYInt() < 0)
                    return HitFallDown(ref effectDvx, ref effectDvy, itr, attackerPos);
                return false;
            }

            if (fall > 20)
            {
                _hitCounters.SetFall(40);
                bool sameDir = AttackerDirMatchesVictim(attackerPos);
                _victim.ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (_victim.GetRuntimeYInt() < 0)
                    return HitFallDown(ref effectDvx, ref effectDvy, itr, attackerPos);
                return false;
            }

            if (fall > 0)
            {
                _hitCounters.SetFall(20);
                _victim.ImmediateFrame(LF2StandardFrames.Injured);
                if (_victim.GetRuntimeYInt() < 0)
                {
                    bool sameDir = AttackerDirMatchesVictim(attackerPos);
                    _victim.ImmediateFrame(sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                }
            }

            return false;
        }

        private bool HitFall(int currentInjury, ref float effectDvy, InteractionArea itr, Vector3 attackerPos)
        {
            float effectDvxDummy = 0f;
            return HitFall(currentInjury, ref effectDvxDummy, ref effectDvy, itr, attackerPos);
        }

        private bool HitFallDown(ref float effectDvx, ref float effectDvy, InteractionArea itr, Vector3 attackerPos)
        {
            _hitCounters.ResetFall();

            bool facingRight = _victim.Runtime.Dir == "right";
            double knockbackX = _victim.KnockbackVx;
            bool flyingBack = (facingRight && knockbackX > 0f) || (!facingRight && knockbackX < 0f);
            int fallFrame = flyingBack ? LF2StandardFrames.FallingBack : LF2StandardFrames.FallingFront;
            _victim.ImmediateFrame(fallFrame);

            if (itr.dvy != 0)
            {
                _victim.KnockbackVy += itr.dvy;
                if ((int)(_victim.KnockbackVy + _victim.GetRuntimeYInt()) > 0)
                    _victim.KnockbackVy = 12.0f;
                effectDvy = itr.dvy;
            }
            else
            {
                _victim.KnockbackVy -= 7.0f;
                effectDvy = -7.0f;
            }

            _victim.KnockbackVx += effectDvx;
            _victim.HitCount++;

            effectDvx = 0f;
            return true;
        }

        private bool AttackerDirMatchesVictim(Vector3 attackerPos)
        {
            bool attackerFacingRight = attackerPos.x > _victim.Runtime.X;
            bool victimFacingRight = _victim.Runtime.Dir == "right";
            return attackerFacingRight == victimFacingRight;
        }

        private void SpawnSpark(InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            int fall = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int sparkPhase = itr.effect == 1 ? 1 : 0;
            int timerInitial = fall > 60
                ? sparkPhase * 20
                : sparkPhase * 20 + 10;

            int sparkX;
            int sparkY;

            if (attacker != null)
            {
                int attackerX = attacker.GetRuntimeXInt();
                int attackerY = attacker.GetRuntimeYInt();
                int attackerZ = attacker.GetRenderZInt();
                int victimX = _victim.GetRuntimeXInt();
                int victimY = _victim.GetRuntimeYInt();
                int centerx = attacker.Frame?.D?.centerx ?? 0;
                int centery = attacker.Frame?.D?.centery ?? 0;

                if (attacker.Dirh() > 0)
                {
                    sparkX = attackerX - centerx + itr.x + itr.w;
                    if (sparkX > victimX)
                        sparkX = victimX;
                }
                else
                {
                    sparkX = attackerX + centerx - itr.x - itr.w;
                    if (sparkX < victimX)
                        sparkX = victimX;
                }

                int hitYOffset = attackerY + (itr.h / 2) + itr.y - centery;
                int lowerY = victimY - centery;
                if (hitYOffset < lowerY)
                {
                    hitYOffset = (lowerY + hitYOffset) >> 1;
                }
                else if (hitYOffset > victimY)
                {
                    hitYOffset = (victimY + hitYOffset) >> 1;
                }

                sparkY = attackerZ + hitYOffset + _victim.BattleRandInt(0, 9) - 4;
                sparkX += _victim.BattleRandInt(0, 9) - 4;
            }
            else
            {
                sparkX = _victim.GetRuntimeXInt();
                sparkY = Mathf.RoundToInt(_victim.GetDisplayZ()) + _victim.GetRuntimeYInt() - 4;
            }

            LF2Entity recordOwner = _victim;
            if (attacker != null)
            {
                int attackerZ = attacker.GetRenderZInt();
                int victimZ = _victim.GetRenderZInt();
                int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
                int victimSlot = _victim.Runtime?.SlotIndex ?? -1;
                if (attackerZ > victimZ || (attackerZ == victimZ && attackerSlot > victimSlot))
                    recordOwner = attacker;
            }

            recordOwner.AddHitRecord(timerInitial, sparkX, sparkY);
        }

        private static LF2Entity ResolveActiveCatcherEntity(LF2LivingObject victim, LF2LivingObject cachedCatching)
        {
            if (cachedCatching is LF2Entity cachedEntity)
                return cachedEntity;

            int catcherSlot = victim?.CatcherSlotIndex ?? -1;
            if (catcherSlot < 0)
                return null;

            LF2Entity catcher = victim.Match?.FindEntityByRuntimeSlotForQuery(catcherSlot);
            if (catcher == null)
                return null;

            int victimSlot = victim.Runtime?.SlotIndex ?? -1;
            return catcher.CaughtSlotIndex == victimSlot ? catcher : null;
        }

        private static bool ResolveCatchHurtable(LF2Entity catcherEntity)
        {
            if (catcherEntity == null)
                return false;

            if (catcherEntity is LF2Character catcherCharacter)
                return catcherCharacter.caught_cpointhurtable();

            CatchPoint cpoint = catcherEntity.Frame?.D?.cpoint;
            return cpoint == null || cpoint.hurtable != 0;
        }

        private static bool IsSameCatchPair(LF2Entity catcherEntity, LF2Entity attacker, LF2LivingObject victim)
        {
            if (attacker == null || victim == null)
                return false;

            int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
            int victimSlot = victim.Runtime?.SlotIndex ?? -1;
            if (attackerSlot < 0 || victimSlot < 0)
                return false;

            if (catcherEntity != null)
                return catcherEntity.Runtime?.SlotIndex == attackerSlot && catcherEntity.CaughtSlotIndex == victimSlot;

            return victim.CatcherSlotIndex == attackerSlot && attacker.CaughtSlotIndex == victimSlot;
        }

        private void HitPostEffect(int effectNum, PhysicsState.BattleVolume rect, float effectDvx, float effectDvy, bool defended, Vector3 attackerPos, int victimState)
        {
            if (defended)
                return;

            if (_victim.ApplyCommonEncodedHitEffectRange(effectNum))
                return;

            int nextFrame = _victim.Trans.Next;

            switch (effectNum)
            {
                case 0:
                case 1:
                    if (nextFrame == LF2StandardFrames.FallingFront || nextFrame == LF2StandardFrames.FallingBack)
                    {
                    }
                    break;

                case 2:
                case 21:
                case 22:
                case 23:
                    goto case 20;

                case 20:
                    if (victimState != LF2States.Burning && victimState != LF2States.FirenSpecific)
                        _victim.ImmediateFrame(LF2StandardFrames.Fire);
                    break;

                case 3:
                case 30:
                    if (victimState != LF2States.Frozen)
                        _victim.ImmediateFrame(LF2StandardFrames.MpDrain);
                    else
                        _victim.ImmediateFrame(LF2StandardFrames.FallingFront2);
                    break;

                case 4:
                    break;
            }
        }
    }
}
