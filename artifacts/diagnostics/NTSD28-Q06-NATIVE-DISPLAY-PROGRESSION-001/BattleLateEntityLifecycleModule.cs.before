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
                        if (ForceLegacyCommonNoOpGatesForDiagnostics)
                        {
                            obj.RunStateSpecialPreCollision();
                        }
                        else
                        {
                            obj.TryApplyNativeC25DefinitionTransition();
                        }
                        if (!world.IsActiveForCurrentPassInternal(obj))
                        {
                            detailDiagnostics?.EndPhase(
                                BattleTickDetailPhase.LateEntityStateSpecial);
                            continue;
                        }

                        SpawnState9996Children(
                            obj,
                            useNativeSynchronizedRandom:
                                !ForceLegacyCommonNoOpGatesForDiagnostics);
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
                    RefreshNativeComputerState(obj, runtimeSlot);
                    int canonicalAttackerRestBeforeFrameTick =
                        obj.AttackExempt;
                    int mirrorAttackerRestBeforeFrameTick =
                        obj.ItrRest?.Arest ?? 0;
                    // Alignment contract: NTSD28-B3-C25F-H-J-TIMER-OWNERS-001.
                    // This marker exists only across the atomic C25g -> C25h boundary.
                    bool renderPhaseTransitionArmedThisTick;
                    obj.BeginNativeC25FrameTickForWorldPass();
                    try
                    {
                        if (obj.Runtime == null ||
                            tickIndex >= obj.Runtime.SuppressLateFrameTickUntilTick)
                        {
                            if (!world.TryExecuteLateCharacterFrameTickForModule(obj))
                                obj.SimFrameTick(tickIndex);
                        }
                    }
                    finally
                    {
                        renderPhaseTransitionArmedThisTick =
                            obj.EndNativeC25FrameTickForWorldPass();
                    }
                    if (!world.IsActiveForCurrentPassInternal(obj))
                    {
                        detailDiagnostics?.EndPhase(
                            BattleTickDetailPhase.LateEntityFrameTick);
                        continue;
                    }
                    AdvanceNativeReactionAndStatusTail(
                        obj,
                        runtimeSlot,
                        renderPhaseTransitionArmedThisTick);
                    AdvanceNativeArmorRecovery(obj);
                    DecrementNativeAttackerRest(obj);
                    world.SyncAttackerRestMirrorAfterFrameTickForModule(
                        obj,
                        canonicalAttackerRestBeforeFrameTick,
                        mirrorAttackerRestBeforeFrameTick);
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

                    int previousActionBeforeCommit = obj.Frame?.Prev ?? 0;
                    if (obj.RunNativeC25State18BrokenWeaponParticles())
                    {
                        FlushQueuedObjectPointTasks(
                            ref opointFactory,
                            ref opointFactoryResolved);
                        if (!world.IsActiveForCurrentPassInternal(obj))
                            continue;
                    }
                    detailDiagnostics?.BeginPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);
                    obj.MirrorLatePrevFrame();
                    detailDiagnostics?.EndPhase(
                        BattleTickDetailPhase.LateEntityPrevFrameMirror);

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
                        CanSkipExactCharacterTail(
                            obj,
                            previousActionBeforeCommit))
                    {
                        LastTailNoOpSkipCountForDiagnostics++;
                    }
                    else
                    {
                        LastTailExecutedCountForDiagnostics++;
                        obj.RunLateTailAfterNativePreviousActionCommit(
                            previousActionBeforeCommit);
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

                    AdvanceNativeHealing(obj);

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

        private static void RefreshNativeComputerState(
            LF2Entity entity,
            int runtimeSlot)
        {
            if (entity?.Runtime == null ||
                runtimeSlot < 0 ||
                runtimeSlot >= 10 ||
                entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character)
            {
                return;
            }

            int state = entity.Frame?.D?.state ?? 0;
            if (state >= 7000 && state <= 7999)
                entity.Runtime.NativeComputerState1B8 = state - 7000;
        }

        private void AdvanceNativeReactionAndStatusTail(
            LF2Entity entity,
            int runtimeSlot,
            bool renderPhaseTransitionArmedThisTick)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null)
                return;

            int objectType = entity.GetCurrentDataObjectTypeForSimulation();
            bool bodySkipped = runtime.LinkState < 0 ||
                (entity.FrameDelay != 0 &&
                 objectType != (int)LF2ObjectType.SpecialAttack);
            if (!bodySkipped)
            {
                if (!renderPhaseTransitionArmedThisTick)
                {
                    if (entity.HitStun > 0)
                        entity.HitStun--;
                    else if (entity.HitStun < 0)
                        entity.HitStun++;
                }

                int state = entity.Frame?.D?.state ?? 0;
                if (objectType == (int)LF2ObjectType.Character &&
                    runtimeSlot >= 0 &&
                    runtimeSlot <= 19 &&
                    (entity.Health?.HP ?? 0) <= 0 &&
                    state == LF2States.Lying &&
                    runtime.HP2Orig > 1 &&
                    entity.HitStun < 1)
                {
                    entity.HitStun = 30;
                }

                DecrementPositive(ref runtime.Fall);
                DecrementPositive(ref runtime.Bdefend);
                DecrementPositive(ref runtime.HitConfirmEa);
            }

            // Alignment contract: NTSD28-B3-OID5152-PRODUCTION-SPLIT-001.
            world.AdvanceOid5152ReactionTimerForModule(entity);
            DecrementPositive(ref runtime.BoundState198);
            DecrementPositive(ref runtime.InputDoubleCost19C);
            DecrementPositive(ref runtime.HitResourceInjuryDouble1A0);
            DecrementPositive(ref runtime.MpRegenBonusTimer1A4);
            DecrementPositive(ref runtime.EffectiveMaxRegenDouble1A8);
            DecrementPositive(ref runtime.HpRegenDouble1AC);
            DecrementPositive(ref runtime.FullRestoreTimer1B0);
            DecrementPositive(ref runtime.InputCostWaived1B4);
            DecrementPositive(ref runtime.NativeComputerState1B8);
            DecrementPositive(ref runtime.NativeTimer1BC);

            if (!bodySkipped && (entity.Health?.HP ?? 0) > 0)
            {
                DecrementPositive(ref runtime.InputActionLock130);
                DecrementPositive(ref runtime.InputRemapState138);
                DecrementPositive(ref runtime.InputProxyCounter14C);
                DecrementPositive(ref runtime.WeakTimer12C);
                DecrementPositive(ref runtime.DelayTimer134);
                DecrementPositive(ref runtime.JoinTimer148);
                AdvanceNativePoison(entity, runtime);
            }

            if (runtime.JoinTimer148 <= 0 &&
                runtime.JoinOverrideActive170 == 1)
            {
                runtime.JoinOverrideActive170 = 0;
                entity.RelationTeam = runtime.JoinOriginalBattleGroup174;
            }

            if (runtime.InputProxyCounter14C <= 0 &&
                runtime.InputProxyEnabled17C == 1)
            {
                runtime.InputProxyEnabled17C = 0;
            }
        }

        private static void AdvanceNativePoison(
            LF2Entity entity,
            NTSDEntityRuntime runtime)
        {
            if (runtime.PoisonTimer120 <= 0)
                return;

            runtime.PoisonTimer120--;
            if (runtime.PoisonTimer120 <= 0 ||
                (runtime.PoisonTimer120 & 0x1f) != 0)
            {
                return;
            }

            int currentHp = entity.Health?.HP ?? 0;
            int poisonDamage = 0;
            if (runtime.PoisonType124 <= 1)
            {
                poisonDamage = runtime.PoisonStrength128;
            }
            else if (runtime.PoisonType124 <= 3)
            {
                poisonDamage = currentHp * runtime.PoisonStrength128 / 100;
            }
            else if (runtime.PoisonType124 <= 5)
            {
                poisonDamage = runtime.MPMax * runtime.PoisonStrength128 / 100;
            }

            runtime.InputHpConsumedTotal34C += poisonDamage;
            if (entity.Health == null)
                return;

            int nextHp = currentHp - poisonDamage;
            if (nextHp < 0)
                nextHp = (runtime.PoisonType124 & 1) != 0 ? 1 : 0;
            entity.Health.HP = nextHp;
        }

        private static void DecrementNativeAttackerRest(LF2Entity entity)
        {
            if (entity?.Runtime == null || entity.AttackExempt <= 0)
                return;

            int objectType = entity.GetCurrentDataObjectTypeForSimulation();
            if (entity.FrameDelay == 0 ||
                objectType == (int)LF2ObjectType.SpecialAttack)
            {
                entity.AttackExempt--;
            }
        }

        private static void AdvanceNativeArmorRecovery(LF2Entity entity)
        {
            NTSDEntityRuntime runtime = entity?.Runtime;
            if (runtime == null || runtime.ArmorRecoveryTimer11C < 0)
                return;

            int objectType = entity.GetCurrentDataObjectTypeForSimulation();
            bool bodyEligible = runtime.LinkState >= 0 &&
                (entity.FrameDelay == 0 ||
                 objectType == (int)LF2ObjectType.SpecialAttack);
            if (!bodyEligible)
                return;

            bool hasArmorBlock =
                entity.TryGetNativeArmorRecoveryProfileForWorldPass(
                    out int armorHp,
                    out int recover);
            BattleNativeArmorRecoveryKernel.Advance(
                ref runtime.RuntimeArmorHp118,
                ref runtime.ArmorRecoveryTimer11C,
                bodyEligible,
                hasArmorBlock,
                armorHp,
                recover);
        }

        private static void AdvanceNativeHealing(LF2Entity entity)
        {
            if (entity?.Runtime == null ||
                entity.Health == null ||
                entity.GetCurrentDataObjectTypeForSimulation() !=
                    (int)LF2ObjectType.Character ||
                entity.Health.HP <= 0)
            {
                return;
            }

            int hp = entity.Health.HP;
            int encodedTimer = entity.HealTimer;
            int ordinaryTimer = entity.CatchTimer;
            bool state1700 = (entity.Frame?.D?.state ?? 0) == 1700;
            BattleNativeHealingKernel.Advance(
                ref hp,
                entity.Health.HPBound,
                ref encodedTimer,
                ref ordinaryTimer,
                state1700);
            entity.Health.HP = hp;
            entity.HealTimer = encodedTimer;
            entity.CatchTimer = ordinaryTimer;
        }

        private static void DecrementPositive(ref int value)
        {
            if (value > 0)
                value--;
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
            {
                SpawnState9996Children(
                    entity,
                    useNativeSynchronizedRandom: false);
            }
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
            bool runsDefinitionTransition = state >= 8000 && state < 9000;
            bool runsState9996Writer =
                state == 9996 &&
                entity.GetCurrentDataObjectTypeForSimulation() ==
                    (int)LF2ObjectType.Character &&
                entity.AttackingCounter == 1;
            return !runsDefinitionTransition && !runsState9996Writer;
        }

        private void SpawnState9996Children(
            LF2Entity spawner,
            bool useNativeSynchronizedRandom)
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
                if (!CanMaterializeState9996Oid(
                        spawnOid,
                        out LF2CharacterDataWrapper targetWrapper))
                    continue;

                int spawnX = spawner.Runtime.XInt + NextState9996Random(
                    useNativeSynchronizedRandom,
                    0x0041F792u,
                    7) - 3;
                int spawnY = spawner.Runtime.YInt + NextState9996Random(
                    useNativeSynchronizedRandom,
                    0x0041F7B6u,
                    7) - 9;
                int spawnZ = spawner.Runtime.ZInt + 1;
                double spawnVy = -(NextState9996Random(
                    useNativeSynchronizedRandom,
                    0x0041F818u,
                    15) / 2) - 5.0;
                double spawnVz;
                if (spawnIndex == 1 || spawnIndex == 3)
                {
                    spawnVz = -3.0 - NextState9996Random(
                        useNativeSynchronizedRandom,
                        0x0041F87Fu,
                        2);
                }
                else if (spawnIndex == 4)
                    spawnVz = 1.0;
                else
                {
                    spawnVz = NextState9996Random(
                        useNativeSynchronizedRandom,
                        0x0041F8A9u,
                        2) + 3.0;
                }

                double spawnVx;
                if (spawnIndex >= 4)
                {
                    spawnVx = NextState9996Random(
                        useNativeSynchronizedRandom,
                        0x0041F92Eu,
                        7) - 3.0;
                }
                else if (spawnIndex >= 2)
                {
                    spawnVx = NextState9996Random(
                        useNativeSynchronizedRandom,
                        0x0041F908u,
                        3) + 10.0;
                }
                else
                {
                    spawnVx = -10.0 - NextState9996Random(
                        useNativeSynchronizedRandom,
                        0x0041F8DDu,
                        3);
                }

                int spawnFrame = NextState9996Random(
                    useNativeSynchronizedRandom,
                    0x0041F955u,
                    4);
                int spawnFacing = NextState9996Random(
                    useNativeSynchronizedRandom,
                    0x0041F96Bu,
                    2);
                if (!HasAuthoredFrame(targetWrapper, spawnFrame))
                    continue;

                OPointCreateTask task =
                    referencePool.Fetch<OPointCreateTask>();
                if (task == null)
                    break;

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
                spawned.SpawnerEntityIndex = useNativeSynchronizedRandom
                    ? -1
                    : spawnerSlot;
                spawned.Team = 0;
                spawned.RelationTeam = 0;
                spawned.OwnerId = -1;
                spawned.RelationOwnerSlot = -1;
                spawned.OwnerEntityIndex = -1;
                spawned.KillCount = -1;
                spawned.AttackExempt = 6;
                if (useNativeSynchronizedRandom && spawned.Health != null)
                {
                    spawned.Health.HP = 10;
                    spawned.Health.HPBound = 10;
                    spawned.Health.HP3 = 10;
                    spawned.Health.PP = 10;
                }
                world.ResetCooldownsForRuntimeSlot(freeSlot, spawned);
                spawned.RefreshRuntimeSnapshot();
            }
        }

        private int NextState9996Random(
            bool useNativeSynchronizedRandom,
            uint callSite,
            int upperBound)
        {
            return useNativeSynchronizedRandom
                ? world.NativeRandom.SynchronizedNext(callSite, upperBound)
                : world.Rng.NextInt(0, upperBound);
        }

        private bool CanMaterializeState9996Oid(
            int objectId,
            out LF2CharacterDataWrapper wrapper)
        {
            wrapper = world.RuntimeCharacterConfigs.Resolve(objectId);
            if (wrapper?.characterData == null)
                return false;

            return world.ResolveLateState9996ObjectDefinitionForModule(objectId) !=
                   null;
        }

        private static bool HasAuthoredFrame(
            LF2CharacterDataWrapper wrapper,
            int frameId)
        {
            List<LF2FrameData> frames = wrapper?.characterData?.frames;
            if (frames == null)
                return false;

            for (int index = 0; index < frames.Count; index++)
            {
                LF2FrameData frame = frames[index];
                if (frame != null && frame.frameId == frameId)
                    return true;
            }

            return false;
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

        private static bool CanSkipExactCharacterTail(
            LF2Entity entity,
            int previousActionBeforeCommit)
        {
            if (entity == null || entity.GetType() != typeof(LF2Character))
                return false;

            NTSDEntityRuntime runtime = entity.Runtime;
            if (runtime == null || runtime.SlotIndex < 10)
                return false;

            LF2FrameInfo frame = entity.Frame;
            if (frame == null)
                return true;

            LF2FrameData previousFrame = entity.GetFrameDataById(
                previousActionBeforeCommit);
            LF2FrameData currentFrame = frame.D;
            if (previousFrame == null || currentFrame == null)
                return true;

            int previousState = previousFrame.state;
            int currentState = currentFrame.state;
            bool transitionBranch1 =
                (previousState == 13 || previousActionBeforeCommit == 200) &&
                currentState != 13 &&
                frame.N != 200;
            return !transitionBranch1;
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
                // Alignment contract: NTSD28-B6-HELD-NEGATIVE-FRAME-LIFECYCLE-GUARD-PRODUCTION-001.
                if (world.IsActiveForCurrentPassInternal(entity) &&
                    entity.Runtime != null && entity.Runtime.LinkState < 0)
                    return true;

                entity.FreeEntityLikeExe();
                return true;
            }

            return false;
        }
    }

    internal static class BattleNativeArmorRecoveryKernel
    {
        internal static void Advance(
            ref int runtimeArmorHp,
            ref int armorRecoveryTimer,
            bool bodyEligible,
            bool hasArmorBlock,
            int profileArmorHp,
            int profileRecover)
        {
            if (armorRecoveryTimer < 0 || !bodyEligible)
                return;

            if (armorRecoveryTimer == 0)
            {
                Reload(
                    ref runtimeArmorHp,
                    ref armorRecoveryTimer,
                    hasArmorBlock,
                    profileArmorHp,
                    profileRecover);
                return;
            }

            if (runtimeArmorHp > 0)
                return;

            armorRecoveryTimer--;
            if (armorRecoveryTimer == 0)
            {
                Reload(
                    ref runtimeArmorHp,
                    ref armorRecoveryTimer,
                    hasArmorBlock,
                    profileArmorHp,
                    profileRecover);
            }
        }

        private static void Reload(
            ref int runtimeArmorHp,
            ref int armorRecoveryTimer,
            bool hasArmorBlock,
            int profileArmorHp,
            int profileRecover)
        {
            if (!hasArmorBlock || profileArmorHp == 0)
            {
                armorRecoveryTimer = -1;
                return;
            }

            runtimeArmorHp = profileArmorHp;
            armorRecoveryTimer = profileRecover > 0 ? profileRecover : -1;
        }
    }

    internal static class BattleNativeHealingKernel
    {
        internal static void Advance(
            ref int hp,
            int effectiveMaxHp,
            ref int encodedTimer,
            ref int ordinaryTimer,
            bool state1700)
        {
            if (encodedTimer / 1000 == 1)
            {
                encodedTimer--;
                bool completed = false;
                if (encodedTimer % 8 == 0)
                {
                    if (hp < effectiveMaxHp)
                    {
                        hp = System.Math.Min(hp + 8, effectiveMaxHp);
                    }
                    else
                    {
                        encodedTimer = 0;
                        completed = true;
                    }
                }

                if (!completed && encodedTimer % 1000 == 0)
                    encodedTimer = 0;
            }

            if (ordinaryTimer > 0)
            {
                ordinaryTimer--;
                if (ordinaryTimer % 8 == 0 && hp < effectiveMaxHp)
                {
                    hp = System.Math.Min(hp + 8, effectiveMaxHp);
                    if (hp >= effectiveMaxHp)
                        ordinaryTimer = 0;
                }
            }

            if (state1700)
                encodedTimer = 1100;
        }
    }
}
