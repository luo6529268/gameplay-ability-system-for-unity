using NTSD.Animation;
using NTSD.Extensions;
using NTSD.Simulation.Ecs;

namespace NTSD.Animation.LF2Objects
{
    /// <summary>
    /// 武器交互解析器。
    ///
    /// 处理武器在碰撞候选序列上的交互消费逻辑（对应原 LF2WeaponBase.Interaction）。
    /// 这里是纯机械抽取：逻辑、常量、字段读取顺序与原 partial 完全一致，
    /// 仅将对武器自身成员的裸引用改写为 `_weapon.X`。
    /// </summary>
    internal sealed class LF2WeaponInteractionResolver : IBattleHitCandidateConsumer
    {
        private readonly LF2WeaponBase _weapon;

        public LF2WeaponInteractionResolver(LF2WeaponBase weapon)
        {
            _weapon = weapon;
        }

        public void RunInteraction()
        {
            BattleHitCandidateSequenceRunner.TryConsume(this);
        }

        LF2Entity IBattleHitCandidateConsumer.Attacker => _weapon;

        void IBattleHitCandidateConsumer.ApplyConsumeEffects(in SceneQueryHit hit)
        {
            _weapon.ApplyReleaseSceneQueryConsumeEffectsInternal(hit);
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

        private bool CanConsumeRecordedCandidate(LF2Entity target)
        {
            if (target == null || target == _weapon || target.Runtime == null)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int selfSlot = _weapon.Runtime?.SlotIndex ?? -1;
            return selfSlot < 0 || target.ItrVrestTest(selfSlot, true);
        }

        private static int GetDataObjectTypeForReleaseConsume(LF2Entity entity)
        {
            return LF2Entity.ResolveCurrentDataObjectType(entity);
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
                return _weapon.TryApplyHit(itr, target);
            }

            if (kindService != null && kindService.IsAttackKind(itr.kind))
            {
                return _weapon.TryApplyHit(itr, target);
            }

            switch (itr.kind)
            {
                case 1:
                    // Alignment contract: R8-COL-005B-001
                    return _weapon.Match?.InteractionWriter.TryApplyGrab(
                        _weapon,
                        target,
                        itr,
                        1) ?? false;
                case 2:
                    return _weapon.HandlePreInteractionKind2(itr, target);
                case 3:
                    return _weapon.Match?.InteractionWriter.TryApplyGrab(
                        _weapon,
                        target,
                        itr,
                        3) ?? false;
                case 7:
                    return _weapon.HandlePreInteractionKind7(itr, target);
                default:
                    return false;
            }
        }

        public void ApplyPickupGrabbedBy(LF2Character character)
        {
            int pickerLink;
            int currentDataOid = LF2Entity.ResolveCurrentDataObjectId(_weapon);
            LF2CharacterData charData =
                _weapon.ResolveRuntimeCharacterData(currentDataOid);
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
            if (character.FrameCache.HasFrame(jumpFrame))
            {
                character.ImmediateFrame(jumpFrame);
            }
        }
    }
}
