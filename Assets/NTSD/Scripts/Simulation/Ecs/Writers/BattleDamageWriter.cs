using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using UnityEngine;

namespace NTSD.Simulation.Ecs
{
    internal sealed class BattleDamageWriter
    {
        internal bool TryApplyCurrentDatTargetHit(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr,
            Vector3 attackerPosition)
        {
            if (world == null || attacker?.Runtime == null ||
                victim?.Runtime == null || itr == null)
            {
                return false;
            }

            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            if (victimType == (int)LF2ObjectType.Character)
            {
                if (victim is LF2Character character)
                    return character.Hit(
                        itr,
                        attacker,
                        attackerPosition,
                        default);

                return LF2CharacterDatHitResolver.CanResolveTarget(victim) &&
                       LF2CharacterDatHitResolver.TryResolveHit(
                           victim,
                           itr,
                           attacker,
                           attackerPosition,
                           default);
            }

            if (victimType == (int)LF2ObjectType.LightWeapon ||
                victimType == (int)LF2ObjectType.HeavyWeapon ||
                victimType == (int)LF2ObjectType.ThrowWeapon ||
                victimType == (int)LF2ObjectType.Drink)
            {
                if (victim is LF2Weapon weapon)
                    return weapon.Hit(itr, attacker);

                return ApplyGenericWeaponTypedHit(
                    world,
                    attacker,
                    victim,
                    itr,
                    victimType);
            }

            if (victimType == (int)LF2ObjectType.SpecialAttack)
            {
                if (victim is LF2SpecialAttack specialAttack)
                    return specialAttack.Hit(itr, attacker);

                return ApplyGenericObjectTypedHit(
                    world,
                    attacker,
                    victim,
                    itr,
                    allowKind9: true);
            }

            if (victimType == (int)LF2ObjectType.Other)
            {
                return ApplyGenericObjectTypedHit(
                    world,
                    attacker,
                    victim,
                    itr,
                    allowKind9: false);
            }

            return false;
        }

        private bool ApplyGenericObjectTypedHit(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr,
            bool allowKind9)
        {
            if (itr.kind == 14)
            {
                world.BoundaryWriter.TryApplyKind14DirectionalBlock(
                    attacker,
                    victim);
                return allowKind9;
            }

            if (itr.kind == 9 && !allowKind9)
                return false;
            if (itr.kind != 0 && itr.kind != 9)
                return false;

            return ApplySpecialAttackDamage(world, attacker, victim, itr);
        }

        private bool ApplyGenericWeaponTypedHit(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr,
            int victimType)
        {
            int attackerSlot = attacker.Runtime.SlotIndex;
            if (attackerSlot >= 0 &&
                victim.ItrRest?.HasVrest(attackerSlot) == true)
            {
                return false;
            }

            if (itr.kind == 9)
            {
                LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
                return true;
            }

            if (itr.kind == 14)
            {
                world.BoundaryWriter.TryApplyKind14DirectionalBlock(
                    attacker,
                    victim);
                return false;
            }

            if (itr.kind == 10 || itr.kind == 11)
            {
                if (itr.kind == 11 && victim.WeaponCount >= 0)
                    return false;
                int victimOid = LF2Entity.ResolveCurrentDataObjectId(victim);
                if (victimOid == 201 || victimOid == 202)
                    return false;

                const double velocityFactor = 0.9345794392523364;
                bool lightLike =
                    victimType == (int)LF2ObjectType.LightWeapon ||
                    victimType == (int)LF2ObjectType.ThrowWeapon ||
                    victimType == (int)LF2ObjectType.Drink;
                int expectedState = lightLike
                    ? LF2States.WeaponInSky
                    : LF2States.HeavyWeaponInSky;
                if (victim.GetState() != expectedState)
                    victim.DirectWriteRawFramePreserveWaitCounter(0);

                victim.KnockbackVx = victim.Runtime.Vx * velocityFactor;
                victim.Runtime.Vx = victim.KnockbackVx;
                victim.KnockbackVz = victim.Runtime.Vz * velocityFactor;
                victim.Runtime.Vz = victim.KnockbackVz;
                ApplyGenericWeaponAirStep(
                    victim,
                    lightLike ? 3.0 : 2.3);
                victim.WeaponCount = NTSDGlobal.Gameplay.FluteCharacterWeaponCount;
                return true;
            }

            if (itr.kind == 15 || itr.kind == 16)
            {
                ApplyGenericWeaponWhirlwind(
                    victim,
                    attacker,
                    victimType);
                return true;
            }

            return itr.kind == 0 &&
                   ApplyWeaponDamage(world, attacker, victim, itr);
        }

        private static void ApplyGenericWeaponAirStep(
            LF2Entity victim,
            double velocityStep)
        {
            if (victim.GetRuntimeYInt() >= -2)
            {
                victim.Runtime.Y = -2.0;
                victim.Runtime.YInt = -2;
                victim.Runtime.Vy = -6.0;
                return;
            }

            if (victim.Runtime.Vy > -6.0)
            {
                victim.Runtime.Vy -= velocityStep;
                victim.KnockbackVy = victim.Runtime.Vy;
            }
        }

        private static void ApplyGenericWeaponWhirlwind(
            LF2Entity victim,
            LF2Entity attacker,
            int victimType)
        {
            bool lightLike =
                victimType == (int)LF2ObjectType.LightWeapon ||
                victimType == (int)LF2ObjectType.ThrowWeapon ||
                victimType == (int)LF2ObjectType.Drink;
            bool heavyLike = victimType == (int)LF2ObjectType.HeavyWeapon;
            if (lightLike)
            {
                int victimOid = LF2Entity.ResolveCurrentDataObjectId(victim);
                if (victimOid == 201 || victimOid == 202)
                    return;
                if (victim.GetState() != LF2States.WeaponInSky)
                    victim.DirectWriteRawFramePreserveWaitCounter(0);
                ApplyGenericWeaponWhirlwindVelocity(victim, attacker, 3.0);
            }
            else if (heavyLike)
            {
                if (victim.GetState() != LF2States.HeavyWeaponInSky)
                    victim.DirectWriteRawFramePreserveWaitCounter(0);
                ApplyGenericWeaponWhirlwindVelocity(victim, attacker, 2.3);
            }
        }

        private static void ApplyGenericWeaponWhirlwindVelocity(
            LF2Entity victim,
            LF2Entity attacker,
            double verticalStep)
        {
            victim.KnockbackVx = victim.Runtime.Vx +
                (victim.Runtime.XInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            victim.Runtime.Vx = victim.KnockbackVx;
            victim.KnockbackVz = victim.Runtime.Vz +
                (victim.Runtime.ZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            victim.Runtime.Vz = victim.KnockbackVz;

            if (victim.GetRuntimeYInt() >= -2)
            {
                victim.Runtime.Y = -2.0;
                victim.Runtime.YInt = -2;
                victim.Runtime.Vy = -6.0;
            }

            if (victim.Runtime.Vy > -6.0)
            {
                victim.Runtime.Vy -= verticalStep;
                victim.KnockbackVy = victim.Runtime.Vy;
            }
        }

        internal bool ApplyStandardCharacterDamage(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                victim?.Runtime == null ||
                victim.Health == null ||
                victimHitCounters == null ||
                itr == null ||
                victim.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                return false;
            }

            LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
            ApplyStandardVitalAndStatWrites(world, attacker, victim, itr.injury);

            victim.HitCount++;
            bool knockdown = ApplyStandardFall(
                attacker,
                victim,
                victimHitCounters,
                itr);

            if (itr.kind != 9)
            {
                LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                    attacker,
                    victim,
                    itr,
                    knockdown);
            }

            float defaultDvx = itr.dvx != 0
                ? attacker.Dirh() * (float)itr.dvx
                : 0f;
            bool skipOid100Tail =
                LF2HitResolveRuntimeData.ShouldSkipOid100KnockbackTail(
                    victim,
                    itr,
                    knockdown);
            float resolvedDvx =
                LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(
                    attacker,
                    victim,
                    itr,
                    knockdown,
                    defaultDvx);
            victim.KnockbackVx += resolvedDvx;
            if (!skipOid100Tail)
                LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(victim);

            ApplyStandardState3000Tail(attacker);

            if (knockdown)
            {
                bool facingRight = victim.Dirh() > 0;
                int fallFrame = facingRight
                    ? (victim.KnockbackVx <= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack)
                    : (victim.KnockbackVx >= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack);
                victim.DirectWriteFramePreserveWaitCounter(fallFrame);
                LF2HitResolveRuntimeData.ApplyKnockdownHeldPairVrest(
                    victim,
                    attacker);
            }

            victimHitCounters.SetHitStateCount(45);
            if (attacker.FrameDelay >= 0)
                attacker.FrameDelay = 3;
            victim.FrameDelay = -3;

            int attackerSlot = attacker.Runtime.SlotIndex;
            if (attackerSlot >= 0)
            {
                if (knockdown)
                    victim.ItrVrestUpdateKnockdown(attackerSlot, itr, true);
                else
                    victim.ItrVrestUpdate(attackerSlot, itr, true);
            }

            int itrArest = LF2Entity.ResolveArestCooldown(itr.arest, itr.vrest);
            attacker.AttackExempt = itrArest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = itrArest;

            LF2HitResolveRuntimeData.ApplyCaughtVictimHurtFrame(
                victim,
                attacker,
                victimHitCounters.Fall);
            if (victimHitCounters.Fall == 80)
                victimHitCounters.SetFall(0);

            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);
            ApplyStandardState1002Tail(attacker, victim);

            if (victim is LF2LivingObject livingVictim &&
                attacker is LF2LivingObject livingAttacker)
            {
                livingVictim.Attacker = livingAttacker;
            }

            return true;
        }

        internal bool ApplyKind16(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                victim?.Runtime == null ||
                victim.Health == null ||
                itr == null)
            {
                return false;
            }

            int adjustedInjury = itr.injury;
            if (victim.FallDamageDiv > 0)
                adjustedInjury = itr.injury * 100 / victim.FallDamageDiv;

            if (victim.Health.HP > 0 &&
                adjustedInjury >= victim.Health.HP &&
                victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.KillStat++;

                int killStatIndex = victim.Unk344;
                if (killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= adjustedInjury;
            victim.Health.HPBound -= adjustedInjury / 3;
            victim.ComboCountVic += adjustedInjury;

            if (victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.ComboCountAtk += adjustedInjury;
            }

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
                world.DamageStats[damageStatIndex] += adjustedInjury;

            victim.QueueBattleSound("SFX_065");
            victim.DirectWriteRawFramePreserveWaitCounter(LF2StandardFrames.MpDrain);
            victim.AttackingCounter = 0;

            int attackerSlot = attacker.Runtime.SlotIndex;
            if (attackerSlot >= 0)
                victim.ItrVrestUpdate(attackerSlot, itr, true);

            ReleaseHeldTarget(world, attacker, victim);
            return true;
        }

        internal bool ApplyWeaponDamage(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                victim?.Runtime == null ||
                itr == null ||
                itr.kind != 0)
            {
                return false;
            }

            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            bool lightThrow = victimType == (int)LF2ObjectType.LightWeapon;
            bool heavyLike = victimType == (int)LF2ObjectType.HeavyWeapon;
            bool flyingA = victimType == (int)LF2ObjectType.ThrowWeapon;
            bool flyingB = victimType == (int)LF2ObjectType.Drink;
            bool flyingLike = flyingA || flyingB;
            bool damageableWeapon = lightThrow || heavyLike || flyingLike;
            bool normalVitalWeapon = lightThrow || heavyLike || flyingA;
            int attackerSlot = attacker.Runtime.SlotIndex;
            int itrArest = LF2Entity.ResolveArestCooldown(itr.arest, itr.vrest);

            if (!flyingB)
                LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
            if (normalVitalWeapon)
                ApplyWeaponNormalVitalAndStatWrites(world, victim, itr.injury);

            if (damageableWeapon)
            {
                if (itr.bdefend == 100)
                    victim.Runtime.WeaponFlightCounter = -1;
                else
                    victim.Runtime.WeaponFlightCounter -= itr.injury;
            }

            // Alignment contract R4-HIT-004: normal weapon type tails own the first
            // hit-confirm/relation writes after the C++ hurt/reaction path completes.
            if (!damageableWeapon)
                victim.RelationTeam = attacker.RelationTeam;
            if (victimType != (int)LF2ObjectType.HeavyWeapon || itr.fall > 40)
                victim.HitCount++;

            victim.FallCounter += itr.fall != 0 ? itr.fall : 20;
            if (damageableWeapon)
                victim.FallCounter = 80;

            bool knockdown = victim.FallCounter > 60 &&
                victimType != (int)LF2ObjectType.SpecialAttack;
            LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                attacker,
                victim,
                itr,
                knockdown);

            float defaultDvx = itr.dvx != 0
                ? attacker.Dirh() * (float)itr.dvx
                : 0f;
            bool skipOid100Tail =
                LF2HitResolveRuntimeData.ShouldSkipOid100KnockbackTail(
                    victim,
                    itr,
                    knockdown);
            if (flyingLike && !skipOid100Tail)
            {
                ApplyFlyingWeaponKnockbackX(attacker, victim, itr);
            }
            else
            {
                float resolvedDvx =
                    LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(
                        attacker,
                        victim,
                        itr,
                        knockdown,
                        defaultDvx);
                victim.KnockbackVx += resolvedDvx;
            }
            if (!skipOid100Tail)
                LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(victim);

            ApplyWeaponAttackerState3000PreKnockdown(attacker, victim);

            if (knockdown)
            {
                if ((!heavyLike &&
                     victimType != (int)LF2ObjectType.SpecialAttack) ||
                    itr.fall > 40)
                {
                    victim.KnockbackVy += itr.dvy != 0 ? itr.dvy : -7.0;
                }

                if ((int)(victim.KnockbackVy + victim.GetRuntimeYInt()) > 0)
                    victim.KnockbackVy = 12.0;

                int hitFrame = victim.Dirh() > 0
                    ? (victim.KnockbackVx <= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack)
                    : (victim.KnockbackVx >= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack);
                victim.DirectWriteRawFramePreserveWaitCounter(hitFrame);
                LF2HitResolveRuntimeData.ApplyKnockdownHeldPairVrest(
                    victim,
                    attacker);
                victim.FallCounter = 0;
            }
            else if (heavyLike)
            {
                victim.SwitchDir(attacker.Runtime.Dir ?? victim.Runtime.Dir);
                if (itr.fall <= 40 &&
                    victim.GetRuntimeYInt() >= 0 &&
                    itr.effect != 4)
                {
                    victim.ImmediateFrame(20);
                }
                else
                {
                    victim.ImmediateFrame(victim.BattleRandInt(0, 6));
                }
            }
            else if (lightThrow || flyingLike)
            {
                victim.ImmediateFrame(victim.BattleRandInt(0, 16));
            }

            victim.HitStateCount = 45;
            if (attacker.FrameDelay >= 0)
                attacker.FrameDelay = 3;
            victim.FrameDelay = -3;
            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);
            attacker.AttackExempt = itrArest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = itrArest;
            if (attackerSlot >= 0 && itr.vrest > 0)
                victim.ItrRest?.SetVrest(attackerSlot, itr.vrest);

            ApplyWeaponAttackerState1002Response(attacker, victim);
            ApplyKind0WeaponVictimTail(attacker, victim, itr);
            victim.RecordKind0Hit(attacker, itr);
            return true;
        }

        internal bool ApplySpecialAttackDamage(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                victim?.Runtime == null ||
                victim.Health == null ||
                itr == null)
            {
                return false;
            }

            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            if (itr.kind == 9)
            {
                if (victimType != (int)LF2ObjectType.SpecialAttack)
                    return false;

                LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
                LF2CharacterData victimData =
                    victim.FrameCache?.Wrapper?.characterData;
                if (!string.IsNullOrEmpty(victimData?.weapon_broken_sound))
                    victim.QueueBattleSound(victimData.weapon_broken_sound);

                attacker.FrameDelay = -3;
                if (victim.GetState() == LF2States.ObjectFlying)
                {
                    victim.HitConfirm2 = 1;
                    victim.DirectWriteRawFramePreserveWaitCounter(40);
                }
                else
                {
                    CopyRelation(attacker, victim);
                    victim.HitConfirm2 = 1;
                    victim.DirectWriteRawFramePreserveWaitCounter(30);
                    ResetType3HitMotion(victim);
                    victim.Runtime.AnimCounter = attacker.Runtime.SlotIndex;
                }
                return true;
            }

            if (itr.kind != 0)
                return false;

            LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
            ApplyType3NormalVitalAndStatWrites(world, victim, itr.injury);
            ApplySpecialObjectHurtTail(world, attacker, victim, itr);
            if (victimType == (int)LF2ObjectType.SpecialAttack)
                ApplyKind0Type3Tail(world, attacker, victim, itr);
            victim.RecordKind0Hit(attacker, itr);
            return true;
        }

        internal void ApplyAlternateDamage(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr)
        {
            if (world == null ||
                attacker?.Runtime == null ||
                victim?.Runtime == null ||
                victim.Health == null ||
                itr == null ||
                LF2HitResolveRuntimeData.ResolveCharacterData(victim) == null ||
                victim.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                return;
            }

            RecordAlternateLeadSound(attacker, victim);

            int injury = itr.injury;
            if (victim.FallDamageDiv > 0)
                injury = injury * 100 / victim.FallDamageDiv;

            int reducedInjury = injury / 10;
            if (victim.Health.HP > 0 &&
                reducedInjury >= victim.Health.HP &&
                victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.KillStat++;

                int killStatIndex = victim.Unk344;
                if (killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= reducedInjury;
            victim.Health.HPBound -= reducedInjury / 3;
            victim.ComboCountVic += reducedInjury;
            if (victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.ComboCountAtk += reducedInjury;
            }

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
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

            int victimPrev2State = victim.GetFrameDataById(
                victim.Runtime.PrevFrame2)?.state ?? 0;
            if (victim.GetRuntimeYInt() == 0)
            {
                int hitStateCount = victimHitCounters?.HitStateCount ??
                    victim.HitStateCount;
                if (hitStateCount > 30 && victimPrev2State == LF2States.Defending)
                {
                    victim.DirectWriteFramePreserveWaitCounter(
                        LF2StandardFrames.DefendBroken);
                }
                else if ((victim.Frame?.N ?? 0) == LF2StandardFrames.Defend)
                {
                    victim.DirectWriteFramePreserveWaitCounter(
                        LF2StandardFrames.Defend1);
                }

                ApplyAlternateGroundKnockback(attacker, victim, itr);
            }
            else
            {
                ApplyAlternateAirKnockback(attacker, victim, itr);
            }

            attacker.Runtime.AttackExempt = itr.arest < 4 && itr.vrest == 0
                ? 4
                : Mathf.Min(itr.arest, 12);
            if (itr.vrest > 0)
            {
                int attackerSlot = attacker.Runtime.SlotIndex;
                if (attackerSlot >= 0)
                {
                    int vrest = itr.vrest > 4 ? Mathf.Min(itr.vrest, 12) : 4;
                    victim.ItrRest?.SetVrest(attackerSlot, vrest);
                }
            }

            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);

            if (FrameStateIs(attacker, LF2States.WeaponThrowing))
            {
                attacker.DirectWriteFramePreserveWaitCounter(
                    attacker.BattleRandInt(0, 16));
                attacker.Runtime.Vx = victim.KnockbackVx * -0.5;
                attacker.Runtime.Vy = -4.0;
                attacker.Runtime.Vz *= -0.6666666666666666;
            }

            DampenAlternateState2000Attacker(attacker, victim);
            ApplyAlternateState3000Tail(attacker);
        }

        private static void ApplyStandardVitalAndStatWrites(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int injury)
        {
            int originalHp = victim.Health.HP;
            if (originalHp > 0 && injury >= originalHp && victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.KillStat++;

                int killStatIndex = victim.Unk344;
                if (killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= injury;
            victim.Health.HPBound -= injury / 3;
            victim.ComboCountVic += injury;
            if (victim.KillCount == -1)
            {
                LF2Entity holder = LF2HitResolveRuntimeData.ResolveHolderCopyEntity(
                    attacker);
                if (holder != null)
                    holder.ComboCountAtk += injury;
            }

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
                world.DamageStats[damageStatIndex] += injury;
        }

        private static void ApplyType3NormalVitalAndStatWrites(
            SimulationWorld world,
            LF2Entity victim,
            int injury)
        {
            // Alignment contract: R4-HIT-001. C++ normal type3 hurt shares only
            // these public vital/stat writes; type0-only kill/holder score stays excluded.
            victim.Health.HP -= injury;
            victim.Health.HPBound -= injury / 3;
            victim.ComboCountVic += injury;

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
                world.DamageStats[damageStatIndex] += injury;
        }

        private static void ApplyWeaponNormalVitalAndStatWrites(
            SimulationWorld world,
            LF2Entity victim,
            int rawInjury)
        {
            int adjustedInjury = rawInjury;
            if (victim.FallDamageDiv > 0)
                adjustedInjury = rawInjury * 100 / victim.FallDamageDiv;

            victim.Health.HP -= adjustedInjury;
            victim.Health.HPBound -= adjustedInjury / 3;
            victim.ComboCountVic += adjustedInjury;

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex >= 1 &&
                damageStatIndex <= 2 &&
                damageStatIndex < world.DamageStats.Length)
            {
                world.DamageStats[damageStatIndex] += adjustedInjury;
            }
        }

        private static void ApplySpecialObjectHurtTail(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            int itrArest = LF2Entity.ResolveArestCooldown(itr.arest, itr.vrest);

            if ((victim.Health?.HP ?? 0) <= 0 || itr.effect == 4)
                victim.FallCounter = 80;

            if (victimType != (int)LF2ObjectType.HeavyWeapon || itr.fall > 40)
                victim.HitCount++;

            victim.FallCounter += itr.fall != 0 ? itr.fall : 20;
            if (ShouldForceObjectFall80(victim))
                victim.FallCounter = 80;

            bool knockdown = false;
            if (victim.FallCounter > 60 &&
                victimType != (int)LF2ObjectType.SpecialAttack)
            {
                victim.FallCounter = 80;
                knockdown = true;
            }
            else if (victimType != (int)LF2ObjectType.SpecialAttack)
            {
                if (victim.FallCounter > 50)
                {
                    victim.FallCounter = 60;
                    victim.DirectWriteRawFramePreserveWaitCounter(226);
                    if (victim.GetRuntimeYInt() < 0)
                    {
                        victim.FallCounter = 80;
                        knockdown = true;
                    }
                }
                else if (victim.FallCounter > 30)
                {
                    victim.FallCounter = 40;
                    victim.DirectWriteRawFramePreserveWaitCounter(
                        victim.Dirh() != attacker.Dirh() ? 222 : 224);
                    if (victim.GetRuntimeYInt() < 0)
                    {
                        victim.FallCounter = 80;
                        knockdown = true;
                    }
                }
                else if (victim.FallCounter > 10)
                {
                    victim.FallCounter = 20;
                    victim.DirectWriteRawFramePreserveWaitCounter(220);
                    if (victim.GetRuntimeYInt() < 0)
                    {
                        victim.DirectWriteRawFramePreserveWaitCounter(
                            victim.Dirh() != attacker.Dirh() ? 222 : 224);
                    }
                }
            }

            LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                attacker,
                victim,
                itr,
                knockdown);
            float defaultDvx = itr.dvx != 0
                ? attacker.Dirh() * (float)itr.dvx
                : 0f;
            float resolvedDvx =
                LF2HitResolveRuntimeData.ResolveStandardDamageKnockbackX(
                    attacker,
                    victim,
                    itr,
                    knockdown,
                    defaultDvx);
            bool skipOid100Tail =
                LF2HitResolveRuntimeData.ShouldSkipOid100KnockbackTail(
                    victim,
                    itr,
                    knockdown);
            if (resolvedDvx != 0f)
                victim.KnockbackVx += resolvedDvx;
            if (!skipOid100Tail)
                LF2HitResolveRuntimeData.ApplyOid100KnockbackTail(victim);

            ApplySpecialState3000ObjectHurtTail(attacker, victim);

            if (knockdown)
            {
                if ((victimType != (int)LF2ObjectType.HeavyWeapon &&
                     victimType != (int)LF2ObjectType.SpecialAttack) ||
                    itr.fall > 40)
                {
                    victim.KnockbackVy += itr.dvy != 0 ? itr.dvy : -7.0;
                }

                if ((int)(victim.KnockbackVy + victim.GetRuntimeYInt()) > 0)
                    victim.KnockbackVy = 12.0;

                int fallFrame = victim.Dirh() > 0
                    ? (victim.KnockbackVx <= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack)
                    : (victim.KnockbackVx >= 0.0
                        ? LF2StandardFrames.FallingFront
                        : LF2StandardFrames.FallingBack);
                victim.DirectWriteRawFramePreserveWaitCounter(fallFrame);
                LF2HitResolveRuntimeData.ApplyKnockdownHeldPairVrest(
                    victim,
                    attacker);
            }

            victim.HitStateCount = 45;
            if (attacker.FrameDelay >= 0)
                attacker.FrameDelay = 3;
            victim.FrameDelay = -3;
            attacker.AttackExempt = itrArest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = itrArest;
            if (attackerSlot >= 0 && itr.vrest > 0)
                victim.ItrRest?.SetVrest(attackerSlot, itr.vrest);

            LF2HitResolveRuntimeData.ApplyCaughtVictimHurtFrame(
                victim,
                attacker,
                victim.FallCounter);
            if (victim.FallCounter == 80)
                victim.FallCounter = 0;

            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);
            ApplySpecialWeaponThrowingTail(attacker, victim, victimType);
        }

        private static bool ShouldForceObjectFall80(LF2Entity victim)
        {
            LF2FrameData previousFrame = victim.GetFrameDataById(
                victim.Frame?.Prev ?? 0);
            if (previousFrame?.state == LF2States.Frozen)
                return true;

            LF2FrameData previousFrame2 = victim.GetFrameDataById(
                victim.Runtime.PrevFrame2);
            if (previousFrame2?.state == LF2States.Falling)
                return true;

            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            return victimType == (int)LF2ObjectType.LightWeapon ||
                   victimType == (int)LF2ObjectType.HeavyWeapon ||
                   victimType == (int)LF2ObjectType.ThrowWeapon ||
                   victimType == (int)LF2ObjectType.Drink;
        }

        private static void ApplySpecialState3000ObjectHurtTail(
            LF2Entity attacker,
            LF2Entity victim)
        {
            if (!FrameStateIs(attacker, LF2States.ProjectileFlying))
                return;

            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            int victimOid = LF2Entity.ResolveCurrentDataObjectId(victim);
            bool nonCharacterVictim =
                victim.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character;
            bool skipReset = nonCharacterVictim &&
                attackerOid == 209 &&
                (victimOid == 200 ||
                 victimOid == 203 ||
                 victimOid == 205 ||
                 victimOid == 206 ||
                 victimOid == 207 ||
                 victimOid == 215 ||
                 victimOid == 216 ||
                 (victimOid == 209 && (victim.Frame?.N ?? 0) == 40));
            if (skipReset)
                return;

            attacker.DirectWriteRawFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
            LF2FrameData frame10 = attacker.GetFrameDataById(10);
            if (frame10 != null)
                attacker.Runtime.Vz = frame10.dvz;
        }

        private static void ApplySpecialWeaponThrowingTail(
            LF2Entity attacker,
            LF2Entity victim,
            int victimType)
        {
            if (!FrameStateIs(attacker, LF2States.WeaponThrowing))
                return;

            attacker.DirectWriteRawFramePreserveWaitCounter(
                attacker.BattleRandInt(0, 16));
            attacker.Runtime.Vx = victim.KnockbackVx * -0.5;
            attacker.Runtime.Vy = -4.0;
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.ThrowWeapon &&
                victimType == (int)LF2ObjectType.ThrowWeapon)
            {
                attacker.KnockbackVx = -victim.KnockbackVx;
            }
        }

        private static void ApplyKind0Type3Tail(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            int victimState = victim.GetState();
            int attackerState = attacker.GetState();
            bool skipToStateSync =
                victimState == LF2States.ObjectFlying ||
                (victimState == LF2States.ObjectExpanding &&
                 attackerState == LF2States.ObjectFlying);

            if (!skipToStateSync)
            {
                LF2Entity attackerHolder = ResolveActiveHolder(world, attacker);
                CopyRelation(
                    attacker.Runtime.LinkState < 0 ? attackerHolder : attacker,
                    victim);
                victim.HitConfirm2 = 1;
                ResetType3HitMotion(victim);

                if (attacker.ObjectId == 209 && IsKarasuOid(victim.ObjectId))
                {
                    CopyRelation(attacker, victim);
                    if (victim.TryApplyRuntimeIdentity(
                        attacker.ObjectId,
                        40,
                        false,
                        out _))
                    {
                        victim.Trans?.SyncDirectFrameData(
                            victim.Frame.D.wait,
                            victim.Frame.D.next,
                            40);
                        victim.Frame.Prev = 40;
                    }
                    skipToStateSync = true;
                }
                else
                {
                    bool frame20 = false;
                    bool checkEffectGate;
                    if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                        (int)LF2ObjectType.Character)
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

                    if (checkEffectGate &&
                        (itr.effect == 2 || itr.effect == 20))
                    {
                        frame20 = true;
                    }

                    victim.DirectWriteHeldFramePreserveWaitCounter(
                        frame20 ? 20 : 30);
                    if (frame20)
                    {
                        if (attacker.Runtime.LinkState < 0)
                        {
                            if (attacker.ObjectId == 213 &&
                                IsKarasuOid(victim.ObjectId))
                            {
                                ReplaceWithActiveKarasuData(world, victim);
                            }
                            CopyRelation(attackerHolder, victim);
                        }
                    }
                    else
                    {
                        if (attacker.ObjectId == 8 &&
                            IsKarasuOid(victim.ObjectId))
                        {
                            ReplaceWithActiveKarasuData(world, victim);
                        }

                        if (attacker.Runtime.LinkState < 0)
                        {
                            if (attacker.ObjectId == 213 &&
                                IsKarasuOid(victim.ObjectId))
                            {
                                ReplaceWithActiveKarasuData(world, victim);
                            }
                            CopyRelation(attackerHolder, victim);
                        }
                    }
                }
            }

            victimState = victim.GetState();
            attackerState = attacker.GetState();
            if ((victimState == LF2States.ObjectFlying &&
                 attackerState == LF2States.ObjectFlying) ||
                (victimState == LF2States.ObjectExpanding &&
                 attackerState == LF2States.ObjectExpanding))
            {
                victim.DirectWriteHeldFramePreserveWaitCounter(20);
                ResetType3HitMotion(victim);
                attacker.DirectWriteHeldFramePreserveWaitCounter(20);
                ResetType3HitMotion(attacker);

                if (attacker.Runtime.LinkState < 0)
                {
                    LF2Entity holder = ResolveActiveHolder(world, attacker);
                    if (holder != null && holder.FrameDelay > 0)
                        holder.FrameDelay = -holder.FrameDelay;
                }
                else if (attacker.FrameDelay > 0)
                {
                    attacker.FrameDelay = -attacker.FrameDelay;
                }
            }

            ApplyType3EffectTail(world, victim, itr.effect);
        }

        private static void ApplyType3EffectTail(
            SimulationWorld world,
            LF2Entity victim,
            int effect)
        {
            LF2FrameData previousFrame = victim.GetFrameDataById(
                victim.Frame?.Prev ?? 0);
            int previousState = previousFrame?.state ?? 0;
            bool characterDat =
                victim.GetCurrentDataObjectTypeForSimulation() ==
                (int)LF2ObjectType.Character;

            if (effect == 3 || effect == 30)
            {
                if (characterDat && previousState != LF2States.Frozen)
                {
                    victim.DirectWriteHeldFramePreserveWaitCounter(200);
                    victim.AttackingCounter = 0;
                    world.QueueSound("SFX_065", victim.Runtime.XInt);
                }
            }
            else if (effect >= 5000 && effect < 6000)
            {
                if (victim.Health != null)
                {
                    int nextPp = victim.Health.PP - (effect - 5000);
                    victim.Health.PP = nextPp < 0 ? 0 : nextPp;
                }
            }
            else if (effect >= 6000 && effect < 7000)
            {
                victim.DirectWriteHeldFramePreserveWaitCounter(effect - 6000);
            }
            else if (effect == 2 || effect == 21 || effect == 22)
            {
                if (characterDat)
                    ApplyType3BurningEffect(world, victim);
            }
            else if (effect == 20)
            {
                if (characterDat && previousState != 18)
                    ApplyType3BurningEffect(world, victim);
            }
            else if (effect == 23)
            {
                world.QueueSound("SFX_068", victim.Runtime.XInt);
            }
        }

        private static void ApplyType3BurningEffect(
            SimulationWorld world,
            LF2Entity victim)
        {
            victim.DirectWriteHeldFramePreserveWaitCounter(203);
            victim.AttackingCounter = 0;
            victim.SwitchDir(victim.KnockbackVx < 0.0 ? "right" : "left");
            world.QueueSound("SFX_068", victim.Runtime.XInt);
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

        private static LF2Entity ResolveActiveHolder(
            SimulationWorld world,
            LF2Entity entity)
        {
            if (world == null || entity?.Runtime == null)
                return null;

            int holderSlot = entity.Runtime.HolderStableId;
            if (holderSlot < 0 || holderSlot >= world.MaxRuntimeSlotsForServices)
                return null;

            return world.FindEntityByRuntimeSlotForQuery(holderSlot);
        }

        private static bool IsKarasuOid(int oid)
        {
            return oid == 200 || oid == 203 || oid == 205 || oid == 206 ||
                   oid == 207 || oid == 215 || oid == 216;
        }

        private static void ReplaceWithActiveKarasuData(
            SimulationWorld world,
            LF2Entity victim)
        {
            for (int slot = 0; slot < world.MaxRuntimeSlotsForServices; slot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotForQuery(slot);
                if (candidate == null || candidate.ObjectId != 209)
                    continue;

                int frameId = victim.Frame?.N ?? 0;
                if (victim.TryApplyRuntimeIdentity(209, frameId, false, out _) &&
                    victim.Frame?.D != null)
                {
                    victim.Trans?.SyncDirectFrameData(
                        victim.Frame.D.wait,
                        victim.Frame.D.next,
                        frameId);
                    victim.Frame.Prev = frameId;
                }
                return;
            }
        }

        private static void ApplyKind0WeaponVictimTail(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            if (victimType == (int)LF2ObjectType.LightWeapon)
            {
                victim.HitConfirm2 = 1;
                victim.DirectWriteRawFramePreserveWaitCounter(
                    victim.BattleRandInt(0, 16));
                victim.RelationTeam = attacker.RelationTeam;
                return;
            }

            if (victimType == (int)LF2ObjectType.ThrowWeapon ||
                victimType == (int)LF2ObjectType.Drink)
            {
                if (attackerSlot >= 0)
                    attacker.ItrRest?.SetVrest(attackerSlot, 30);
                victim.HitConfirm2 = 1;
                victim.DirectWriteRawFramePreserveWaitCounter(
                    victim.BattleRandInt(0, 16));
                victim.RelationTeam = attacker.RelationTeam;
                return;
            }

            if (victimType != (int)LF2ObjectType.HeavyWeapon)
                return;

            victim.HitConfirm2 = 1;
            int vrest = itr.fall <= 40 && itr.effect != 4 ? 3 : 19;
            if (attackerSlot >= 0 && attacker.Runtime.LinkState == -2)
            {
                int holderSlot = attacker.Runtime.HolderStableId;
                LF2Entity holder = victim.Match?.FindEntityByRuntimeSlotForQuery(
                    holderSlot);
                holder?.ItrRest?.SetVrest(attackerSlot, vrest);
            }
            else if (attackerSlot >= 0 &&
                     attacker.GetCurrentDataObjectTypeForSimulation() !=
                         (int)LF2ObjectType.HeavyWeapon)
            {
                attacker.ItrRest?.SetVrest(attackerSlot, vrest);
            }

            victim.SwitchDir(attacker.Runtime.Dir);
            if (itr.fall <= 40 &&
                victim.GetRuntimeYInt() >= 0 &&
                itr.effect != 4)
            {
                victim.DirectWriteRawFramePreserveWaitCounter(20);
            }
            else
            {
                victim.DirectWriteRawFramePreserveWaitCounter(
                    victim.BattleRandInt(0, 6));
            }
            victim.RelationTeam = attacker.RelationTeam;
        }

        private static void ApplyFlyingWeaponKnockbackX(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            bool attackerState2000 =
                FrameStateIs(attacker, LF2States.HeavyWeaponInSky);
            if (attackerState2000 && itr.dvx != 0)
            {
                victim.KnockbackVx +=
                    attacker.GetRuntimeXInt() < victim.GetRuntimeXInt()
                        ? itr.dvx
                        : -itr.dvx;
                return;
            }

            double scaled = System.Math.Abs(victim.Runtime.Vx) * 0.55f;
            if (itr.dvx > scaled)
            {
                victim.KnockbackVx += attacker.Dirh() > 0
                    ? itr.dvx
                    : -itr.dvx;
            }
            else if (attacker.Dirh() > 0)
            {
                if (victim.KnockbackVx > 0.0)
                    victim.KnockbackVx += itr.dvx;
                else if (victim.Runtime.Vx < 0.0)
                    victim.KnockbackVx = (float)(-scaled);
            }
            else
            {
                if (victim.KnockbackVx < 0.0)
                    victim.KnockbackVx -= itr.dvx;
                else if (victim.Runtime.Vx > 0.0)
                    victim.KnockbackVx = (float)(-scaled);
            }
        }

        private static void ApplyWeaponAttackerState3000PreKnockdown(
            LF2Entity attacker,
            LF2Entity victim)
        {
            if (!FrameStateIs(attacker, LF2States.ProjectileFlying))
                return;

            int attackerOid = LF2Entity.ResolveCurrentDataObjectId(attacker);
            int victimOid = LF2Entity.ResolveCurrentDataObjectId(victim);
            bool nonCharacterVictim = victim.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character;
            bool skipReset = nonCharacterVictim &&
                attackerOid == 209 &&
                (IsKarasuOid(victimOid) ||
                 (victimOid == 209 && (victim.Frame?.N ?? 0) == 40));
            if (skipReset)
                return;

            attacker.DirectWriteRawFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
            LF2FrameData frame10 = attacker.GetFrameDataById(10);
            if (frame10 != null)
                attacker.Runtime.Vz = frame10.dvz;
        }

        private static void ApplyWeaponAttackerState1002Response(
            LF2Entity attacker,
            LF2Entity victim)
        {
            if (!FrameStateIs(attacker, LF2States.WeaponThrowing))
                return;

            attacker.DirectWriteRawFramePreserveWaitCounter(
                attacker.BattleRandInt(0, 16));
            attacker.Runtime.Vx = -(victim.KnockbackVx * 0.5);
            attacker.Runtime.Vy = -4.0;

            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.ThrowWeapon &&
                victim.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.ThrowWeapon)
            {
                attacker.KnockbackVx = -victim.KnockbackVx;
            }
        }

        private static bool ApplyStandardFall(
            LF2Entity attacker,
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr)
        {
            int fallIncrement = itr.fall != 0
                ? itr.fall
                : NTSDGlobal.Default.Fall.Value;
            int previousState = victim.GetFrameDataById(
                victim.Frame?.Prev ?? 0)?.state ?? 0;
            int previous2State = victim.GetFrameDataById(
                    victim.Runtime.PrevFrame2)?.state
                ?? victim.Frame?.Prev2D?.state
                ?? 0;

            bool forceKnockdown = victim.Health.HP <= 0 ||
                                  itr.effect == 4 ||
                                  previousState == LF2States.Frozen ||
                                  previous2State == LF2States.Falling;
            victimHitCounters.AddFall(fallIncrement);
            if (forceKnockdown || victimHitCounters.Fall > 60)
            {
                ApplyStandardVerticalKnockback(victim, victimHitCounters, itr);
                return true;
            }

            if (victimHitCounters.Fall > 40)
            {
                victimHitCounters.SetFall(60);
                victim.DirectWriteFramePreserveWaitCounter(
                    LF2StandardFrames.Injured6);
                if (victim.GetRuntimeYInt() < 0)
                {
                    ApplyStandardVerticalKnockback(victim, victimHitCounters, itr);
                    return true;
                }
                return false;
            }

            if (victimHitCounters.Fall > 20)
            {
                victimHitCounters.SetFall(40);
                bool sameDirection = attacker.Dirh() == victim.Dirh();
                victim.DirectWriteFramePreserveWaitCounter(
                    sameDirection
                        ? LF2StandardFrames.Injured4
                        : LF2StandardFrames.Injured2);
                if (victim.GetRuntimeYInt() < 0)
                {
                    ApplyStandardVerticalKnockback(victim, victimHitCounters, itr);
                    return true;
                }
                return false;
            }

            if (victimHitCounters.Fall > 0)
            {
                victimHitCounters.SetFall(20);
                victim.DirectWriteFramePreserveWaitCounter(
                    LF2StandardFrames.Injured);
                if (victim.GetRuntimeYInt() < 0)
                {
                    bool sameDirection = attacker.Dirh() == victim.Dirh();
                    victim.DirectWriteFramePreserveWaitCounter(
                        sameDirection
                            ? LF2StandardFrames.Injured4
                            : LF2StandardFrames.Injured2);
                }
            }

            return false;
        }

        private static void ApplyStandardVerticalKnockback(
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr)
        {
            victimHitCounters.ResetFall();
            if (itr.dvy != 0)
            {
                victim.KnockbackVy += itr.dvy;
                if ((int)(victim.KnockbackVy + victim.GetRuntimeYInt()) > 0)
                    victim.KnockbackVy = 12.0f;
                return;
            }

            victim.KnockbackVy -= 7.0f;
        }

        private static void ApplyStandardState1002Tail(
            LF2Entity attacker,
            LF2Entity victim)
        {
            if (!FrameStateIs(attacker, LF2States.WeaponThrowing))
                return;

            attacker.DirectWriteFramePreserveWaitCounter(
                attacker.BattleRandInt(0, 16));
            attacker.Runtime.Vx = victim.KnockbackVx * -0.5;
            attacker.Runtime.Vy = -4.0;
            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.ThrowWeapon &&
                victim.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.ThrowWeapon)
            {
                attacker.KnockbackVx = -victim.KnockbackVx;
            }
        }

        private static void ApplyStandardState3000Tail(LF2Entity attacker)
        {
            if (!FrameStateIs(attacker, LF2States.ProjectileFlying))
                return;

            attacker.DirectWriteFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
            LF2FrameData frame10 = attacker.GetFrameDataById(10);
            if (frame10 != null)
                attacker.Runtime.Vz = frame10.dvz;
        }

        private static void RecordAlternateLeadSound(
            LF2Entity attacker,
            LF2Entity victim)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData victimData =
                LF2HitResolveRuntimeData.ResolveCharacterData(victim);
            if (attackerData == null || victimData == null)
                return;

            if (attacker.GetCurrentDataObjectTypeForSimulation() ==
                (int)LF2ObjectType.SpecialAttack)
            {
                if (!string.IsNullOrWhiteSpace(attackerData.weapon_broken_sound))
                    attacker.QueueBattleSound(attackerData.weapon_broken_sound);
                return;
            }

            victim.QueueBattleSound(victim.ObjectId == 37 || victim.ObjectId == 6
                ? "SFX_017"
                : "SFX_002");
        }

        private static bool FrameStateIs(LF2Entity entity, int state)
        {
            return entity?.Frame?.D?.state == state;
        }

        private static void ApplyAlternateGroundKnockback(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            if (victim.FallCounter == 80 &&
                victim.Runtime.Vx < 3.0 &&
                victim.Runtime.Vx > -3.0 &&
                itr.dvx == 0)
            {
                if (FrameStateIs(attacker, LF2States.HeavyWeaponInSky))
                {
                    victim.KnockbackVx +=
                        attacker.GetRuntimeXInt() < victim.GetRuntimeXInt()
                            ? 6.0
                            : -6.0;
                }
                else
                {
                    victim.KnockbackVx += attacker.Dirh() > 0 ? 3.0 : -3.0;
                }
                return;
            }

            if (FrameStateIs(attacker, LF2States.HeavyWeaponInSky))
            {
                victim.KnockbackVx +=
                    attacker.GetRuntimeXInt() < victim.GetRuntimeXInt()
                        ? itr.dvx
                        : -itr.dvx;
            }
            else if (itr.effect == 22 || itr.effect == 23)
            {
                victim.KnockbackVx +=
                    victim.GetRuntimeXInt() <= attacker.GetRuntimeXInt()
                        ? itr.dvx
                        : -itr.dvx;
            }
            else
            {
                int halfDvx = itr.dvx / 2;
                victim.KnockbackVx += attacker.Dirh() > 0 ? halfDvx : -halfDvx;
            }
        }

        private static void ApplyAlternateAirKnockback(
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
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
                victim.KnockbackVx +=
                    victim.GetRuntimeXInt() <= attacker.GetRuntimeXInt()
                        ? itr.dvx
                        : -itr.dvx;
            }
            else
            {
                victim.KnockbackVx += attacker.Dirh() > 0 ? itr.dvx : -itr.dvx;
            }
        }

        private static void DampenAlternateState2000Attacker(
            LF2Entity attacker,
            LF2Entity victim)
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

        private static void ApplyAlternateState3000Tail(LF2Entity attacker)
        {
            if (!FrameStateIs(attacker, LF2States.ProjectileFlying))
                return;

            attacker.DirectWriteFramePreserveWaitCounter(10);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
        }

        private static void ReleaseHeldTarget(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim)
        {
            int heldTargetSlot = victim.Runtime.ResolveActiveHeldSlotIndex();
            if (victim.Runtime.LinkState != 2 || heldTargetSlot < 0)
                return;

            LF2Entity heldTarget = world.FindEntityByRuntimeSlotForQuery(
                heldTargetSlot);
            int victimSlot = victim.Runtime.SlotIndex;
            if (heldTarget?.Runtime == null ||
                heldTarget.Runtime.LinkState != -2 ||
                !heldTarget.Runtime.IsActivelyHeldBySlot(victimSlot))
            {
                return;
            }

            attacker.ItrRest?.SetVrest(heldTargetSlot, 45);
            victim.ItrRest?.SetVrest(heldTargetSlot, 30);
            victim.Runtime.LinkState = 0;
            heldTarget.Runtime.LinkState = 0;
            heldTarget.ImmediateFrame(heldTarget.BattleRandInt(0, 6));
            heldTarget.Runtime.Vy = -1f;
            heldTarget.RefreshRuntimeSnapshot();
            victim.RefreshRuntimeSnapshot();
        }
    }
}
