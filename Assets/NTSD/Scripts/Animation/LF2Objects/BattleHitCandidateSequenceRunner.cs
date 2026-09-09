using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NTSD.Extensions;

namespace NTSD.Animation.LF2Objects
{
    internal interface IBattleHitCandidateConsumer
    {
        LF2Entity Attacker { get; }

        void ApplyConsumeEffects(in SceneQueryHit hit);

        void BeforeDispatch(int itrIndex);

        bool Dispatch(
            INTSDItrKindService kindService,
            InteractionArea itr,
            LF2Entity target);
    }

    /// <summary>
    /// Owns the authority candidate-consumption order shared by character, DAT,
    /// weapon and special-attack shells. The concrete consumer still owns its
    /// type-specific writer; this runner only removes the four duplicated loops.
    /// </summary>
    internal static class BattleHitCandidateSequenceRunner
    {
        internal static bool TryConsume(IBattleHitCandidateConsumer consumer)
        {
            LF2Entity attacker = consumer?.Attacker;
            SimulationWorld world = attacker?.Match;
            ILF2SceneQuery sceneQuery = world?.SceneQuery;
            if (sceneQuery == null)
                return false;

            if (!sceneQuery.TryGetCollisionCandidateRange(
                    attacker,
                    out CollisionCandidateRange candidates))
            {
                return false;
            }

            return TryConsumeCaptured(consumer, in candidates);
        }

        internal static bool TryConsumeCaptured(
            IBattleHitCandidateConsumer consumer,
            in CollisionCandidateRange candidates)
        {
            LF2Entity attacker = consumer?.Attacker;
            LF2FrameData collisionFrame = attacker?.GetCollisionFrameData();
            SimulationWorld world = attacker?.Match;
            INTSDItrKindService kindService = world?.ItrKindService;
            if (collisionFrame?.itrs == null ||
                kindService == null)
            {
                return false;
            }

            int candidateLimit = candidates.Count;
            for (int candidateIndex = 0;
                 candidateIndex < candidateLimit;
                 candidateIndex++)
            {
                if (!candidates.TryGet(candidateIndex, out SceneQueryHit candidate))
                    continue;

                if (TryConsumeCandidate(
                        consumer,
                        collisionFrame,
                        kindService,
                        in candidate))
                {
                    break;
                }
            }

            return true;
        }

        private static bool TryConsumeCandidate(
            IBattleHitCandidateConsumer consumer,
            LF2FrameData collisionFrame,
            INTSDItrKindService kindService,
            in SceneQueryHit candidate)
        {
            LF2Entity attacker = consumer.Attacker;
            int itrIndex = candidate.ItrIndex;
            if (itrIndex < 0 || itrIndex >= collisionFrame.itrs.Count)
                return false;
            InteractionArea originalItr = collisionFrame.itrs[itrIndex];
            if (originalItr == null)
                return false;

            SimulationWorld world = attacker.Match;
            LF2Entity target = candidate.ResolveCurrentTarget(world);
            if (target == null)
                return false;

            // NTSD 2.8 checks the persistent special-hit latch after the frozen-pair/vrest gate
            // and aborts the attacker only for character DAT targets, before ITR resolution.
            // Alignment contract: NTSD28-B5-SPECIAL-HIT-LATCH-ATOMIC-PRODUCTION-INTEGRATION-001.
            bool canConsume = CanConsumeRecordedCandidate(attacker, target);
            if (!canConsume)
                return false;

            if (attacker.Runtime.SpecialHitLatch0EB &&
                target.GetCurrentDataObjectTypeForSimulation() == (int)LF2ObjectType.Character)
            {
                return true;
            }

            if (BruteForceSceneQuery.IsReleaseConsumerPairBlocked(attacker, target))
                return false;

            InteractionArea runtimeItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                collisionFrame,
                originalItr,
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            if (runtimeItr == null)
                return false;

            // Alignment contract: NTSD28-B5-CANDIDATE-EFFECT-TYPE-PRODUCTION-FILTER-001.
            // The release consumer revalidates the resolved runtime ITR before any
            // disposition or writer can observe the candidate.
            if (!BattleHitCandidateEffectTypeResolver.Accepts(
                    runtimeItr.effect,
                    target.GetCurrentDataObjectTypeForSimulation()))
            {
                return false;
            }

            // Alignment contract: NTSD28-B5-HIT-GROUP-ELIGIBILITY-ATOMIC-PRODUCTION-INTEGRATION-001.
            // Formal candidates re-use their candidate-time pair values. Only
            // hand-authored compatibility candidates are sampled live here.
            BattleHitCandidatePairSnapshot pairSnapshot = candidate.PairSnapshot.Valid
                ? candidate.PairSnapshot
                : BattleHitCandidatePairSnapshotFactory.Capture(
                    attacker,
                    target,
                    world);
            int activeModeHitGroupGate18 =
                world.Runtime?.NativeHitResourceRules?.ActiveModeHitGroupGate18 ??
                NTSD28HitResourceRulesRuntimeState.DefaultActiveModeHitGroupGate18;
            if (!BattleHitGroupEligibilityResolver.Resolve(
                    originalItr.kind,
                    runtimeItr.effect,
                    in pairSnapshot,
                    activeModeHitGroupGate18).Accepted)
            {
                return false;
            }
            if (originalItr.kind == 5 &&
                runtimeItr.kind == 0 &&
                pairSnapshot.TargetObjectType == (int)LF2ObjectType.Character &&
                !BattleHitGroupEligibilityResolver
                    .AcceptsSubstitutedKind5Character(in pairSnapshot))
            {
                return false;
            }

            // Alignment contract: R4-COL-003. C++ collision.cpp evaluates this only
            // after its local kind5/4/9 runtime-itr conversions, then aborts the
            // whole attacker before any disposition or writer observes this pair.
            int targetCurrentState = target.Frame?.D?.state ?? 0;
            if (runtimeItr.kind == 0 &&
                runtimeItr.effect == 21 &&
                (targetCurrentState == LF2States.Burning ||
                 targetCurrentState == LF2States.FirenSpecific))
            {
                return true;
            }

            if (world.ShouldObserveBattleHitExecutionPlanLegacyPreprocess)
            {
                world.ObserveBattleHitExecutionPlanLegacyPreprocess(
                    attacker,
                    target,
                    runtimeItr,
                    zeroAttackerHpOnConsume,
                    releaseHeavyHeldTargetOnConsume);
            }

            var hit = new SceneQueryHit(
                target,
                candidate.BodyX,
                itrIndex,
                runtimeItr,
                zeroAttackerHpOnConsume,
                releaseHeavyHeldTargetOnConsume,
                pairSnapshot);
            BattleHitCandidateDisposition disposition =
                LF2HitResolveRuntimeData.ResolveCandidateDisposition(
                    target,
                    runtimeItr,
                    canConsume);
            if (world.ShouldObserveBattleHitExecutionPlanLegacyDisposition)
            {
                world.ObserveBattleHitExecutionPlanLegacyDisposition(
                    attacker,
                    target,
                    runtimeItr,
                    disposition);
            }

            if (!canConsume || disposition == BattleHitCandidateDisposition.Unsupported)
                return false;

            if (disposition == BattleHitCandidateDisposition.HitConfirm)
            {
                ObserveSimpleWriterBefore(world, attacker, target, runtimeItr, disposition);
                target.HitConfirmCounter = 3;
                ObserveSimpleWriterAfter(world, attacker, target);
                return false;
            }

            if (disposition == BattleHitCandidateDisposition.Kind1Grab ||
                disposition == BattleHitCandidateDisposition.Kind3Grab ||
                disposition == BattleHitCandidateDisposition.Pickup)
            {
                ObserveSimpleWriterBefore(world, attacker, target, runtimeItr, disposition);
                consumer.Dispatch(kindService, runtimeItr, target);
                ObserveSimpleWriterAfter(world, attacker, target);
                return false;
            }

            if (!LF2HitResolveRuntimeData.IsAttackDisposition(disposition))
                return false;

            if (BattleFirstBodyResponseWriter
                    .IsUnarmoredContinuationDisposition(disposition) &&
                runtimeItr.kind == 0)
            {
                // Alignment contract:
                // NTSD28-B5-FIRST-BDY-RESPONSE-ATOMIC-PRODUCTION-INTEGRATION-001.
                // The native first-current-BDY branch precedes all generic consume
                // effects and ordinary damage, and a success terminates only this
                // attacker's remaining candidate sequence.
                if (world.ShouldObserveBattleHitExecutionPlanLegacyFirstBodyResponseAttempt)
                {
                    world.PrepareBattleHitExecutionPlanLegacyFirstBodyResponseAttemptObservation(
                        attacker,
                        target,
                        runtimeItr,
                        disposition);
                }
                BattleFirstBodyResponseAttemptResult firstBodyResponse =
                    BattleFirstBodyResponseWriter.TryApply(
                        world,
                        attacker,
                        target,
                        runtimeItr);
                if (world.ShouldObserveBattleHitExecutionPlanLegacyFirstBodyResponseAttempt)
                {
                    world.ObserveBattleHitExecutionPlanLegacyFirstBodyResponseAttempt(
                        attacker,
                        target,
                        in firstBodyResponse);
                }
                if (firstBodyResponse.Applied)
                    return true;
            }

            if (world.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects)
            {
                world.PrepareBattleHitExecutionPlanLegacyConsumeEffectsObservation(
                    attacker,
                    target);
            }
            consumer.ApplyConsumeEffects(in hit);
            if (world.ShouldObserveBattleHitExecutionPlanLegacyConsumeEffects)
            {
                world.ObserveBattleHitExecutionPlanLegacyConsumeEffects(attacker, target);
            }

            bool abortAfterSuccessfulHit =
                LF2HitResolveRuntimeData.ShouldAbortRemainingHitPairsAfterOid300Redirect(
                    target,
                    runtimeItr);
            if (world.ShouldObserveBattleHitExecutionPlanLegacyDispatch)
            {
                world.PrepareBattleHitExecutionPlanLegacyDispatchObservation(
                    attacker,
                    target,
                    runtimeItr);
            }

            bool observeWriterEffect = ShouldObserveWriterEffect(
                world,
                attacker,
                target,
                runtimeItr,
                disposition);
            bool observeLifecycleEffect = attacker is LF2SpecialAttack &&
                disposition == BattleHitCandidateDisposition.Damage &&
                world.ShouldObserveBattleHitExecutionPlanLegacyLifecycleEffect &&
                world.CanProjectBattleHitExecutionPlanLegacyLifecycleEffect(
                    attacker,
                    target,
                    runtimeItr,
                    disposition);
            if (observeWriterEffect)
            {
                world.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                    attacker,
                    target,
                    runtimeItr,
                    disposition);
            }
            if (observeLifecycleEffect)
            {
                world.PrepareBattleHitExecutionPlanLegacyLifecycleEffectObservation(
                    attacker,
                    target,
                    runtimeItr,
                    disposition);
            }

            int nativeComboAttackerSlot = attacker.Runtime?.SlotIndex ?? -1;
            int nativeComboTargetSlot = target.Runtime?.SlotIndex ?? -1;
            consumer.BeforeDispatch(itrIndex);
            bool dispatched = disposition == BattleHitCandidateDisposition.Kind8
                ? BattleKind8ControlRelationWriter.TryApply(
                    world,
                    attacker,
                    target,
                    runtimeItr)
                : consumer.Dispatch(kindService, runtimeItr, target);
            TryProduceNativeComboAfterDispatch(
                world,
                disposition,
                dispatched,
                nativeComboAttackerSlot,
                nativeComboTargetSlot);
            if (observeWriterEffect)
                world.ObserveBattleHitExecutionPlanLegacyWriterEffect(attacker, target);
            if (observeLifecycleEffect)
                world.ObserveBattleHitExecutionPlanLegacyLifecycleEffect(attacker);
            if (world.ShouldObserveBattleHitExecutionPlanLegacyDispatch)
            {
                world.ObserveBattleHitExecutionPlanLegacyDispatch(
                    attacker,
                    dispatched,
                    dispatched && abortAfterSuccessfulHit);
            }

            return dispatched && abortAfterSuccessfulHit;
        }

        internal static bool TryProduceNativeComboAfterDispatch(
            SimulationWorld world,
            BattleHitCandidateDisposition disposition,
            bool dispatched,
            int attackerSlot,
            int targetSlot)
        {
            return dispatched &&
                   disposition == BattleHitCandidateDisposition.Damage &&
                   BattleNativeComboOrdinaryProducer.TryApply(
                       world,
                       attackerSlot,
                       targetSlot);
        }

        private static bool CanConsumeRecordedCandidate(
            LF2Entity attacker,
            LF2Entity target)
        {
            if (target == null || target == attacker || target.Runtime == null)
                return false;
            if (target.Runtime.PendingFlushDestroy || target.FrameCache == null)
                return false;

            int attackerSlot = attacker.Runtime?.SlotIndex ?? -1;
            return attackerSlot < 0 || target.ItrVrestTest(attackerSlot, true);
        }

        private static void ObserveSimpleWriterBefore(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            BattleHitCandidateDisposition disposition)
        {
            if (!world.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect)
                return;

            world.PrepareBattleHitExecutionPlanLegacyWriterEffectObservation(
                attacker,
                target,
                itr,
                disposition);
        }

        private static void ObserveSimpleWriterAfter(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target)
        {
            if (world.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect)
                world.ObserveBattleHitExecutionPlanLegacyWriterEffect(attacker, target);
        }

        private static bool ShouldObserveWriterEffect(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr,
            BattleHitCandidateDisposition disposition)
        {
            if (!world.ShouldObserveBattleHitExecutionPlanLegacyWriterEffect)
                return false;

            return disposition == BattleHitCandidateDisposition.Kind8 ||
                   disposition == BattleHitCandidateDisposition.Kind14 ||
                   disposition == BattleHitCandidateDisposition.Kind10Or11 ||
                   disposition == BattleHitCandidateDisposition.Kind15 ||
                   (disposition == BattleHitCandidateDisposition.Damage &&
                    world.CanProjectBattleHitExecutionPlanLegacyWriterEffect(
                        attacker,
                        target,
                        itr,
                        disposition));
        }
    }
}
