using NTSD.Animation.LF2Objects;
using NTSD.Input;

namespace NTSD.Simulation
{
    /// <summary>
    /// Owns the match-scoped frame-input application and roster binding rules.
    /// The world keeps one module instance so no mutable input state is shared
    /// across battles.
    /// </summary>
    internal sealed class SimulationFrameInputModule
    {
        private readonly struct FrameInputMapping
        {
            public FrameInputMapping(SimulationInputButtons button, FuncKeyMask key)
            {
                Button = button;
                Key = key;
            }

            public SimulationInputButtons Button { get; }
            public FuncKeyMask Key { get; }
        }

        private readonly SimulationWorld world;
        private readonly FrameInputMapping[] mappings =
        {
            new FrameInputMapping(SimulationInputButtons.Right, FuncKeyMask.right),
            new FrameInputMapping(SimulationInputButtons.Left, FuncKeyMask.left),
            new FrameInputMapping(SimulationInputButtons.Up, FuncKeyMask.up),
            new FrameInputMapping(SimulationInputButtons.Down, FuncKeyMask.down),
            new FrameInputMapping(SimulationInputButtons.Attack, FuncKeyMask.att),
            new FrameInputMapping(SimulationInputButtons.Jump, FuncKeyMask.jump),
            new FrameInputMapping(SimulationInputButtons.Defend, FuncKeyMask.def),
        };

        public SimulationFrameInputModule(SimulationWorld world)
        {
            this.world = world;
        }

        public void ApplyFrameInputSet(FrameInputSet frameInput)
        {
            if (frameInput?.Players == null || frameInput.Players.Count == 0)
                return;

            for (int i = 0; i < frameInput.Players.Count; i++)
            {
                SimulationPlayerInput playerInput = frameInput.Players[i];
                if (!TryResolveRosterInputEntity(playerInput.PlayerSlot, out LF2Entity entity) ||
                    entity.AiControlled ||
                    !entity.TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                {
                    continue;
                }

                if (entity is LF2Character character)
                    character.InputState?.SyncProgressFromRuntime(entity.Runtime);

                for (int keyIndex = 0; keyIndex < mappings.Length; keyIndex++)
                {
                    FrameInputMapping mapping = mappings[keyIndex];
                    bool down = (playerInput.Buttons & mapping.Button) != 0;
                    controller.InputBuffer.EnqueueCompletePacketKeyForTick(
                        frameInput.TickIndex,
                        mapping.Key,
                        down);
                }
            }
        }

        public bool TryCaptureLocalFrameInput(
            int tickIndex,
            SimulationPlayerInput[] destination,
            out int playerCount)
        {
            playerCount = 0;
            BattleSlotRuntimeState[] rosterSlots = world.Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return true;

            for (int playerSlot = 0; playerSlot < rosterSlots.Length; playerSlot++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[playerSlot];
                if (rosterSlot?.Active != true || !rosterSlot.IsHuman)
                    continue;

                if (destination == null || playerCount >= destination.Length ||
                    !TryResolveRosterInputEntity(playerSlot, out LF2Entity entity) ||
                    !entity.TryGetSharedInputControllerForSimulation(out ILF2Controller controller) ||
                    controller is not ILocalFrameInputSource localSource)
                {
                    playerCount = 0;
                    return false;
                }

                destination[playerCount++] = new SimulationPlayerInput(
                    playerSlot,
                    localSource.CaptureHeldSimulationButtons());
            }

            return true;
        }

        public void DiscardDirectLocalInputTick(int tickIndex)
        {
            BattleSlotRuntimeState[] rosterSlots = world.Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return;

            for (int playerSlot = 0; playerSlot < rosterSlots.Length; playerSlot++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[playerSlot];
                if (rosterSlot?.Active != true || !rosterSlot.IsHuman ||
                    !TryResolveRosterInputEntity(playerSlot, out LF2Entity entity) ||
                    !entity.TryGetSharedInputControllerForSimulation(out ILF2Controller controller))
                {
                    continue;
                }

                controller.InputBuffer?.DiscardTick(tickIndex);
            }
        }

        public bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity)
        {
            return TryResolveRosterEntity(playerSlot, requireHuman: true, out entity);
        }

        public bool TryResolveRosterEntity(
            int playerSlot,
            bool requireHuman,
            out LF2Entity entity)
        {
            entity = null;
            BattleRosterRuntimeState roster = world.Runtime?.Roster;
            if (roster?.Slots == null || playerSlot < 0 || playerSlot >= roster.Slots.Length)
                return false;

            BattleSlotRuntimeState rosterSlot = roster.Slots[playerSlot];
            if (rosterSlot == null || !rosterSlot.Active ||
                (requireHuman && !rosterSlot.IsHuman))
            {
                return false;
            }

            entity = ResolveBoundRosterSlotEntity(rosterSlot.RuntimeSlotIndex, rosterSlot);
            if (entity == null && rosterSlot.StableId >= 0)
                entity = FindRosterEntityByStableId(rosterSlot.StableId, rosterSlot);

            if (entity == null)
                entity = ResolveRosterSlotEntity(playerSlot, rosterSlot);

            if (entity == null)
            {
                int runtimeSlotCapacity = world.RuntimeSlotCapacityForDiagnostics;
                for (int runtimeSlot = 0; runtimeSlot < runtimeSlotCapacity; runtimeSlot++)
                {
                    LF2Entity candidate = ResolveRosterSlotEntity(runtimeSlot, rosterSlot);
                    if (candidate == null ||
                        IsRuntimeSlotBoundToOtherRosterPlayer(runtimeSlot, playerSlot))
                    {
                        continue;
                    }

                    entity = candidate;
                    break;
                }
            }

            if (entity == null)
                return false;

            rosterSlot.RuntimeSlotIndex = entity.Runtime.SlotIndex;
            rosterSlot.StableId = entity.Runtime.StableId;
            return true;
        }

        public void RefreshActiveHumanRosterInputBindings()
        {
            BattleSlotRuntimeState[] rosterSlots = world.Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return;

            for (int playerSlot = 0; playerSlot < rosterSlots.Length; playerSlot++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[playerSlot];
                if (rosterSlot?.Active == true && rosterSlot.IsHuman)
                    TryResolveRosterInputEntity(playerSlot, out _);
            }
        }

        public bool IsBoundActiveHumanRosterInputEntity(LF2Entity entity)
        {
            if (entity?.Runtime == null || entity.AiControlled)
                return false;

            BattleSlotRuntimeState[] rosterSlots = world.Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return false;

            for (int playerSlot = 0; playerSlot < rosterSlots.Length; playerSlot++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[playerSlot];
                if (rosterSlot?.Active != true || !rosterSlot.IsHuman ||
                    rosterSlot.RuntimeSlotIndex != entity.Runtime.SlotIndex)
                {
                    continue;
                }

                if (rosterSlot.StableId < 0 ||
                    rosterSlot.StableId == entity.Runtime.StableId)
                {
                    return true;
                }
            }

            return false;
        }

        private LF2Entity ResolveBoundRosterSlotEntity(
            int runtimeSlot,
            BattleSlotRuntimeState rosterSlot)
        {
            if (!IsValidRuntimeSlot(runtimeSlot))
                return null;

            LF2Entity candidate = world.FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return BoundRosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity ResolveRosterSlotEntity(
            int runtimeSlot,
            BattleSlotRuntimeState rosterSlot)
        {
            if (!IsValidRuntimeSlot(runtimeSlot))
                return null;

            LF2Entity candidate = world.FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return RosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity FindRosterEntityByStableId(
            int stableId,
            BattleSlotRuntimeState rosterSlot)
        {
            int runtimeSlotCapacity = world.RuntimeSlotCapacityForDiagnostics;
            for (int runtimeSlot = 0; runtimeSlot < runtimeSlotCapacity; runtimeSlot++)
            {
                LF2Entity candidate = world.FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (candidate?.Runtime?.StableId == stableId &&
                    BoundRosterEntityMatches(candidate, rosterSlot))
                {
                    return candidate;
                }
            }

            return null;
        }

        private bool IsRuntimeSlotBoundToOtherRosterPlayer(int runtimeSlot, int playerSlot)
        {
            BattleSlotRuntimeState[] rosterSlots = world.Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return false;

            for (int i = 0; i < rosterSlots.Length; i++)
            {
                if (i != playerSlot && rosterSlots[i]?.Active == true &&
                    rosterSlots[i].RuntimeSlotIndex == runtimeSlot)
                {
                    return true;
                }
            }

            return false;
        }

        private bool RosterEntityMatches(
            LF2Entity candidate,
            BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null ||
                !world.IsActiveForCurrentPassInternal(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
            {
                return false;
            }

            if (candidate.GetCurrentDataObjectTypeForSimulation() !=
                (int)LF2ObjectType.Character)
            {
                return false;
            }

            if (rosterSlot.CharacterId >= 0 &&
                candidate.ObjectId != rosterSlot.CharacterId)
            {
                return false;
            }

            return candidate.Team == rosterSlot.Team;
        }

        private bool BoundRosterEntityMatches(
            LF2Entity candidate,
            BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null ||
                !world.IsActiveForCurrentPassInternal(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
            {
                return false;
            }

            return rosterSlot.StableId < 0 ||
                   candidate.Runtime.StableId == rosterSlot.StableId;
        }

        private bool IsValidRuntimeSlot(int runtimeSlot)
        {
            return runtimeSlot >= 0 &&
                   runtimeSlot < world.RuntimeSlotCapacityForDiagnostics;
        }
    }
}
