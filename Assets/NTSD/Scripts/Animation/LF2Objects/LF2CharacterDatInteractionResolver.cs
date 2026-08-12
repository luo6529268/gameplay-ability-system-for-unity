using NTSD.Animation;
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2CharacterDatInteractionResolver
    {
        private readonly LF2Entity _attacker;

        internal LF2CharacterDatInteractionResolver(LF2Entity attacker)
        {
            _attacker = attacker;
        }

        internal static bool CanResolveAttacker(LF2Entity attacker)
        {
            return attacker != null &&
                   attacker is not LF2Character &&
                   attacker.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character;
        }

        internal static void TryConsumeUnifiedStep7CandidateSequence(LF2Entity attacker)
        {
            if (!CanResolveAttacker(attacker))
                return;

            attacker.ConsumeCharacterDatInteractionCandidates();
        }

        internal static bool TryApplyPreInteraction(
            LF2Entity attacker,
            InteractionArea itr,
            LF2Entity target)
        {
            if (!CanResolveAttacker(attacker) || itr == null || target == null)
                return false;

            switch (itr.kind)
            {
                case 1:
                    return LF2CharacterInteractionResolver.TryApplyKind1Grab(attacker, target, itr);
                case 3:
                    return LF2CharacterInteractionResolver.TryApplyKind3Grab(attacker, target, itr);
                case 2:
                case 7:
                    return TryApplyCurrentDatPickupCandidate(attacker, target, itr.kind);
                default:
                    return false;
            }
        }

        internal void TryConsumeUnifiedStep7CandidateSequence()
        {
            ILF2SceneQuery sceneQuery = _attacker.Match?.SceneQuery;
            INTSDItrKindService kindService = _attacker.Match?.ItrKindService;
            if (sceneQuery == null || kindService == null)
                return;

            if (!sceneQuery.TryGetCollisionCandidateRange(_attacker, out CollisionCandidateRange candidates))
                return;

            LF2FrameData collisionFrame = _attacker.GetCollisionFrameData();
            if (collisionFrame?.itrs == null)
                return;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                if (!candidates.TryGet(candidateIndex, out SceneQueryHit hitInfo))
                    continue;
                int itrIndex = hitInfo.ItrIndex;
                if (itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count)
                    continue;

                LF2Entity target = hitInfo.ResolveCurrentTarget(_attacker.Match);
                if (target == null)
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    _attacker,
                    target,
                    collisionFrame,
                    collisionFrame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null)
                    continue;

                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyPreprocess == true)
                {
                    _attacker.Match.ObserveBattleHitExecutionPlanLegacyPreprocess(
                        _attacker,
                        target,
                        itr,
                        zeroAttackerHpOnConsume,
                        releaseHeavyHeldTargetOnConsume);
                }

                hitInfo = new SceneQueryHit(
                    target,
                    hitInfo.BodyX,
                    hitInfo.ItrIndex,
                    itr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);

                bool canConsume = CanConsumeRecordedCandidate(target);
                BattleHitCandidateDisposition disposition =
                    LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                        target,
                        itr,
                        canConsume);
                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyDisposition == true)
                {
                    _attacker.Match.ObserveBattleHitExecutionPlanLegacyDisposition(
                        _attacker,
                        target,
                        itr,
                        disposition);
                }

                if (!canConsume ||
                    disposition == BattleHitCandidateDisposition.Unsupported)
                    continue;
                if (disposition == BattleHitCandidateDisposition.HitConfirm)
                {
                    if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true)
                    {
                        _attacker.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                            _attacker,
                            target,
                            itr,
                            disposition);
                    }
                    target.HitConfirmCounter = 3;
                    if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true)
                    {
                        _attacker.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                            _attacker,
                            target);
                    }
                    continue;
                }

                if (disposition == BattleHitCandidateDisposition.Kind1Grab ||
                    disposition == BattleHitCandidateDisposition.Kind3Grab ||
                    disposition == BattleHitCandidateDisposition.Pickup)
                {
                    bool observePreInteractionWriterEffect =
                        _attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true;
                    if (observePreInteractionWriterEffect)
                    {
                        _attacker.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                            _attacker,
                            target,
                            itr,
                            disposition);
                    }
                    DispatchInteractionByKind(kindService, itr, target);
                    if (observePreInteractionWriterEffect)
                    {
                        _attacker.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                            _attacker,
                            target);
                    }
                    continue;
                }

                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects == true)
                {
                    _attacker.Match.PrepareBattleHitExecutionPlanLegacyConsumeEffectsObservation(
                        _attacker,
                        target);
                }
                _attacker.ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(hitInfo);
                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects == true)
                {
                    _attacker.Match.ObserveBattleHitExecutionPlanLegacyConsumeEffects(
                        _attacker,
                        target);
                }
                bool abortAfterSuccessfulHit = LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect(
                    target,
                    itr);
                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyDispatch == true)
                {
                    _attacker.Match.PrepareBattleHitExecutionPlanLegacyDispatchObservation(
                        _attacker,
                        target,
                        itr);
                }
                bool observeWriterEffect =
                    (disposition == BattleHitCandidateDisposition.Kind8 ||
                     disposition == BattleHitCandidateDisposition.Kind14 ||
                     disposition == BattleHitCandidateDisposition.Kind10Or11 ||
                     disposition == BattleHitCandidateDisposition.Kind15Or16 ||
                     (disposition == BattleHitCandidateDisposition.Damage &&
                      _attacker.Match?.CanProjectBattleHitExecutionPlanLegacyWriterEffect(
                          _attacker,
                          target,
                          itr,
                          disposition) == true)) &&
                    _attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true;
                if (observeWriterEffect)
                {
                    _attacker.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                        _attacker,
                        target,
                        itr,
                        disposition);
                }
                bool dispatched = DispatchInteractionByKind(kindService, itr, target);
                if (observeWriterEffect)
                {
                    _attacker.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                        _attacker,
                        target);
                }
                if (_attacker.Match?.ShouldObserveBattleHitExecutionPlanLegacyDispatch == true)
                {
                    _attacker.Match.ObserveBattleHitExecutionPlanLegacyDispatch(
                        _attacker,
                        dispatched,
                        dispatched && abortAfterSuccessfulHit);
                }
                if (!dispatched)
                    continue;
                if (abortAfterSuccessfulHit)
                    return;
            }
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 1 || itr.kind == 2 || itr.kind == 3 || itr.kind == 7)
                return TryApplyPreInteraction(_attacker, itr, target);

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                Vector3 attackerPos = new Vector3((float)_attacker.Runtime.X, (float)_attacker.Runtime.Y, (float)_attacker.Runtime.Z);
                int targetType = LF2Entity.ResolveCurrentDataObjectType(target);
                if (targetType == (int)LF2ObjectType.Character)
                {
                    if (target is LF2Character character)
                        return character.Hit(itr, _attacker, attackerPos, default);
                    if (LF2CharacterDatHitResolver.CanResolveTarget(target))
                        return LF2CharacterDatHitResolver.TryResolveHit(target, itr, _attacker, attackerPos, default);
                }
                if (target is LF2WeaponBase weapon)
                    return weapon.Hit(itr, _attacker);
                if (target is LF2SpecialAttack specialAttack)
                    return specialAttack.Hit(itr, _attacker);
                return target is LF2LivingObject livingTarget && livingTarget.Hit(itr, _attacker, attackerPos, default);
            }
            return false;
        }

        internal static bool TryApplyCurrentDatPickupCandidate(
            LF2Entity attacker,
            LF2Entity target,
            int kind)
        {
            if (attacker?.Runtime == null || target?.Runtime == null)
                return false;
            if (kind != 2 && kind != 7)
                return false;
            if (kind == 7 && attacker.Runtime.LinkState != 0)
                return false;

            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            int attackerSlot = attacker.Runtime.SlotIndex;
            int targetSlot = target.Runtime.SlotIndex;
            if (attackerSlot < 0 || targetSlot < 0)
                return false;

            int linkState;
            int targetLinkState;
            if (kind == 7)
            {
                int targetOid = target.FrameCache?.Wrapper?.characterId ?? target.ObjectId;
                linkState = 1;
                targetLinkState = -1;
                if (targetOid == 120 || targetOid == 124)
                    linkState = 101;
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    linkState = 4;
                    targetLinkState = -4;
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    linkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                    targetLinkState = -linkState;
                }
            }
            else
            {
                if (targetType == (int)LF2ObjectType.LightWeapon)
                {
                    linkState = 1;
                    attacker.DirectWriteRawFramePreserveWaitCounter(LF2StandardFrames.PickingLight);
                }
                else if (targetType == (int)LF2ObjectType.HeavyWeapon)
                {
                    linkState = 2;
                    attacker.DirectWriteRawFramePreserveWaitCounter(LF2StandardFrames.PickingHeavy);
                }
                else if (targetType == (int)LF2ObjectType.ThrowWeapon)
                {
                    linkState = 4;
                    attacker.DirectWriteRawFramePreserveWaitCounter(LF2StandardFrames.PickingLight);
                }
                else if (targetType == (int)LF2ObjectType.Drink)
                {
                    linkState = target.Health != null && target.Health.HP > 0 ? 6 : 4;
                    attacker.DirectWriteRawFramePreserveWaitCounter(LF2StandardFrames.PickingLight);
                    if (target.Health == null || target.Health.HP <= 0)
                        target.Runtime.WeaponFlightCounter = 0;
                }
                else
                {
                    return false;
                }

                attacker.AttackingCounter = 0;
                targetLinkState = -linkState;
            }

            attacker.Runtime.LinkState = linkState;
            target.Runtime.LinkState = targetLinkState;
            target.RelationTeam = attacker.RelationTeam;
            attacker.Runtime.TargetSlotIndex = targetSlot;
            attacker.Runtime.HeldWeaponStableId = targetSlot;
            target.Runtime.HolderStableId = attackerSlot;
            target.HolderCopySlot = attackerSlot;
            attacker.Runtime.PickupCount++;
            if (targetType == (int)LF2ObjectType.Drink &&
                (target.Health == null || target.Health.HP <= 0))
            {
                target.Runtime.WeaponFlightCounter = 0;
            }
            return true;
        }

        private bool CanConsumeRecordedCandidate(LF2Entity target)
        {
            if (target == null || target == _attacker || target.Runtime == null)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int selfSlot = _attacker.Runtime?.SlotIndex ?? -1;
            return selfSlot < 0 || target.ItrVrestTest(selfSlot, true);
        }

    }
}
