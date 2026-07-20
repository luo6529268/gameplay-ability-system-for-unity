using System;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    [Flags]
    public enum SimulationInputButtons : byte
    {
        None = 0,
        Right = 1 << 0,
        Left = 1 << 1,
        Up = 1 << 2,
        Down = 1 << 3,
        Attack = 1 << 4,
        Jump = 1 << 5,
        Defend = 1 << 6,
    }

    [Serializable]
    public readonly struct SimulationPlayerInput
    {
        public SimulationPlayerInput(int playerSlot, SimulationInputButtons buttons)
        {
            PlayerSlot = playerSlot;
            Buttons = buttons;
        }

        public int PlayerSlot { get; }
        public SimulationInputButtons Buttons { get; }
    }

    [Serializable]
    public sealed class FrameInputSet
    {
        private static readonly IReadOnlyList<SimulationPlayerInput> NoPlayers =
            Array.Empty<SimulationPlayerInput>();

        public FrameInputSet(int tickIndex, IReadOnlyList<SimulationPlayerInput> players = null)
        {
            TickIndex = tickIndex;
            Players = players ?? NoPlayers;
        }

        public int TickIndex { get; }
        public IReadOnlyList<SimulationPlayerInput> Players { get; }

        public static FrameInputSet Empty(int tickIndex)
        {
            return new FrameInputSet(tickIndex);
        }

        internal static Dictionary<int, FrameInputSet> BuildDenseTraceTimeline(
            int ticks,
            IEnumerable<int> activeHumanPlayerSlots,
            IEnumerable<FrameInputSet> sparseFrames)
        {
            var orderedSlots = new List<int>();
            var heldButtons = new Dictionary<int, SimulationInputButtons>();
            if (activeHumanPlayerSlots != null)
            {
                foreach (int playerSlot in activeHumanPlayerSlots)
                {
                    if (heldButtons.ContainsKey(playerSlot))
                        continue;

                    heldButtons[playerSlot] = SimulationInputButtons.None;
                    orderedSlots.Add(playerSlot);
                }
            }
            orderedSlots.Sort();

            var updatesByTick = new Dictionary<int, List<SimulationPlayerInput>>();
            if (sparseFrames != null)
            {
                foreach (FrameInputSet frame in sparseFrames)
                {
                    if (frame == null || frame.TickIndex <= 0 || frame.TickIndex > ticks)
                        continue;
                    if (!updatesByTick.TryGetValue(frame.TickIndex, out List<SimulationPlayerInput> updates))
                    {
                        updates = new List<SimulationPlayerInput>();
                        updatesByTick[frame.TickIndex] = updates;
                    }

                    for (int i = 0; i < frame.Players.Count; i++)
                        updates.Add(frame.Players[i]);
                }
            }

            var result = new Dictionary<int, FrameInputSet>();
            for (int tick = 1; tick <= ticks; tick++)
            {
                if (updatesByTick.TryGetValue(tick, out List<SimulationPlayerInput> updates))
                {
                    for (int i = 0; i < updates.Count; i++)
                    {
                        SimulationPlayerInput update = updates[i];
                        if (heldButtons.ContainsKey(update.PlayerSlot))
                            heldButtons[update.PlayerSlot] = update.Buttons;
                    }
                }

                var players = new SimulationPlayerInput[orderedSlots.Count];
                for (int i = 0; i < orderedSlots.Count; i++)
                {
                    int playerSlot = orderedSlots[i];
                    players[i] = new SimulationPlayerInput(playerSlot, heldButtons[playerSlot]);
                }
                result[tick] = new FrameInputSet(tick, players);
            }
            return result;
        }
    }
}
