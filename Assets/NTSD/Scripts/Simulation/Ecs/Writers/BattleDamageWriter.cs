using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using UnityEngine;

namespace NTSD.Simulation.Ecs
{
    internal sealed class BattleDamageWriter
    {
        private static readonly int[] NativeImpactImmuneObjectIds = { 201, 202 };

        internal static BattleNativeImpactPlan CreateNativeImpactPlan(
            SimulationWorld world, LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            if (world == null || attacker?.Runtime == null || victim?.Runtime == null || itr == null)
                return default;

            NTSDEntityRuntime source = attacker.Runtime;
            NTSDEntityRuntime target = victim.Runtime;
            int firstSlot = source.OwnerSlotIndex;
            NTSDEntityRuntime first = ResolveNativeImpactOwner(world, firstSlot);
            int creditSlot = first?.OwnerSlotIndex ?? -1;
            NTSDEntityRuntime credit = ResolveNativeImpactOwner(world, creditSlot);
            var input = new BattleNativeImpactInput
            {
                Kind = itr.kind,
                TargetType = victim.GetCurrentDataObjectTypeForSimulation(),
                TargetState = victim.GetState(),
                TargetAction = victim.Frame?.N ?? target.Frame,
                TargetObjectId = LF2Entity.ResolveCurrentDataObjectId(victim),
                Environment = target.EnvironmentState320,
                CatchSource = target.CatchSourceSlot90,
                ImpactSource = target.ImpactSourceSlot164,
                Respond = itr.respond,
                FirstOwnerSlot = firstSlot,
                FirstOwnerValid = first != null,
                CreditSlot = creditSlot,
                CreditOwnerValid = credit != null,
                AttackerSlot = source.SlotIndex,
                Rule94 = 20,
                ImmunityAudited = true,
                ImmuneObjectIds = NativeImpactImmuneObjectIds,
                YInt = target.YInt,
                Y = target.Y,
                Vx = target.Vx,
                Vy = target.Vy,
                Vz = target.Vz,
                PendingX = target.KnockbackVx,
                PendingY = target.KnockbackVy,
                PendingZ = target.KnockbackVz,
            };
            return BattleNativeImpactResolver.Resolve(in input);
        }

        private static NTSDEntityRuntime ResolveNativeImpactOwner(SimulationWorld world, int slot)
        {
            if (!world.TryGetRuntimeSlotReadOnlyView(slot, out RuntimeSlotTable.ReadOnlySlotView view) || !view.Claimed)
                return null;
            return view.Entity?.Runtime ?? view.RawRuntime;
        }

        internal bool TryApplyNativeImpact(SimulationWorld world, LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            BattleNativeImpactPlan plan = CreateNativeImpactPlan(world, attacker, victim, itr);
            if (!plan.Applied)
                return false;

            // Alignment contract: NTSD28-B6-NATIVE-IMPACT-ATOMIC-PRODUCTION-INTEGRATION-PRODUCTION-001
            NTSDEntityRuntime target = victim.Runtime;
            for (int i = 0; i < plan.OperationCount; i++)
            {
                BattleNativeImpactOperation op = plan.GetOperation(i);
                switch (op.Kind)
                {
                    case BattleNativeImpactWriteKind.Environment: target.EnvironmentState320 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.CatchSource: target.CatchSourceSlot90 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.ImpactSource: target.ImpactSourceSlot164 = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.Action: victim.DirectWriteNativeRawFramePreserveWaitCounter((int)op.Value); break;
                    case BattleNativeImpactWriteKind.Vx: target.Vx = op.Value; break;
                    case BattleNativeImpactWriteKind.Vz: target.Vz = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingX: target.KnockbackVx = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingZ: target.KnockbackVz = op.Value; break;
                    case BattleNativeImpactWriteKind.YInt: target.YInt = (int)op.Value; break;
                    case BattleNativeImpactWriteKind.Y: target.Y = op.Value; break;
                    case BattleNativeImpactWriteKind.Vy: target.Vy = op.Value; break;
                    case BattleNativeImpactWriteKind.PendingY: target.KnockbackVy = op.Value; break;
                }
            }
            return true;
        }

        internal readonly struct NativeEffectActionOverrideDecision
        {
            internal NativeEffectActionOverrideDecision(
                int attackerAction,
                int targetAction)
            {
                AttackerAction = attackerAction;
                TargetAction = targetAction;
            }

            internal int AttackerAction { get; }
            internal int TargetAction { get; }
        }

        internal readonly struct NativeKind0PostEffectActionDecision
        {
            internal NativeKind0PostEffectActionDecision(int action, int facing)
            {
                Action = action;
                Facing = facing;
            }

            internal int Action { get; }
            internal int Facing { get; }
        }

        internal readonly struct NativeType3AttackerPostHitActionDecision
        {
            internal NativeType3AttackerPostHitActionDecision(
                int action,
                bool hasSelectedFrame,
                int selectedFrameDvx)
            {
                Action = action;
                HasSelectedFrame = hasSelectedFrame;
                SelectedFrameDvx = selectedFrameDvx;
            }

            internal int Action { get; }
            internal bool HasSelectedFrame { get; }
            internal int SelectedFrameDvx { get; }
            internal bool Applies => Action != int.MinValue;
        }

        internal static NativeType3AttackerPostHitActionDecision
            ResolveNativeType3AttackerPostHitAction(LF2Entity attacker)
        {
            LF2FrameData currentFrame = attacker?.Frame?.D;
            if (currentFrame == null ||
                (currentFrame.state != LF2States.ProjectileFlying &&
                 (currentFrame.state != 3007 ||
                  (currentFrame.cover != 2 && currentFrame.cover != 3))))
            {
                return new NativeType3AttackerPostHitActionDecision(
                    int.MinValue,
                    false,
                    0);
            }

            int action = currentFrame.hit_Fj != 0 ? currentFrame.hit_Fj : 10;
            LF2FrameData selectedFrame = attacker.FrameCache?.GetNativeFrameDataById(action);
            return new NativeType3AttackerPostHitActionDecision(
                action,
                selectedFrame != null,
                selectedFrame?.dvx ?? 0);
        }

        private static void ApplyNativeType3AttackerPostHitAction(
            LF2Entity attacker)
        {
            NativeType3AttackerPostHitActionDecision decision =
                ResolveNativeType3AttackerPostHitAction(attacker);
            if (!decision.Applies)
                return;

            attacker.DirectWriteNativeRawFramePreserveWaitCounter(decision.Action);
            attacker.AttackingCounter = 0;
            attacker.Runtime.Vx = 0.0;
            if (decision.HasSelectedFrame)
                attacker.Runtime.Vz = decision.SelectedFrameDvx;
        }

        internal static NativeKind0PostEffectActionDecision ResolveNativeKind0PostEffectAction(
            LF2Entity target,
            InteractionArea interaction,
            double pendingHorizontalImpulse)
        {
            if (target?.Runtime == null || interaction == null ||
                interaction.kind != 0 ||
                target.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return default;
            }

            int previousState = target.FrameCache?.GetNativeFrameDataById(
                target.Frame?.Prev ?? 0)?.state ?? 0;
            if ((interaction.effect == 3 || interaction.effect == 30) &&
                previousState != 13)
            {
                return new NativeKind0PostEffectActionDecision(200, -1);
            }

            bool action203 = interaction.effect == 2 ||
                             interaction.effect == 21 ||
                             interaction.effect == 22 ||
                             (interaction.effect == 20 && previousState != 18);
            return action203
                ? new NativeKind0PostEffectActionDecision(
                    203,
                    pendingHorizontalImpulse >= 0.0 ? 1 : 0)
                : default;
        }

        internal static NativeEffectActionOverrideDecision ResolveNativeEffectActionOverride(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            int targetHpAfterDamage)
        {
            if (attacker?.Runtime == null || target?.Runtime == null || interaction == null)
                return default;

            LF2FrameData latchedFrame = target.FrameCache?.GetNativeFrameDataById(
                target.Trans?.WaitCounter ?? target.Runtime.WaitCounter);
            int targetProperty = LF2HitResolveRuntimeData
                .ResolveCharacterData(target)?.property ?? 0;
            LF2FrameData previousFrame = target.FrameCache?.GetNativeFrameDataById(
                target.Frame?.Prev ?? 0);
            return ResolveNativeEffectActionOverrideForProjectedTarget(
                attacker,
                interaction,
                targetHpAfterDamage,
                target.GetCurrentDataObjectTypeForSimulation(),
                targetProperty,
                latchedFrame,
                previousFrame);
        }

        internal static NativeEffectActionOverrideDecision
            ResolveNativeEffectActionOverrideForProjectedTarget(
                LF2Entity attacker,
                InteractionArea interaction,
                int targetHpAfterDamage,
                int targetObjectType,
                int targetProperty,
                LF2FrameData latchedFrame,
                LF2FrameData previousFrame)
        {
            if (attacker?.Runtime == null || interaction == null)
                return default;

            int effect = interaction.effect;
            int caughtAction = ResolveFirstAction(interaction.caughtact);
            int latchedBodyKind = latchedFrame?.primaryBodyKindForEffectSuppression ?? 0;
            if (latchedBodyKind == 50 || latchedBodyKind == 52 ||
                latchedFrame?.state == 602 || latchedFrame?.state == 603 ||
                targetProperty == 2 || targetProperty == 3 ||
                (effect >= 8 && effect <= 16 &&
                 (caughtAction == -2 || caughtAction == -3)) ||
                !NativeEffectActionTargetTypeMatches(
                    effect,
                    targetObjectType))
            {
                return default;
            }

            if (interaction.pickedact > 0)
            {
                if (previousFrame == null || previousFrame.state != interaction.pickedact)
                    return default;
            }

            int attackerAction = ResolveFirstAction(interaction.catchingact);
            return new NativeEffectActionOverrideDecision(
                attackerAction > 0 ? attackerAction : -1,
                caughtAction > 0 && targetHpAfterDamage > 0 ? caughtAction : -1);
        }

        private static int ResolveFirstAction(int[] actions)
        {
            return actions != null && actions.Length > 0 ? actions[0] : 0;
        }

        private static bool NativeEffectActionTargetTypeMatches(
            int effect,
            int targetObjectType)
        {
            switch (effect)
            {
                case 8:
                case 13:
                    return targetObjectType == (int)LF2ObjectType.Character;
                case 9:
                case 14:
                    return targetObjectType == (int)LF2ObjectType.SpecialAttack;
                case 10:
                case 15:
                    return targetObjectType == (int)LF2ObjectType.Character ||
                           targetObjectType == (int)LF2ObjectType.SpecialAttack;
                case 11:
                case 16:
                    return targetObjectType == (int)LF2ObjectType.LightWeapon ||
                           targetObjectType == (int)LF2ObjectType.HeavyWeapon ||
                           targetObjectType == (int)LF2ObjectType.ThrowWeapon ||
                           targetObjectType == (int)LF2ObjectType.Drink;
                case 12:
                    return true;
                default:
                    return false;
            }
        }

        private static void ApplyNativeEffectActionOverride(
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            NativeEffectActionOverrideDecision decision =
                ResolveNativeEffectActionOverride(
                    attacker,
                    target,
                    interaction,
                    target.Health?.HP ?? 0);
            if (decision.AttackerAction > 0)
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(decision.AttackerAction);
            if (decision.TargetAction > 0)
                target.DirectWriteNativeRawFramePreserveWaitCounter(decision.TargetAction);
        }

        private static void ApplyNativeKind0PostEffectAction(
            LF2Entity target,
            InteractionArea interaction)
        {
            NativeKind0PostEffectActionDecision decision =
                ResolveNativeKind0PostEffectAction(
                    target,
                    interaction,
                    target.KnockbackVx);
            if (decision.Action <= 0)
                return;

            target.DirectWriteNativeRawFramePreserveWaitCounter(decision.Action);
            target.AttackingCounter = 0;
            if (decision.Facing >= 0)
                target.SwitchDir(decision.Facing == 0 ? "right" : "left");
        }

        internal static void ApplyConfirmedInputStatuses(
            NTSD28NativeRandom random,
            NTSDEntityRuntime target,
            InteractionArea interaction)
        {
            if (random == null || target == null || interaction == null)
                return;

            if (interaction.poison != 0)
            {
                int roll = random.SynchronizedNext(0x0041649Cu, 100);
                int chance = interaction.poison % 100;
                if (chance == 0 || roll + 1 <= chance)
                {
                    int payload = interaction.poison / 100;
                    target.PoisonTimer120 = (payload / 100) % 1000;
                    target.PoisonType124 = (payload / 100) / 1000;
                    target.PoisonStrength128 = payload % 100;
                }
            }

            if (TryApplyEncodedStatus(
                    random,
                    interaction.weak,
                    0x004164D5u,
                    out int value))
            {
                target.WeakTimer12C = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.bound,
                    0x004164FCu,
                    out value))
            {
                target.BoundState198 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.facing,
                    0x00416523u,
                    out value))
            {
                target.InputDoubleCost19C = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.manacle,
                    0x00416547u,
                    out value))
            {
                target.InputActionLock130 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.delay,
                    0x0041656Bu,
                    out value))
            {
                target.DelayTimer134 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.join,
                    0x00416592u,
                    out value))
            {
                target.JoinTimer148 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.mimic,
                    0x004165B9u,
                    out value))
            {
                target.InputProxyCounter14C = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.dx,
                    0x004165E0u,
                    out value))
            {
                target.StatusDx1C0 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.dy,
                    0x0041660Au,
                    out value))
            {
                target.StatusDy1C4 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.dz,
                    0x00416634u,
                    out value))
            {
                target.StatusDz1C8 = value;
            }
            if (TryApplyEncodedStatus(
                    random,
                    interaction.gain,
                    0x0041665Eu,
                    out value))
            {
                target.StatusGain1CC = value;
            }
            if (!TryApplyEncodedStatus(
                    random,
                    interaction.confus,
                    0x00416685u,
                    out value))
            {
                return;
            }

            target.InputRemapState138 = value;
            byte[] remap = target.InputRemapIndices13C;
            if (remap == null ||
                remap.Length != NTSDEntityRuntime.NativeInputRemapCount)
            {
                return;
            }

            for (int index = 0; index < remap.Length; index++)
                remap[index] = byte.MaxValue;
            for (int index = 0; index < remap.Length; index++)
            {
                byte candidate = (byte)random.SynchronizedNext(
                    0x004166B2u,
                    NTSDEntityRuntime.NativeInputRemapCount);
                while (ContainsRemapCandidate(remap, index, candidate))
                {
                    candidate = (byte)((candidate + 1) %
                        NTSDEntityRuntime.NativeInputRemapCount);
                }
                remap[index] = candidate;
            }
        }

        private static bool TryApplyEncodedStatus(
            NTSD28NativeRandom random,
            int encoded,
            uint callSite,
            out int value)
        {
            value = 0;
            if (encoded == 0)
                return false;

            int roll = random.SynchronizedNext(callSite, 100);
            int chance = encoded / 1000;
            if (chance != 0 && roll >= chance)
                return false;

            value = encoded % 1000;
            return true;
        }

        private static bool ContainsRemapCandidate(
            byte[] remap,
            int count,
            byte candidate)
        {
            for (int index = 0; index < count; index++)
            {
                if (remap[index] == candidate)
                    return true;
            }
            return false;
        }

        internal static bool ArmNativeUnarmoredHitMotion(
            NTSDEntityRuntime target,
            NTSDEntityRuntime attacker,
            InteractionArea interaction)
        {
            if (target == null || attacker == null || interaction == null)
                return false;

            bool stabilizationBypass =
                target.Fall == 80 &&
                target.Vx > -5.0 &&
                target.Vx < 5.0 &&
                interaction.dvx == 0;
            if (stabilizationBypass)
                return false;

            target.KnockbackVz += interaction.dvz;
            target.StatusHitFacing1D0 = attacker.IsFacingLeft ? 1 : 0;
            target.StatusDx1C0 = interaction.dx;
            target.StatusDy1C4 = interaction.dy;
            target.StatusDz1C8 = interaction.dz;
            target.StatusGain1CC = 1;
            target.StatusPickedAction1D4 = interaction.pickedact != 0
                ? interaction.pickedact
                : 191;
            target.StatusPickingAction1D8 = interaction.pickingact != 0
                ? interaction.pickingact
                : 185;
            return true;
        }

        internal static void ApplyNativeJoinAndMimicSideEffects(
            NTSDEntityRuntime attacker,
            NTSDEntityRuntime target)
        {
            if (attacker == null || target == null)
                return;

            if (target.JoinTimer148 > 0 &&
                target.JoinOverrideActive170 != 1)
            {
                target.JoinOverrideActive170 = 1;
                target.JoinOriginalBattleGroup174 = target.RelationTeam;
                target.RelationTeam = attacker.RelationTeam;
            }

            NTSD28InputProxyControlLifecycle.TryEnableFromConfirmedHit(
                attacker,
                target);
        }

        internal static void ApplyNativeHitDisplaySteps(
            NTSDEntityRuntime target,
            int injury)
        {
            if (target == null || injury <= 0)
                return;

            int commonStep = injury / 10;
            target.DisplayScoreStep1F4 = commonStep;
            target.DisplayDamageStep1FC = commonStep;
            target.DisplayCurrentHpStep204 = commonStep;
            target.DisplayEffectiveMaxHpStep20C = injury / 20;
        }

        internal static int ResolveNativeUnarmoredHpInjury(
            int injury,
            int targetDamageScale,
            int attackerWeakTimer)
        {
            if (targetDamageScale > 0)
            {
                int scaledProduct = unchecked(
                    (int)((uint)injury * 100u));
                injury = scaledProduct / targetDamageScale;
            }

            if (attackerWeakTimer > 0)
                injury /= 2;
            return injury;
        }

        internal static int ResolveNativeHitResourceInjury(
            int injury,
            int injuryDouble,
            int definitionAttackingPercent,
            int activeModePercent)
        {
            if (injuryDouble > 0)
            {
                injury = unchecked((int)((uint)injury * 2u));
            }

            int multiplier = definitionAttackingPercent;
            if (multiplier < 1)
                multiplier = activeModePercent;
            if (multiplier < 1 || injury == 0)
                return injury;

            int product = unchecked(
                (int)((uint)injury * (uint)multiplier));
            int quotient = product / 100;
            int remainder = product % 100;
            if (remainder >= 50)
                quotient++;
            return quotient;
        }

        internal static void ApplyNativeHitResourceTransaction(
            NTSDEntityRuntime resourceAttacker,
            NTSDEntityRuntime target,
            int resourceInjury,
            int hitResourceSuppression,
            int drain,
            int gain,
            bool localModeEnabled,
            int attackerInjuryMpPercent,
            int targetInjuryMpPercent,
            int resourceAttackerBaseMaxMp)
        {
            if (resourceAttacker == null || target == null ||
                !localModeEnabled)
            {
                return;
            }

            if (resourceInjury > 0 &&
                hitResourceSuppression != 1 &&
                resourceAttacker.ObjType == 0 &&
                target.ObjType == 0)
            {
                if (attackerInjuryMpPercent > 0)
                {
                    int delta = (int)(((long)resourceInjury *
                        attackerInjuryMpPercent) / 100L);
                    resourceAttacker.PP = unchecked(
                        resourceAttacker.PP + delta);
                }
                if (targetInjuryMpPercent > 0)
                {
                    int delta = (int)(((long)resourceInjury *
                        targetInjuryMpPercent) / 100L);
                    target.PP = unchecked(target.PP + delta);
                }
            }

            if (target.ObjType == 0 && drain > 0 && drain <= target.PP)
            {
                target.PP -= drain;
                target.InputMpConsumedTotal350 = unchecked(
                    target.InputMpConsumedTotal350 + drain);
            }

            if (resourceAttacker.ObjType != 0)
                return;

            if (gain < 0)
            {
                int cost = unchecked(-gain);
                if (cost <= resourceAttacker.PP)
                {
                    resourceAttacker.PP = unchecked(
                        resourceAttacker.PP + gain);
                    resourceAttacker.InputMpConsumedTotal350 = unchecked(
                        resourceAttacker.InputMpConsumedTotal350 + cost);
                }
                return;
            }

            int candidateMp = unchecked(resourceAttacker.PP + gain);
            if (candidateMp <= resourceAttackerBaseMaxMp)
                resourceAttacker.PP = candidateMp;
        }

        internal static LF2Entity ResolveNativeHitResourceAttacker(
            SimulationWorld world,
            int physicalAttackerSlot)
        {
            if (world == null)
                return null;

            LF2Entity resolved = world.FindEntityByRuntimeSlotForQuery(
                physicalAttackerSlot);
            if (resolved?.Runtime == null)
                return null;

            for (int depth = 0; depth < 2; depth++)
            {
                int ownerSlot = resolved.Runtime.OwnerSlotIndex;
                if (ownerSlot < 0 || ownerSlot == resolved.Runtime.SlotIndex)
                    break;

                resolved = world.FindEntityByRuntimeSlotForQuery(ownerSlot);
                if (resolved?.Runtime == null)
                    return null;
            }
            return resolved;
        }

        internal static LF2Entity ResolveNativeStandardHitCredit(
            SimulationWorld world,
            LF2Entity physicalAttacker)
        {
            if (physicalAttacker?.Runtime == null)
                return null;

            int sourceSlot = physicalAttacker.Runtime.SlotIndex;
            if ((physicalAttacker.Runtime.Kind4SourceCount92 & 0xFFFF) != 0)
            {
                sourceSlot = unchecked((ushort)
                    physicalAttacker.Runtime.CatchSourceSlot90);
            }

            return ResolveNativeHitResourceAttacker(world, sourceSlot);
        }

        internal static int ResolveNativeAttackingInjury(
            SimulationWorld world,
            LF2Entity attacker,
            int rawInjury)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            int activeModePercent = world?.Runtime?.NativeHitResourceRules?.
                ActiveModeAttackingPercent1C ??
                NTSD28HitResourceRulesRuntimeState
                    .DefaultActiveModeAttackingPercent1C;
            return BattleOrdinaryCharacterDamageRouteResolver
                .ResolveNativeAttackingInjury(
                    rawInjury,
                    attackerData?.definition_attacking ?? 0,
                    activeModePercent);
        }

        private static void ApplyNativeStandardHitCreditAndConsume(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int effectiveInjury)
        {
            // Alignment contract: NTSD28-B5-KIND4-ATOMIC-PRODUCTION-INTEGRATION-001.
            // Attribution must be resolved before consuming the pending count,
            // because the nonzero low word selects CatchSourceSlot90 as its root.
            LF2Entity credit = ResolveNativeStandardHitCredit(world, attacker);
            if (victim?.Runtime != null &&
                victim.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character &&
                victim.Runtime.OrdinaryCreditGate2F4 == -1 &&
                credit?.Runtime != null)
            {
                credit.Runtime.InputScoreTotal348 = unchecked(
                    credit.Runtime.InputScoreTotal348 + effectiveInjury);
            }

            if ((attacker.Runtime.Kind4SourceCount92 & 0xFFFF) != 0)
                attacker.Runtime.Kind4SourceCount92--;
        }

        private static void ApplyNativeStandardHitResourceTransfer(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            LF2Entity resourceAttacker = ResolveNativeHitResourceAttacker(
                world, attacker.Runtime.SlotIndex);
            if (resourceAttacker?.Runtime == null)
                return;

            NTSD28HitResourceRulesRuntimeState rules =
                world.Runtime?.NativeHitResourceRules;
            int resourceInjury = ResolveNativeHitResourceInjury(
                itr.injury,
                attacker.Runtime.HitResourceInjuryDouble1A0,
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker)?
                    .definition_attacking ?? 0,
                rules?.ActiveModeAttackingPercent1C ??
                    NTSD28HitResourceRulesRuntimeState
                        .DefaultActiveModeAttackingPercent1C);
            ApplyNativeHitResourceTransaction(
                resourceAttacker.Runtime,
                victim.Runtime,
                resourceInjury,
                attacker.Runtime.HitResourceSuppression15C,
                itr.drain,
                itr.gain,
                attacker.Runtime.InputLocalResourceEnabled49D034,
                rules?.AttackerInjuryMpPercent34 ??
                    NTSD28HitResourceRulesRuntimeState
                        .DefaultAttackerInjuryMpPercent34,
                rules?.TargetInjuryMpPercent38 ??
                    NTSD28HitResourceRulesRuntimeState
                        .DefaultTargetInjuryMpPercent38,
                resourceAttacker.Runtime.PPMax);
        }

        private static void ApplyNativeStandardHitKnockout(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int effectiveInjury)
        {
            if (victim?.Runtime == null || victim.Health == null ||
                victim.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                victim.Runtime.OrdinaryCreditGate2F4 != -1 ||
                victim.Health.HP <= 0 ||
                effectiveInjury < victim.Health.HP)
            {
                return;
            }

            LF2Entity credit = ResolveNativeStandardHitCredit(world, attacker);
            if (credit?.Runtime != null)
            {
                credit.Runtime.KnockoutCount358++;
                world?.RecordNativeStandardHitKnockout(attacker, victim, credit);
            }
        }

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

            if (BattleNativeImpactResolver.IsImpactKind(itr.kind))
                return TryApplyNativeImpact(world, attacker, victim, itr);

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

            if (BattleNativeImpactResolver.IsImpactKind(itr.kind))
                return TryApplyNativeImpact(world, attacker, victim, itr);

            if (itr.kind == 15)
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
                if (victim.GetState() != LF2States.WeaponInSky)
                    victim.DirectWriteNativeRawFramePreserveWaitCounter(0);
                ApplyGenericWeaponWhirlwindVelocity(victim, attacker);
            }
            else if (heavyLike)
            {
                if (victim.GetState() != LF2States.HeavyWeaponInSky)
                    victim.DirectWriteNativeRawFramePreserveWaitCounter(0);
                ApplyGenericWeaponWhirlwindVelocity(victim, attacker);
            }
        }

        private static void ApplyGenericWeaponWhirlwindVelocity(
            LF2Entity victim,
            LF2Entity attacker)
        {
            victim.KnockbackVx = victim.Runtime.Vx +
                (victim.Runtime.XInt > attacker.Runtime.XInt ? -1.0 : 1.0);
            victim.Runtime.Vx = victim.KnockbackVx;
            victim.KnockbackVz = victim.Runtime.Vz +
                (victim.Runtime.ZInt > attacker.Runtime.ZInt ? -0.5 : 0.5);
            victim.Runtime.Vz = victim.KnockbackVz;

        }

        private static void ApplyNativeStandardHitRest(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int timingReduction = world?.Runtime?.NativeStandardHitRest?.
                TimingReduction4A9FF4 ??
                NTSD28StandardHitRestRuntimeState
                    .DefaultTimingReduction4A9FF4;
            BattleStandardHitRestResult result =
                BattleStandardHitRestResolver.Resolve(
                    attacker.FrameDelay,
                    target.FrameDelay,
                    interaction.recover,
                    attackerData?.definition_effect ?? 0,
                    targetData?.definition_effect ?? 0,
                    interaction.arest,
                    interaction.vrest,
                    timingReduction);

            attacker.FrameDelay = result.AttackerHold;
            target.FrameDelay = result.TargetHold;
            attacker.AttackExempt = result.Arest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = result.Arest;

            int attackerSlot = attacker.Runtime.SlotIndex;
            if (result.ShouldWriteVrest && attackerSlot >= 0)
                target.ItrRest?.SetVrest(attackerSlot, result.Vrest);
        }

        private static void ApplyNativeReducedHitRest(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            LF2ArmorData selectedArmor = null)
        {
            LF2CharacterData attackerData =
                LF2HitResolveRuntimeData.ResolveCharacterData(attacker);
            LF2CharacterData targetData =
                LF2HitResolveRuntimeData.ResolveCharacterData(target);
            int timingReduction = world?.Runtime?.NativeStandardHitRest?.
                TimingReduction4A9FF4 ??
                NTSD28StandardHitRestRuntimeState
                    .DefaultTimingReduction4A9FF4;
            BattleReducedHitRestResult result =
                BattleReducedHitRestResolver.Resolve(
                    attacker.FrameDelay,
                    target.FrameDelay,
                    attackerData?.definition_effect ?? 0,
                    targetData?.definition_effect ?? 0,
                    selectedArmor != null,
                    selectedArmor?.delay ?? -1,
                    interaction.arest,
                    interaction.vrest,
                    timingReduction);

            attacker.FrameDelay = result.AttackerHold;
            target.FrameDelay = result.TargetHold;
            if (result.ClearTargetFrameCounter)
                target.Runtime.FrameWaitCounter = 0;
            attacker.AttackExempt = result.Arest;
            if (attacker.ItrRest != null)
                attacker.ItrRest.Arest = result.Arest;

            int attackerSlot = attacker.Runtime.SlotIndex;
            if (result.ShouldWriteVrest && attackerSlot >= 0)
                target.ItrRest?.SetVrest(attackerSlot, result.Vrest);
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

            BattleOrdinaryCharacterDamageRoute route =
                BattleOrdinaryCharacterDamageRouteResolver.Resolve(
                    world,
                    attacker,
                    victim,
                    itr);
            if (route.Kind ==
                BattleOrdinaryCharacterDamageRouteKind.Unsupported)
            {
                return false;
            }
            if (route.UsesReducedHit)
            {
                ApplyAlternateDamage(
                    world,
                    attacker,
                    victim,
                    victimHitCounters,
                    itr,
                    route.Kind == BattleOrdinaryCharacterDamageRouteKind
                        .ReducedType1Armor
                        ? route.Armor
                        : null);
                BattleNativeHitSparkWriter.Append(world, attacker, victim, itr,
                    attacker.ResolveNativeHitCandidateIndex(itr), route.Armor, true, false);
                return true;
            }

            LF2ArmorData brokenArmor = route.Kind ==
                BattleOrdinaryCharacterDamageRouteKind
                    .UnarmoredType1BrokenFallback
                ? route.Armor
                : null;
            if (brokenArmor != null)
                victim.Runtime.RuntimeArmorHp118 = -1;

            LF2HitResolveRuntimeData.RecordDamageEffectSound(attacker, itr);
            int effectiveInjury = ResolveNativeUnarmoredHpInjury(
                itr.injury,
                victim.Runtime.IncomingDamageScale340,
                attacker.Runtime.WeakTimer12C);
            ApplyStandardVitalAndStatWrites(
                world,
                attacker,
                victim,
                effectiveInjury);
            // Alignment contract: NTSD28-Q07-STANDARD-HIT-RESOURCE-TRANSACTION-001.
            ApplyNativeStandardHitResourceTransfer(
                world, attacker, victim, itr);
            ApplyNativeHitDisplaySteps(victim.Runtime, itr.injury);
            ApplyConfirmedInputStatuses(world.NativeRandom, victim.Runtime, itr);
            ApplyNativeJoinAndMimicSideEffects(
                attacker.Runtime,
                victim.Runtime);

            victim.HitCount++;
            bool knockdown = ApplyStandardFall(
                attacker,
                victim,
                victimHitCounters,
                itr,
                brokenArmor != null);

            if (itr.kind != 9)
            {
                LF2HitResolveRuntimeData.RecordStandardHurtSounds(
                    attacker,
                    victim,
                    itr,
                    knockdown);
            }

            ArmNativeUnarmoredHitMotion(
                victim.Runtime,
                attacker.Runtime,
                itr);

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

            CompleteNativeBrokenArmorFallback(victim, brokenArmor);

            if (brokenArmor != null && knockdown)
            {
                ApplyStandardVerticalKnockback(
                    victim,
                    victimHitCounters,
                    itr);
            }

            ApplyNativeType3AttackerPostHitAction(attacker);
            // Alignment contract: NTSD28-B5-SYSTEM-TABLE-ATTACKER-TERMINAL-PRODUCTION-001.
            ApplyNativeJohnBiscuitAttackerTerminal(attacker);

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

            victim.Runtime.Bdefend = 45;
            ApplyNativeStandardHitRest(world, attacker, victim, itr);

            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);
            ApplyStandardState1002Tail(attacker, victim);
            ApplyNativeEffectActionOverride(attacker, victim, itr);
            ApplyNativeKind0PostEffectAction(victim, itr);

            if (victim is LF2LivingObject livingVictim &&
                attacker is LF2LivingObject livingAttacker)
            {
                livingVictim.Attacker = livingAttacker;
            }

            victim.RecordKind0Hit(attacker, itr);
            return true;
        }

        private static void ApplyNativeJohnBiscuitAttackerTerminal(
            LF2Entity attacker)
        {
            if (LF2Entity.ResolveCurrentDataObjectId(attacker) == 214 &&
                attacker.Health != null)
            {
                attacker.Health.HP = 0;
            }
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
            if (normalVitalWeapon)
            {
                int effectiveInjury = ResolveNativeUnarmoredHpInjury(
                    itr.injury,
                    victim.Runtime.IncomingDamageScale340,
                    attacker.Runtime.WeakTimer12C);
                ApplyWeaponNormalVitalAndStatWrites(
                    world,
                    victim,
                    effectiveInjury);
                ApplyNativeStandardHitCreditAndConsume(
                    world,
                    attacker,
                    victim,
                    effectiveInjury);
                ApplyNativeHitDisplaySteps(victim.Runtime, itr.injury);
            }
            if (normalVitalWeapon)
            {
                ApplyConfirmedInputStatuses(
                    world.NativeRandom,
                    victim.Runtime,
                    itr);
                ApplyNativeJoinAndMimicSideEffects(
                    attacker.Runtime,
                    victim.Runtime);
            }

            if (damageableWeapon)
            {
                int durabilityInjury = ResolveNativeAttackingInjury(
                    world,
                    attacker,
                    itr.injury);
                if (itr.bdefend == 100)
                    victim.Runtime.WeaponFlightCounter = -1;
                else
                    victim.Runtime.WeaponFlightCounter -= durabilityInjury;
            }

            // Alignment contract: NTSD28-Q06-UNARMORED-WEAPON-REACTION-001.
            // Native weapon reactions retain timer 80 and do not transfer team or roll a victim action.
            victim.Runtime.Bdefend = 45;
            victim.FallCounter = 80;
            if (victim.Runtime.LinkState > 0)
            {
                int childSlot = victim.Runtime.TargetSlotIndex;
                LF2Entity child = world.FindEntityByRuntimeSlotForQuery(childSlot);
                if (child?.Runtime?.HolderStableId == victim.Runtime.SlotIndex)
                {
                    attacker.ItrRest?.SetVrest(childSlot, 45);
                    victim.ItrRest?.SetVrest(childSlot, 30);
                }
            }
            if (attacker.GetCurrentDataObjectTypeForSimulation() == 3)
            {
                string sound = LF2HitResolveRuntimeData.ResolveCharacterData(attacker)?.weapon_broken_sound;
                if (!string.IsNullOrEmpty(sound))
                    attacker.QueueBattleSound(sound);
            }

            ArmNativeUnarmoredHitMotion(victim.Runtime, attacker.Runtime, itr);
            ApplyNativeUnarmoredHorizontalResponse(attacker, victim, itr, flyingLike);
            if (!heavyLike || itr.fall > 40)
                victim.HitCount++;

            ApplyNativeBrokenArmorFallback(attacker, victim);
            if (!heavyLike || itr.fall > 40)
            {
                if (itr.dvy != 0)
                {
                    victim.KnockbackVy += itr.dvy;
                    if ((int)(victim.GetRuntimeYInt() + victim.KnockbackVy) > 0)
                        victim.KnockbackVy = 12.0;
                }
                else
                {
                    victim.KnockbackVy -= 7.0;
                }
                bool front = victim.Dirh() < 0 ? victim.KnockbackVx >= 0.0 : victim.KnockbackVx <= 0.0;
                victim.DirectWriteNativeRawFramePreserveWaitCounter(front ? 180 : 186);
            }

            ApplyNativeStandardHitRest(world, attacker, victim, itr);
            ApplyNativeUnarmoredAttackerPostHit(world, attacker, victim);
            ApplyNativeEffectActionOverride(attacker, victim, itr);
            victim.RecordKind0Hit(attacker, itr);
            return true;
        }

        private static void ApplyNativeUnarmoredHorizontalResponse(
            LF2Entity attacker, LF2Entity victim, InteractionArea itr, bool flyingLike)
        {
            double sign = attacker.Dirh() < 0 ? -1.0 : 1.0;
            double motionX = victim.Runtime.Vx;
            var frame = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame.N);
            if (victim.FallCounter == 80 && motionX > -5.0 && motionX < 5.0 && itr.dvx == 0)
            {
                victim.KnockbackVx += sign * 5.0;
            }
            else if (frame?.state == 2000)
            {
                victim.KnockbackVx += attacker.GetRuntimeXInt() < victim.GetRuntimeXInt() ? itr.dvx : -itr.dvx;
            }
            else if (flyingLike)
            {
                bool inDirection = sign < 0.0 ? victim.KnockbackVx < 0.0 : victim.KnockbackVx > 0.0;
                if (itr.dvx > 0.55 * System.Math.Abs(motionX) || inDirection)
                    victim.KnockbackVx += sign * itr.dvx;
                else if (sign < 0.0 ? motionX > 0.0 : motionX < 0.0)
                    victim.KnockbackVx = -0.55 * motionX;
                if (LF2Entity.ResolveCurrentDataObjectId(attacker) == 100 && attacker.Runtime.LinkState < 0)
                {
                    victim.KnockbackVx *= 2.5;
                    if (victim.KnockbackVx > 0.0 && victim.KnockbackVx < 10.0)
                        victim.KnockbackVx = 10.0;
                    else if (victim.KnockbackVx < 0.0 && victim.KnockbackVx > -10.0)
                        victim.KnockbackVx = -10.0;
                    victim.QueueBattleSound("SFX_039");
                }
            }
            else
            {
                victim.KnockbackVx += sign * itr.dvx;
            }
        }

        private static void ApplyNativeUnarmoredAttackerPostHit(
            SimulationWorld world, LF2Entity attacker, LF2Entity victim)
        {
            var frame = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame.N);
            if (frame == null)
                return;
            if (frame.state == 1002)
            {
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(world.NativeRandom.SynchronizedNext(0xEEu, 16));
                attacker.Runtime.Vx = -victim.KnockbackVx * 0.5;
                attacker.Runtime.Vy = -4.0;
                if (attacker.GetCurrentDataObjectTypeForSimulation() == 4 && victim.GetCurrentDataObjectTypeForSimulation() == 4)
                    attacker.KnockbackVx = -victim.KnockbackVx;
            }
            else if (frame.state == 3000 || (frame.state == 3007 && (frame.cover == 2 || frame.cover == 3)))
            {
                int action = frame.hit_Fj != 0 ? frame.hit_Fj : 10;
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(action);
                attacker.AttackingCounter = 0;
                attacker.Runtime.Vx = 0.0;
                var selected = attacker.FrameCache.GetNativeFrameDataById(action);
                if (selected != null)
                    attacker.Runtime.Vz = selected.dvx;
            }
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
                    victim.Runtime.SpecialHitLatch0EB = true;
                    victim.DirectWriteNativeRawFramePreserveWaitCounter(40);
                }
                else
                {
                    CopyRelation(attacker, victim);
                    victim.Runtime.OwnerSlotIndex = attacker.Runtime.OwnerSlotIndex;
                    victim.Runtime.SpecialHitLatch0EB = true;
                    int action = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame.N)?.hit_j ?? 0;
                    if (action == 0)
                        action = victim.FrameCache?.GetNativeFrameDataById(victim.Frame.N)?.hit_j ?? 0;
                    if (action == 0)
                        action = 30;
                    victim.DirectWriteNativeRawFramePreserveWaitCounter(action);
                    ResetType3HitMotion(victim);
                    victim.Runtime.AnimCounter = attacker.Runtime.SlotIndex;
                }
                return true;
            }

            if (itr.kind != 0)
                return false;

            var matchedTargetData = LF2HitResolveRuntimeData.ResolveCharacterData(victim);
            if ((victimType == (int)LF2ObjectType.SpecialAttack ||
                 (victimType == (int)LF2ObjectType.Other && matchedTargetData != null &&
                  (matchedTargetData.armors == null || matchedTargetData.armors.Count == 0))) &&
                TryApplyNativeType3MatchedPairEarlyBranch(
                    world,
                    attacker,
                    victim,
                    itr))
            {
                return true;
            }

            if (victimType == (int)LF2ObjectType.Other)
            {
                if (attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame?.N ?? -1) == null)
                    return false;
            }
            int effectiveInjury = ResolveNativeUnarmoredHpInjury(
                itr.injury,
                victim.Runtime.IncomingDamageScale340,
                attacker.Runtime.WeakTimer12C);
            ApplyType3NormalVitalAndStatWrites(
                world,
                victim,
                effectiveInjury);
            ApplyNativeStandardHitCreditAndConsume(
                world,
                attacker,
                victim,
                effectiveInjury);
            ApplyNativeHitDisplaySteps(victim.Runtime, itr.injury);
            if (victimType == (int)LF2ObjectType.SpecialAttack ||
                victimType == (int)LF2ObjectType.Other)
            {
                ApplyConfirmedInputStatuses(
                    world.NativeRandom,
                    victim.Runtime,
                    itr);
                ApplyNativeJoinAndMimicSideEffects(
                    attacker.Runtime,
                    victim.Runtime);
            }
            if (victimType == (int)LF2ObjectType.Other)
                ApplyNativeType5HurtTail(world, attacker, victim, itr);
            else
                ApplySpecialObjectHurtTail(world, attacker, victim, itr);
            if (victimType == (int)LF2ObjectType.SpecialAttack)
                ApplyKind0Type3Tail(world, attacker, victim, itr);
            ApplyNativeEffectActionOverride(attacker, victim, itr);
            victim.RecordKind0Hit(attacker, itr);
            return true;
        }

        private static bool TryApplyNativeType3MatchedPairEarlyBranch(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            int targetState = target.FrameCache?.GetNativeFrameDataById(target.Frame?.N ?? -1)?.state ?? -1;
            int attackerState = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame?.N ?? -1)?.state ?? -1;
            if (!((targetState == LF2States.ObjectFlying &&
                   attackerState == LF2States.ObjectFlying) ||
                  (targetState == LF2States.ObjectExpanding &&
                   attackerState == LF2States.ObjectExpanding)))
            {
                return false;
            }

            ApplyNativeStandardHitRest(world, attacker, target, interaction);
            ApplyNativeType3PairReset(target);
            ApplyNativeType3PairReset(attacker);
            ReleaseNativeType3AttackerMotionHold(world, attacker);
            return true;
        }

        private static void ApplyNativeBrokenArmorFallback(LF2Entity attacker, LF2Entity target)
        {
            var route = attacker.NativeHitRoute;
            if (!route.HasValue || route.Value.Kind != BattleOrdinaryCharacterDamageRouteKind.UnarmoredType1BrokenFallback ||
                target.Runtime.RuntimeArmorHp118 != -1 || route.Value.Armor == null)
                return;
            if (route.Value.Armor.action != 0)
                target.DirectWriteNativeRawFramePreserveWaitCounter(route.Value.Armor.action);
            target.Runtime.RuntimeArmorHp118++;
        }

        internal bool TryApplyNativeNoncharacterReducedHit(
            SimulationWorld world, LF2Entity attacker, LF2Entity target, InteractionArea itr,
            in BattleOrdinaryCharacterDamageRoute route)
        {
            if (world == null || attacker?.Runtime == null || target?.Runtime == null ||
                target.Health == null || itr == null || itr.kind != 0 || !route.UsesReducedHit)
                return false;
            int type = target.GetCurrentDataObjectTypeForSimulation();
            if (type < 1 || type > 6 || world.GetRawRestVrest(target.Runtime.SlotIndex, attacker.Runtime.SlotIndex) > 0)
                return false;
            var frame = attacker.FrameCache?.GetNativeFrameDataById(attacker.Frame?.N ?? -1);
            if (frame == null)
                return false;
            var armor = route.Kind == BattleOrdinaryCharacterDamageRouteKind.ReducedType1Armor ? route.Armor : null;
            var damage = default(BattleReducedHitDamageResult);
            if (type != 6)
            {
                damage = BattleReducedHitDamageResolver.Resolve(itr.injury, armor != null, armor?.type ?? 0,
                    armor?.decrease ?? 0, armor?.mp ?? 0, armor?.hp ?? 0, target.Runtime.IncomingDamageScale340);
                if (!damage.Supported)
                    return false;
                target.Health.HP -= damage.HpDamage;
                target.Health.HPBound -= damage.HpDamage / 3;
                target.Health.PP -= damage.MpDamage;
                target.Runtime.InputHpConsumedTotal34C = unchecked(target.Runtime.InputHpConsumedTotal34C + damage.HpDamage);
                target.Runtime.InputMpConsumedTotal350 = unchecked(target.Runtime.InputMpConsumedTotal350 + damage.MpDamage);
                ApplyNativeStandardHitCreditAndConsume(world, attacker, target, damage.HpDamage);
                var resourceAttacker = ResolveNativeHitResourceAttacker(world, attacker.Runtime.SlotIndex);
                if (resourceAttacker?.Runtime != null && resourceAttacker.Runtime.ObjType == 0 &&
                    attacker.Runtime.InputLocalResourceEnabled49D034)
                {
                    // Native current MP is PP; the legacy MP bank is a separate carrier.
                    if (itr.gain < 0)
                    {
                        int cost = unchecked(-itr.gain);
                        if (cost <= resourceAttacker.Runtime.PP)
                        {
                            resourceAttacker.Runtime.PP = unchecked(resourceAttacker.Runtime.PP + itr.gain);
                            resourceAttacker.Runtime.InputMpConsumedTotal350 = unchecked(
                                resourceAttacker.Runtime.InputMpConsumedTotal350 + cost);
                        }
                    }
                    else
                    {
                        int candidate = unchecked(resourceAttacker.Runtime.PP + itr.gain);
                        if (candidate <= resourceAttacker.Runtime.MPMax)
                            resourceAttacker.Runtime.PP = candidate;
                    }
                }
            }
            if (type == 1 || type == 2 || type == 4 || type == 6)
            {
                target.Runtime.WeaponFlightCounter = unchecked(target.Runtime.WeaponFlightCounter -
                    ResolveNativeAttackingInjury(world, attacker, itr.injury));
                if (itr.bdefend == 100)
                    target.Runtime.WeaponFlightCounter = -1;
            }
            target.Runtime.RuntimeArmorHp118 = unchecked(target.Runtime.RuntimeArmorHp118 + damage.RuntimeArmorHpDelta);
            if (target.Health.HP <= 0)
                target.FallCounter = 80;
            target.AttackingCounter = 0;
            if (target.Runtime.RuntimeArmorHp118 <= 0)
                target.Runtime.Bdefend = unchecked(target.Runtime.Bdefend + itr.bdefend);

            bool air = target.GetRuntimeYInt() > target.Runtime.CollisionYReference;
            double sign = attacker.Dirh() < 0 ? -1.0 : 1.0;
            double impulse;
            if (air)
                impulse = sign * (target.FallCounter == 80 && target.Runtime.Vx > -6.0 &&
                    target.Runtime.Vx < 6.0 && itr.dvx < 6 ? 6.0 : itr.dvx);
            else if (target.FallCounter == 80 && target.Runtime.Vx > -3.0 && target.Runtime.Vx < 3.0 && itr.dvx == 0)
                impulse = frame.state == 2000 ? (attacker.GetRuntimeXInt() < target.GetRuntimeXInt() ? 6.0 : -6.0) : sign;
            else if (frame.state == 2000)
                impulse = attacker.GetRuntimeXInt() < target.GetRuntimeXInt() ? itr.dvx : -itr.dvx;
            else
                impulse = sign * (itr.dvx / 2.0);
            target.KnockbackVx += impulse;
            target.HitCount++;
            if (!air)
            {
                var current = target.FrameCache?.GetNativeFrameDataById(target.Frame?.N ?? -1);
                int state = current?.state ?? 0;
                int threshold = armor == null ? 30 : System.Math.Max(armor.ratio, 30);
                if (target.Runtime.Bdefend > threshold && (state == 7 || state == 70 || state == 75))
                    target.DirectWriteNativeRawFramePreserveWaitCounter(112);
                else if (target.Frame?.N == 110)
                    target.DirectWriteNativeRawFramePreserveWaitCounter(111);
            }
            ApplyNativeReducedHitRest(world, attacker, target, itr, armor);
            if (attacker.Runtime.LinkState < 0)
            {
                var holder = ResolveActiveHolder(world, attacker);
                if (holder != null)
                    holder.FrameDelay = attacker.FrameDelay;
            }
            if (frame.state == 1002)
            {
                attacker.DirectWriteNativeRawFramePreserveWaitCounter(world.NativeRandom.SynchronizedNext(0xF3u, 16));
                attacker.Runtime.Vx = -target.KnockbackVx * 0.5;
                attacker.Runtime.Vy = -4.0;
                attacker.Runtime.Vz /= -1.5;
            }
            else if (frame.state == 2000)
            {
                if (ShouldDampenNativeReducedState2000(attacker.GetRuntimeXInt(), target.GetRuntimeXInt(), attacker.Runtime.Vx))
                {
                    attacker.Runtime.Vx /= 2.5;
                    attacker.Runtime.Vz /= 2.5;
                }
            }
            else
                ApplyNativeUnarmoredAttackerPostHit(world, attacker, target);
            BattleNativeHitSparkWriter.Append(world, attacker, target, itr,
                attacker.ResolveNativeHitCandidateIndex(itr), armor, true, false);
            return true;
        }

        internal void ApplyAlternateDamage(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            LF2HitCountersModule victimHitCounters,
            InteractionArea itr,
            LF2ArmorData selectedArmor = null)
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

            BattleReducedHitDamageResult damage =
                BattleReducedHitDamageResolver.Resolve(
                    itr.injury,
                    selectedArmor != null,
                    selectedArmor?.type ?? 0,
                    selectedArmor?.decrease ?? 0,
                    selectedArmor?.mp ?? 0,
                    selectedArmor?.hp ?? 0,
                    victim.Runtime.IncomingDamageScale340);
            if (!damage.Supported)
                return;
            int reducedInjury = damage.HpDamage;
            ApplyNativeStandardHitKnockout(
                world,
                attacker,
                victim,
                reducedInjury);
            if (victim.Health.HP > 0 &&
                reducedInjury >= victim.Health.HP &&
                victim.Runtime.OrdinaryCreditGate2F4 == -1)
            {
                int killStatIndex = victim.Unk344;
                if (killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= reducedInjury;
            victim.Health.HPBound -= reducedInjury / 3;
            victim.Health.PP -= damage.MpDamage;
            victim.Runtime.InputHpConsumedTotal34C = unchecked(
                victim.Runtime.InputHpConsumedTotal34C + reducedInjury);
            victim.Runtime.InputMpConsumedTotal350 = unchecked(
                victim.Runtime.InputMpConsumedTotal350 + damage.MpDamage);
            victim.Runtime.RuntimeArmorHp118 = unchecked(
                victim.Runtime.RuntimeArmorHp118 +
                damage.RuntimeArmorHpDelta);
            victim.ComboCountVic += reducedInjury;
            ApplyNativeStandardHitCreditAndConsume(
                world,
                attacker,
                victim,
                reducedInjury);
            // Alignment contract: NTSD28-Q07-REDUCED-HIT-RESOURCE-TRANSACTION-001.
            ApplyNativeStandardHitResourceTransfer(
                world, attacker, victim, itr);

            int damageStatIndex = victim.Unk344;
            if (damageStatIndex > 0 && damageStatIndex < world.DamageStats.Length)
                world.DamageStats[damageStatIndex] += reducedInjury;

            if (victim.Health.HP <= 0)
                victim.FallCounter = 80;

            victim.AttackingCounter = 0;
            if (victim.Runtime.RuntimeArmorHp118 <= 0)
            {
                victim.Runtime.Bdefend = unchecked(
                    victim.Runtime.Bdefend + itr.bdefend);
            }
            victim.HitCount++;
            ApplyNativeReducedHitRest(
                world,
                attacker,
                victim,
                itr,
                selectedArmor);

            int victimPrev2State = victim.GetFrameDataById(
                victim.Runtime.PrevFrame2)?.state ?? 0;
            if (victim.GetRuntimeYInt() == 0)
            {
                int bdefend = victim.Runtime.Bdefend;
                int actionThreshold = selectedArmor != null
                    ? System.Math.Max(selectedArmor.ratio, 30)
                    : 30;
                if (bdefend > actionThreshold &&
                    victimPrev2State == LF2States.Defending)
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
            ApplyNativeType3AttackerPostHitAction(attacker);
        }

        private static void CompleteNativeBrokenArmorFallback(
            LF2Entity victim,
            LF2ArmorData brokenArmor)
        {
            if (brokenArmor == null ||
                victim?.Runtime?.RuntimeArmorHp118 != -1)
            {
                return;
            }

            if (brokenArmor.action != 0)
            {
                victim.DirectWriteFramePreserveWaitCounter(
                    brokenArmor.action);
            }
            victim.Runtime.RuntimeArmorHp118++;
        }

        private static void ApplyStandardVitalAndStatWrites(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int injury)
        {
            int originalHp = victim.Health.HP;
            ApplyNativeStandardHitKnockout(
                world,
                attacker,
                victim,
                injury);
            if (originalHp > 0 && injury >= originalHp &&
                victim.Runtime.OrdinaryCreditGate2F4 == -1)
            {
                int killStatIndex = victim.Unk344;
                if (killStatIndex > 0 && killStatIndex < world.KillStats.Length)
                    world.KillStats[killStatIndex]++;
            }

            victim.Health.HP -= injury;
            victim.Health.HPBound -= injury / 3;
            victim.Runtime.InputHpConsumedTotal34C = unchecked(
                victim.Runtime.InputHpConsumedTotal34C + injury);
            ApplyNativeStandardHitCreditAndConsume(
                world,
                attacker,
                victim,
                injury);
            victim.ComboCountVic += injury;

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
            victim.Runtime.InputHpConsumedTotal34C = unchecked(
                victim.Runtime.InputHpConsumedTotal34C + injury);
        }

        private static void ApplyWeaponNormalVitalAndStatWrites(
            SimulationWorld world,
            LF2Entity victim,
            int effectiveInjury)
        {
            victim.Health.HP -= effectiveInjury;
            victim.Health.HPBound -= effectiveInjury / 3;
            victim.Runtime.InputHpConsumedTotal34C = unchecked(
                victim.Runtime.InputHpConsumedTotal34C + effectiveInjury);
        }

        private static void ApplyNativeType5HurtTail(
            SimulationWorld world, LF2Entity attacker, LF2Entity victim, InteractionArea itr)
        {
            // Alignment contract: NTSD28-Q06-TYPE5-UNARMORED-UNITY-001.
            victim.Runtime.Bdefend = 45;
            int reaction = unchecked(((victim.Health?.HP ?? 0) <= 0 ? 80 : victim.FallCounter) +
                (itr.fall == 0 ? 20 : itr.fall));
            var previous = victim.FrameCache?.GetNativeFrameDataById(victim.Frame?.Prev ?? -1);
            var snapshot = victim.FrameCache?.GetNativeFrameDataById(victim.Frame?.Prev2 ?? -1);
            if (previous?.state == 13 || snapshot?.state == 12)
                reaction = 80;
            bool above = victim.GetRuntimeYInt() < victim.Runtime.CollisionYReference;
            int directional = victim.Dirh() == attacker.Dirh() ? 224 : 222;
            int action = -1;
            if (reaction > 60)
                reaction = 80;
            else if (reaction > 40)
            {
                action = 226;
                reaction = above ? 80 : 60;
            }
            else if (reaction > 20)
            {
                action = directional;
                reaction = above ? 80 : 40;
            }
            else if (reaction > 0)
            {
                action = above ? directional : 220;
                reaction = 20;
            }
            victim.FallCounter = reaction;
            if (action >= 0)
                victim.DirectWriteNativeRawFramePreserveWaitCounter(action);
            if (reaction == 80 && victim.Runtime.LinkState > 0)
            {
                int childSlot = victim.Runtime.TargetSlotIndex;
                var child = world.FindEntityByRuntimeSlotForQuery(childSlot);
                if (child?.Runtime?.HolderStableId == victim.Runtime.SlotIndex)
                {
                    attacker.ItrRest?.SetVrest(childSlot, 45);
                    victim.ItrRest?.SetVrest(childSlot, 30);
                }
            }
            if (attacker.GetCurrentDataObjectTypeForSimulation() == 3)
            {
                string cue = LF2HitResolveRuntimeData.ResolveCharacterData(attacker)?.weapon_broken_sound;
                if (!string.IsNullOrEmpty(cue))
                    attacker.QueueBattleSound(cue);
            }
            ArmNativeUnarmoredHitMotion(victim.Runtime, attacker.Runtime, itr);
            ApplyNativeUnarmoredHorizontalResponse(attacker, victim, itr, false);
            victim.HitCount++;
            ApplyNativeBrokenArmorFallback(attacker, victim);
            if (reaction == 80)
            {
                if (itr.dvy != 0)
                {
                    victim.KnockbackVy += itr.dvy;
                    if ((int)(victim.GetRuntimeYInt() + victim.KnockbackVy) > 0)
                        victim.KnockbackVy = 12.0;
                }
                else
                {
                    victim.KnockbackVy -= 7.0;
                }
                bool front = victim.Dirh() < 0 ? victim.KnockbackVx >= 0.0 : victim.KnockbackVx <= 0.0;
                victim.DirectWriteNativeRawFramePreserveWaitCounter(front ? 180 : 186);
            }
            ApplyNativeStandardHitRest(world, attacker, victim, itr);
            ApplyNativeUnarmoredAttackerPostHit(world, attacker, victim);
        }

        private static void ApplySpecialObjectHurtTail(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            InteractionArea itr)
        {
            int victimType = victim.GetCurrentDataObjectTypeForSimulation();
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

            if (attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.SpecialAttack)
            {
                string cue = LF2HitResolveRuntimeData.ResolveCharacterData(attacker)?.weapon_broken_sound;
                if (!string.IsNullOrEmpty(cue))
                    attacker.QueueBattleSound(cue);
            }
            ArmNativeUnarmoredHitMotion(
                victim.Runtime,
                attacker.Runtime,
                itr);
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

            ApplyNativeBrokenArmorFallback(attacker, victim);
            ApplyNativeType3AttackerPostHitAction(attacker);

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

            victim.Runtime.Bdefend = 45;
            ApplyNativeStandardHitRest(world, attacker, victim, itr);

            if (victim.FallCounter == 80)
                victim.FallCounter = 0;

            LF2HitResolveRuntimeData.ApplyActiveHolderFrameDelay(attacker);
            ApplySpecialWeaponThrowingTail(world, attacker, victim, victimType);
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

        private static void ApplySpecialWeaponThrowingTail(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity victim,
            int victimType)
        {
            if (!FrameStateIs(attacker, LF2States.WeaponThrowing))
                return;

            attacker.DirectWriteNativeRawFramePreserveWaitCounter(
                world.NativeRandom.SynchronizedNext(0xEEu, 16));
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
            bool skipToStateSync = victimState == LF2States.ObjectFlying;
            bool kindTransformCandidate =
                IsNativeLockedKindTransformCandidate(attacker, victim);

            if (!skipToStateSync && kindTransformCandidate)
            {
                ApplyNativeLockedKindTransform(attacker, victim);
            }
            else if (!skipToStateSync)
            {
                ApplyNativeType3TargetGenericContinuation(
                    world,
                    attacker,
                    victim,
                    itr);
            }

            victimState = victim.GetState();
            attackerState = attacker.GetState();
            if ((victimState == LF2States.ObjectFlying &&
                 attackerState == LF2States.ObjectFlying) ||
                (victimState == LF2States.ObjectExpanding &&
                 attackerState == LF2States.ObjectExpanding))
            {
                ApplyNativeType3PairReset(victim);
                ApplyNativeType3PairReset(attacker);
            }

            ReleaseNativeType3AttackerMotionHold(world, attacker);
        }

        internal static void ApplyNativeType3PairReset(LF2Entity entity)
        {
            if (entity?.Runtime == null)
                return;

            LF2FrameData latchedFrame = entity.FrameCache?.GetNativeFrameDataById(
                entity.Trans?.WaitCounter ?? entity.Runtime.WaitCounter);
            int action = latchedFrame?.hit_Uj ?? 0;
            if (action == 0)
                action = 20;
            entity.DirectWriteNativeRawFramePreserveWaitCounter(action);
            entity.AttackingCounter = 0;
            entity.KnockbackVx = 0.0;
            entity.KnockbackVy = 0.0;
            entity.KnockbackVz = 0.0;
        }

        internal static void ReleaseNativeType3AttackerMotionHold(
            SimulationWorld world,
            LF2Entity attacker)
        {
            if (world == null || attacker?.Runtime == null)
                return;

            LF2Entity holdOwner = attacker;
            if (attacker.Runtime.LinkState < 0)
            {
                holdOwner = ResolveActiveHolder(world, attacker);
                if (holdOwner == null)
                    return;
                holdOwner.FrameDelay = attacker.FrameDelay;
            }

            if (holdOwner.FrameDelay > 0)
                holdOwner.FrameDelay = -holdOwner.FrameDelay;
        }

        internal static bool ApplyNativeType3TargetGenericContinuation(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            LF2FrameData responseFrame = target?.Frame?.D;
            if (world == null || attacker?.Runtime == null ||
                target?.Runtime == null || interaction == null ||
                responseFrame == null ||
                responseFrame.state == LF2States.ObjectFlying ||
                IsNativeLockedKindTransformCandidate(attacker, target))
            {
                return false;
            }

            LF2Entity ownershipSource = attacker;
            int ownershipSourceSlot = attacker.Runtime.SlotIndex;
            if (attacker.Runtime.LinkState < 0)
            {
                LF2Entity activeParent = ResolveActiveHolder(world, attacker);
                if (activeParent?.Runtime != null)
                {
                    ownershipSource = activeParent;
                    ownershipSourceSlot = activeParent.Runtime.SlotIndex;
                }
            }

            target.RelationTeam = ownershipSource.RelationTeam;
            target.Runtime.OwnerSlotIndex = ownershipSource.Runtime.OwnerSlotIndex;
            target.Runtime.AnimCounter = ownershipSourceSlot;
            target.Runtime.SpecialHitLatch0EB = true;
            target.KnockbackVx = 0.0;
            target.KnockbackVy = 0.0;
            target.KnockbackVz = 0.0;

            bool ordinaryFjPath =
                (attacker.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character ||
                 attacker.Runtime.LinkState < 0) &&
                interaction.effect != 2 &&
                interaction.effect != 20;
            int action = ordinaryFjPath
                ? responseFrame.hit_Fj
                : responseFrame.hit_Uj;
            if (action == 0)
                action = ordinaryFjPath ? 30 : 20;

            target.DirectWriteNativeRawFramePreserveWaitCounter(action);
            target.AttackingCounter = 0;
            return true;
        }

        internal static bool ApplyNativeLockedKindTransform(
            LF2Entity attacker,
            LF2Entity target)
        {
            LoganKindRecord record = BattleKindTableRules.FindTransform(attacker, target);
            int responseAction = BattleKindTableRules.ResponseFrame(record);
            if (record == null ||
                attacker.FrameCache?.Wrapper == null)
            {
                return false;
            }

            LF2CharacterDataWrapper sourceWrapper = attacker.FrameCache.Wrapper;
            int sourceObjectId = LF2Entity.ResolveCurrentDataObjectId(attacker);
            target.RelationTeam = attacker.RelationTeam;
            target.Runtime.OwnerSlotIndex = attacker.Runtime.OwnerSlotIndex;
            target.ObjectId = sourceObjectId;
            target.FrameCache.Load(sourceWrapper);
            target.DirectWriteNativeRawFramePreserveWaitCounter(responseAction);
            if (target.Frame.D != null)
            {
                target.Trans?.SyncDirectFrameData(
                    target.Frame.D.wait,
                    target.Frame.D.next,
                    responseAction);
            }
            else
            {
                target.Trans?.SyncWaitCounterFrame(responseAction);
            }
            target.Frame.Prev = responseAction;
            target.AttackingCounter = 0;
            target.Runtime.SpecialHitLatch0EB = true;
            target.KnockbackVx = 0.0;
            target.KnockbackVy = 0.0;
            target.KnockbackVz = 0.0;
            target.RefreshRuntimeSnapshot();
            return true;
        }

        internal static bool IsNativeLockedKindTransformCandidate(
            LF2Entity attacker,
            LF2Entity target)
        {
            return BattleKindTableRules.FindTransform(attacker, target) != null;
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
            InteractionArea itr,
            bool deferVerticalKnockback)
        {
            int fallIncrement = itr.fall != 0
                ? itr.fall
                : NTSDGlobal.Default.Fall.Value;
            int previousState = victim.FrameCache?.GetNativeFrameDataById(
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
                if (!deferVerticalKnockback)
                {
                    ApplyStandardVerticalKnockback(
                        victim,
                        victimHitCounters,
                        itr);
                }
                return true;
            }

            if (victimHitCounters.Fall > 40)
            {
                victimHitCounters.SetFall(60);
                victim.DirectWriteFramePreserveWaitCounter(
                    LF2StandardFrames.Injured6);
                if (victim.GetRuntimeYInt() < 0)
                {
                    if (!deferVerticalKnockback)
                    {
                        ApplyStandardVerticalKnockback(
                            victim,
                            victimHitCounters,
                            itr);
                    }
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
                    if (!deferVerticalKnockback)
                    {
                        ApplyStandardVerticalKnockback(
                            victim,
                            victimHitCounters,
                            itr);
                    }
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
                double halfDvx = itr.dvx / 2.0;
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

            if (!ShouldDampenNativeReducedState2000(
                    attacker.Runtime.X,
                    victim.Runtime.X,
                    attacker.Runtime.Vx))
            {
                return;
            }

            attacker.Runtime.Vx /= 2.5;
            attacker.Runtime.Vz /= 2.5;
        }

        // Alignment contract: NTSD28-B5-REDUCED-STATE2000-AWAY-DAMPING-PRODUCTION-001.
        internal static bool ShouldDampenNativeReducedState2000(
            double attackerX,
            double targetX,
            double attackerVx)
        {
            return (attackerX > targetX && attackerVx >= 0.0) ||
                   (attackerX < targetX && attackerVx <= 0.0);
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
