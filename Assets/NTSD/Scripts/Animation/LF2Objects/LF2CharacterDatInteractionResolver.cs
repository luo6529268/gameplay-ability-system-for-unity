using NTSD.Animation;
using NTSD.Extensions;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using UnityEngine;

namespace NTSD.Animation.LF2Objects
{
    internal sealed class LF2CharacterDatInteractionResolver : IBattleHitCandidateConsumer
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
                    return attacker.Match?.InteractionWriter.TryApplyGrab(
                        attacker,
                        target,
                        itr,
                        1) ?? false;
                case 3:
                    return attacker.Match?.InteractionWriter.TryApplyGrab(
                        attacker,
                        target,
                        itr,
                        3) ?? false;
                case 2:
                case 7:
                    return attacker.Match?.InteractionWriter.TryApplyPickup(
                        attacker,
                        target,
                        itr.kind) ?? false;
                default:
                    return false;
            }
        }

        internal void TryConsumeUnifiedStep7CandidateSequence()
        {
            BattleHitCandidateSequenceRunner.TryConsume(this);
        }

        LF2Entity IBattleHitCandidateConsumer.Attacker => _attacker;

        void IBattleHitCandidateConsumer.ApplyConsumeEffects(in SceneQueryHit hit)
        {
            _attacker.ApplyReleaseSceneQueryConsumeEffectsForCharacterDatInteraction(hit);
        }

        void IBattleHitCandidateConsumer.BeforeDispatch(int itrIndex)
        {
        }

        bool IBattleHitCandidateConsumer.Dispatch(
            INTSDItrKindService kindService,
            InteractionArea itr,
            LF2Entity target)
        {
            return DispatchInteractionByKind(kindService, itr, target);
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
