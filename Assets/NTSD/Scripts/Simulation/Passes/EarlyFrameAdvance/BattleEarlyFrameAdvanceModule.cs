using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the early frame-advance teleport and state 500/501 special pass.
    /// World retains only the stable scheduling and diagnostics façades.
    /// </summary>
    internal sealed class BattleEarlyFrameAdvanceModule
    {
        private readonly SimulationWorld world;
        private readonly List<LF2Entity> activeEntities =
            new List<LF2Entity>(16);
        private readonly List<RuntimeEntityHandle> state500Handles =
            new List<RuntimeEntityHandle>(16);
        private readonly List<RuntimeEntityHandle> state501Handles =
            new List<RuntimeEntityHandle>(16);

        internal BattleEarlyFrameAdvanceModule(SimulationWorld world)
        {
            this.world = world;
        }

        internal bool ForceLegacyForDiagnostics { get; set; }
        internal int LastTeleportRefreshCountForDiagnostics { get; private set; }
        internal int LastTeleportSnapshotSkipCountForDiagnostics { get; private set; }
        internal bool LastStateHandlePathUsedForDiagnostics { get; private set; }
        internal int LastStateHandleFallbackCountForDiagnostics { get; private set; }

        internal void PrepareCapacity(int entityCapacity)
        {
            EnsureCapacity(activeEntities, entityCapacity);
            EnsureCapacity(state500Handles, entityCapacity);
            EnsureCapacity(state501Handles, entityCapacity);
        }

        internal void Run(int tickIndex)
        {
            LastTeleportRefreshCountForDiagnostics = 0;
            LastTeleportSnapshotSkipCountForDiagnostics = 0;
            LastStateHandlePathUsedForDiagnostics = false;
            LastStateHandleFallbackCountForDiagnostics = 0;

            if (ForceLegacyForDiagnostics)
            {
                RunLegacy();
                return;
            }

            bool teleportGate = world.FrameToggle != 0;
            bool handleSnapshotValid = TryBuildStateHandleSnapshot(
                out ulong occupancyEpoch,
                out int logicalCapacity);
            if (!handleSnapshotValid)
            {
                // A partial slot-table proof cannot stand in for the authority's
                // complete active snapshot. Rebuild the exact legacy view before
                // running any entity callbacks.
                world.GetActiveEntitiesByRuntimeSlotForModule(activeEntities);
                state500Handles.Clear();
                state501Handles.Clear();
            }

            for (int i = 0; i < activeEntities.Count; i++)
            {
                LF2Entity entity = activeEntities[i];
                if (entity == null)
                    continue;

                bool mutated =
                    entity.RunEarlyTeleportSpecialsPhaseWithMutationReport(
                        activeEntities,
                        teleportGate);
                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;
                if (mutated)
                {
                    LastTeleportRefreshCountForDiagnostics++;
                    world.RefreshRuntimeSnapshotForModule(entity);
                }
                else
                {
                    LastTeleportSnapshotSkipCountForDiagnostics++;
                }
            }

            if (handleSnapshotValid &&
                ValidateStateHandleSnapshot(
                    occupancyEpoch,
                    logicalCapacity))
            {
                LastStateHandlePathUsedForDiagnostics = true;
                if (!RunStateHandles(
                        state500Handles,
                        500,
                        occupancyEpoch,
                        logicalCapacity) ||
                    !RunStateHandles(
                        state501Handles,
                        501,
                        occupancyEpoch,
                        logicalCapacity))
                {
                    LastStateHandlePathUsedForDiagnostics = false;
                    LastStateHandleFallbackCountForDiagnostics++;
                    RunState500Specials(activeEntities);
                    RunState501Specials(activeEntities);
                }
            }
            else
            {
                LastStateHandleFallbackCountForDiagnostics++;
                RunState500Specials(activeEntities);
                RunState501Specials(activeEntities);
            }

            state500Handles.Clear();
            state501Handles.Clear();
            activeEntities.Clear();
        }

        private void RunLegacy()
        {
            bool teleportGate = world.FrameToggle != 0;
            world.GetActiveEntitiesByRuntimeSlotForModule(activeEntities);
            for (int i = 0; i < activeEntities.Count; i++)
            {
                LF2Entity entity = activeEntities[i];
                if (entity == null)
                    continue;

                entity.RunEarlyTeleportSpecialsPhase(
                    activeEntities,
                    teleportGate);
                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;
                LastTeleportRefreshCountForDiagnostics++;
                world.RefreshRuntimeSnapshotForModule(entity);
            }

            RunState500Specials(activeEntities);
            RunState501Specials(activeEntities);
            activeEntities.Clear();
        }

        private bool TryBuildStateHandleSnapshot(
            out ulong occupancyEpoch,
            out int logicalCapacity)
        {
            activeEntities.Clear();
            state500Handles.Clear();
            state501Handles.Clear();

            occupancyEpoch =
                world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance;
            logicalCapacity =
                world.RuntimeSlotLogicalCapacityForEarlyFrameAdvance;
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
                if (!world.TryResolveRuntimeHandleForEarlyFrameAdvance(
                        handle,
                        out LF2Entity resolved) ||
                    !ReferenceEquals(resolved, entity))
                {
                    return false;
                }

                if (!world.IsActiveForCurrentPassInternal(entity))
                    continue;

                activeEntities.Add(entity);
                int state = entity.Frame?.D?.state ?? -1;
                if (state == 500)
                    state500Handles.Add(handle);
                else if (state == 501)
                    state501Handles.Add(handle);
            }

            return occupancyEpoch ==
                       world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance &&
                   logicalCapacity ==
                       world.RuntimeSlotLogicalCapacityForEarlyFrameAdvance;
        }

        private bool ValidateStateHandleSnapshot(
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            if (occupancyEpoch !=
                    world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance ||
                logicalCapacity !=
                    world.RuntimeSlotLogicalCapacityForEarlyFrameAdvance)
            {
                return false;
            }

            return ValidateStateHandles(state500Handles, 500) &&
                   ValidateStateHandles(state501Handles, 501) &&
                   occupancyEpoch ==
                       world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance;
        }

        private bool ValidateStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (!TryResolveStateHandle(
                        handles[i],
                        expectedState,
                        out _))
                {
                    return false;
                }
            }

            return true;
        }

        private bool RunStateHandles(
            List<RuntimeEntityHandle> handles,
            int expectedState,
            ulong occupancyEpoch,
            int logicalCapacity)
        {
            for (int i = 0; i < handles.Count; i++)
            {
                if (occupancyEpoch !=
                        world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance ||
                    logicalCapacity !=
                        world.RuntimeSlotLogicalCapacityForEarlyFrameAdvance ||
                    !TryResolveStateHandle(
                        handles[i],
                        expectedState,
                        out LF2Entity entity))
                {
                    return false;
                }

                if (expectedState == 500)
                    RunState500Special(entity);
                else
                    RunState501Special(entity, activeEntities);
            }

            return occupancyEpoch ==
                       world.RuntimeSlotOccupancyEpochForEarlyFrameAdvance &&
                   logicalCapacity ==
                       world.RuntimeSlotLogicalCapacityForEarlyFrameAdvance;
        }

        private bool TryResolveStateHandle(
            RuntimeEntityHandle handle,
            int expectedState,
            out LF2Entity entity)
        {
            entity = null;
            if (!handle.IsValid ||
                !world.TryResolveRuntimeHandleForEarlyFrameAdvance(
                    handle,
                    out LF2Entity resolved) ||
                resolved == null ||
                resolved.Runtime == null ||
                resolved.Runtime.SlotIndex != handle.Slot ||
                !world.IsActiveForCurrentPassInternal(resolved) ||
                resolved.Frame?.D?.state != expectedState)
            {
                return false;
            }

            entity = resolved;
            return true;
        }

        private void RunState500Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunState500Special(entity);
            }
        }

        private void RunState500Special(LF2Entity entity)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null || frame.state != 500)
                return;

            if (entity.TransformTargetObjectId == -1 ||
                entity.TransformOriginalObjectId >= 0)
            {
                // BMD-023: state=500 reset branch must mirror baseline SetFrameImmediate:
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                entity.DirectWriteFramePreserveWaitCounter(0);
                world.RefreshRuntimeSnapshotForModule(entity);
            }
        }

        private void RunState501Specials(List<LF2Entity> entities)
        {
            if (entities == null || entities.Count == 0)
                return;

            for (int i = 0; i < entities.Count; i++)
            {
                LF2Entity entity = entities[i];
                RunState501Special(entity, entities);
            }
        }

        private void RunState501Special(
            LF2Entity entity,
            List<LF2Entity> activeEntities)
        {
            LF2FrameData frame = entity?.Frame?.D;
            if (frame == null ||
                frame.state != 501 ||
                entity.TransformTargetObjectId <= -1)
            {
                return;
            }

            LF2CharacterDataWrapper wrapper =
                world.RuntimeCharacterConfigs.Resolve(
                    entity.TransformTargetObjectId);
            if (wrapper == null)
                return;

            entity.TransformOriginalObjectId = entity.ObjectId;
            entity.FrameCache.Load(wrapper);
            entity.ObjectId = entity.TransformTargetObjectId;
            // BMD-023: state=501 transform branch must mirror baseline SetFrameImmediate:
            // write Frame + FrameWaitCounter only, never Attacking. Unity's
            // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
            entity.DirectWriteRawFramePreserveWaitCounter(0);
            world.RefreshRuntimeSnapshotForModule(entity);

            int ownerSlotIndex = entity.Runtime?.SlotIndex ?? -1;
            if (ownerSlotIndex < 0)
                return;

            for (int j = 0; j < activeEntities.Count; j++)
            {
                LF2Entity child = activeEntities[j];
                if (child == null)
                    continue;
                if (child.KillCount != ownerSlotIndex)
                    continue;
                if (child.Health != null && child.Health.HP <= 0)
                    continue;

                child.FrameCache.Load(wrapper);
                child.ObjectId = entity.ObjectId;
                // BMD-023: state=501 child-transform branch must mirror baseline SetFrameImmediate.
                // The authority selects from the integer Y snapshot, not the floating render position.
                // write Frame + FrameWaitCounter only, never Attacking. Unity's
                // ImmediateFrame zeros AttackingCounter as a side effect (LF2Entity.cs:824).
                child.DirectWriteRawFramePreserveWaitCounter(
                    child.Runtime != null && child.Runtime.YInt < 0
                        ? 212
                        : 0);
                world.RefreshRuntimeSnapshotForModule(child);
            }
        }

        private static void EnsureCapacity<T>(List<T> values, int capacity)
        {
            if (values.Capacity < capacity)
                values.Capacity = capacity;
        }
    }
}
