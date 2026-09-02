using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Animation.LF2Tasks;
using NTSD.Simulation.Spatial;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the mutable registry stores for one SimulationWorld. World keeps the
    /// compatibility API while registry behavior is migrated out of the historical
    /// partial implementation in independently verified slices.
    /// </summary>
    internal sealed class SimulationRegistryModule
    {
        private sealed class RuntimeStableIdComparer : IComparer<ISimObject>
        {
            public int Compare(ISimObject left, ISimObject right)
            {
                int leftStableId = left is LF2Entity leftEntity
                    ? leftEntity.Runtime.StableId
                    : left?.StableId ?? int.MinValue;
                int rightStableId = right is LF2Entity rightEntity
                    ? rightEntity.Runtime.StableId
                    : right?.StableId ?? int.MinValue;
                return leftStableId.CompareTo(rightStableId);
            }
        }

        private readonly SimulationWorld world;
        private readonly IComparer<ISimObject> runtimeStableIdComparer =
            new RuntimeStableIdComparer();
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private readonly HashSet<LF2Entity> activeRuntimeEntitySnapshotSeen =
            new HashSet<LF2Entity>();
        private static readonly System.Comparison<LF2Entity>
            ActiveRuntimeEntitySnapshotComparison = CompareRuntimeSlotOrder;
#endif
        internal SimulationRegistryModule(
            SimulationWorld world,
            int runtimeSlotCapacity,
            int reservedRuntimeSlotCount,
            int dynamicRuntimeSlotStart,
            BattleRuntimeProfile runtimeProfile,
            int maxActiveRuntimeEntities)
        {
            this.world = world ?? throw new System.ArgumentNullException(nameof(world));
            ObjectBuckets = new SimulationObjectBucketRegistry();
            RuntimeSlots = new RuntimeSlotTable(
                runtimeSlotCapacity,
                reservedRuntimeSlotCount,
                dynamicRuntimeSlotStart);
            RuntimeRestStore = new RuntimeRestStore(runtimeSlotCapacity);
            RuntimeProfile = runtimeProfile;
            MaxActiveRuntimeEntities = maxActiveRuntimeEntities;
        }

        internal RuntimeSlotTable RuntimeSlots { get; }
        internal RuntimeRestStore RuntimeRestStore { get; }
        internal SimulationObjectBucketRegistry ObjectBuckets { get; }
        internal Dictionary<int, SimulationObjectBucket> CompatibilityBuckets =>
            ObjectBuckets.LookupForCompatibility;
        internal BattleRuntimeProfile RuntimeProfile { get; }
        internal int MaxActiveRuntimeEntities { get; }
        internal int NextAutoStableId { get; set; } = 100;
        internal bool IsTicking { get; set; }
        internal int CameraX { get; set; }
        internal int CameraVelocity { get; set; }
        internal bool PendingDestroyScanCacheValid { get; set; }
        internal long PendingDestroyScanMutationEpoch { get; set; }
        internal ulong PendingDestroyScanOccupancyEpoch { get; set; }
        internal long PendingDestroyFullScanCount { get; set; }
        internal long PendingDestroySkipCount { get; set; }
        internal long PendingDestroyVisitedEntityCount { get; set; }
        internal long NullRegistrationRejectCount { get; set; }
        internal long BucketCapacityRejectCount { get; set; }
        internal long DuplicateRegistrationRejectCount { get; set; }
        internal long RuntimeSlotCapacityRejectCount { get; set; }
        internal long RuntimeRestBindRejectCount { get; set; }
        internal long StableIdRegistrationRejectCount { get; set; }
        internal long MissingUnregisterCount { get; set; }
        internal long RuntimeSlotReleaseRejectCount { get; set; }
        internal bool ForceLegacyPendingDestroyScan { get; set; }
        internal bool EnableRegistryLifecycleLogging { get; set; }
        internal IBattleParityStructuralEventSink StructuralEventSink { get; private set; }
        internal int StructuralEventCursorSlot { get; set; } = -1;
        internal Dictionary<ISimObject, int> StructuralPendingUnregisterSlots
        {
            get;
            private set;
        }
        private int structuralEventTick;
        private string structuralEventPass = string.Empty;

        internal void SetStructuralEventSink(
            IBattleParityStructuralEventSink sink,
            int tick,
            string pass)
        {
            StructuralEventSink = sink;
            structuralEventTick = tick;
            structuralEventPass = pass ?? string.Empty;
            StructuralEventCursorSlot = -1;
            if (sink != null)
            {
                StructuralPendingUnregisterSlots ??=
                    new Dictionary<ISimObject, int>();
                StructuralPendingUnregisterSlots.Clear();
            }
            else
            {
                StructuralPendingUnregisterSlots = null;
            }
        }

        internal void SetStructuralEventContext(int tick, string pass)
        {
            if (StructuralEventSink == null)
                return;
            structuralEventTick = tick;
            structuralEventPass = pass ?? string.Empty;
            StructuralEventCursorSlot = -1;
        }

        internal void EmitStructuralEvent(
            string action,
            int slot,
            int searchStart,
            int searchEndExclusive,
            string before,
            string after,
            string sourceKind,
            int actorSlot = -1)
        {
            IBattleParityStructuralEventSink sink = StructuralEventSink;
            if (sink == null)
                return;

            sink.Record(new BattleParityStructuralEvent
            {
                Tick = structuralEventTick,
                Pass = structuralEventPass,
                Action = action,
                CursorSlot = StructuralEventCursorSlot,
                ActorSlot = actorSlot >= 0
                    ? actorSlot
                    : StructuralEventCursorSlot,
                Slot = slot,
                SearchStart = searchStart,
                SearchEndExclusive = searchEndExclusive,
                Before = before,
                After = after,
                LifecycleEpoch = slot >= 0 && RuntimeSlots.IsAddressable(slot)
                    ? RuntimeSlots.GetAllocationEpoch(slot)
                    : 0UL,
                SourceKind = sourceKind,
            });
        }

        internal static string StructuralSourceKind(LF2Entity entity)
        {
            if (entity?.Runtime?.SpawnSemantic ==
                (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return "stage";
            }
            return entity != null && entity.UsesDynamicRuntimeSlot()
                ? "dynamic"
                : "general";
        }

        internal NTSDEntityRuntime GetRawRuntimeSlotState(int runtimeSlot)
        {
            return RuntimeSlots.GetRawRuntime(runtimeSlot);
        }

        internal bool TryGetCurrentRuntimeHandle(
            int runtimeSlot,
            LF2Entity expectedEntity,
            out RuntimeEntityHandle handle)
        {
            return RuntimeSlots.TryGetCurrentHandle(
                runtimeSlot,
                expectedEntity,
                out handle);
        }

        internal bool TryResolveRuntimeHandle(
            RuntimeEntityHandle handle,
            out LF2Entity entity)
        {
            return RuntimeSlots.TryResolve(handle, out entity);
        }

        internal bool TryGetRuntimeSlotReadOnlyView(
            int runtimeSlot,
            out RuntimeSlotTable.ReadOnlySlotView view)
        {
            if (!RuntimeSlots.IsAddressable(runtimeSlot))
            {
                view = default;
                return false;
            }

            view = RuntimeSlots.GetReadOnlyView(runtimeSlot);
            return true;
        }

        internal bool ContainsRegisteredEntityStableId(int stableId)
        {
            for (int bucketIndex = 0;
                 bucketIndex < ObjectBuckets.OrderedCount;
                 bucketIndex++)
            {
                List<ISimObject> items =
                    ObjectBuckets.GetOrderedBucket(bucketIndex).items;
                for (int itemIndex = 0; itemIndex < items.Count; itemIndex++)
                {
                    if (items[itemIndex] is LF2Entity entity &&
                        entity.Runtime.StableId == stableId)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        internal static int GetRuntimeSlotOrder(LF2Entity entity)
        {
            if (entity == null)
                return int.MaxValue;
            int slot = entity.Runtime?.SlotIndex ?? -1;
            return slot >= 0 ? slot : entity.StableId;
        }

        internal static int CompareRuntimeSlotOrder(LF2Entity left, LF2Entity right)
        {
            int comparison = GetRuntimeSlotOrder(left).CompareTo(
                GetRuntimeSlotOrder(right));
            if (comparison != 0)
                return comparison;
            return (left?.StableId ?? int.MaxValue).CompareTo(
                right?.StableId ?? int.MaxValue);
        }

        internal static void RefreshRuntimeSnapshot(ISimObject obj)
        {
            if (obj is LF2Entity entity)
                entity.RefreshRuntimeSnapshot();
        }

        internal static bool IsActiveForCurrentPass(
            ISimObject obj,
            List<ISimObject> pendingUnregister)
        {
            if (obj == null || pendingUnregister.Contains(obj))
                return false;

            if (obj is LF2Entity entity && entity.Runtime != null)
            {
                if (entity.Runtime.OidMergeDormant ||
                    entity.Runtime.PendingFlushDestroy)
                {
                    return false;
                }
            }

            return true;
        }

        internal void GetNonEntityRendererObjects(List<ISimObject> destination)
        {
            destination.Clear();
            if (!CompatibilityBuckets.TryGetValue(
                    SimOrderConstants.Renderer,
                    out SimulationObjectBucket bucket))
            {
                return;
            }

            bucket.EnsureSorted(runtimeStableIdComparer);
            for (int index = 0; index < bucket.items.Count; index++)
            {
                if (bucket.items[index] is LF2Entity)
                    continue;
                if (bucket.items[index] is LF2ObjectRenderer)
                    destination.Add(bucket.items[index]);
            }
        }

        internal bool HasUnityPresentationBindings()
        {
            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity entity = RuntimeSlots.GetCurrentOccupant(runtimeSlot);
                if (entity == null)
                    continue;
                if (!ReferenceEquals(entity.Renderer, null) ||
                    !ReferenceEquals(entity.ShadowRenderer, null))
                {
                    return true;
                }
            }

            return false;
        }

        internal int AllocateStableId()
        {
            return NextAutoStableId++;
        }

        internal int AllocateRuntimeSlot(
            LF2Entity entity,
            out RuntimeSlotAllocationTicket allocationTicket)
        {
            allocationTicket = RuntimeSlotAllocationTicket.Invalid;
            world.ReleasePendingDestroySlotsForRegistry();

            if (RuntimeSlots.ClaimedCount >= MaxActiveRuntimeEntities)
                return -1;

            bool requiresDynamicSlot = entity.UsesDynamicRuntimeSlot();
            int requiredSlot = entity.RequiredRuntimeSlot;
            int runtimeSlotCapacity = RuntimeSlots.LogicalCapacity;
            int dynamicStart = world.DynamicRuntimeSlotStartForServices;
            if (requiredSlot != -1)
            {
                if (requiredSlot >= runtimeSlotCapacity &&
                    !world.TryGrowDesktopRuntimeSlotsForRegistry(
                        (long)requiredSlot + 1))
                {
                    return RecordStructuralSearch(
                        -1,
                        requiredSlot,
                        requiredSlot + 1,
                        entity);
                }

                if (!RuntimeSlots.TryBeginClaim(
                        requiredSlot,
                        entity,
                        out allocationTicket))
                {
                    return RecordStructuralSearch(
                        -1,
                        entity.Runtime.SpawnSemantic ==
                            (int)ReleaseSpawnSemantic.StageSpawnAt
                            ? 20
                            : requiredSlot,
                        entity.Runtime.SpawnSemantic ==
                            (int)ReleaseSpawnSemantic.StageSpawnAt
                            ? RuntimeSlots.LogicalCapacity
                            : requiredSlot + 1,
                        entity);
                }

                return RecordStructuralSearch(
                    requiredSlot,
                    entity.Runtime.SpawnSemantic ==
                        (int)ReleaseSpawnSemantic.StageSpawnAt
                        ? 20
                        : requiredSlot,
                    entity.Runtime.SpawnSemantic ==
                        (int)ReleaseSpawnSemantic.StageSpawnAt
                        ? RuntimeSlots.LogicalCapacity
                        : requiredSlot + 1,
                    entity);
            }

            int existingSlot = entity.Runtime?.SlotIndex ?? -1;
            bool existingSlotInRange = existingSlot >= 0 &&
                                       existingSlot < RuntimeSlots.LogicalCapacity;
            bool existingSlotInAllowedRange = !requiresDynamicSlot ||
                                              existingSlot >= dynamicStart;
            int minimumExistingSlot = requiresDynamicSlot ? dynamicStart : 0;
            if (existingSlotInRange && existingSlotInAllowedRange &&
                existingSlot >= minimumExistingSlot &&
                RuntimeSlots.TryBeginClaim(
                    existingSlot,
                    entity,
                    out allocationTicket))
            {
                return RecordStructuralSearch(
                    existingSlot,
                    minimumExistingSlot,
                    RuntimeSlots.LogicalCapacity,
                    entity);
            }

            int startSlot = requiresDynamicSlot ? dynamicStart : 0;
            int allocatedSlot = RuntimeSlots.BeginAllocateLowest(
                startSlot,
                entity,
                out allocationTicket);
            if (allocatedSlot >= 0 ||
                !world.TryGrowDesktopRuntimeSlotsForRegistry(
                    (long)RuntimeSlots.LogicalCapacity + 1))
            {
                return RecordStructuralSearch(
                    allocatedSlot,
                    startSlot,
                    RuntimeSlots.LogicalCapacity,
                    entity);
            }

            return RecordStructuralSearch(
                RuntimeSlots.BeginAllocateLowest(
                    startSlot,
                    entity,
                    out allocationTicket),
                startSlot,
                RuntimeSlots.LogicalCapacity,
                entity);
        }

        internal int FindFirstFreeRuntimeSlot(
            int startSlot,
            int endSlotExclusive)
        {
            world.ReleasePendingDestroySlotsForRegistry();
            if (RuntimeSlots.ClaimedCount >= MaxActiveRuntimeEntities)
                return -1;

            bool scansCurrentTail =
                endSlotExclusive >= RuntimeSlots.LogicalCapacity;
            int slot = RuntimeSlots.PeekLowest(startSlot, endSlotExclusive);
            if (slot >= 0 || !scansCurrentTail ||
                !world.TryGrowDesktopRuntimeSlotsForRegistry(
                    (long)RuntimeSlots.LogicalCapacity + 1))
            {
                return slot;
            }

            return RuntimeSlots.PeekLowest(
                startSlot,
                RuntimeSlots.LogicalCapacity);
        }

        private int RecordStructuralSearch(
            int slot,
            int searchStart,
            int searchEndExclusive,
            LF2Entity entity)
        {
            if (StructuralEventSink == null)
                return slot;
            EmitStructuralEvent(
                "search",
                slot,
                searchStart,
                searchEndExclusive,
                "free",
                slot >= 0 ? "selected" : "exhausted",
                StructuralSourceKind(entity));
            return slot;
        }

        internal bool RestoreStageSpawnRestState(
            int runtimeSlot,
            LF2Entity entity)
        {
            if (!RuntimeSlots.IsAddressable(runtimeSlot) ||
                entity?.Runtime == null ||
                entity.Runtime.SlotIndex != runtimeSlot ||
                entity.Runtime.SpawnSemantic !=
                    (int)ReleaseSpawnSemantic.StageSpawnAt)
            {
                return false;
            }

            return entity.ItrRest != null &&
                   entity.ItrRest.IsBoundTo(RuntimeRestStore, runtimeSlot);
        }

        internal int GetRawRestArest(int runtimeSlot)
        {
            return RuntimeRestStore.GetARest(runtimeSlot);
        }

        internal int GetRawRestVrest(int victimSlot, int attackerSlot)
        {
            return RuntimeRestStore.GetVRest(victimSlot, attackerSlot);
        }

        internal int CountActiveObjects(List<ISimObject> pendingUnregister)
        {
            int count = 0;
            for (int bucketIndex = 0;
                 bucketIndex < ObjectBuckets.OrderedCount;
                 bucketIndex++)
            {
                SimulationObjectBucket bucket =
                    ObjectBuckets.GetOrderedBucket(bucketIndex);
                if (bucket == null)
                    continue;

                for (int itemIndex = 0;
                     itemIndex < bucket.items.Count;
                     itemIndex++)
                {
                    ISimObject obj = bucket.items[itemIndex];
                    if (obj is LF2Entity entity)
                    {
                        if (pendingUnregister.Contains(entity))
                            continue;
                        if (entity.Runtime != null &&
                            (entity.Runtime.OidMergeDormant ||
                             entity.Runtime.PendingFlushDestroy))
                        {
                            continue;
                        }
                    }

                    count++;
                }
            }

            return count;
        }

        internal void ReleasePendingDestroySlots(
            List<LF2Entity> pendingSlotReleasedDestroy)
        {
            long mutationEpoch =
                world.RuntimeMutationTrackerForServices.PendingFlushDestroyEpoch;
            ulong occupancyEpoch = RuntimeSlots.OccupancyEpoch;
            if (!ForceLegacyPendingDestroyScan &&
                PendingDestroyScanCacheValid &&
                PendingDestroyScanMutationEpoch == mutationEpoch &&
                PendingDestroyScanOccupancyEpoch == occupancyEpoch)
            {
                PendingDestroySkipCount++;
                return;
            }

            PendingDestroyFullScanCount++;
            for (int slot = 0; slot < RuntimeSlots.LogicalCapacity; slot++)
            {
                LF2Entity entity = RuntimeSlots.GetCurrentOccupant(slot);
                if (entity == null)
                    continue;

                PendingDestroyVisitedEntityCount++;
                if (entity.Runtime == null ||
                    !entity.Runtime.PendingFlushDestroy ||
                    entity.Runtime.SlotIndex != slot)
                {
                    continue;
                }

                if (ReleaseRuntimeSlotAndClearPresentationBinding(entity) &&
                    !pendingSlotReleasedDestroy.Contains(entity))
                {
                    pendingSlotReleasedDestroy.Add(entity);
                }
            }

            long completedMutationEpoch =
                world.RuntimeMutationTrackerForServices.PendingFlushDestroyEpoch;
            PendingDestroyScanMutationEpoch = completedMutationEpoch;
            PendingDestroyScanOccupancyEpoch = RuntimeSlots.OccupancyEpoch;
            PendingDestroyScanCacheValid =
                mutationEpoch == completedMutationEpoch;
        }

        internal bool ReleaseRuntimeSlotAndClearPresentationBinding(
            LF2Entity entity)
        {
            NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry
                .ResetOwnerRuntimeBinding(entity?.Renderer);
            if (ReleaseRuntimeSlot(entity))
                return true;

            int slot = entity?.Runtime?.SlotIndex ?? -1;
            if (slot >= 0 &&
                TryGetCurrentRuntimeHandle(
                    slot,
                    entity,
                    out RuntimeEntityHandle restoredHandle))
            {
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry
                    .BindOwnerRuntime(entity.Renderer, restoredHandle);
            }

            return false;
        }

        internal void RollbackRuntimeSlotRegistration(
            LF2Entity entity,
            RuntimeSlotAllocationTicket allocationTicket)
        {
            entity?.ItrRest?.Unbind(false);
            if (entity != null && allocationTicket.IsValid)
            {
                RuntimeEntityHandle releasedHandle = allocationTicket.Handle;
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry
                    .ResetOwnerRuntimeBinding(entity.Renderer);
                if (RuntimeSlots.TryRollbackAllocation(allocationTicket, entity))
                {
                    world.AiUnifiedRowPublisherForServices
                        .InvalidateAfterOccupancyChange();
                    world.IdentityWriter.Release(releasedHandle);
                    world.CharacterInputWriter.Release(releasedHandle);
                    world.FrameMotionWriter.Release(releasedHandle);
                    world.RelationLinkWriter.Release(releasedHandle);
                    world.VitalWriter.Release(releasedHandle);
                    world.StructuralWriter.RecordGenerationRelease(
                        entity,
                        releasedHandle);
                }
            }
            entity?.Runtime?.BindWorldMutationTracker(null);
            entity?.SetRuntimeSlotIndex(-1);
        }

        private bool ReleaseRuntimeSlot(LF2Entity entity)
        {
            int slot = entity.Runtime?.SlotIndex ?? -1;
            if (slot < 0)
                return true;
            if (slot >= RuntimeSlots.LogicalCapacity ||
                !object.ReferenceEquals(
                    RuntimeSlots.GetCurrentOccupant(slot),
                    entity))
            {
                RuntimeSlotReleaseRejectCount++;
                if (!world.RuntimeCapacity.IsSealed)
                {
                    UnityEngine.Debug.LogError(
                        "[SimulationWorld] Refusing runtime slot release without the matching claim: " +
                        $"EntitySlot={slot}, StableId={entity.StableId}");
                }
                return false;
            }

            bool wasBound = entity.ItrRest?.IsBound == true;
            if (wasBound && entity.ItrRest.BoundVictimSlot != slot)
            {
                RuntimeSlotReleaseRejectCount++;
                if (!world.RuntimeCapacity.IsSealed)
                {
                    UnityEngine.Debug.LogError(
                        "[SimulationWorld] Refusing runtime slot release with a mismatched rest binding: " +
                        $"EntitySlot={slot}, BoundVictimSlot={entity.ItrRest.BoundVictimSlot}, " +
                        $"StableId={entity.StableId}");
                }
                return false;
            }
            if (wasBound && !entity.ItrRest.Unbind(false))
                return false;

            TryGetCurrentRuntimeHandle(
                slot,
                entity,
                out RuntimeEntityHandle releasedHandle);
            if (!RuntimeSlots.Release(slot, entity))
            {
                RestoreRuntimeRestBindingAfterReleaseFailure(
                    entity,
                    slot,
                    wasBound);
                return false;
            }

            world.AiUnifiedRowPublisherForServices
                .InvalidateAfterOccupancyChange();
            world.IdentityWriter.Release(releasedHandle);
            world.CharacterInputWriter.Release(releasedHandle);
            world.FrameMotionWriter.Release(releasedHandle);
            world.RelationLinkWriter.Release(releasedHandle);
            world.VitalWriter.Release(releasedHandle);
            world.StructuralWriter.RecordGenerationRelease(
                entity,
                releasedHandle);

            if (StructuralEventSink != null)
            {
                EmitStructuralEvent(
                    "free",
                    slot,
                    -1,
                    -1,
                    "active",
                    "free",
                    StructuralSourceKind(entity),
                    slot);
            }
            entity.Runtime.BindWorldMutationTracker(null);
            entity.SetRuntimeSlotIndex(-1);
            return true;
        }

        private void RestoreRuntimeRestBindingAfterReleaseFailure(
            LF2Entity entity,
            int slot,
            bool wasBound)
        {
            if (!wasBound ||
                entity.ItrRest.Bind(RuntimeRestStore, slot, false))
            {
                return;
            }

            RuntimeSlotReleaseRejectCount++;
            if (!world.RuntimeCapacity.IsSealed)
            {
                UnityEngine.Debug.LogError(
                    "[SimulationWorld] Failed to restore runtime rest binding after slot release rollback: " +
                    $"Slot={slot}, StableId={entity.StableId}");
            }
        }

        internal void RegisterCore(
            ISimObject obj,
            List<ISimObject> pendingUnregister,
            List<LF2Entity> pendingSlotReleasedDestroy)
        {
            if (obj == null)
            {
                NullRegistrationRejectCount++;
                if (!world.RuntimeCapacity.IsSealed)
                    UnityEngine.Debug.LogError(
                        "[SimulationWorld] Cannot register null object");
                return;
            }

            if (pendingUnregister.Remove(obj))
            {
                UnregisterImmediate(obj);
            }

            int simOrder = obj.SimOrder;
            if (!CompatibilityBuckets.TryGetValue(
                    simOrder,
                    out SimulationObjectBucket bucket))
            {
                bucket = ObjectBuckets.GetOrCreate(simOrder);
                if (bucket == null)
                {
                    BucketCapacityRejectCount++;
                    if (!world.RuntimeCapacity.IsSealed)
                    {
                        UnityEngine.Debug.LogError(
                            "[SimulationWorld] Registration rejected because the sealed " +
                            "simulation bucket pool is exhausted: " +
                            $"SimOrder={simOrder}, StableId={obj.StableId}");
                    }
                    return;
                }
            }

            if (bucket.items.Contains(obj))
            {
                DuplicateRegistrationRejectCount++;
                if (!world.RuntimeCapacity.IsSealed)
                {
                    UnityEngine.Debug.LogWarning(
                        "[SimulationWorld] Object already registered: " +
                        $"SimOrder={simOrder}, StableId={obj.StableId}");
                }
                return;
            }

            if (obj is LF2Entity registeredEntity)
            {
                pendingSlotReleasedDestroy.Remove(registeredEntity);
                registeredEntity.ItrRest?.Unbind(false);
                int runtimeSlot = AllocateRuntimeSlot(
                    registeredEntity,
                    out RuntimeSlotAllocationTicket allocationTicket);
                registeredEntity.SetRuntimeSlotIndex(runtimeSlot);
                registeredEntity.ClearRequiredRuntimeSlot();
                if (runtimeSlot < 0)
                {
                    ObjectBuckets.RemoveIfEmpty(simOrder, bucket);
                    RuntimeSlotCapacityRejectCount++;
                    if (!world.RuntimeCapacity.IsSealed)
                    {
                        UnityEngine.Debug.LogWarning(
                            "[SimulationWorld] Runtime slot exhausted; registration rejected: " +
                            $"StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                    }
                    return;
                }

                world.AiUnifiedRowPublisherForServices
                    .InvalidateAfterOccupancyChange();
                world.StructuralWriter.RecordGenerationClaim(
                    registeredEntity,
                    runtimeSlot);
                registeredEntity.Runtime.BindWorldMutationTracker(
                    world.RuntimeMutationTrackerForServices);
                GetRawRuntimeSlotState(runtimeSlot)?.Reset();
                bool restBindingReady =
                    registeredEntity.Runtime.SpawnSemantic ==
                    (int)ReleaseSpawnSemantic.StageSpawnAt
                        ? world.TryResetAndBindStageSpawnCooldownsForRegistry(
                            runtimeSlot,
                            registeredEntity)
                        : world.ResetCooldownsForRuntimeSlot(
                            runtimeSlot,
                            registeredEntity);
                if (!RuntimeSlots.TryCompleteRequiredSideEffect(
                        allocationTicket,
                        restBindingReady))
                {
                    RollbackRuntimeSlotRegistration(
                        registeredEntity,
                        allocationTicket);
                    ObjectBuckets.RemoveIfEmpty(simOrder, bucket);
                    RuntimeRestBindRejectCount++;
                    if (!world.RuntimeCapacity.IsSealed)
                    {
                        UnityEngine.Debug.LogError(
                            "[SimulationWorld] Runtime rest bind failed; registration rejected: " +
                            $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                    }
                    return;
                }

                int requestedStableId = registeredEntity.Runtime.StableId;
                if (requestedStableId > 0)
                {
                    if (ContainsRegisteredEntityStableId(requestedStableId) ||
                        requestedStableId == int.MaxValue)
                    {
                        RollbackRuntimeSlotRegistration(
                            registeredEntity,
                            allocationTicket);
                        ObjectBuckets.RemoveIfEmpty(simOrder, bucket);
                        StableIdRegistrationRejectCount++;
                        if (!world.RuntimeCapacity.IsSealed)
                        {
                            UnityEngine.Debug.LogError(
                                "[SimulationWorld] StableId registration rejected: " +
                                $"StableId={requestedStableId}, " +
                                $"Type={registeredEntity.GetType().Name}");
                        }
                        return;
                    }

                    if (requestedStableId >= NextAutoStableId)
                        NextAutoStableId = requestedStableId + 1;
                }
                else
                {
                    registeredEntity.AssignStableIdForRegistration(
                        AllocateStableId());
                }

                if (!registeredEntity.ShouldDeferInitialRuntimeSnapshot())
                    registeredEntity.RefreshRuntimeSnapshot();

                if (!RuntimeSlots.TryCommitAllocation(allocationTicket, out _))
                {
                    RollbackRuntimeSlotRegistration(
                        registeredEntity,
                        allocationTicket);
                    ObjectBuckets.RemoveIfEmpty(simOrder, bucket);
                    RuntimeSlotReleaseRejectCount++;
                    if (!world.RuntimeCapacity.IsSealed)
                    {
                        UnityEngine.Debug.LogError(
                            "[SimulationWorld] Runtime slot allocation commit failed: " +
                            $"Slot={runtimeSlot}, StableId={registeredEntity.StableId}, " +
                            $"Type={registeredEntity.GetType().Name}");
                    }
                    return;
                }

                if (StructuralEventSink != null)
                {
                    EmitStructuralEvent(
                        "allocate",
                        runtimeSlot,
                        -1,
                        -1,
                        "free",
                        "active",
                        StructuralSourceKind(registeredEntity));
                }
            }

            bucket.items.Add(obj);
            bucket.dirty = true;
            obj.OnAdded(world.Context);
            if (obj is LF2Entity addedEntity &&
                TryGetCurrentRuntimeHandle(
                    addedEntity.Runtime.SlotIndex,
                    addedEntity,
                    out RuntimeEntityHandle runtimeHandle))
            {
                world.CharacterInputWriter.Bind(
                    addedEntity.Runtime,
                    runtimeHandle);
                world.IdentityWriter.Bind(addedEntity, runtimeHandle);
                world.FrameMotionWriter.Bind(addedEntity, runtimeHandle);
                world.RelationLinkWriter.Bind(
                    addedEntity.Runtime,
                    runtimeHandle);
                world.VitalWriter.Bind(addedEntity.Runtime, runtimeHandle);
                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry
                    .BindOwnerRuntime(addedEntity.Renderer, runtimeHandle);
            }

            if (EnableRegistryLifecycleLogging)
            {
                UnityEngine.Debug.Log(
                    $"[SimulationWorld] Registered: SimOrder={simOrder}, " +
                    $"StableId={obj.StableId}, Type={obj.GetType().Name}");
            }
        }

        internal void UnregisterCore(
            ISimObject obj,
            List<ISimObject> pendingUnregister)
        {
            if (obj == null)
            {
                MissingUnregisterCount++;
                if (!world.RuntimeCapacity.IsSealed)
                    UnityEngine.Debug.LogError(
                        "[SimulationWorld] Cannot unregister null object");
                return;
            }

            if (IsTicking)
            {
                int pendingSlot = -1;
                if (StructuralEventSink != null &&
                    obj is LF2Entity pendingSlotEntity)
                {
                    pendingSlot = pendingSlotEntity.Runtime?.SlotIndex ?? -1;
                }
                if (obj is LF2Entity pendingEntity &&
                    !ReleaseRuntimeSlotAndClearPresentationBinding(pendingEntity))
                {
                    return;
                }
                if (!pendingUnregister.Contains(obj))
                    pendingUnregister.Add(obj);
                if (StructuralEventSink != null)
                {
                    if (pendingSlot >= 0)
                        StructuralPendingUnregisterSlots[obj] = pendingSlot;
                    EmitStructuralEvent(
                        "unregister-deferred",
                        pendingSlot,
                        -1,
                        -1,
                        "active",
                        "pending",
                        obj is LF2Entity pendingSource
                            ? StructuralSourceKind(pendingSource)
                            : "general");
                }
                return;
            }

            UnregisterImmediate(obj);
        }

        internal void FlushPendingUnregister(
            List<ISimObject> pendingUnregister)
        {
            if (pendingUnregister.Count == 0)
                return;
            foreach (ISimObject obj in pendingUnregister)
            {
                int pendingSlot = -1;
                if (StructuralEventSink != null &&
                    StructuralPendingUnregisterSlots != null &&
                    StructuralPendingUnregisterSlots.TryGetValue(
                        obj,
                        out int recordedSlot))
                {
                    pendingSlot = recordedSlot;
                }
                UnregisterImmediate(obj);
                if (StructuralEventSink != null)
                {
                    EmitStructuralEvent(
                        "unregister-flush",
                        pendingSlot,
                        -1,
                        -1,
                        "pending",
                        "free",
                        obj is LF2Entity entity
                            ? StructuralSourceKind(entity)
                            : "general");
                    StructuralPendingUnregisterSlots?.Remove(obj);
                }
            }
            pendingUnregister.Clear();
        }

        internal void FlushPendingEntityDestroy(
            List<LF2Entity> pendingSlotReleasedDestroy,
            List<LF2Entity> entityScratch)
        {
            entityScratch.Clear();
            for (int index = 0;
                 index < pendingSlotReleasedDestroy.Count;
                 index++)
            {
                LF2Entity released = pendingSlotReleasedDestroy[index];
                if (released != null && !entityScratch.Contains(released))
                    entityScratch.Add(released);
            }
            pendingSlotReleasedDestroy.Clear();

            for (int runtimeSlot = 0;
                 runtimeSlot < RuntimeSlots.LogicalCapacity;
                 runtimeSlot++)
            {
                LF2Entity entity = RuntimeSlots.GetCurrentOccupant(runtimeSlot);
                if (entity?.Runtime != null &&
                    entity.Runtime.PendingFlushDestroy &&
                    !entityScratch.Contains(entity))
                {
                    entityScratch.Add(entity);
                }
            }

            for (int index = 0; index < entityScratch.Count; index++)
            {
                LF2Entity entity = entityScratch[index];
                if (entity.Runtime != null)
                    entity.Runtime.PendingFlushDestroy = false;
                entity.FreeEntityLikeExe();
            }

            entityScratch.Clear();
        }

        private void UnregisterImmediate(ISimObject obj)
        {
            int bucketKey = obj.SimOrder;
            CompatibilityBuckets.TryGetValue(
                bucketKey,
                out SimulationObjectBucket bucket);
            if (bucket == null || !bucket.items.Contains(obj))
            {
                bucket = null;
                for (int bucketIndex = 0;
                     bucketIndex < ObjectBuckets.OrderedCount;
                     bucketIndex++)
                {
                    SimulationObjectBucket candidateBucket =
                        ObjectBuckets.GetOrderedBucket(bucketIndex);
                    if (candidateBucket == null ||
                        !candidateBucket.items.Contains(obj))
                    {
                        continue;
                    }

                    bucketKey = candidateBucket.SimOrder;
                    bucket = candidateBucket;
                    break;
                }
            }

            if (bucket == null)
            {
                MissingUnregisterCount++;
                if (!world.RuntimeCapacity.IsSealed)
                {
                    UnityEngine.Debug.LogWarning(
                        "[SimulationWorld] Object not found in buckets: " +
                        $"CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                }
                return;
            }

            if (obj is LF2Entity entity &&
                entity.Runtime?.SlotIndex >= 0 &&
                !ReleaseRuntimeSlotAndClearPresentationBinding(entity))
            {
                return;
            }

            if (!bucket.items.Remove(obj))
            {
                MissingUnregisterCount++;
                if (!world.RuntimeCapacity.IsSealed)
                {
                    UnityEngine.Debug.LogWarning(
                        "[SimulationWorld] Object not found in buckets: " +
                        $"CurrentSimOrder={obj.SimOrder}, StableId={obj.StableId}");
                }
                return;
            }

            bucket.dirty = true;
            obj.OnRemoved(world.Context);
            ObjectBuckets.RemoveIfEmpty(bucketKey, bucket);
            if (EnableRegistryLifecycleLogging)
            {
                UnityEngine.Debug.Log(
                    $"[SimulationWorld] Unregistered: SimOrder={bucketKey}, " +
                    $"StableId={obj.StableId}, Type={obj.GetType().Name}");
            }
        }

        internal void ResetRegisteredObjects(
            HashSet<ISimObject> registeredObjects,
            List<ISimObject> pendingUnregister,
            List<LF2Entity> pendingSlotReleasedDestroy,
            List<LF2Entity> entityScratch)
        {
            (world.SceneQuery as BruteForceSceneQuery)?.ResetFormalSpatialBroadphase();

            registeredObjects.Clear();
            for (int bucketIndex = 0;
                 bucketIndex < ObjectBuckets.OrderedCount;
                 bucketIndex++)
            {
                SimulationObjectBucket bucket =
                    ObjectBuckets.GetOrderedBucket(bucketIndex);
                for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                {
                    ISimObject item = bucket.items[itemIndex];
                    if (item != null)
                        registeredObjects.Add(item);
                }
            }

            IsTicking = false;
            pendingUnregister.Clear();
            pendingSlotReleasedDestroy.Clear();
            entityScratch.Clear();

            foreach (ISimObject item in registeredObjects)
            {
                item.OnRemoved(world.Context);
                if (item is not LF2Entity entity)
                    continue;

                NTSD.Animation.Rendering.BattleCentralPresentationMountRegistry
                    .ResetOwnerRuntimeBinding(entity.Renderer);
                entity.ItrRest?.Unbind(false);
                entity.ItrRest?.Reset();
                entity.Runtime?.BindWorldMutationTracker(null);
                entity.Reset();
                entity.Runtime?.Reset();
                entity.SetRuntimeSlotIndex(-1);
                entity.ClearRequiredRuntimeSlot();
                entity.FrameCache?.Clear();
                if (entity.Frame != null)
                {
                    entity.Frame.PN = 0;
                    entity.Frame.Prev = 0;
                    entity.WriteCurrentFrameId(0);
                    entity.Frame.D = null;
                    entity.Frame.Prev2 = 0;
                    entity.Frame.Prev2D = null;
                }

                entity.Trans?.Reset();
                entity.Effect?.Reset();
                entity.Sprite?.SetPresentationSuppressed(true);
                entity.Sprite?.Hide();
                entity.Sprite?.HideShadow();
            }

            ObjectBuckets.Clear();
            world.AiUnifiedRowPublisherForServices.EndPass();
            RuntimeSlots.Reset();
            world.IdentityWriter.Reset();
            world.CharacterInputWriter.Reset();
            world.FrameMotionWriter.Reset();
            world.RelationLinkWriter.Reset();
            world.VitalWriter.Reset();
            RuntimeRestStore.ResetWorld();
            registeredObjects.Clear();
        }

        internal bool TryShutdownAndClearLogicState(
            List<LF2Entity> entityScratch,
            out int releasedLogicEntities,
            out string failureReason)
        {
            releasedLogicEntities = 0;
            failureReason = string.Empty;
            if (IsTicking)
            {
                failureReason = "simulation-world-is-still-ticking";
                return false;
            }

            entityScratch.Clear();
            for (int bucketIndex = 0;
                 bucketIndex < ObjectBuckets.OrderedCount;
                 bucketIndex++)
            {
                SimulationObjectBucket bucket =
                    ObjectBuckets.GetOrderedBucket(bucketIndex);
                if (bucket == null)
                    continue;

                for (int itemIndex = 0; itemIndex < bucket.items.Count; itemIndex++)
                {
                    if (bucket.items[itemIndex] is not LF2Entity entity ||
                        entityScratch.Contains(entity))
                    {
                        continue;
                    }

                    if (!ReferenceEquals(entity.Renderer, null) ||
                        !ReferenceEquals(entity.ShadowRenderer, null))
                    {
                        entityScratch.Clear();
                        failureReason =
                            "renderer-binding-remained-before-world-logic-cleanup";
                        return false;
                    }

                    entityScratch.Add(entity);
                }
            }

            for (int index = 0; index < entityScratch.Count; index++)
            {
                world.LogicReferencePool?.Release(entityScratch[index]);
                releasedLogicEntities++;
            }

            world.ResetRuntimeState();
            if (world.ObjectCount != 0 || RuntimeSlots.ClaimedCount != 0 ||
                ObjectBuckets.OrderedCount != 0 ||
                world.LogicReferencePool?.ActiveCount != 0)
            {
                failureReason =
                    "world-registry-or-runtime-slots-remained-after-shutdown-reset";
                return false;
            }

            return true;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        internal void GetActiveRuntimeEntitySnapshot(List<LF2Entity> destination)
        {
            if (destination == null)
                return;

            destination.Clear();
            activeRuntimeEntitySnapshotSeen.Clear();
            try
            {
                for (int runtimeSlot = 0;
                     runtimeSlot < RuntimeSlots.LogicalCapacity;
                     runtimeSlot++)
                {
                    RuntimeSlotTable.ReadOnlySlotView view =
                        RuntimeSlots.GetReadOnlyView(runtimeSlot);
                    if (!view.Claimed || view.Entity == null ||
                        view.Generation == 0)
                    {
                        continue;
                    }

                    var handle = new RuntimeEntityHandle(
                        runtimeSlot,
                        view.Generation);
                    if (!RuntimeSlots.TryResolve(handle, out LF2Entity entity) ||
                        entity == null ||
                        entity.Runtime?.PendingFlushDestroy == true ||
                        !activeRuntimeEntitySnapshotSeen.Add(entity))
                    {
                        continue;
                    }

                    destination.Add(entity);
                }

                destination.Sort(ActiveRuntimeEntitySnapshotComparison);
            }
            finally
            {
                activeRuntimeEntitySnapshotSeen.Clear();
            }
        }
#endif
    }
}
