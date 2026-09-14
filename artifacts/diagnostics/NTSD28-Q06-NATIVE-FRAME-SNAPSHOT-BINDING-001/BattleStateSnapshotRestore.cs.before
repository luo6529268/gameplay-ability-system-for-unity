using System;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.Rendering;

namespace NTSD.Simulation
{
    public enum BattleStateSnapshotRestoreFailure
    {
        None = 0,
        InvalidSnapshot = 1,
        IdentityMismatch = 2,
        WorldBusy = 3,
        WorldConfigurationMismatch = 4,
        RuntimeSlotMismatch = 5,
        EntityIdentityMismatch = 6,
        EntityPayloadMismatch = 7,
        EntityShellMismatch = 8,
        RelationshipMismatch = 9,
        RuntimeStateRestoreFailed = 10,
        RestStateRestoreFailed = 11,
        PendingEventRestoreFailed = 12,
        DerivedStateRebuildFailed = 13,
        EntityBaseShellRestoreFailed = 14,
        LivingShellRestoreFailed = 15,
        CharacterShellRestoreFailed = 16,
        WeaponShellRestoreFailed = 17,
        SpecialOtherShellRestoreFailed = 18,
        EntityShellPresenceMismatch = 19,
        EntityBaseShellValidationFailed = 20,
        RequiredRuntimeSlotMismatch = 21,
        CurrentFrameDataUnavailable = 22,
        CollisionFrameDataUnavailable = 23,
        TrackerParentUnavailable = 24,
        TopologyShellUnavailable = 25,
        TopologyRestoreFailed = 26,
    }

    internal sealed class BattleStateSnapshotRestoreModule
    {
        private readonly SimulationWorld world;

        internal BattleStateSnapshotRestoreModule(SimulationWorld world)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
        }

        private RuntimeSlotTable _runtimeSlots =>
            world.RuntimeSlotTableForModules;
        private RuntimeRestStore _runtimeRestStore =>
            world.RuntimeRestStoreForServices;
        private BattleRuntimeProfile activeRuntimeProfile =>
            world.RuntimeProfileForServices;
        private int RuntimeSlotCapacity => world.RuntimeSlotCapacity;
        private CollisionBroadphaseBackend CollisionBroadphaseForServices =>
            world.CollisionBroadphaseForServices;
        private SimulationBattleBufferModule battleBuffers =>
            world.BattleBuffersForServices;
        private SimulationObjectBucketRegistry objectBucketRegistry =>
            world.ObjectBucketRegistryForSnapshotRestore;
        private BattleLogicEntityFactory logicEntityFactory =>
            world.LogicEntityFactory;
        private BattleRuntimeDataCatalog RuntimeDataCatalog =>
            world.RuntimeDataCatalog;
        private BattleRuntimeState Runtime => world.Runtime;
        private DeterministicRng Rng => world.Rng;
        private NTSD28NativeRandom NativeRandom => world.NativeRandom;
        private int ObjectCount => world.ObjectCount;

        internal bool TryRestoreBattleStateSnapshot(
            Lockstep.LockstepSessionIdentity identity,
            BattleStateSnapshotBuffer snapshot,
            out BattleStateSnapshotRestoreFailure failure)
        {
            if (!ValidateBattleStateSnapshotRestore(
                    identity,
                    snapshot,
                    out failure))
            {
                return false;
            }

            // A transferred bootstrap snapshot can target a freshly constructed
            // world whose rest storage has not entered the battle phase yet.
            // Running worlds are already prepared, so this remains allocation-free
            // on the warm restore path.
            _runtimeRestStore.PrepareForBattle();

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                    continue;

                LF2Entity entity = view.Entity;
                if (entity.ItrRest?.IsBound == true &&
                    !entity.ItrRest.Unbind(false))
                {
                    failure = BattleStateSnapshotRestoreFailure.RestStateRestoreFailed;
                    return false;
                }
            }

            if (!RestoreSnapshotTopology(snapshot))
            {
                failure = BattleStateSnapshotRestoreFailure.TopologyRestoreFailed;
                return false;
            }

            if (!RestoreCoreScalarState(snapshot.Core) ||
                !snapshot.RosterResults.TryRestoreTo(Runtime) ||
                !snapshot.StageSpawn.TryRestoreTo(Runtime))
            {
                failure = BattleStateSnapshotRestoreFailure.RuntimeStateRestoreFailed;
                return false;
            }

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                // Alignment contract: NTSD28-Q05-UNCLAIMED-RAW-RUNTIME-RESTORE-001
                // Raw payload ownership is independent of whether the slot currently has an entity.
                if (snapshot.EntityRuntime.HasRawRuntime(runtimeSlot))
                {
                    NTSDEntityRuntime raw = view.RawRuntime ?? _runtimeSlots.GetRawRuntime(runtimeSlot);
                    if (!snapshot.EntityRuntime.TryCopyRawRuntime(runtimeSlot, raw))
                    {
                        failure = BattleStateSnapshotRestoreFailure.EntityPayloadMismatch;
                        return false;
                    }
                }
                else
                {
                    view.RawRuntime?.Reset();
                }

                if (view.Claimed && !snapshot.EntityRuntime.TryCopyEntityRuntime(
                        runtimeSlot, view.Entity.Runtime))
                {
                    failure = BattleStateSnapshotRestoreFailure.EntityPayloadMismatch;
                    return false;
                }
            }

            if (!BattleWorldRestSnapshotModule.TryRestore(
                    _runtimeRestStore,
                    snapshot.Rest))
            {
                failure = BattleStateSnapshotRestoreFailure.RestStateRestoreFailed;
                return false;
            }

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                    continue;

                LF2Entity entity = view.Entity;
                BattleEntityBaseShellSnapshot baseState =
                    snapshot.EntityBaseShell.GetState(runtimeSlot);
                if (!TryResolveSnapshotHandle(
                        baseState.TrackerParentHandle,
                        out LF2Entity trackerParent) ||
                    !entity.TryRestoreBaseShellForSnapshot(
                        baseState,
                        snapshot.EntityBaseShell,
                        runtimeSlot,
                        trackerParent))
                {
                    failure = BattleStateSnapshotRestoreFailure.EntityBaseShellRestoreFailed;
                    return false;
                }

                if (entity is LF2LivingObject living)
                {
                    BattleLivingShellSnapshot livingState =
                        snapshot.LivingShell.GetState(runtimeSlot);
                    if (!TryResolveSnapshotHandle(
                            livingState.CatchingHandle,
                            out LF2LivingObject catching) ||
                        !TryResolveSnapshotHandle(
                            livingState.AttackerHandle,
                            out LF2LivingObject attacker) ||
                        !living.TryRestoreLivingShellForSnapshot(
                            livingState,
                            catching,
                            attacker))
                    {
                        failure = BattleStateSnapshotRestoreFailure.LivingShellRestoreFailed;
                        return false;
                    }
                }

                if (entity is LF2Character character)
                {
                    BattleCharacterShellSnapshot characterState =
                        snapshot.CharacterShell.GetState(runtimeSlot);
                    if (!TryResolveSnapshotHandle(
                            characterState.HeldWeaponHandle,
                            out LF2Entity heldEntity) ||
                        (heldEntity != null && heldEntity is not ILF2Object) ||
                        !character.TryRestoreCharacterShellForSnapshot(
                            characterState,
                            heldEntity as ILF2Object))
                    {
                        failure = BattleStateSnapshotRestoreFailure.CharacterShellRestoreFailed;
                        return false;
                    }
                }

                if (entity is LF2WeaponBase weapon &&
                    !weapon.TryRestoreWeaponShellForSnapshot(
                        snapshot.WeaponShell.GetState(runtimeSlot)))
                {
                    failure = BattleStateSnapshotRestoreFailure.WeaponShellRestoreFailed;
                    return false;
                }

                if (entity is LF2SpecialAttack special)
                {
                    BattleSpecialOtherShellSnapshot specialState =
                        snapshot.SpecialOtherShell.GetState(runtimeSlot);
                    if (!TryResolveSnapshotHandle(
                            specialState.ParentHandle,
                            out LF2LivingObject parent) ||
                        !special.TryRestoreSpecialShellForSnapshot(
                            specialState,
                            parent))
                    {
                        failure = BattleStateSnapshotRestoreFailure.SpecialOtherShellRestoreFailed;
                        return false;
                    }
                }
                else if (entity is LF2OtherObject other &&
                         !other.TryRestoreOtherShellForSnapshot(
                             snapshot.SpecialOtherShell.GetState(runtimeSlot)))
                {
                    failure = BattleStateSnapshotRestoreFailure.SpecialOtherShellRestoreFailed;
                    return false;
                }
            }

            if (!RebuildDerivedStateAfterSnapshotRestore())
            {
                failure = BattleStateSnapshotRestoreFailure.DerivedStateRebuildFailed;
                return false;
            }
            if (!snapshot.PendingEvents.TryRestoreTo(battleBuffers))
            {
                failure = BattleStateSnapshotRestoreFailure.PendingEventRestoreFailed;
                return false;
            }
            if (ObjectCount != snapshot.Core.ObjectCount)
            {
                failure = BattleStateSnapshotRestoreFailure.DerivedStateRebuildFailed;
                return false;
            }

            failure = BattleStateSnapshotRestoreFailure.None;
            return true;
        }

        private bool ValidateBattleStateSnapshotRestore(
            Lockstep.LockstepSessionIdentity identity,
            BattleStateSnapshotBuffer snapshot,
            out BattleStateSnapshotRestoreFailure failure)
        {
            if (snapshot == null || !snapshot.IsValid ||
                snapshot.SchemaVersion != BattleStateSnapshotBuffer.CurrentSchemaVersion)
            {
                failure = BattleStateSnapshotRestoreFailure.InvalidSnapshot;
                return false;
            }
            if (identity == null ||
                snapshot.ProtocolSchemaVersion != identity.SchemaVersion ||
                snapshot.IdentityFingerprint != identity.IdentityFingerprint ||
                snapshot.Core.ProtocolSchemaVersion != identity.SchemaVersion ||
                snapshot.Core.IdentityFingerprint != identity.IdentityFingerprint)
            {
                failure = BattleStateSnapshotRestoreFailure.IdentityMismatch;
                return false;
            }
            if (!world.IsBattleSnapshotBoundaryReady)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldBusy;
                return false;
            }

            BattleWorldCoreScalarSnapshot core = snapshot.Core;
            for (int slot = 0; slot < RuntimeSlotCapacity; slot++)
            {
                LF2Entity current = _runtimeSlots.GetReadOnlyView(slot).Entity;
                if (current != null &&
                    (!ReferenceEquals(current.Renderer, null) || !ReferenceEquals(current.ShadowRenderer, null)) &&
                    !RetainsLocalShell(snapshot.RuntimeSlots, current, slot))
                {
                    failure = BattleStateSnapshotRestoreFailure.EntityShellMismatch;
                    return false;
                }
            }
            if (core.RuntimeProfile != activeRuntimeProfile ||
                core.RuntimeSlotCapacity != RuntimeSlotCapacity ||
                core.CollisionBroadphase != CollisionBroadphaseForServices ||
                core.ClaimedRuntimeSlotCount != snapshot.RuntimeSlots.ClaimedCount ||
                snapshot.RuntimeSlots.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.EntityRuntime.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.EntityBaseShell.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.LivingShell.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.CharacterShell.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.WeaponShell.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.SpecialOtherShell.SlotCapacity != RuntimeSlotCapacity ||
                snapshot.Rest.LogicalCapacity != RuntimeSlotCapacity ||
                snapshot.PendingEvents.PendingUnregisterCount != 0 ||
                snapshot.PendingEvents.PendingSlotReleasedDestroyCount != 0 ||
                battleBuffers.PendingUnregister.Count != 0 ||
                battleBuffers.PendingSlotReleasedDestroy.Count != 0 ||
                battleBuffers.PendingSounds.Capacity < snapshot.PendingEvents.SoundCount)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldConfigurationMismatch;
                return false;
            }

            if (!snapshot.EntityRuntime.HasCanonicalPayloadStorage)
            {
                failure = BattleStateSnapshotRestoreFailure.EntityPayloadMismatch;
                return false;
            }

            // Alignment contract: NTSD28-Q05-SNAPSHOT-RENDERER-REGISTRY-RETENTION-001.
            // Registered renderers count as World objects but never claim battle slots.
            if (!TryCountRetainedRendererRegistrations(snapshot.RuntimeSlots, out int rendererCount) ||
                core.ObjectCount != snapshot.EntityRuntime.CountActiveEntitiesForRestore() + rendererCount)
            {
                failure = BattleStateSnapshotRestoreFailure.WorldConfigurationMismatch;
                return false;
            }

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                BattleRuntimeSlotSnapshot expected =
                    snapshot.RuntimeSlots.GetSlot(runtimeSlot);
                NTSDEntityRuntime currentRaw = _runtimeSlots.GetReadOnlyView(runtimeSlot).RawRuntime;
                if (currentRaw != null && !currentRaw.HasCanonicalSnapshotStorage)
                {
                    failure = BattleStateSnapshotRestoreFailure.EntityPayloadMismatch;
                    return false;
                }
                if (!expected.Claimed)
                    continue;

                snapshot.RuntimeSlots.TryGetLocalEntityShell(
                    runtimeSlot,
                    out LF2Entity entity);
                if (entity != null)
                {
                    if (entity.Runtime == null ||
                        expected.EntityKind !=
                            BattleWorldRuntimeSlotSnapshotBuffer.ResolveEntityKind(entity) ||
                        expected.StableId != entity.Runtime.StableId ||
                        expected.CurrentDataObjectId !=
                            LF2Entity.ResolveCurrentDataObjectId(entity) ||
                        expected.CurrentDataObjectType !=
                            entity.GetCurrentDataObjectTypeForSimulation())
                    {
                        failure = BattleStateSnapshotRestoreFailure.EntityIdentityMismatch;
                        return false;
                    }
                }
                else if (!CanMaterializeSnapshotShell(expected))
                {
                    failure = BattleStateSnapshotRestoreFailure.TopologyShellUnavailable;
                    return false;
                }

                bool expectsLiving = entity != null
                    ? entity is LF2LivingObject
                    : expected.EntityKind == BattleRuntimeEntityKind.Character;
                bool expectsCharacter = entity != null
                    ? entity is LF2Character
                    : expected.EntityKind == BattleRuntimeEntityKind.Character;
                bool expectsWeapon = entity != null
                    ? entity is LF2WeaponBase
                    : expected.EntityKind == BattleRuntimeEntityKind.Weapon;
                bool expectsSpecialOther = entity != null
                    ? entity is LF2SpecialAttack || entity is LF2OtherObject
                    : expected.EntityKind == BattleRuntimeEntityKind.SpecialAttack ||
                      expected.EntityKind == BattleRuntimeEntityKind.Other;
                if (!snapshot.EntityRuntime.HasEntityRuntime(runtimeSlot) ||
                    !snapshot.EntityRuntime.HasRawRuntime(runtimeSlot) ||
                    !snapshot.EntityBaseShell.HasEntity(runtimeSlot) ||
                    snapshot.LivingShell.HasLiving(runtimeSlot) !=
                        expectsLiving ||
                    snapshot.CharacterShell.HasCharacter(runtimeSlot) !=
                        expectsCharacter ||
                    snapshot.WeaponShell.HasWeapon(runtimeSlot) !=
                        expectsWeapon ||
                    snapshot.SpecialOtherShell.HasEntity(runtimeSlot) !=
                        expectsSpecialOther)
                {
                    failure = BattleStateSnapshotRestoreFailure.EntityShellPresenceMismatch;
                    return false;
                }

                BattleEntityBaseShellSnapshot baseState =
                    snapshot.EntityBaseShell.GetState(runtimeSlot);
                if (baseState.RequiredRuntimeSlot != -1)
                {
                    failure = BattleStateSnapshotRestoreFailure.RequiredRuntimeSlotMismatch;
                    return false;
                }
                if (!HasSnapshotFrameData(
                        expected,
                        entity,
                        baseState.FrameDataId))
                {
                    failure = BattleStateSnapshotRestoreFailure.CurrentFrameDataUnavailable;
                    return false;
                }
                if (!HasSnapshotFrameData(
                        expected,
                        entity,
                        baseState.CollisionFrameDataId))
                {
                    failure = BattleStateSnapshotRestoreFailure.CollisionFrameDataUnavailable;
                    return false;
                }
                if (!CanResolveSnapshotHandle(
                        snapshot.RuntimeSlots,
                        baseState.TrackerParentHandle))
                {
                    failure = BattleStateSnapshotRestoreFailure.TrackerParentUnavailable;
                    return false;
                }
                if (expectsLiving)
                {
                    BattleLivingShellSnapshot state =
                        snapshot.LivingShell.GetState(runtimeSlot);
                    if (!CanResolveSnapshotHandle<LF2LivingObject>(
                            snapshot.RuntimeSlots,
                            state.CatchingHandle) ||
                        !CanResolveSnapshotHandle<LF2LivingObject>(
                            snapshot.RuntimeSlots,
                            state.AttackerHandle))
                    {
                        failure = BattleStateSnapshotRestoreFailure.RelationshipMismatch;
                        return false;
                    }
                }
                if (expectsCharacter)
                {
                    RuntimeEntityHandle handle = snapshot.CharacterShell
                        .GetState(runtimeSlot).HeldWeaponHandle;
                    if (!CanResolveSnapshotHandle<ILF2Object>(
                            snapshot.RuntimeSlots,
                            handle))
                    {
                        failure = BattleStateSnapshotRestoreFailure.RelationshipMismatch;
                        return false;
                    }
                }
                if (entity is LF2SpecialAttack ||
                    (entity == null &&
                     expected.EntityKind == BattleRuntimeEntityKind.SpecialAttack))
                {
                    RuntimeEntityHandle handle = snapshot.SpecialOtherShell
                        .GetState(runtimeSlot).ParentHandle;
                    if (!CanResolveSnapshotHandle<LF2LivingObject>(
                            snapshot.RuntimeSlots,
                            handle))
                    {
                        failure = BattleStateSnapshotRestoreFailure.RelationshipMismatch;
                        return false;
                    }
                }
            }

            failure = BattleStateSnapshotRestoreFailure.None;
            return true;
        }

        private static bool RetainsLocalShell(
            BattleWorldRuntimeSlotSnapshotBuffer snapshot, LF2Entity entity, int currentSlot)
        {
            if (snapshot.TryGetLocalEntityShell(currentSlot, out LF2Entity local) && ReferenceEquals(local, entity))
                return true;
            for (int slot = 0; slot < snapshot.SlotCapacity; slot++)
                if (slot != currentSlot && snapshot.TryGetLocalEntityShell(slot, out local) && ReferenceEquals(local, entity))
                    return true;
            return false;
        }

        private bool RestoreSnapshotTopology(
            BattleStateSnapshotBuffer snapshot)
        {
            BattleWorldRuntimeSlotSnapshotBuffer runtimeSlots =
                snapshot?.RuntimeSlots;
            if (runtimeSlots == null ||
                runtimeSlots.SlotCapacity != RuntimeSlotCapacity)
            {
                return false;
            }

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView current =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!current.Claimed || current.Entity == null)
                    continue;

                LF2Entity entity = current.Entity;
                BattleCentralPresentationMountRegistry.ResetOwnerRuntimeBinding(
                    entity.Renderer);
                entity.BindRegisteredWorldForSnapshotRestore(null);
                entity.SetRuntimeSlotIndex(-1);
                // Alignment contract: NTSD28-Q05-SNAPSHOT-RETIRED-SHELL-POOL-RETURN-001.
                if (!RetainsLocalShell(runtimeSlots, entity, runtimeSlot))
                    world.LogicReferencePool.Release(entity);
            }

            RemoveEntityBucketItemsForRestore();
            bool requiresShellMaterialization = false;
            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                BattleRuntimeSlotSnapshot state = runtimeSlots.GetSlot(runtimeSlot);
                if (state.Claimed &&
                    !runtimeSlots.TryGetLocalEntityShell(runtimeSlot, out _))
                {
                    requiresShellMaterialization = true;
                    break;
                }
            }

            if (requiresShellMaterialization)
            {
                _runtimeSlots.ClearTopologyForSnapshotShellMaterialization();
                for (int runtimeSlot = 0;
                     runtimeSlot < RuntimeSlotCapacity;
                     runtimeSlot++)
                {
                    BattleRuntimeSlotSnapshot state =
                        runtimeSlots.GetSlot(runtimeSlot);
                    if (!state.Claimed ||
                        runtimeSlots.TryGetLocalEntityShell(runtimeSlot, out _))
                    {
                        continue;
                    }

                    BattleWeaponShellSnapshot weaponState =
                        snapshot.WeaponShell.HasWeapon(runtimeSlot)
                            ? snapshot.WeaponShell.GetState(runtimeSlot)
                            : default;
                    LF2Entity entity = logicEntityFactory.CreateSnapshotShell(
                        runtimeSlot,
                        state,
                        snapshot.EntityBaseShell.GetState(runtimeSlot),
                        weaponState,
                        out _);
                    if (entity == null ||
                        !runtimeSlots.TrySetLocalEntityShellForRestore(
                            runtimeSlot,
                            entity))
                    {
                        return false;
                    }
                }

                RemoveEntityBucketItemsForRestore();
                _runtimeSlots.ClearTopologyForSnapshotShellMaterialization();
            }
            if (!_runtimeSlots.TryRestoreSnapshotTopology(runtimeSlots))
                return false;

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                BattleRuntimeSlotSnapshot state = runtimeSlots.GetSlot(runtimeSlot);
                if (!state.Claimed ||
                    !runtimeSlots.TryGetLocalEntityShell(
                        runtimeSlot,
                        out LF2Entity entity))
                {
                    continue;
                }

                SimulationObjectBucket bucket =
                    objectBucketRegistry.GetOrCreate(entity.SimOrder);
                if (bucket == null)
                    return false;

                entity.BindRegisteredWorldForSnapshotRestore(world);
                entity.SetRuntimeSlotIndex(runtimeSlot);
                bucket.items.Add(entity);
                bucket.dirty = true;
            }

            return _runtimeSlots.ClaimedCount == runtimeSlots.ClaimedCount;
        }

        private bool TryCountRetainedRendererRegistrations(
            BattleWorldRuntimeSlotSnapshotBuffer snapshot, out int count)
        {
            count = 0;
            for (int bucketIndex = 0; bucketIndex < objectBucketRegistry.OrderedCount; bucketIndex++)
            {
                var items = objectBucketRegistry.GetOrderedBucket(bucketIndex).items;
                for (int index = 0; index < items.Count; index++)
                {
                    ISimObject item = items[index];
                    if (item is LF2Entity)
                        continue;
                    if (item is not LF2ObjectRenderer renderer || renderer == null ||
                        renderer.LogicObject is not LF2Entity owner ||
                        !ReferenceEquals(owner.Renderer, renderer) ||
                        !RetainsLocalShell(snapshot, owner, owner.Runtime.SlotIndex))
                    {
                        return false;
                    }
                    count++;
                }
            }
            return true;
        }

        private void RemoveEntityBucketItemsForRestore()
        {
            for (int bucketIndex = objectBucketRegistry.OrderedCount - 1; bucketIndex >= 0; bucketIndex--)
            {
                SimulationObjectBucket bucket = objectBucketRegistry.GetOrderedBucket(bucketIndex);
                for (int index = bucket.items.Count - 1; index >= 0; index--)
                {
                    if (bucket.items[index] is LF2Entity)
                    {
                        bucket.items.RemoveAt(index);
                        bucket.dirty = true;
                    }
                }
                objectBucketRegistry.RemoveIfEmpty(bucket.SimOrder, bucket);
            }
        }

        private bool RestoreCoreScalarState(in BattleWorldCoreScalarSnapshot core)
        {
            if (Runtime?.Match == null ||
                Runtime.Stage == null ||
                Runtime.StageProgression == null ||
                Runtime.Flow == null ||
                Runtime.FunctionKeys == null ||
                Rng == null ||
                NativeRandom == null)
            {
                return false;
            }

            BattleWorldMatchScalarSnapshot match = core.Match;
            Runtime.Match.LocalGameModeId = match.LocalGameModeId;
            Runtime.Match.BattleGameModeId = match.BattleGameModeId;
            Runtime.Match.BackgroundId = match.BackgroundId;
            Runtime.Match.Difficulty = match.Difficulty;
            Runtime.Match.StageIdx = match.StageIdx;
            Runtime.Match.RandomStage = match.RandomStage;
            Runtime.Match.RuntimeStageCount = match.RuntimeStageCount;
            Runtime.Match.Seed = match.Seed;
            Runtime.Match.PpMode = match.PpMode;

            BattleWorldStageScalarSnapshot stage = core.Stage;
            Runtime.Stage.BaseStageWidthPx = stage.BaseStageWidthPx;
            Runtime.Stage.StageWidthPx = stage.StageWidthPx;
            Runtime.Stage.ZMin = stage.ZMin;
            Runtime.Stage.ZMax = stage.ZMax;
            Runtime.Stage.PerspectiveNear = stage.PerspectiveNear;
            Runtime.Stage.PerspectiveFar = stage.PerspectiveFar;
            Runtime.Stage.BoundLeft = stage.BoundLeft;
            Runtime.Stage.BoundRight = stage.BoundRight;
            Runtime.Stage.XMaxOverride = stage.XMaxOverride;
            Runtime.Stage.CameraMaxOverride = stage.CameraMaxOverride;

            BattleWorldProgressionScalarSnapshot progression = core.Progression;
            Runtime.StageProgression.StageSeriesIdx = progression.StageSeriesIdx;
            Runtime.StageProgression.WaveIdx = progression.WaveIdx;
            Runtime.StageProgression.Round = progression.Round;
            Runtime.StageProgression.RoundMax = progression.RoundMax;
            Runtime.StageProgressionValid = progression.StageProgressionValid;
            Runtime.StageSpawnWaveApplied = progression.StageSpawnWaveApplied;
            Runtime.StageSpawnWaveDeferredEntryApplied =
                progression.StageSpawnWaveDeferredEntryApplied;

            BattleWorldFlowScalarSnapshot flow = core.Flow;
            Runtime.Flow.CurrentTickIndex = flow.CurrentTickIndex;
            Runtime.Flow.SparkRenderFrame = flow.SparkRenderFrame;
            Runtime.Flow.AiPhaseGate = flow.AiPhaseGate;
            Runtime.Flow.InputPhase = flow.InputPhase;
            Runtime.Flow.FrameMod12 = flow.FrameMod12;
            Runtime.Flow.FrameToggle = flow.FrameToggle;
            Runtime.Flow.AiDifficulty = flow.AiDifficulty;
            Runtime.Flow.AiRand3 = flow.AiRand3;
            Runtime.Flow.AiRand5 = flow.AiRand5;
            Runtime.Flow.AiRand15 = flow.AiRand15;
            Runtime.Flow.AiRand20 = flow.AiRand20;
            Runtime.Flow.AiMoveMode = flow.AiMoveMode;
            Runtime.Flow.AiStageTargetX = flow.AiStageTargetX;
            Runtime.Flow.BattleExitCountdown = flow.BattleExitCountdown;
            Runtime.Flow.RouteOutRequest = flow.RouteOutRequest;
            Runtime.Flow.InitStatsRequest = flow.InitStatsRequest;
            Runtime.Flow.Mode2Request = flow.Mode2Request;
            Runtime.Flow.BattleStepMode = flow.BattleStepMode;
            Runtime.Flow.BattleStepGate = flow.BattleStepGate;
            Runtime.Flow.DjaGuardGlobal44F224 = flow.DjaGuardGlobal44F224;
            Runtime.Flow.HumanInputPolledExternally =
                flow.HumanInputPolledExternally;
            Runtime.Flow.NeedClearInput = flow.NeedClearInput;
            Runtime.NativeWorldClock ??= new NTSD28NativeWorldClockState();
            Runtime.NativeWorldClock.ResourcePhase12 =
                core.NativeClock.ResourcePhase12;
            Runtime.NativeWorldClock.ResourcePhase3 =
                core.NativeClock.ResourcePhase3;
            Runtime.NativeWorldClock.FrameSequence =
                core.NativeClock.FrameSequence;
            BattleWorldFunctionKeyScalarSnapshot functionKeys = core.FunctionKeys;
            Runtime.FunctionKeys.RestoreForSnapshot(
                functionKeys.LockState,
                functionKeys.HitResourceEnabled,
                functionKeys.F6EventCount,
                functionKeys.F7EventCount,
                functionKeys.F8EventCount,
                functionKeys.F9EventCount,
                functionKeys.PendingFullMp,
                functionKeys.PendingObjectCommand,
                functionKeys.QueuedEventByte,
                functionKeys.LastAcceptedEventByte);
            Runtime.NativeHitResourceRules ??=
                new NTSD28HitResourceRulesRuntimeState();
            BattleWorldHitResourceRulesScalarSnapshot hitResourceRules =
                core.HitResourceRules;
            Runtime.NativeHitResourceRules.RestoreForSnapshot(
                hitResourceRules.ActiveModeHitGroupGate18,
                hitResourceRules.ActiveModeAttackingPercent1C,
                hitResourceRules.AttackerInjuryMpPercent34,
                hitResourceRules.TargetInjuryMpPercent38,
                hitResourceRules.NegativeEnvironmentDamage90);
            Runtime.NativeCombo ??= new NTSD28NativeComboRuntimeState();
            BattleWorldNativeComboScalarSnapshot nativeCombo = core.NativeCombo;
            Runtime.NativeCombo.RestoreForSnapshot(
                nativeCombo.RecordPresent,
                nativeCombo.Bound,
                nativeCombo.Facing,
                nativeCombo.Respond,
                nativeCombo.CaughtAct);
            Runtime.NativeStandardHitRest ??=
                new NTSD28StandardHitRestRuntimeState();
            Runtime.NativeStandardHitRest.RestoreForSnapshot(
                core.StandardHitRest.TimingReduction4A9FF4);
            world.SetOneTuInputForBattle(core.OneTuInput);

            Rng.RestoreState(core.RngState, core.RngCallCount);
            if (!NativeRandom.TryRestoreScalarState(core.NativeRandomState))
            {
                return false;
            }
            world.RestoreSnapshotOwnerScalars(
                core.ReleaseCameraX,
                core.ReleaseCameraVelocity,
                core.NextAutoStableId);
            return true;
        }

        private bool RebuildDerivedStateAfterSnapshotRestore()
        {
            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlotCapacity;
                 runtimeSlot++)
            {
                RuntimeSlotTable.ReadOnlySlotView view =
                    _runtimeSlots.GetReadOnlyView(runtimeSlot);
                if (!view.Claimed)
                    continue;

                LF2Entity entity = view.Entity;
                if (entity.ItrRest == null ||
                    !entity.ItrRest.Bind(_runtimeRestStore, runtimeSlot, false))
                {
                    return false;
                }
            }

            return world.RebuildDerivedStateAfterSnapshotRestore();
        }

        private bool CanMaterializeSnapshotShell(
            in BattleRuntimeSlotSnapshot state)
        {
            if (!state.Claimed ||
                state.Generation == 0 ||
                !IsMaterializableEntityKind(state.EntityKind) ||
                state.CurrentDataObjectId <= 0)
            {
                return false;
            }

            ObjectDefinition definition = RuntimeDataCatalog
                .GetObjectDefinition(state.CurrentDataObjectId);
            return definition != null &&
                   definition.type == state.CurrentDataObjectType &&
                   RuntimeDataCatalog.GetCharacterData(
                       state.CurrentDataObjectId) != null;
        }

        private bool HasSnapshotFrameData(
            in BattleRuntimeSlotSnapshot state,
            LF2Entity localEntity,
            int frameId)
        {
            if (frameId < 0)
                return true;
            if (localEntity != null)
                return localEntity.GetFrameDataById(frameId) != null;

            LF2CharacterData data = RuntimeDataCatalog.GetCharacterData(
                state.CurrentDataObjectId);
            if (data?.frames == null)
                return false;
            for (int index = 0; index < data.frames.Count; index++)
            {
                LF2FrameData frame = data.frames[index];
                if (frame != null && frame.frameId == frameId)
                    return true;
            }
            return false;
        }

        private static bool IsMaterializableEntityKind(
            BattleRuntimeEntityKind entityKind)
        {
            return entityKind == BattleRuntimeEntityKind.Character ||
                   entityKind == BattleRuntimeEntityKind.Weapon ||
                   entityKind == BattleRuntimeEntityKind.SpecialAttack ||
                   entityKind == BattleRuntimeEntityKind.Other;
        }

        private static bool CanResolveSnapshotHandle(
            BattleWorldRuntimeSlotSnapshotBuffer runtimeSlots,
            RuntimeEntityHandle handle)
        {
            if (!handle.IsValid)
                return true;
            if (runtimeSlots == null ||
                (uint)handle.Slot >= (uint)runtimeSlots.SlotCapacity)
            {
                return false;
            }

            BattleRuntimeSlotSnapshot state = runtimeSlots.GetSlot(handle.Slot);
            return state.Claimed &&
                   state.Generation == handle.Generation &&
                   (runtimeSlots.TryGetLocalEntityShell(handle.Slot, out _) ||
                    IsMaterializableEntityKind(state.EntityKind));
        }

        private static bool CanResolveSnapshotHandle<T>(
            BattleWorldRuntimeSlotSnapshotBuffer runtimeSlots,
            RuntimeEntityHandle handle)
            where T : class
        {
            if (!handle.IsValid)
                return true;
            if (!CanResolveSnapshotHandle(runtimeSlots, handle))
            {
                return false;
            }

            if (runtimeSlots.TryGetLocalEntityShell(
                    handle.Slot,
                    out LF2Entity entity))
            {
                return entity is T;
            }

            BattleRuntimeEntityKind entityKind =
                runtimeSlots.GetSlot(handle.Slot).EntityKind;
            Type requestedType = typeof(T);
            return entityKind switch
            {
                BattleRuntimeEntityKind.Character =>
                    requestedType.IsAssignableFrom(typeof(LF2Character)),
                BattleRuntimeEntityKind.Weapon =>
                    requestedType.IsAssignableFrom(typeof(LF2Weapon)),
                BattleRuntimeEntityKind.SpecialAttack =>
                    requestedType.IsAssignableFrom(typeof(LF2SpecialAttack)),
                BattleRuntimeEntityKind.Other =>
                    requestedType.IsAssignableFrom(typeof(LF2OtherObject)),
                _ => false,
            };
        }

        private bool TryResolveSnapshotHandle(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            if (!handle.IsValid)
            {
                entity = null;
                return true;
            }
            return _runtimeSlots.TryResolve(handle, out entity);
        }

        private bool TryResolveSnapshotHandle<T>(
            RuntimeEntityHandle handle,
            out T entity)
            where T : class
        {
            entity = null;
            if (!handle.IsValid)
                return true;
            if (!_runtimeSlots.TryResolve(handle, out LF2Entity resolved) ||
                resolved is not T typed)
            {
                return false;
            }
            entity = typed;
            return true;
        }
    }
}
