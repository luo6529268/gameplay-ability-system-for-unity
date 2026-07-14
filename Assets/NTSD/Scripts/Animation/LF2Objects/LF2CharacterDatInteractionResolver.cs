using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Extensions;
using NTSD.Simulation;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2CharacterDatInteractionResolver
    {
        private readonly LF2Entity _attacker;

        private LF2CharacterDatInteractionResolver(LF2Entity attacker)
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

            new LF2CharacterDatInteractionResolver(attacker).TryConsumeUnifiedStep7CandidateSequenceInternal();
        }

        private void TryConsumeUnifiedStep7CandidateSequenceInternal()
        {
            ILF2SceneQuery sceneQuery = _attacker.Match?.SceneQuery;
            INTSDItrKindService kindService = _attacker.Match?.ItrKindService;
            if (sceneQuery == null || kindService == null)
                return;

            if (!sceneQuery.TryGetCollisionCandidateSequence(_attacker, out List<SceneQueryHit> candidates))
                return;

            LF2FrameData collisionFrame = _attacker.GetCollisionFrameData();
            if (collisionFrame?.itrs == null || candidates == null)
                return;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                int liveCandidateCount = _attacker.Runtime?.HitCandidateCount ?? candidates.Count;
                if (liveCandidateCount < 0) liveCandidateCount = 0;
                if (liveCandidateCount > candidates.Count) liveCandidateCount = candidates.Count;
                if (candidateIndex >= liveCandidateCount)
                    break;

                SceneQueryHit hitInfo = candidates[candidateIndex];
                int itrIndex = hitInfo.ItrIndex;
                if (itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count)
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    _attacker,
                    hitInfo.Target,
                    collisionFrame,
                    collisionFrame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null)
                    continue;

                hitInfo = new SceneQueryHit(
                    hitInfo.Target,
                    hitInfo.BodyX,
                    hitInfo.ItrIndex,
                    itr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);

                LF2Entity target = hitInfo.Target;
                if (ShouldAbortReleaseConsume(itr, target))
                    return;
                if (itr.kind == 6)
                {
                    _attacker.TryApplyKind6HitConfirmForCharacterDatInteraction(itr, target);
                    continue;
                }
                if (!CanInteractTarget(itr, target))
                    continue;

                _attacker.ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(hitInfo);
                if (!DispatchInteractionByKind(kindService, itr, target))
                    continue;

                _attacker.ItrArestUpdate(itr);
                int selfSlot = _attacker.Runtime?.SlotIndex ?? -1;
                if (selfSlot >= 0 && target.ItrVrestTest(selfSlot, true))
                    target.ItrVrestUpdate(selfSlot, itr, true);

                if ((_attacker.Runtime?.HitCandidateCount ?? 0) <= 0)
                    return;

                break;
            }
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
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
                return target is LF2LivingObject livingTarget && livingTarget.Hit(itr, _attacker, attackerPos, default);
            }
            return false;
        }

        private bool CanInteractTarget(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;
            if (target == _attacker)
                return false;
            if (target.Frame?.D == null)
                return false;
            if (target.Health != null && target.Health.HP <= 0)
                return false;
            if (!BruteForceSceneQuery.IsReleaseItrGeometry(itr))
                return false;
            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(_attacker, target))
                return false;
            if (!BruteForceSceneQuery.RuntimeConsumeItrAllowed(_attacker, itr, target))
                return false;
            int selfSlot = _attacker.Runtime?.SlotIndex ?? -1;
            if (selfSlot >= 0 && !target.ItrVrestTest(selfSlot, true))
                return false;
            target.HitConfirmCounter = 3;
            return true;
        }

        private bool ShouldAbortReleaseConsume(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;
            if (_attacker.HitConfirm2 != 0 && LF2Entity.ResolveCurrentDataObjectType(target) == (int)LF2ObjectType.Character)
                return true;
            return itr.kind == 0 &&
                   itr.effect == 21 &&
                   (target.GetState() == LF2States.Burning || target.GetState() == LF2States.FirenSpecific);
        }
    }
}
