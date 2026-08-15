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

            SimulationWorld world = attacker.Match;
            LF2Entity target = candidate.ResolveCurrentTarget(world);
            if (target == null)
                return false;

            InteractionArea runtimeItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(
                attacker,
                target,
                collisionFrame,
                collisionFrame.itrs[itrIndex],
                out bool zeroAttackerHpOnConsume,
                out bool releaseHeavyHeldTargetOnConsume);
            if (runtimeItr == null)
                return false;

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
                releaseHeavyHeldTargetOnConsume);
            bool canConsume = CanConsumeRecordedCandidate(attacker, target);
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

            consumer.BeforeDispatch(itrIndex);
            bool dispatched = consumer.Dispatch(kindService, runtimeItr, target);
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
                   disposition == BattleHitCandidateDisposition.Kind15Or16 ||
                   (disposition == BattleHitCandidateDisposition.Damage &&
                    world.CanProjectBattleHitExecutionPlanLegacyWriterEffect(
                        attacker,
                        target,
                        itr,
                        disposition));
        }
    }
}
