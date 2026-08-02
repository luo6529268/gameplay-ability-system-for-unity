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
        public SimulationPlayerInput(
            int playerSlot,
            SimulationInputButtons buttons,
            SimulationInputButtons pressedButtons = SimulationInputButtons.None,
            SimulationInputButtons releasedButtons = SimulationInputButtons.None)
        {
            PlayerSlot = playerSlot;
            Buttons = buttons;
            PressedButtons = pressedButtons;
            ReleasedButtons = releasedButtons;
        }

        public int PlayerSlot { get; }
        public SimulationInputButtons Buttons { get; }
        public SimulationInputButtons PressedButtons { get; }
        public SimulationInputButtons ReleasedButtons { get; }

        public bool CanonicalEquals(in SimulationPlayerInput other)
        {
            return PlayerSlot == other.PlayerSlot &&
                   Buttons == other.Buttons &&
                   PressedButtons == other.PressedButtons &&
                   ReleasedButtons == other.ReleasedButtons;
        }
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

        public int TickIndex { get; private set; }
        public IReadOnlyList<SimulationPlayerInput> Players { get; private set; }

        internal void ResetPreallocated(int tickIndex, SimulationPlayerInput[] players)
        {
            TickIndex = tickIndex;
            Players = players ?? NoPlayers;
        }

        public bool IsCanonicalFor(int expectedTick, IReadOnlyList<int> canonicalPlayerSlots)
        {
            if (TickIndex != expectedTick || canonicalPlayerSlots == null ||
                Players == null || Players.Count != canonicalPlayerSlots.Count)
            {
                return false;
            }

            for (int i = 0; i < canonicalPlayerSlots.Count; i++)
            {
                if (Players[i].PlayerSlot != canonicalPlayerSlots[i])
                    return false;
            }

            return true;
        }

        public ulong GetCanonicalHash64()
        {
            const ulong offset = 14695981039346656037UL;
            const ulong prime = 1099511628211UL;
            ulong hash = offset;
            AddInt(ref hash, TickIndex, prime);
            AddInt(ref hash, Players?.Count ?? 0, prime);
            if (Players == null)
                return hash;

            for (int i = 0; i < Players.Count; i++)
            {
                SimulationPlayerInput player = Players[i];
                AddInt(ref hash, player.PlayerSlot, prime);
                hash = (hash ^ (byte)player.Buttons) * prime;
                hash = (hash ^ (byte)player.PressedButtons) * prime;
                hash = (hash ^ (byte)player.ReleasedButtons) * prime;
            }

            return hash;
        }

        public static FrameInputSet Empty(int tickIndex)
        {
            return new FrameInputSet(tickIndex);
        }

        private static void AddInt(ref ulong hash, int value, ulong prime)
        {
            unchecked
            {
                uint bits = (uint)value;
                hash = (hash ^ (byte)bits) * prime;
                hash = (hash ^ (byte)(bits >> 8)) * prime;
                hash = (hash ^ (byte)(bits >> 16)) * prime;
                hash = (hash ^ (byte)(bits >> 24)) * prime;
            }
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
