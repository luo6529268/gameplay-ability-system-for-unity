using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Extensions;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 武器交互解析器。
    ///
    /// 处理武器在碰撞候选序列上的交互消费逻辑（对应原 LF2WeaponBase.Interaction）。
    /// 这里是纯机械抽取：逻辑、常量、字段读取顺序与原 partial 完全一致，
    /// 仅将对武器自身成员的裸引用改写为 `_weapon.X`。
    /// </summary>
    internal sealed class LF2WeaponInteractionResolver
    {
        private readonly LF2WeaponBase _weapon;

        public LF2WeaponInteractionResolver(LF2WeaponBase weapon)
        {
            _weapon = weapon;
        }

        public void RunInteraction()
        {
            if (_weapon.RelationTeam == 0) return;

            var sceneQuery = _weapon.Match?.SceneQuery;
            var kindService = _weapon.Match?.ItrKindService;
            if (sceneQuery == null || kindService == null) return;

            if (sceneQuery.TryGetCollisionCandidateSequence(_weapon, out var candidateSequence))
            {
                ConsumeInteractionCandidateSequence(_weapon.GetCollisionFrameData(), kindService, candidateSequence);
                return;
            }
        }

        private void ConsumeInteractionCandidateSequence(
            LF2FrameData frame,
            INTSDItrKindService kindService,
            List<SceneQueryHit> candidates)
        {
            LF2FrameData collisionFrame = _weapon.GetCollisionFrameData();
            if (collisionFrame?.itrs == null || candidates == null)
                return;

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0; candidateIndex < candidateLimit; candidateIndex++)
            {
                int liveCandidateCount = _weapon.Runtime?.HitCandidateCount ?? candidates.Count;
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
                    _weapon,
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
                if (!_weapon.CanInteractTargetInternal(itr, target)) continue;
                _weapon.ApplyReleaseSceneQueryConsumeEffectsInternal(hitInfo);
                if (!DispatchInteractionByKind(kindService, itr, target)) continue;
                if (ShouldAbortAfterSuccessfulReleaseHit(itr, target))
                    return;

                _weapon.ItrArestUpdate(itr);
                int selfSlot = _weapon.Runtime?.SlotIndex ?? -1;
                if (selfSlot >= 0 && target.ItrVrestTest(selfSlot, true))
                    target.ItrVrestUpdate(selfSlot, itr, true);

                if ((_weapon.Runtime?.HitCandidateCount ?? 0) <= 0)
                    return;

                break;
            }
        }

        private bool ShouldAbortReleaseConsume(InteractionArea itr, LF2Entity target)
        {
            if (itr == null || target == null)
                return false;

            if (_weapon.HitConfirm2 != 0 && GetDataObjectTypeForReleaseConsume(target) == (int)LF2ObjectType.Character)
                return true;

            if (itr.kind == 0 &&
                itr.effect == 21 &&
                (target.GetState() == LF2States.Burning || target.GetState() == LF2States.FirenSpecific))
            {
                return true;
            }

            return false;
        }

        private static int GetDataObjectTypeForReleaseConsume(LF2Entity entity)
        {
            if (entity == null)
                return -1;

            int wrapperOid = entity.FrameCache?.Wrapper?.characterId ?? entity.ObjectId;
            ObjectDefinition definition = GameDataManager.Instance?.GetObjectById(wrapperOid);
            return definition?.type ?? entity.ReleaseEntityType;
        }

        private static bool ShouldAbortAfterSuccessfulReleaseHit(InteractionArea itr, LF2Entity target)
        {
            return itr != null &&
                   target != null &&
                   itr.kind == 0 &&
                   target.ObjectId == 300;
        }

        private static bool IsReleaseNearestCandidatePath(InteractionArea itr)
        {
            return itr != null && itr.vrest == 0 && itr.kind != 1 && itr.kind != 2 && itr.kind != 7;
        }

        private bool DispatchInteractionByKind(INTSDItrKindService kindService, InteractionArea itr, LF2Entity target)
        {
            if (itr.kind == 8)
            {
                if (target == null || GetDataObjectTypeForReleaseConsume(target) != (int)LF2ObjectType.Character) return false;
                if (DeferState3005Kind8LeadIn()) return false;
                return _weapon.TryApplyHit(itr, target);
            }

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return _weapon.TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                case 1:
                    return _weapon.HandlePreInteractionKind1(itr, target);
                case 2:
                    return _weapon.HandlePreInteractionKind2(itr, target);
                case 3:
                    return _weapon.HandleWeaponKind3Stick(itr, target);
                case 7:
                    return _weapon.HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        private bool DeferState3005Kind8LeadIn()
        {
            var activeFrame = _weapon.Frame?.D;
            if (activeFrame == null || activeFrame.state != LF2States.ObjectFlying)
            {
                return false;
            }

            if (activeFrame.hit_Fa > 0 || (activeFrame.opoints != null && activeFrame.opoints.Count > 0))
            {
                return true;
            }

            int activeFrameId = activeFrame.frameId != 0 ? activeFrame.frameId : _weapon.Frame?.N ?? 0;
            if (activeFrame.next <= 0 || activeFrame.next == activeFrameId)
            {
                return false;
            }

            var nextFrame = _weapon.GetFrameDataById(activeFrame.next);
            return nextFrame != null
                && (nextFrame.hit_Fa > 0 || (nextFrame.opoints != null && nextFrame.opoints.Count > 0));
        }

        public void ApplyPickupGrabbedBy(LF2Character character)
        {
            int pickerLink;
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(_weapon.ObjectId);
            int typeSub = charData?.type_sub ?? 0;
            if (typeSub == 0x78 || typeSub == 0x7C)
                pickerLink = 101;
            else if (_weapon.IsHeavy)
                pickerLink = 2;
            else if (_weapon.WeaponType == 4)
                pickerLink = 4;
            else if (_weapon.WeaponType == 6)
                pickerLink = _weapon.Health.HP > 0 ? 6 : 4;
            else
                pickerLink = 0;

            character.GrabbedBy = pickerLink;
            _weapon.GrabbedBy = -pickerLink;
        }

        public void ApplyPickupFrameJump(LF2Character character)
        {
            int jumpFrame = _weapon.IsHeavy ? 116 : 115;
            if (character.GetFrameDataById(jumpFrame) != null)
            {
                character.ImmediateFrame(jumpFrame);
            }
        }
    }
}
