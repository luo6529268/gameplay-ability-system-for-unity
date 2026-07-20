using NTSD.Animation.LF2Objects;
using NTSD.Input;

namespace NTSD.Simulation
{
    public partial class SimulationWorld
    {
        private static readonly (SimulationInputButtons button, FuncKeyMask key)[] FrameInputKeys =
        {
            (SimulationInputButtons.Right, FuncKeyMask.right),
            (SimulationInputButtons.Left, FuncKeyMask.left),
            (SimulationInputButtons.Up, FuncKeyMask.up),
            (SimulationInputButtons.Down, FuncKeyMask.down),
            (SimulationInputButtons.Attack, FuncKeyMask.att),
            (SimulationInputButtons.Jump, FuncKeyMask.jump),
            (SimulationInputButtons.Defend, FuncKeyMask.def),
        };

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

                // The frame packet is a complete held-state snapshot. Queue every key so an
                // authoritative replay packet is applied after any local callback queued for
                // the same tick; NTSDInputStateModule derives the press/release edges once.
                for (int keyIndex = 0; keyIndex < FrameInputKeys.Length; keyIndex++)
                {
                    (SimulationInputButtons button, FuncKeyMask key) mapping = FrameInputKeys[keyIndex];
                    bool down = (playerInput.Buttons & mapping.button) != 0;
                    controller.InputBuffer.EnqueueForTick(frameInput.TickIndex, mapping.key, down);
                }
            }
        }

        internal bool TryResolveRosterInputEntity(int playerSlot, out LF2Entity entity)
        {
            return TryResolveRosterEntity(playerSlot, requireHuman: true, out entity);
        }

        internal bool TryResolveRosterEntity(int playerSlot, bool requireHuman, out LF2Entity entity)
        {
            entity = null;
            BattleRosterRuntimeState roster = Runtime?.Roster;
            if (roster?.Slots == null || playerSlot < 0 || playerSlot >= roster.Slots.Length)
                return false;

            BattleSlotRuntimeState rosterSlot = roster.Slots[playerSlot];
            if (rosterSlot == null || !rosterSlot.Active || (requireHuman && !rosterSlot.IsHuman))
                return false;

            entity = ResolveBoundRosterSlotEntity(rosterSlot.RuntimeSlotIndex, rosterSlot);
            if (entity == null && rosterSlot.StableId >= 0)
                entity = FindRosterEntityByStableId(rosterSlot.StableId, rosterSlot);

            if (entity == null)
                entity = ResolveRosterSlotEntity(playerSlot, rosterSlot);

            if (entity == null)
            {
                for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
                {
                    LF2Entity candidate = ResolveRosterSlotEntity(runtimeSlot, rosterSlot);
                    if (candidate == null || IsRuntimeSlotBoundToOtherRosterPlayer(runtimeSlot, playerSlot))
                        continue;

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

        internal void RefreshActiveHumanRosterInputBindings()
        {
            BattleSlotRuntimeState[] rosterSlots = Runtime?.Roster?.Slots;
            if (rosterSlots == null)
                return;

            for (int playerSlot = 0; playerSlot < rosterSlots.Length; playerSlot++)
            {
                BattleSlotRuntimeState rosterSlot = rosterSlots[playerSlot];
                if (rosterSlot?.Active == true && rosterSlot.IsHuman)
                    TryResolveRosterInputEntity(playerSlot, out _);
            }
        }

        internal bool IsBoundActiveHumanRosterInputEntity(LF2Entity entity)
        {
            if (entity?.Runtime == null || entity.AiControlled)
                return false;

            BattleSlotRuntimeState[] rosterSlots = Runtime?.Roster?.Slots;
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

                if (rosterSlot.StableId < 0 || rosterSlot.StableId == entity.Runtime.StableId)
                    return true;
            }

            return false;
        }

        private LF2Entity ResolveBoundRosterSlotEntity(int runtimeSlot, BattleSlotRuntimeState rosterSlot)
        {
            if (runtimeSlot < 0 || runtimeSlot >= MaxRuntimeSlots)
                return null;

            LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return BoundRosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity ResolveRosterSlotEntity(int runtimeSlot, BattleSlotRuntimeState rosterSlot)
        {
            if (runtimeSlot < 0 || runtimeSlot >= MaxRuntimeSlots)
                return null;

            LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
            return RosterEntityMatches(candidate, rosterSlot) ? candidate : null;
        }

        private LF2Entity FindRosterEntityByStableId(int stableId, BattleSlotRuntimeState rosterSlot)
        {
            for (int runtimeSlot = 0; runtimeSlot < MaxRuntimeSlots; runtimeSlot++)
            {
                LF2Entity candidate = FindEntityByRuntimeSlotIncludingDormant(runtimeSlot);
                if (candidate?.Runtime?.StableId == stableId && BoundRosterEntityMatches(candidate, rosterSlot))
                    return candidate;
            }

            return null;
        }

        private bool IsRuntimeSlotBoundToOtherRosterPlayer(int runtimeSlot, int playerSlot)
        {
            BattleSlotRuntimeState[] rosterSlots = Runtime?.Roster?.Slots;
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

        private bool RosterEntityMatches(LF2Entity candidate, BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null || !IsActiveForCurrentPass(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
                return false;
            if (candidate.GetCurrentDataObjectTypeForSimulation() != (int)LF2ObjectType.Character)
                return false;
            if (rosterSlot.CharacterId >= 0 && candidate.ObjectId != rosterSlot.CharacterId)
                return false;
            return candidate.Team == rosterSlot.Team;
        }

        private bool BoundRosterEntityMatches(LF2Entity candidate, BattleSlotRuntimeState rosterSlot)
        {
            if (candidate?.Runtime == null || !IsActiveForCurrentPass(candidate) ||
                candidate.AiControlled == rosterSlot.IsHuman)
            {
                return false;
            }

            return rosterSlot.StableId < 0 || candidate.Runtime.StableId == rosterSlot.StableId;
        }
    }
}
