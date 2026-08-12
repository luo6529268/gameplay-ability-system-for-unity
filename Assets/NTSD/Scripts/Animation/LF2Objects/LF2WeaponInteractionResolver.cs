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
    internal sealed class LF2WeaponInteractionResolver
    {
        private readonly LF2WeaponBase _weapon;

        public LF2WeaponInteractionResolver(LF2WeaponBase weapon)
        {
            _weapon = weapon;
        }

        public void RunInteraction()
        {
            var sceneQuery = _weapon.Match?.SceneQuery;
            var kindService = _weapon.Match?.ItrKindService;
            if (sceneQuery == null || kindService == null) return;

            if (sceneQuery.TryGetCollisionCandidateRange(_weapon, out var candidateSequence))
            {
                ConsumeInteractionCandidateSequence(_weapon.GetCollisionFrameData(), kindService, candidateSequence);
                return;
            }
        }

        private void ConsumeInteractionCandidateSequence(
            LF2FrameData frame,
            INTSDItrKindService kindService,
            CollisionCandidateRange candidates)
        {
            LF2FrameData collisionFrame = _weapon.GetCollisionFrameData();
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

                LF2Entity target = hitInfo.ResolveCurrentTarget(_weapon.Match);
                if (target == null)
                    continue;

                bool zeroAttackerHpOnConsume;
                bool releaseHeavyHeldTargetOnConsume;
                InteractionArea itr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                    _weapon,
                    target,
                    collisionFrame,
                    collisionFrame.itrs[itrIndex],
                    out zeroAttackerHpOnConsume,
                    out releaseHeavyHeldTargetOnConsume);
                if (itr == null) continue;

                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyPreprocess == true)
                {
                    _weapon.Match.ObserveBattleHitExecutionPlanLegacyPreprocess(
                        _weapon,
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
                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyDisposition == true)
                {
                    _weapon.Match.ObserveBattleHitExecutionPlanLegacyDisposition(
                        _weapon,
                        target,
                        itr,
                        disposition);
                }

                if (!canConsume ||
                    disposition == BattleHitCandidateDisposition.Unsupported)
                    continue;
                if (disposition == BattleHitCandidateDisposition.HitConfirm)
                {
                    if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true)
                    {
                        _weapon.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                            _weapon,
                            target,
                            itr,
                            disposition);
                    }
                    target.HitConfirmCounter = 3;
                    if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true)
                    {
                        _weapon.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                            _weapon,
                            target);
                    }
                    continue;
                }
                if (disposition == BattleHitCandidateDisposition.Kind1Grab ||
                    disposition == BattleHitCandidateDisposition.Kind3Grab ||
                    disposition == BattleHitCandidateDisposition.Pickup)
                {
                    bool observePreInteractionWriterEffect =
                        _weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true;
                    if (observePreInteractionWriterEffect)
                    {
                        _weapon.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                            _weapon,
                            target,
                            itr,
                            disposition);
                    }
                    DispatchInteractionByKind(kindService, itr, target);
                    if (observePreInteractionWriterEffect)
                    {
                        _weapon.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                            _weapon,
                            target);
                    }
                    continue;
                }

                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects == true)
                {
                    _weapon.Match.PrepareBattleHitExecutionPlanLegacyConsumeEffectsObservation(
                        _weapon,
                        target);
                }
                _weapon.ApplyReleaseSceneQueryConsumeEffectsInternal(hitInfo);
                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects == true)
                {
                    _weapon.Match.ObserveBattleHitExecutionPlanLegacyConsumeEffects(
                        _weapon,
                        target);
                }
                bool abortAfterSuccessfulHit = LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect(
                    target,
                    itr);
                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyDispatch == true)
                {
                    _weapon.Match.PrepareBattleHitExecutionPlanLegacyDispatchObservation(
                        _weapon,
                        target,
                        itr);
                }
                bool observeWriterEffect =
                    (disposition == BattleHitCandidateDisposition.Kind8 ||
                     disposition == BattleHitCandidateDisposition.Kind14 ||
                     disposition == BattleHitCandidateDisposition.Kind10Or11 ||
                     disposition == BattleHitCandidateDisposition.Kind15Or16 ||
                     (disposition == BattleHitCandidateDisposition.Damage &&
                      _weapon.Match?.CanProjectBattleHitExecutionPlanLegacyWriterEffect(
                          _weapon,
                          target,
                          itr,
                          disposition) == true)) &&
                    _weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect == true;
                if (observeWriterEffect)
                {
                    _weapon.Match.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                        _weapon,
                        target,
                        itr,
                        disposition);
                }
                bool dispatched = DispatchInteractionByKind(kindService, itr, target);
                if (observeWriterEffect)
                {
                    _weapon.Match.ObserveBattleHitExecutionPlanLegacyWriterEffect(
                        _weapon,
                        target);
                }
                if (_weapon.Match?.ShouldObserveBattleHitExecutionPlanLegacyDispatch == true)
                {
                    _weapon.Match.ObserveBattleHitExecutionPlanLegacyDispatch(
                        _weapon,
                        dispatched,
                        dispatched && abortAfterSuccessfulHit);
                }
                if (!dispatched) continue;
                if (abortAfterSuccessfulHit)
                    return;

            }
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
                    return _weapon.HandlePreInteractionKind1(itr, target);
                case 2:
                    return _weapon.HandlePreInteractionKind2(itr, target);
                case 3:
                    return LF2CharacterInteractionResolver.TryApplyKind3Grab(_weapon, target, itr);
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
            var charData = CharacterAnimtorManager.Instance?.GetCharacterData(currentDataOid);
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
