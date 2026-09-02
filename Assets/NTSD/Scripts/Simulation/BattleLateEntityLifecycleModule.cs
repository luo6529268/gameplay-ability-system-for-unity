using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation.Ecs;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the late entity lifecycle pass, including late state specials,
    /// OPoint materialization boundaries, cleanup, tail and snapshot diagnostics.
    /// </summary>
    internal sealed class BattleLateEntityLifecycleModule
    {
        private readonly SimulationWorld world;
        private readonly List<LF2Entity> entityScratch =
            new List<LF2Entity>(16);

        internal BattleLateEntityLifecycleModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal bool ForceLegacyTailNoOpForDiagnostics { get; set; } = true;
        internal bool ForceLegacyCommonNoOpGatesForDiagnostics { get; set; }
        internal BattleLateRuntimeSnapshotMode RuntimeSnapshotModeForDiagnostics
        {
            get;
            set;
        } = BattleLateRuntimeSnapshotMode.ConsolidatedFinal;

        internal int LastTailNoOpSkipCountForDiagnostics { get; private set; }
        internal int LastTailExecutedCountForDiagnostics { get; private set; }
        internal int LastOpointFactoryResolveCountForDiagnostics { get; private set; }
        internal int LastOpointFlushCountForDiagnostics { get; private set; }
        internal int LastStateSpecialNoOpSkipCountForDiagnostics { get; private set; }
        internal int LastRecoveryNoOpSkipCountForDiagnostics { get; private set; }
        internal int LastDeathOpointNoOpSkipCountForDiagnostics { get; private set; }
        internal int LastCleanupNoOpSkipCountForDiagnostics { get; private set; }

        internal void PrepareCapacity(int entityCapacity)
        {
            if (entityScratch.Capacity < entityCapacity)
                entityScratch.Capacity = entityCapacity;
        }

        internal void Run(int tickIndex)
        {
            LastTailNoOpSkipCountForDiagnostics = 0;
            LastTailExecutedCountForDiagnostics = 0;
            LastOpointFactoryResolveCountForDiagnostics = 0;
            LastOpointFlushCountForDiagnostics = 0;
            LastStateSpecialNoOpSkipCountForDiagnostics = 0;
            LastRecoveryNoOpSkipCountForDiagnostics = 0;
            LastDeathOpointNoOpSkipCountForDiagnostics = 0;
            LastCleanupNoOpSkipCountForDiagnostics = 0;
            BattleTickDetailPhaseDiagnostics detailDiagnostics =
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics;
            // The production object-point factory is pass-stable. Resolve it lazily so an
            // empty LateEntityUpdateAll invocation retains the existing no-auto-create behavior.
            IBattleObjectPointStructuralMaterializer opointFactory = null;
            bool opointFactoryResolved = false;
            if (world.HasLateEntityStructuralEventSinkForModule)
            {
                world.BeginLateEntityStructuralEventContextForModule(tickIndex);
            }
            world.BeginDeferredEntityMutationPass();
            try
            {
                for (int runtimeSlot = 0;
                     runtimeSlot < world.RuntimeSlotCapacity;
                     runtimeSlot++)
                {
                    LF2Entity obj =
                        world.FindEntityByRuntimeSlotCurrentForLateModule(
                            runtimeSlot);

                    if (obj == null)
                        continue;

                    if (world.HasLateEntityStructuralEventSinkForModule)
                    {
                        world.EmitLateEntityStructuralScanForModule(
                            runtimeSlot,
                            obj);
                    }

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);
                    if (CanSkipExactCharacterStateSpecial(obj))
                    {
                        LastStateSpecialNoOpSkipCountForDiagnostics++;
                    }
                    else
                    {
                        obj.RunStateSpecialPreCollision();
                        if (!world.IsActiveForCurrentPassInternal(obj))
                        {
                            detailDiagnostics?.EndPhase(
                                BattleTickDetailPhase.LateEntityStateSpecial);
                            continue;
                        }

                        SpawnState9996Children(obj);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityStateSpecial);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityRecovery);
                    BattleEcsCharacterRecoveryResult recoveryResult =
                        ForceLegacyCommonNoOpGatesForDiagnostics
                            ? BattleEcsCharacterRecoveryResult.CompatibilityFallback
                            : world.ExecuteLateCharacterRecoveryForModule(
                                obj,
                                tickIndex);
                    if (recoveryResult ==
                        BattleEcsCharacterRecoveryResult.ProvenNoOp)
                    {
                        LastRecoveryNoOpSkipCountForDiagnostics++;
                    }
                    else if (recoveryResult ==
                             BattleEcsCharacterRecoveryResult.CompatibilityFallback)
                    {
                        obj.RunPreCollisionRecoveryPhase(tickIndex);
                        if (!world.IsActiveForCurrentPassInternal(obj))
                        {
                            detailDiagnostics?.EndPhase(
                                BattleTickDetailPhase.LateEntityRecovery);
                            continue;
                        }
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityRecovery);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);
                    if (obj.Runtime == null ||
                        tickIndex >= obj.Runtime.SuppressLateFrameTickUntilTick)
                    {
                        if (!world.TryExecuteLateCharacterFrameTickForModule(obj))
                            obj.SimFrameTick(tickIndex);
                    }
                    if (!world.IsActiveForCurrentPassInternal(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameTick);
                        continue;
                    }
                    if (RuntimeSnapshotModeForDiagnostics ==
                        BattleLateRuntimeSnapshotMode.LegacyThree)
                    {
                        RefreshRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.FrameTick,
                            detailDiagnostics);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameTick);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);
                    bool exitedLateFrameTick = HandleFrameTickExit(
                        obj,
                        detailDiagnostics);
                    if (exitedLateFrameTick)
                    {
                        if (obj is LF2SpecialAttack)
                        {
                            FlushQueuedObjectPointTasks(
                                ref opointFactory,
                                ref opointFactoryResolved);
                        }
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameExit);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityFrameExit);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);
                    if (CanSkipExactCharacterDeathOpoint(obj))
                    {
                        LastDeathOpointNoOpSkipCountForDiagnostics++;
                    }
                    else
                    {
                        obj.RunLateDeathOpointPreCleanupPhase();
                        if (!world.IsActiveForCurrentPassInternal(obj))
                        {
                            detailDiagnostics?.EndPhase(
                                BattleTickDetailPhase.LateEntityDeathOpoint);
                            continue;
                        }
                    }
                    if (RuntimeSnapshotModeForDiagnostics ==
                        BattleLateRuntimeSnapshotMode.LegacyThree)
                    {
                        RefreshRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.DeathOpoint,
                            detailDiagnostics);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityDeathOpoint);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);
                    LF2FrameData opointFrame = obj.Frame?.D;
                    bool frameHasOpoint = opointFrame != null &&
                        ((opointFrame.opoints != null &&
                          opointFrame.opoints.Count > 0) ||
                         opointFrame.opoint.HasValue);
                    if (frameHasOpoint && !opointFactoryResolved)
                    {
                        opointFactory =
                            world.ResolveLateObjectPointStructuralMaterializerForModule();
                        opointFactoryResolved = true;
                        LastOpointFactoryResolveCountForDiagnostics++;
                    }
                    bool processedOpoint = false;
                    if (opointFactory != null && frameHasOpoint)
                    {
                        world.StructuralWriter.ProcessLateOpointSegment(
                            opointFactory,
                            obj,
                            tickIndex);
                        processedOpoint = true;
                    }
                    if (processedOpoint &&
                        !world.IsActiveForCurrentPassInternal(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityOpointProcess);
                        continue;
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityOpointProcess);

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    bool completedLateCleanup;
                    if (CanSkipExactCharacterCleanup(obj))
                    {
                        LastCleanupNoOpSkipCountForDiagnostics++;
                        completedLateCleanup = false;
                    }
                    else
                    {
                        completedLateCleanup =
                            obj.TryRunLatePostOpointCleanupPhase();
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityCleanup);
                    if (completedLateCleanup)
                    {
                        detailDiagnostics?.BeginPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        FlushQueuedObjectPointTasks(
                            ref opointFactory,
                            ref opointFactoryResolved);
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    if (!ForceLegacyTailNoOpForDiagnostics &&
                        CanSkipExactCharacterTail(obj))
                    {
                        LastTailNoOpSkipCountForDiagnostics++;
                    }
                    else
                    {
                        LastTailExecutedCountForDiagnostics++;
                        obj.RunLateTailBeforePrevFrame();
                    }
                    FlushQueuedObjectPointTasks(
                        ref opointFactory,
                        ref opointFactoryResolved);
                    if (!world.IsActiveForCurrentPassInternal(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                        continue;
                    }

                    if (RuntimeSnapshotModeForDiagnostics ==
                            BattleLateRuntimeSnapshotMode.LegacyThree ||
                        obj.RequiresRuntimeSnapshotAfterLateEntityUpdate())
                    {
                        RefreshRuntimeSnapshot(
                            obj,
                            BattleLateRuntimeSnapshotStage.TailAndQueuedFlush,
                            detailDiagnostics);
                    }
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityTailAndQueuedFlush);
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                    obj.MirrorLatePrevFrame();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                }
            }
            finally
            {
                world.EndLateEntityMutationTickingForModule();
                if (world.HasLateEntityStructuralEventSinkForModule)
                    world.EndLateEntityStructuralEventContextForModule();
                detailDiagnostics?.BeginPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
                world.FlushLateEntityPendingMutationsForModule();
                detailDiagnostics?.EndPhase(
                    BattleTickDetailPhase.LateEntityFinalPendingFlush);
            }
        }

        internal void RunStateSpecialPreCollisionForSelfCheck(
            LF2Entity entity)
        {
            if (entity == null ||
                !world.IsActiveForCurrentPassInternal(entity))
            {
                return;
            }

            entity.RunStateSpecialPreCollision();
            if (world.IsActiveForCurrentPassInternal(entity))
                SpawnState9996Children(entity);
        }

        internal void RefreshTransitionRuntimeSnapshot(LF2Entity entity)
        {
            if (RuntimeSnapshotModeForDiagnostics ==
                BattleLateRuntimeSnapshotMode.ConsolidatedFinal)
            {
                return;
            }

            RefreshRuntimeSnapshot(
                entity,
                BattleLateRuntimeSnapshotStage.TransitionInternal,
                world.ActiveBattleTickDetailPhaseDiagnosticsForDiagnostics);
        }

        private bool CanSkipExactCharacterStateSpecial(LF2Entity entity)
        {
            if (ForceLegacyCommonNoOpGatesForDiagnostics ||
                entity?.GetType() != typeof(LF2Character))
            {
                return false;
            }

            int state = entity.Frame?.D?.state ?? -1;
            bool runsState9996Writer =
                state == 9996 &&
                entity.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character &&
                entity.AttackingCounter == 1;
            return state != 9995 &&
                   (state < 4000 || state >= 5000) &&
                   (state < 8000 || state >= 9000) &&
                   !runsState9996Writer;
        }

        private void SpawnState9996Children(LF2Entity spawner)
        {
            if (spawner?.Frame?.D?.state != 9996 ||
                spawner.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                spawner.AttackingCounter != 1)
            {
                return;
            }

            ILF2ObjectPointFactory factory =
                world.ResolveObjectPointFactoryForSimulation();
            BattleLogicReferencePool referencePool = world.LogicReferencePool;
            if (factory == null || referencePool == null)
                return;

            int spawnerSlot = spawner.Runtime?.SlotIndex ?? -1;
            for (int spawnIndex = 0; spawnIndex < 5; spawnIndex++)
            {
                int freeSlot = world.FindFirstFreeRuntimeSlotForModule(
                    world.DynamicRuntimeSlotStartForServices,
                    world.RuntimeSlotCapacity);
                if (freeSlot < 0)
                    break;

                int spawnOid = spawnIndex == 4 ? 218 : 217;
                if (!CanMaterializeState9996Oid(spawnOid))
                    continue;

                OPointCreateTask task =
                    referencePool.Fetch<OPointCreateTask>();
                if (task == null)
                    break;

                int spawnX = spawner.Runtime.XInt + world.Rng.NextInt(0, 7) - 3;
                int spawnY = spawner.Runtime.YInt + world.Rng.NextInt(0, 7) - 9;
                int spawnZ = spawner.Runtime.ZInt + 1;
                double spawnVy = -(world.Rng.NextInt(0, 15) / 2) - 5.0;
                double spawnVz;
                if (spawnIndex == 1 || spawnIndex == 3)
                    spawnVz = -3.0 - world.Rng.NextInt(0, 2);
                else if (spawnIndex == 4)
                    spawnVz = 1.0;
                else
                    spawnVz = world.Rng.NextInt(0, 2) + 3.0;

                double spawnVx;
                if (spawnIndex >= 4)
                    spawnVx = world.Rng.NextInt(0, 7) - 3.0;
                else if (spawnIndex >= 2)
                    spawnVx = world.Rng.NextInt(0, 3) + 10.0;
                else
                    spawnVx = -10.0 - world.Rng.NextInt(0, 3);

                int spawnFrame = world.Rng.NextInt(0, 4);
                int spawnFacing = world.Rng.NextInt(0, 2);
                task.opoint = new ObjectPoint
                {
                    oid = spawnOid,
                    kind = 0,
                    action = spawnFrame,
                    facing = spawnFacing,
                };
                task.parent = null;
                task.targetWorld = world;
                task.team = 0;
                task.relationTeam = 0;
                task.holderCopySlot = 99;
                task.dir = "right";
                task.requiredRuntimeSlot = freeSlot;
                task.preserveActionZero = true;
                task.skipPostInitZOffset = true;
                task.useDirectRuntimePosition = true;
                task.directX = spawnX;
                task.directY = spawnY;
                task.directZ = spawnZ;
                task.useInitialRuntimeIntPosition = true;
                task.initialRuntimeX = spawnX;
                task.initialRuntimeY = spawnY;
                task.initialRuntimeZ = spawnZ;
                task.useDirectVelocity = true;
                task.directVx = spawnVx;
                task.directVy = spawnVy;
                task.directVz = spawnVz;
                task.attackExempt = 6;

                LF2Entity spawned;
                try
                {
                    spawned = factory.CreateObjectImmediate(task);
                }
                finally
                {
                    referencePool.Recycle(task);
                }

                if (spawned == null ||
                    spawned.Runtime?.SlotIndex != freeSlot)
                {
                    break;
                }

                // Alignment contract: R7-LATE-001. This branch is a direct
                // Entity::reset/init writer, not a relation-inheriting opoint.
                spawned.SpawnerEntityIndex = spawnerSlot;
                spawned.Team = 0;
                spawned.RelationTeam = 0;
                spawned.OwnerId = -1;
                spawned.RelationOwnerSlot = -1;
                spawned.OwnerEntityIndex = -1;
                spawned.HolderCopySlot = 99;
                spawned.KillCount = -1;
                spawned.AttackExempt = 6;
                world.ResetCooldownsForRuntimeSlot(freeSlot, spawned);
                spawned.RefreshRuntimeSnapshot();
            }
        }

        private bool CanMaterializeState9996Oid(int objectId)
        {
            LF2CharacterDataWrapper wrapper =
                world.RuntimeCharacterConfigs.Resolve(objectId);
            if (wrapper?.characterData == null)
                return false;

            return world.ResolveLateState9996ObjectDefinitionForModule(objectId) !=
                   null;
        }

        private bool CanSkipExactCharacterDeathOpoint(LF2Entity entity)
        {
            if (ForceLegacyCommonNoOpGatesForDiagnostics ||
                entity?.GetType() != typeof(LF2Character))
            {
                return false;
            }

            return entity.GetCurrentDataObjectTypeForSimulation() !=
                       (int)LF2ObjectType.Character ||
                   entity.Health == null ||
                   entity.Health.HP > 0 ||
                   entity.Runtime == null;
        }

        private bool CanSkipExactCharacterCleanup(LF2Entity entity)
        {
            if (ForceLegacyCommonNoOpGatesForDiagnostics ||
                entity?.GetType() != typeof(LF2Character))
            {
                return false;
            }

            return entity.GetCurrentDataObjectTypeForSimulation() ==
                       (int)LF2ObjectType.Character ||
                   entity.Runtime == null ||
                   entity.Runtime.WeaponFlightCounter >= 0;
        }

        private void FlushQueuedObjectPointTasks(
            ref IBattleObjectPointStructuralMaterializer opointFactory,
            ref bool opointFactoryResolved)
        {
            LastOpointFlushCountForDiagnostics++;
            if (!opointFactoryResolved)
            {
                opointFactory =
                    world.ResolveLateObjectPointStructuralMaterializerForModule();
                opointFactoryResolved = true;
                LastOpointFactoryResolveCountForDiagnostics++;
            }

            opointFactory?.FlushTasks();
        }

        private static bool CanSkipExactCharacterTail(LF2Entity entity)
        {
            if (entity == null || entity.GetType() != typeof(LF2Character))
                return false;

            NTSDEntityRuntime runtime = entity.Runtime;
            if (runtime == null || runtime.SlotIndex < 10)
                return false;

            LF2FrameInfo frame = entity.Frame;
            if (frame == null)
                return true;

            LF2FrameData previousFrame = entity.GetFrameDataById(frame.Prev);
            LF2FrameData currentFrame = frame.D;
            if (previousFrame == null || currentFrame == null)
                return true;

            int previousState = previousFrame.state;
            int currentState = currentFrame.state;
            bool transitionBranch1 =
                (previousState == 13 || frame.Prev == 200) &&
                currentState != 13 &&
                frame.N != 200;
            bool transitionBranch2 =
                previousState == 18 || previousState == 19;
            return !transitionBranch1 && !transitionBranch2;
        }

        private void RefreshRuntimeSnapshot(
            LF2Entity entity,
            BattleLateRuntimeSnapshotStage stage,
            BattleTickDetailPhaseDiagnostics diagnostics)
        {
            if (diagnostics == null)
            {
                world.RefreshRuntimeSnapshotForModule(entity);
                return;
            }

            diagnostics.BeginLateRuntimeSnapshot(stage);
            try
            {
                world.RefreshRuntimeSnapshotForModule(entity);
            }
            finally
            {
                diagnostics.EndLateRuntimeSnapshot(stage);
            }
        }

        private bool HandleFrameTickExit(
            LF2Entity entity,
            BattleTickDetailPhaseDiagnostics diagnostics)
        {
            if (entity?.Frame == null)
                return false;

            int frameId = entity.Frame.N;
            int frameGroup = frameId / 100;
            if (frameGroup == 11 || frameGroup == 12)
            {
                int ownerSlot = world.GetRuntimeSlotOrderForLateModule(entity);
                world.GetAllEntities(entityScratch);
                for (int i = 0; i < entityScratch.Count; i++)
                {
                    LF2Entity other = entityScratch[i];
                    if (other != null && other.KillCount == ownerSlot)
                        other.HitStun = 1100 - frameId;
                }

                entityScratch.Clear();
                entity.HitStun = 1100 - frameId;
                entity.DirectWriteFramePreserveWaitCounter(0);
                RefreshRuntimeSnapshot(
                    entity,
                    BattleLateRuntimeSnapshotStage.FrameExit,
                    diagnostics);
                return true;
            }

            if (frameId < 0 ||
                frameId >= LF2FrameCache.MaxFrameIdExclusive)
            {
                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }
    }
}
