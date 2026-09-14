using NTSD.App;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
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

        internal static bool ShouldAbortRemainingHitPairsAfterOid300Redirect(
            LF2Entity victim,
            InteractionArea itr)
        {
            if (victim == null || itr == null || itr.kind != 0)
                return false;

            int currentOid = victim.FrameCache?.Wrapper?.characterId ?? victim.ObjectId;
            if (currentOid != 300)
                return false;

            int currentFrameId = victim.Frame?.N ?? 0;
            LF2FrameData currentFrame = victim.GetFrameDataById(currentFrameId);
            LF2FrameData futureFrame = victim.GetFrameDataById(currentFrameId + 6);
            return currentFrame?.bodies != null &&
                   currentFrame.bodies.Count > 0 &&
                   currentFrame.bodies[0].X > 1000 &&
                   futureFrame?.bodies != null &&
                   futureFrame.bodies.Count > 0;
        }

        internal static BattleHitCandidateDisposition ResolveCandidateDisposition(
            LF2Entity victim,
            InteractionArea itr,
            bool consumeGateAccepted)
        {
            if (!consumeGateAccepted)
                return BattleHitCandidateDisposition.RejectedByConsumeGate;
            if (itr == null)
                return BattleHitCandidateDisposition.Unsupported;

            int victimOid = victim?.FrameCache?.Wrapper?.characterId ??
                victim?.ObjectId ?? -1;
            if (itr.kind == 0 && victimOid == 300)
                return BattleHitCandidateDisposition.Oid300Redirect;

            switch (itr.kind)
            {
                case 0:
                case 9:
                    return BattleHitCandidateDisposition.Damage;
                case 6:
                    return BattleHitCandidateDisposition.HitConfirm;
                case 8:
                    return BattleHitCandidateDisposition.Kind8;
                case 14:
                    return BattleHitCandidateDisposition.Kind14;
                case 15:
                    return BattleHitCandidateDisposition.Kind15;
                case 10:
                case 11:
                case 17:
                case 18:
                    return BattleHitCandidateDisposition.Kind10Or11;
                case 1:
                    return BattleHitCandidateDisposition.Kind1Grab;
                case 3:
                    return BattleHitCandidateDisposition.Kind3Grab;
                case 2:
                case 7:
                    return BattleHitCandidateDisposition.Pickup;
                default:
                    return BattleHitCandidateDisposition.Unsupported;
            }
        }

        internal static bool IsAttackDisposition(
            BattleHitCandidateDisposition disposition)
        {
            return disposition == BattleHitCandidateDisposition.Oid300Redirect ||
                   disposition == BattleHitCandidateDisposition.Damage ||
                   disposition == BattleHitCandidateDisposition.Kind8 ||
                   disposition == BattleHitCandidateDisposition.Kind14 ||
                   disposition == BattleHitCandidateDisposition.Kind15 ||
                   disposition == BattleHitCandidateDisposition.Kind10Or11;
        }

        internal static float ResolveStandardDamageKnockbackX(LF2Entity attacker, LF2Entity victim, InteractionArea itr, bool knockback, float defaultDvx)
        {
            if (attacker == null || victim == null || itr == null)
                return defaultDvx;

            bool attackerState2000 = ResolveAttackerState(attacker) == LF2States.HeavyWeaponInSky;
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

        internal static bool ShouldSkipOid100KnockbackTail(
            LF2Entity victim,
            InteractionArea itr,
            bool knockback)
        {
            return victim?.Runtime != null && itr != null &&
                   knockback && victim.Runtime.Vx > -5f && victim.Runtime.Vx < 5f &&
                   itr.dvx == 0f;
        }

        internal static void ApplyKnockdownHeldPairVrest(
            LF2Entity victim,
            LF2Entity attacker)
        {
            if (victim?.Runtime == null || attacker?.Runtime == null ||
                victim.Runtime.LinkState <= 0)
            {
                return;
            }

            int victimSlot = victim.Runtime.SlotIndex;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int heldTargetSlot = victim.Runtime.ResolveActiveHeldSlotIndex();
            if (victimSlot < 0 || attackerSlot < 0 || heldTargetSlot < 0)
                return;

            LF2Entity heldTarget = victim.Match?.FindEntityByRuntimeSlotForQuery(
                heldTargetSlot);
            if (heldTarget?.Runtime == null ||
                heldTarget.Runtime.HolderStableId != victimSlot)
            {
                return;
            }

            heldTarget.ItrRest?.SetVrest(attackerSlot, 45);
            victim.ItrRest?.SetVrest(heldTargetSlot, 30);
        }

        internal static void ApplyActiveHolderFrameDelay(LF2Entity attacker)
        {
            if (attacker?.Runtime == null || attacker.Runtime.LinkState >= 0)
                return;

            int holderSlot = attacker.Runtime.ResolveActiveHolderSlotIndex();
            LF2Entity holder = holderSlot >= 0
                ? attacker.Match?.FindEntityByRuntimeSlotForQuery(holderSlot)
                : null;
            if (holder != null)
                holder.FrameDelay = attacker.FrameDelay;
        }

        internal static LF2CharacterData ResolveCharacterData(LF2Entity entity)
        {
            return (entity as LF2LivingObject)?._FrameDataWrapper?.characterData
                ?? entity?.FrameCache?.Wrapper?.characterData;
        }

        internal static void RecordDamageEffectSound(LF2Entity attacker, InteractionArea itr)
        {
            if (attacker == null || itr == null)
                return;

            string cue = itr.effect switch
            {
                0 => "SFX_001",
                1 => "SFX_002",
                2 => "SFX_006",
                3 => "SFX_010",
                4 => "SFX_011",
                5 => "SFX_004",
                _ => "SFX_001",
            };
            attacker.QueueBattleSound(cue);
        }

        internal static void RecordStandardHurtSounds(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr,
            bool knockback)
        {
            if (attacker == null || victim == null || itr == null)
                return;

            LF2CharacterData attackerData = ResolveCharacterData(attacker);
            LF2CharacterData victimData = ResolveCharacterData(victim);
            int victimType = victim.GetCurrentDataObjectTypeForSimulation();

            if (attackerData != null &&
                attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.SpecialAttack &&
                !string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
            {
                attacker.QueueBattleSound(attackerData.weapon_broken_sound);
            }

            if (victimType == (int)LF2ObjectType.Character)
            {
                (knockback ? victim : attacker).QueueBattleSound(knockback ? "SFX_006" : "SFX_001");
                if (itr.effect == 1)
                {
                    if (knockback)
                    {
                        victim.QueueBattleSound("SFX_033");
                        victim.QueueBattleSound("SFX_006");
                    }
                    else
                    {
                        victim.QueueBattleSound("SFX_032");
                        attacker.QueueBattleSound("SFX_001");
                    }
                }
            }

            if (victimType > 0 &&
                victimData != null &&
                !string.IsNullOrWhiteSpace(victimData.weapon_hit_sound))
            {
                victim.QueueBattleSound(victimData.weapon_hit_sound);
            }
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

            LF2CharacterData attackerData = LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData victimData = LF2HitResolveRuntimeData.ResolveCharacterData(victim);
            if (attackerData == null ||
                victimData == null ||
                victim.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
            {
                return false;
            }

            BattleOrdinaryDefenseResult result =
                BattleOrdinaryDefenseResolver.Resolve(
                    itr.kind,
                    itr.effect,
                    itr.spark,
                    itr.dbdefend,
                    itr.dvx,
                    attacker.Dirh(),
                    victim.Dirh(),
                    victim.Frame?.D?.state ?? 0,
                    victim.Health.HP,
                    attackerData.type_sub);
            return result.Decision ==
                BattleOrdinaryDefenseDecisionKind.Applies;
        }

    }

    internal sealed class LF2CharacterDatHitResolver
    {
        private readonly LF2Entity _victim;
        private readonly LF2LivingObject _livingVictim;
        private readonly LF2HitCountersModule _hitCounters;
        private readonly InteractionArea _runtimeItr = new InteractionArea();

        public LF2CharacterDatHitResolver(
            LF2Entity victim,
            LF2HitCountersModule hitCounters)
        {
            _victim = victim;
            _livingVictim = victim as LF2LivingObject;
            _hitCounters = hitCounters ??
                throw new System.ArgumentNullException(nameof(hitCounters));
        }

        internal static bool CanResolveTarget(LF2Entity target)
        {
            return target != null &&
                   target.Health != null &&
                   target.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        internal static bool TryResolveHit(LF2Entity target, InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!CanResolveTarget(target))
                return false;

            return target.TryResolveCharacterDatHit(itr, attacker, attackerPos, vol);
        }

        public bool ResolveHit(InteractionArea itr, LF2Entity attacker, Vector3 attackerPos, PhysicsState.BattleVolume vol)
        {
            if (!PassBaseHit(attacker))
                return false;

            if (itr.kind == 4)
            {
                if (attacker.Runtime == null ||
                    attacker.Runtime.EnvironmentState320 <= 0)
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

            bool acceptHit = false;
            bool defended = false;
            bool isKnockdown = false;
            float effectDvx = 0f;
            float effectDvy = 0f;
            int injury = 0;
            int effectNum = 0;
            bool hitCountAlreadyRecorded = false;
            bool standardDamageApplied = false;

            int victimState = _victim.GetState();

            if (itr.kind == 0 || itr.kind == 9)
            {
                acceptHit = true;

                int attackerDir = attacker.Dirh();
                effectDvx = itr.dvx != 0 ? attackerDir * itr.dvx : 0f;
                effectDvy = itr.dvy != 0 ? itr.dvy : 0f;

                effectNum = itr.effect;

                int currentVictimOid = _victim.FrameCache?.Wrapper?.characterId ?? _victim.ObjectId;
                if (currentVictimOid == 300)
                {
                    LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
                    _hitCounters.SetHitStateCount(45);
                    LF2FrameData frameNow = _victim.Frame?.D;
                    LF2FrameData futureFrame = _victim.GetFrameDataById((_victim.Frame?.N ?? 0) + 6);
                    int currentBodyX = (frameNow?.bodies != null && frameNow.bodies.Count > 0)
                        ? frameNow.bodies[0].X
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

                SimulationWorld standardWorld = _victim.Match ?? attacker.Match;
                bool standardDamageSucceeded =
                    standardWorld?.DamageWriter.ApplyStandardCharacterDamage(
                        standardWorld,
                        attacker,
                        _victim,
                        _hitCounters,
                        itr) == true;
                return standardDamageSucceeded;
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
                    attacker.DirectWriteRawFramePreserveWaitCounter(itr.dvx);
                    attacker.Runtime.X = _victim.Runtime.X;
                    attacker.Runtime.Z = _victim.Runtime.Z + 1f;
                    attacker.Runtime.XInt = _victim.Runtime.XInt;
                    attacker.Runtime.ZInt = _victim.Runtime.ZInt + 1;
                }
                return true;
            }
            else if (itr.kind == 14)
            {
                ApplyKind14DirectionalBlockFrom(attacker);
                return false;
            }
            else if (NTSD.Simulation.Ecs.BattleNativeImpactResolver.IsImpactKind(itr.kind))
            {
                SimulationWorld impactWorld = _victim.Match ?? attacker?.Match;
                return impactWorld?.DamageWriter.TryApplyNativeImpact(impactWorld, attacker, _victim, itr) == true;
            }
            else if (itr.kind == 15)
            {
                if (attacker != null)
                    ApplyWhirlwindCharacterForce(attacker);
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
                if (attacker.ItrRest != null)
                    attacker.ItrRest.Arest = itrArest;

                if (attacker.FrameDelay >= 0)
                    attacker.FrameDelay = 3;
                _victim.FrameDelay = -3;

                LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);

                if (!isKnockdown && _victim.Runtime.Vy == 0f &&
                    _hitCounters.HitStateCount >= 30 && itr.kind == 7)
                {
                    _victim.ImmediateFrame(LF2StandardFrames.DefendBroken);
                }

                bool addedNonKnockdownDvx = false;
                if (!defended)
                {
                    bool skipOid100KnockbackTail =
                        LF2HitResolveRuntimeData.ShouldSkipOid100KnockbackTail(
                            _victim,
                            itr,
                            isKnockdown);
                    float resolvedDvx = LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(attacker, _victim, itr, isKnockdown, effectDvx);
                    if (isKnockdown)
                    {
                        _victim.KnockbackVx += resolvedDvx;
                        if (!skipOid100KnockbackTail)
                            LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(_victim);
                        bool facingRight = _victim.Dirh() > 0;
                        int fallFrame = facingRight
                            ? (_victim.KnockbackVx <= 0.0 ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack)
                            : (_victim.KnockbackVx >= 0.0 ? LF2StandardFrames.FallingFront : LF2StandardFrames.FallingBack);
                        _victim.DirectWriteFramePreserveWaitCounter(fallFrame);
                        LF2HitResolveRuntimeData.ApplyKnockdownHeldPairVrest(
                            _victim,
                            attacker);
                    }
                    else if (resolvedDvx != 0f)
                    {
                        _victim.KnockbackVx += resolvedDvx;
                        if (!hitCountAlreadyRecorded)
                            _victim.HitCount++;
                        addedNonKnockdownDvx = true;
                    }

                    if (!isKnockdown && !skipOid100KnockbackTail)
                        LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(_victim);
                }

                if (ResolveAttackerState(attacker) == LF2States.WeaponThrowing)
                {
                    NTSDEntityRuntime attackerRuntime = attacker.Runtime;
                    if (attackerRuntime != null)
                    {
                        attacker.DirectWriteFramePreserveWaitCounter(attacker.BattleRandInt(0, 16));
                        attackerRuntime.Vx = _victim.KnockbackVx * -0.5f;
                        attackerRuntime.Vy = -4.0f;
                        if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon &&
                            _victim.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.ThrowWeapon)
                        {
                            attacker.KnockbackVx = -_victim.KnockbackVx;
                        }
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

                if (_hitCounters.Fall == 80)
                    _hitCounters.SetFall(0);
            }

            if (acceptHit && !standardDamageApplied)
                ApplyHitInjury(injury);

            if (acceptHit && itr.kind == 0)
            {
                SpawnSpark(itr, attacker, attackerPos, vol);
            }

            return acceptHit;
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

            if (_victim.RegisteredWorldForSimulation?.BoundaryWriter
                    .TryApplyKind14DirectionalBlock(attacker, _victim) == true)
            {
                return;
            }

            int attackerX = attacker.Runtime.XInt;
            int attackerZ = attacker.Runtime.ZInt;
            int victimX = _victim.Runtime.XInt;
            int victimZ = _victim.Runtime.ZInt;

            if (attackerX > victimX + 5 && (_victim.Runtime.Vx > 0f || _victim.KnockbackVx > 0f))
                _victim.Runtime.XBoundPositive = true;
            else if (attackerX < victimX - 5 && (_victim.Runtime.Vx < 0f || _victim.KnockbackVx < 0f))
                _victim.Runtime.XBoundNegative = true;

            if (attackerZ > victimZ + 2 && (_victim.Runtime.Vz > 0f || _victim.KnockbackVz > 0f))
                _victim.Runtime.ZBoundPositive = true;
            else if (attackerZ < victimZ - 2 && (_victim.Runtime.Vz < 0f || _victim.KnockbackVz < 0f))
                _victim.Runtime.ZBoundNegative = true;
        }


        private void ApplyWhirlwindCharacterForce(LF2Entity attacker)
        {
            _victim.KnockbackVx = _victim.Runtime.Vx +
                (_victim.Runtime.XInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            _victim.Runtime.Vx = _victim.KnockbackVx;
            _victim.KnockbackVz = _victim.Runtime.Vz +
                (_victim.Runtime.ZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            _victim.Runtime.Vz = _victim.KnockbackVz;
            ApplyAirStep(3.0);
            _victim.RefreshRuntimeSnapshot();
        }

        private void ApplyAirStep(double vyStep)
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
                _victim.KnockbackVy = _victim.Runtime.Vy;
            }
        }

        private static int ResolveAttackerState(LF2Entity attacker)
        {
            if (attacker is LF2WeaponBase weapon)
                return weapon.Frame?.D?.state ?? 0;

            return attacker?.GetState() ?? 0;
        }

        private bool HitFall(int currentInjury, ref float effectDvx, ref float effectDvy, InteractionArea itr, LF2Entity attacker)
        {
            int fallInc = itr.fall != 0 ? itr.fall : NTSDGlobal.Default.Fall.Value;
            int prevState = _victim.GetFrameDataById(_victim.Frame?.Prev ?? 0)?.state ?? 0;
            int prev2State = _victim.GetFrameDataById(
                    _victim.Runtime?.PrevFrame2 ?? 0)?.state
                ?? _victim.Frame?.Prev2D?.state
                ?? 0;

            bool forceKnockback = _victim.Health.HP <= 0 ||
                                  itr.effect == 4 ||
                                  prevState == LF2States.Frozen ||
                                  prev2State == LF2States.Falling;

            if (forceKnockback)
            {
                _hitCounters.AddFall(fallInc);
                return HitFallDown(ref effectDvx, ref effectDvy, itr, default);
            }

            _hitCounters.AddFall(fallInc);
            int fall = _hitCounters.Fall;

            if (fall > 60)
                return HitFallDown(ref effectDvx, ref effectDvy, itr, default);

            if (fall > 40)
            {
                _hitCounters.SetFall(60);
                _victim.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.Injured6);
                if (_victim.GetRuntimeYInt() < 0)
                    return HitFallDown(ref effectDvx, ref effectDvy, itr, default);
                return false;
            }

            if (fall > 20)
            {
                _hitCounters.SetFall(40);
                bool sameDir = attacker != null && attacker.Dirh() == _victim.Dirh();
                _victim.DirectWriteFramePreserveWaitCounter(
                    sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                if (_victim.GetRuntimeYInt() < 0)
                    return HitFallDown(ref effectDvx, ref effectDvy, itr, default);
                return false;
            }

            if (fall > 0)
            {
                _hitCounters.SetFall(20);
                _victim.DirectWriteFramePreserveWaitCounter(LF2StandardFrames.Injured);
                if (_victim.GetRuntimeYInt() < 0)
                {
                    bool sameDir = attacker != null && attacker.Dirh() == _victim.Dirh();
                    _victim.DirectWriteFramePreserveWaitCounter(
                        sameDir ? LF2StandardFrames.Injured4 : LF2StandardFrames.Injured2);
                }
            }

            return false;
        }

        private bool HitFall(int currentInjury, ref float effectDvy, InteractionArea itr, LF2Entity attacker)
        {
            float effectDvxDummy = 0f;
            return HitFall(currentInjury, ref effectDvxDummy, ref effectDvy, itr, attacker);
        }

        private bool HitFallDown(ref float effectDvx, ref float effectDvy, InteractionArea itr, Vector3 attackerPos)
        {
            _hitCounters.ResetFall();

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

            LF2FrameData frame = catcherEntity.Frame?.D;
            return frame == null ||
                   !frame.TryGetPrimaryCatchPoint(
                       out BattleCatchPointValue cpoint) ||
                   cpoint.Hurtable != 0;
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
