using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Extensions;

namespace NTSD.Animation.LF2Objects
{
    public abstract partial class LF2WeaponBase
    {
        public virtual void Interaction()
        {
            if (RelationTeam == 0) return;

            var sceneQuery = Match?.SceneQuery;
            var kindService = Match?.ItrKindService;
            if (sceneQuery == null || kindService == null) return;

            if (sceneQuery.TryGetCollisionCandidateSequence(this, out var candidateSequence))
            {
                ConsumeInteractionCandidateSequence(GetCollisionFrameData(), kindService, candidateSequence);
                return;
            }
        }

        private void ConsumeInteractionCandidateSequence(
            LF2FrameData frame,
            INTSDItrKindService kindService,
            List<SceneQueryHit> candidates)
        {
            LF2FrameData collisionFrame = GetCollisionFrameData();
            if (collisionFrame?.itrs == null || candidates == null)
                return;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                int liveCandidateCount = Runtime?.HitCandidateCount ?? candidates.Count;
                if (liveCandidateCount < 0)
                    liveCandidateCount = 0;
                if (liveCandidateCount > candidates.Count)
                    liveCandidateCount = candidates.Count;
                if (candidateIndex >= liveCandidateCount)
                    break;

                SceneQueryHit hitInfo = candidates[candidateIndex];
                int itrIndex = hitInfo.ItrIndex;
                if (itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count)
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    this,
                    hitInfo.Target,
                    collisionFrame,
                    collisionFrame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null) continue;

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
                if (!CanInteractTarget(itr, target, hitInfo.BodyX)) continue;
                ApplyReleaseSceneQueryConsumeEffects(hitInfo);
                if (!DispatchInteractionByKind(kindService, itr, target)) continue;
                if (ShouldAbortAfterSuccessfulReleaseHit(itr, target))
                    return;

                ItrArestUpdate(itr);
                int selfSlot = Runtime?.SlotIndex ?? -1;
                if (selfSlot >= 0)
                    target.ItrVrestUpdate(selfSlot, itr, true);

                if ((Runtime?.HitCandidateCount ?? 0) <= 0)
                    return;

                break;
            }
        }

        protected bool ShouldAbortReleaseConsume(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;

            if (HitConfirm2 != 0 && GetDataObjectTypeForReleaseConsume(target) == (int)LF2ObjectType.Character)
                return true;

            if (itr.kind == 0 &&
                itr.effect == 21 &&
                (target.GetState() == LF2States.Burning || target.GetState() == LF2States.FirenSpecific))
            {
                return true;
            }

            return false;
        }

        protected static int GetDataObjectTypeForReleaseConsume(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = entity.FrameCache?.Wrapper?.characterId ?? entity.ObjectId;
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        protected static bool ShouldAbortAfterSuccessfulReleaseHit(InteractionArea itr, LF2Entity target)
        {
            return itr != null &&
                   target != null &&
                   itr.kind == 0 &&
                   target.ObjectId == 300;
        }

        protected static bool IsReleaseNearestCandidatePath(InteractionArea itr)
        {
            return itr != null && itr.vrest == 0 && itr.kind != 1 && itr.kind != 2 && itr.kind != 7;
        }

        protected virtual bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 8)
            {
                if (target == null || GetDataObjectTypeForReleaseConsume(target) != (int)LF2ObjectType.Character) return false;
                if (DeferState3005Kind8LeadIn()) return false;
                return TryApplyHit(itr, target);
            }

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                case 1:
                    return HandlePreInteractionKind1(itr, target);
                case 2:
                    return HandlePreInteractionKind2(itr, target);
                case 3:
                    return HandleWeaponKind3Stick(itr, target);
                case 7:
                    return HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        private bool DeferState3005Kind8LeadIn()
        {
            var activeFrame = Frame?.D;
            if (activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
            {
                return false;
            }

            if (activeFrame.hit_Fa > 0 || (activeFrame.opoints != null && activeFrame.opoints.Count > 0))
            {
                return true;
            }

            int activeFrameId = activeFrame.frameId != 0 ? activeFrame.frameId : Frame?.N ?? 0;
            if (activeFrame.next <= 0 || activeFrame.next == activeFrameId)
            {
                return false;
            }

            var nextFrame = GetFrameDataById(activeFrame.next);
            return nextFrame != null
                && (nextFrame.hit_Fa > 0 || (nextFrame.opoints != null && nextFrame.opoints.Count > 0));
        }

        private void ApplyPickupGrabbedBy(LF2Character character)
        {
            int pickerLink;
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(ObjectId);
            int typeSub = charData?.type_sub ?? 0;
            if (typeSub == 0x78 || typeSub == 0x7C)
                pickerLink = 101;
            else if (IsHeavy)
                pickerLink = 2;
            else if (WeaponType == 4)
                pickerLink = 4;
            else if (WeaponType == 6)
                pickerLink = Health.HP > 0 ? 6 : 4;
            else
                pickerLink = 0;

            character.GrabbedBy = pickerLink;
            GrabbedBy = -pickerLink;
        }

        private void ApplyPickupFrameJump(LF2Character character)
        {
            int jumpFrame = IsHeavy ? 116 : 115;
            if (character.GetFrameDataById(jumpFrame) != null)
            {
                character.ImmediateFrame(jumpFrame);
            }
        }
    }
}
