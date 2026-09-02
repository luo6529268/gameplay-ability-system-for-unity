using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation.Ecs;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns pre-interaction, character/object hit consumption and collision
    /// consumption boundaries while preserving the World scheduling façade.
    /// </summary>
    internal sealed class BattleInteractionPipeline
    {
        private readonly SimulationWorld world;
        private readonly List<LF2Entity> participantScratch =
            new List<LF2Entity>(16);

        internal BattleInteractionPipeline(SimulationWorld world)
        {
            this.world = world;
        }

        internal bool ForceLegacyPreInteractionForDiagnostics { get; set; }
        internal bool ForceLegacyPreInteractionCrossPassProofForDiagnostics
        {
            get;
            set;
        }
        internal bool ForceLegacyPreInteractionParticipantFilteringForDiagnostics
        {
            get;
            set;
        }
        internal bool ForceLegacyEmptyCharacterHitConsumeForDiagnostics
        {
            get;
            set;
        }
        internal bool ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics
        {
            get;
            set;
        } = true;
        internal bool ForceLegacyEmptyObjectHitConsumeForDiagnostics
        {
            get;
            set;
        }

        internal int LastPreInteractionScannedCountForDiagnostics { get; private set; }
        internal int LastPreInteractionExecutedCountForDiagnostics { get; private set; }
        internal int LastPreInteractionProofSkipCountForDiagnostics { get; private set; }
        internal int LastPreInteractionSnapshotSkipCountForDiagnostics { get; private set; }
        internal int LastPreInteractionFailClosedCountForDiagnostics { get; private set; }
        internal int LastPreInteractionCpointCheckProofSkipCountForDiagnostics { get; private set; }
        internal int LastPreInteractionMismatchTailProofSkipCountForDiagnostics { get; private set; }
        internal int LastPreInteractionHeldSyncProofSkipCountForDiagnostics { get; private set; }
        internal bool LastPreInteractionWholePassProofSucceededForDiagnostics { get; private set; }
        internal int LastPreInteractionWholePassParticipantCountForDiagnostics { get; private set; }
        internal bool LastPreInteractionCrossPassProofUsedForDiagnostics { get; private set; }
        internal int LastEmptyCharacterHitConsumeSkipCountForDiagnostics { get; private set; }
        internal int LastCharacterHitConsumeExecutedCountForDiagnostics { get; private set; }
        internal int LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics { get; private set; }
        internal int LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics { get; private set; }
        internal int LastEmptyObjectHitConsumeSkipCountForDiagnostics { get; private set; }
        internal int LastObjectHitConsumeExecutedCountForDiagnostics { get; private set; }

        internal void PrepareCapacity(int entityCapacity)
        {
            if (participantScratch.Capacity < entityCapacity)
                participantScratch.Capacity = entityCapacity;
        }

        internal void EndCollisionCandidateConsumption()
        {
            if (world.SceneQuery is BruteForceSceneQuery bruteForce)
                bruteForce.EndCollisionCandidateConsumption();
        }

        internal void RunPostInteraction(int tickIndex)
        {
            LastEmptyCharacterHitConsumeSkipCountForDiagnostics = 0;
            LastCharacterHitConsumeExecutedCountForDiagnostics = 0;
            LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics = 0;
            LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics = 0;
            bool runtimeCandidateCountGate =
                !ForceLegacyEmptyCharacterHitConsumeForDiagnostics &&
                !ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics &&
                world.SceneQuery is BruteForceSceneQuery sceneQuery &&
                sceneQuery.TryProveLegacyRuntimeCandidateCountsForCurrentTick();
            if (runtimeCandidateCountGate)
                LastCharacterRuntimeCandidateCountGateAppliedForDiagnostics = 1;
            else if (!ForceLegacyEmptyCharacterHitConsumeForDiagnostics &&
                     !ForceLegacyCharacterRuntimeCandidateCountGateForDiagnostics)
            {
                LastCharacterRuntimeCandidateCountGateFallbackForDiagnostics = 1;
            }
            world.CaptureBattleHitExecutionPlanPass(
                tickIndex,
                BattleHitExecutionPass.Character,
                runtimeCandidateCountGate);
            bool observeLegacyConsumption =
                world.BeginBattleHitExecutionPlanLegacyObservation(
                    tickIndex,
                    BattleHitExecutionPass.Character);

            world.BeginDeferredEntityMutationPass();
            try
            {
                BattleEcsHitExecutionPlan hitPlan =
                    world.HitExecutionPlanForInteractionModule;
                if (hitPlan.TryValidateDataOrientedPass(
                        tickIndex,
                        BattleHitExecutionPass.Character))
                {
                    int participantCount = hitPlan
                        .GetDataOrientedParticipantCount(
                            BattleHitExecutionPass.Character);
                    for (int participantIndex = 0;
                         participantIndex < participantCount;
                         participantIndex++)
                    {
                        if (hitPlan.TryGetDataOrientedParticipant(
                                BattleHitExecutionPass.Character,
                                participantIndex,
                                out LF2Entity entity,
                                out CollisionCandidateRange candidates))
                        {
                            ConsumeDataOrientedCharacterHitParticipant(
                                entity,
                                tickIndex,
                                in candidates);
                        }
                    }
                }
                else
                {
                    foreach (LF2Entity entity in
                             world.ActiveEntitiesByRuntimeSlotForModule)
                    {
                        ConsumeCharacterHitParticipant(
                            entity,
                            tickIndex,
                            runtimeCandidateCountGate);
                    }
                }
            }
            finally
            {
                world.EndDeferredEntityMutationPass();
            }
            if (observeLegacyConsumption)
                world.EndBattleHitExecutionPlanLegacyObservation();
        }

        internal void RunObjectInteraction(int tickIndex)
        {
            bool passProvenEmpty =
                !ForceLegacyEmptyObjectHitConsumeForDiagnostics &&
                CanUseEmptyObjectInteractionProof() &&
                world.SceneQuery is BruteForceSceneQuery sceneQuery &&
                sceneQuery.TryProveNoObjectInteractionCandidatesForCurrentTick();
            world.CaptureBattleHitExecutionPlanPass(
                tickIndex,
                BattleHitExecutionPass.Object,
                passProvenEmpty: passProvenEmpty);
            bool observeLegacyConsumption =
                world.BeginBattleHitExecutionPlanLegacyObservation(
                    tickIndex,
                    BattleHitExecutionPass.Object);
            LastEmptyObjectHitConsumeSkipCountForDiagnostics = 0;
            LastObjectHitConsumeExecutedCountForDiagnostics = 0;

            if (passProvenEmpty)
            {
                LastEmptyObjectHitConsumeSkipCountForDiagnostics = 1;
                if (observeLegacyConsumption)
                    world.EndBattleHitExecutionPlanLegacyObservation();
                return;
            }

            world.BeginDeferredEntityMutationPass();
            try
            {
                BattleEcsHitExecutionPlan hitPlan =
                    world.HitExecutionPlanForInteractionModule;
                if (hitPlan.TryValidateDataOrientedPass(
                        tickIndex,
                        BattleHitExecutionPass.Object))
                {
                    int participantCount = hitPlan
                        .GetDataOrientedParticipantCount(
                            BattleHitExecutionPass.Object);
                    for (int participantIndex = 0;
                         participantIndex < participantCount;
                         participantIndex++)
                    {
                        if (hitPlan.TryGetDataOrientedParticipant(
                                BattleHitExecutionPass.Object,
                                participantIndex,
                                out LF2Entity entity,
                                out CollisionCandidateRange candidates))
                        {
                            ConsumeDataOrientedObjectHitParticipant(
                                entity,
                                tickIndex,
                                in candidates);
                        }
                    }
                }
                else
                {
                    foreach (LF2Entity entity in
                             world.ActiveEntitiesByRuntimeSlotForModule)
                    {
                        ConsumeObjectHitParticipant(entity, tickIndex);
                    }
                }
            }
            finally
            {
                world.EndDeferredEntityMutationPass();
            }
            if (observeLegacyConsumption)
                world.EndBattleHitExecutionPlanLegacyObservation();
        }

        internal void RunPreInteraction(int tickIndex)
        {
            LastPreInteractionScannedCountForDiagnostics = 0;
            LastPreInteractionExecutedCountForDiagnostics = 0;
            LastPreInteractionProofSkipCountForDiagnostics = 0;
            LastPreInteractionSnapshotSkipCountForDiagnostics = 0;
            LastPreInteractionFailClosedCountForDiagnostics = 0;
            LastPreInteractionCpointCheckProofSkipCountForDiagnostics = 0;
            LastPreInteractionMismatchTailProofSkipCountForDiagnostics = 0;
            LastPreInteractionHeldSyncProofSkipCountForDiagnostics = 0;
            LastPreInteractionWholePassProofSucceededForDiagnostics = false;
            LastPreInteractionWholePassParticipantCountForDiagnostics = 0;
            LastPreInteractionCrossPassProofUsedForDiagnostics = false;

            world.BeginDeferredEntityMutationPass();
            try
            {
                if (!ForceLegacyPreInteractionForDiagnostics &&
                    TryProveWholePreInteractionPassNoOp(
                        tickIndex,
                        out int participantCount))
                {
                    ApplyWholePreInteractionNoOpDiagnostics(participantCount);
                    return;
                }

                world.GetActiveEntitiesByRuntimeSlotForModule(participantScratch);
                if (participantScratch.Count == 0)
                    return;

                for (int i = 0; i < participantScratch.Count; i++)
                {
                    LF2Entity entity = participantScratch[i];
                    if (entity?.Runtime != null &&
                        tickIndex <
                            entity.Runtime.SuppressPreInteractionUntilTick)
                    {
                        continue;
                    }
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;

                    if (!ForceLegacyPreInteractionParticipantFilteringForDiagnostics &&
                        CanSkipCpointCheckParticipant(entity))
                    {
                        LastPreInteractionProofSkipCountForDiagnostics++;
                        LastPreInteractionSnapshotSkipCountForDiagnostics++;
                        LastPreInteractionCpointCheckProofSkipCountForDiagnostics++;
                        continue;
                    }

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunCpointCheckStep10();
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;
                    world.RefreshRuntimeSnapshotForModule(entity);
                }

                for (int i = 0; i < participantScratch.Count; i++)
                {
                    LF2Entity entity = participantScratch[i];
                    if (entity?.Runtime != null &&
                        tickIndex <
                            entity.Runtime.SuppressPreInteractionUntilTick)
                    {
                        continue;
                    }
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;

                    if (!ForceLegacyPreInteractionParticipantFilteringForDiagnostics &&
                        CanSkipCpointMismatchTailParticipant(entity))
                    {
                        LastPreInteractionProofSkipCountForDiagnostics++;
                        LastPreInteractionSnapshotSkipCountForDiagnostics++;
                        LastPreInteractionMismatchTailProofSkipCountForDiagnostics++;
                        continue;
                    }

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunCpointMismatchTailStep10();
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;
                    world.RefreshRuntimeSnapshotForModule(entity);
                }

                participantScratch.Clear();

                // Keep the authority live ascending scan without allocating a
                // tick-capturing delegate. Newborns above the cursor join this
                // pass, while a recycled lower slot waits for the next pass.
                for (int runtimeSlot = 0;
                     runtimeSlot <
                         world.PreInteractionRuntimeSlotLogicalCapacityForModule;
                     runtimeSlot++)
                {
                    LF2Entity entity =
                        world.GetCurrentRuntimeSlotOccupantForInteractionModule(
                            runtimeSlot);
                    if (entity == null)
                        continue;
                    if (entity.Runtime != null &&
                        tickIndex <
                            entity.Runtime.SuppressPreInteractionUntilTick)
                    {
                        continue;
                    }
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;

                    if (!ForceLegacyPreInteractionParticipantFilteringForDiagnostics &&
                        CanSkipWeaponSyncHeldParticipant(entity))
                    {
                        LastPreInteractionProofSkipCountForDiagnostics++;
                        LastPreInteractionSnapshotSkipCountForDiagnostics++;
                        LastPreInteractionHeldSyncProofSkipCountForDiagnostics++;
                        continue;
                    }

                    LastPreInteractionScannedCountForDiagnostics++;
                    LastPreInteractionExecutedCountForDiagnostics++;
                    entity.RunWeaponSyncHeldStep10();
                    if (!world.IsActiveForCurrentPassInternal(entity))
                        continue;
                    world.RefreshRuntimeSnapshotForModule(entity);
                }
            }
            finally
            {
                participantScratch.Clear();
                world.EndDeferredEntityMutationPass();
            }
        }

        private void ConsumeCharacterHitParticipant(
            LF2Entity entity,
            int tickIndex,
            bool runtimeCandidateCountGate)
        {
            if (entity == null || !entity.SupportsPostInteractionPhase())
                return;
            if (entity.Runtime != null &&
                tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
            {
                return;
            }

            if (!ForceLegacyEmptyCharacterHitConsumeForDiagnostics &&
                CanSkipEmptyCharacterHitConsume(
                    entity,
                    runtimeCandidateCountGate))
            {
                LastEmptyCharacterHitConsumeSkipCountForDiagnostics++;
                return;
            }

            LastCharacterHitConsumeExecutedCountForDiagnostics++;
            entity.SimPostInteraction(tickIndex);
            if (world.IsActiveForCurrentPassInternal(entity))
                world.RefreshRuntimeSnapshotForModule(entity);
        }

        private void ConsumeDataOrientedCharacterHitParticipant(
            LF2Entity entity,
            int tickIndex,
            in CollisionCandidateRange candidates)
        {
            if (entity == null || !entity.SupportsPostInteractionPhase())
                return;
            if (entity.Runtime != null &&
                tickIndex < entity.Runtime.SuppressPostInteractionUntilTick)
            {
                return;
            }

            if (!ForceLegacyEmptyCharacterHitConsumeForDiagnostics &&
                candidates.Count == 0 &&
                entity.GetType() == typeof(LF2Character) &&
                entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
            {
                LastEmptyCharacterHitConsumeSkipCountForDiagnostics++;
                return;
            }

            LastCharacterHitConsumeExecutedCountForDiagnostics++;
            if (entity.TryGetBattleHitCandidateConsumer(
                    BattleHitExecutionPass.Character,
                    out IBattleHitCandidateConsumer consumer))
            {
                BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                    consumer,
                    in candidates);
            }

            if (world.IsActiveForCurrentPassInternal(entity))
                world.RefreshRuntimeSnapshotForModule(entity);
        }

        private bool CanSkipEmptyCharacterHitConsume(
            LF2Entity entity,
            bool runtimeCandidateCountGate)
        {
            if (entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                !entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
            {
                return false;
            }

            if (runtimeCandidateCountGate)
                return entity.Runtime.HitCandidateCount == 0;

            if (world.SceneQuery == null ||
                !world.SceneQuery.TryGetCollisionCandidateRange(
                    entity,
                    out CollisionCandidateRange candidates))
            {
                return false;
            }

            return candidates.Count == 0;
        }

        private void ConsumeObjectHitParticipant(
            LF2Entity entity,
            int tickIndex)
        {
            if (entity == null || !entity.SupportsObjectInteractionPhase())
                return;
            if (entity.Runtime != null &&
                tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
            {
                return;
            }

            LastObjectHitConsumeExecutedCountForDiagnostics++;
            entity.SimObjectInteraction(tickIndex);
            if (entity is LF2SpecialAttack)
                world.FlushQueuedObjectPointTasks();
            if (world.IsActiveForCurrentPassInternal(entity))
                world.RefreshRuntimeSnapshotForModule(entity);
        }

        private void ConsumeDataOrientedObjectHitParticipant(
            LF2Entity entity,
            int tickIndex,
            in CollisionCandidateRange candidates)
        {
            if (entity == null || !entity.SupportsObjectInteractionPhase())
                return;
            if (entity.Runtime != null &&
                tickIndex < entity.Runtime.SuppressObjectInteractionUntilTick)
            {
                return;
            }

            LastObjectHitConsumeExecutedCountForDiagnostics++;
            if (entity.TryGetBattleHitCandidateConsumer(
                    BattleHitExecutionPass.Object,
                    out IBattleHitCandidateConsumer consumer))
            {
                BattleHitCandidateSequenceRunner.TryConsumeCaptured(
                    consumer,
                    in candidates);
            }

            if (entity is LF2SpecialAttack)
                world.FlushQueuedObjectPointTasks();
            if (world.IsActiveForCurrentPassInternal(entity))
                world.RefreshRuntimeSnapshotForModule(entity);
        }

        private bool CanUseEmptyObjectInteractionProof()
        {
            foreach (LF2Entity entity in
                     world.ActiveEntitiesByRuntimeSlotForModule)
            {
                if (entity == null || !entity.SupportsObjectInteractionPhase())
                    continue;

                // These production shells either consume only the frozen candidate
                // range or have an empty object-interaction implementation. Derived
                // test/custom shells fail closed so virtual side effects are preserved.
                Type entityType = entity.GetType();
                if (entityType != typeof(LF2Character) &&
                    entityType != typeof(LF2SpecialAttack) &&
                    entityType != typeof(LF2Weapon) &&
                    entityType != typeof(LF2OtherObject))
                {
                    return false;
                }
            }

            return true;
        }

        private void ApplyWholePreInteractionNoOpDiagnostics(
            int participantCount)
        {
            LastPreInteractionWholePassProofSucceededForDiagnostics = true;
            LastPreInteractionWholePassParticipantCountForDiagnostics =
                participantCount;
            LastPreInteractionScannedCountForDiagnostics = participantCount;
            LastPreInteractionProofSkipCountForDiagnostics = participantCount * 3;
            LastPreInteractionSnapshotSkipCountForDiagnostics = participantCount * 3;
            LastPreInteractionCpointCheckProofSkipCountForDiagnostics =
                participantCount;
            LastPreInteractionMismatchTailProofSkipCountForDiagnostics =
                participantCount;
            LastPreInteractionHeldSyncProofSkipCountForDiagnostics =
                participantCount;
        }

        private static bool CanSkipCpointCheckParticipant(LF2Entity entity)
        {
            if (entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                !entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
            {
                return false;
            }

            LF2FrameData frame = entity.GetCollisionFrameData();
            return frame == null ||
                   !frame.TryGetPrimaryCatchPoint(
                       out BattleCatchPointValue cpoint) ||
                   cpoint.Kind != 1 ||
                   entity.FrameDelay < 0;
        }

        private static bool CanSkipCpointMismatchTailParticipant(
            LF2Entity entity)
        {
            if (entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                !entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
            {
                return false;
            }

            LF2FrameData frame = entity.Frame?.D;
            return frame == null ||
                   !frame.TryGetPrimaryCatchPoint(
                       out BattleCatchPointValue cpoint) ||
                   cpoint.Kind != 2;
        }

        private static bool CanSkipWeaponSyncHeldParticipant(LF2Entity entity)
        {
            if (entity == null ||
                entity.GetType() != typeof(LF2Character) ||
                !entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
            {
                return false;
            }

            var character = (LF2Character)entity;
            LF2FrameData frame = character.Frame?.D;
            if (frame != null &&
                frame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue cpoint) &&
                cpoint.Kind == 1 &&
                frame.state == LF2States.Catching)
            {
                return false;
            }

            NTSDEntityRuntime runtime = character.Runtime;
            return runtime.LinkState == 0 &&
                   runtime.TargetSlotIndex == -1 &&
                   runtime.HeldWeaponStableId == -1 &&
                   character.HeldWeaponReferenceInternal == null;
        }

        private bool TryProveWholePreInteractionPassNoOp(
            int tickIndex,
            out int participantCount)
        {
            participantCount = 0;
            int logicalCapacity =
                world.PreInteractionRuntimeSlotLogicalCapacityForModule;
            int claimedCount =
                world.PreInteractionClaimedRuntimeSlotCountForModule;
            ulong occupancyEpoch =
                world.PreInteractionRuntimeSlotOccupancyEpochForModule;
            long pendingDestroyEpoch =
                world.PreInteractionPendingDestroyEpochForModule;
            int pendingUnregisterCount =
                world.PreInteractionPendingUnregisterCountForModule;

            for (int runtimeSlot = 0;
                 runtimeSlot < logicalCapacity;
                 runtimeSlot++)
            {
                if (!world.TryGetRuntimeSlotReadOnlyView(
                        runtimeSlot,
                        out RuntimeSlotTable.ReadOnlySlotView view))
                {
                    return false;
                }
                if (!view.Claimed)
                {
                    if (view.Entity != null)
                        return false;
                    continue;
                }

                LF2Entity entity = view.Entity;
                if (entity == null ||
                    view.Generation == 0 ||
                    entity.Runtime == null ||
                    entity.Runtime.SlotIndex != runtimeSlot)
                {
                    return false;
                }

                var handle =
                    new RuntimeEntityHandle(runtimeSlot, view.Generation);
                if (!world.TryResolveRuntimeHandleForInteractionModule(
                        handle,
                        out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (entity.Runtime.SuppressPreInteractionUntilTick > tickIndex ||
                    !world.IsActiveForCurrentPassInternal(entity))
                {
                    continue;
                }

                participantCount++;
                if (!TryProveNeutralPreInteractionParticipant(entity))
                    return false;
            }

            return logicalCapacity ==
                       world.PreInteractionRuntimeSlotLogicalCapacityForModule &&
                   claimedCount ==
                       world.PreInteractionClaimedRuntimeSlotCountForModule &&
                   occupancyEpoch ==
                       world.PreInteractionRuntimeSlotOccupancyEpochForModule &&
                   pendingDestroyEpoch ==
                       world.PreInteractionPendingDestroyEpochForModule &&
                   pendingUnregisterCount ==
                       world.PreInteractionPendingUnregisterCountForModule;
        }

        private static bool TryProveNeutralPreInteractionParticipant(
            LF2Entity entity)
        {
            if (entity == null || entity.GetType() != typeof(LF2Character))
                return false;
            if (!entity.IsBaseRuntimeSnapshotCurrentForPreInteractionNoOp())
                return false;

            var character = (LF2Character)entity;
            NTSDEntityRuntime runtime = character.Runtime;
            if (runtime.LinkState != 0 ||
                runtime.TargetSlotIndex != -1 ||
                runtime.HeldWeaponStableId != -1 ||
                character.HeldWeaponReferenceInternal != null)
            {
                return false;
            }

            LF2FrameData collisionFrame = character.GetCollisionFrameData();
            if (collisionFrame != null &&
                collisionFrame.TryGetPrimaryCatchPoint(
                    out BattleCatchPointValue collisionCpoint) &&
                (collisionCpoint.Kind == 1 || collisionCpoint.Kind == 2))
            {
                return false;
            }

            LF2FrameData currentFrame = character.Frame?.D;
            return currentFrame == null ||
                   !currentFrame.TryGetPrimaryCatchPoint(
                       out BattleCatchPointValue currentCpoint) ||
                   (currentCpoint.Kind != 1 && currentCpoint.Kind != 2);
        }
    }
}
