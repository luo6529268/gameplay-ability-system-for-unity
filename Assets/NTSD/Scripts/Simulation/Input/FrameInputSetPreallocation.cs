using System;
using System.Collections;
using System.Collections.Generic;

namespace NTSD.Simulation
{
    /// <summary>
    /// Client-owned reusable storage for hot paths. Public FrameInputSet values remain
    /// platform-independent and read-only; only instances created here can be reset.
    /// </summary>
    internal static class FrameInputSetPreallocation
    {
        internal static FrameInputSet CreateReusable()
        {
            return new ReusableFrameInputSet();
        }

        internal static void ResetPreallocated(
            this FrameInputSet frame,
            int tickIndex,
            SimulationPlayerInput[] players)
        {
            ResetPreallocated(frame, tickIndex, players, players?.Length ?? 0);
        }

        internal static void ResetPreallocated(
            this FrameInputSet frame,
            int tickIndex,
            SimulationPlayerInput[] players,
            int playerCount)
        {
            if (frame is not ReusableFrameInputSet reusable)
            {
                throw new InvalidOperationException(
                    "Only a Client-owned reusable FrameInputSet can be reset.");
            }

            reusable.Reset(tickIndex, players, playerCount);
        }

        internal static bool IsReusable(FrameInputSet frame)
        {
            return frame is ReusableFrameInputSet;
        }

        private sealed class ReusableFrameInputSet : FrameInputSet
        {
            private static readonly IReadOnlyList<SimulationPlayerInput> NoPlayers =
                Array.Empty<SimulationPlayerInput>();
            private readonly PreallocatedPlayerList preallocatedPlayers =
                new PreallocatedPlayerList();
            private int reusableTickIndex;
            private IReadOnlyList<SimulationPlayerInput> reusablePlayers = NoPlayers;

            internal ReusableFrameInputSet()
                : base(0)
            {
            }

            public sealed override int TickIndex => reusableTickIndex;
            public sealed override IReadOnlyList<SimulationPlayerInput> Players => reusablePlayers;

            internal void Reset(
                int tickIndex,
                SimulationPlayerInput[] players,
                int playerCount)
            {
                if (playerCount < 0 || playerCount > (players?.Length ?? 0))
                    throw new ArgumentOutOfRangeException(nameof(playerCount));

                reusableTickIndex = tickIndex;
                if (players == null || playerCount == 0)
                {
                    reusablePlayers = NoPlayers;
                    return;
                }

                preallocatedPlayers.Reset(players, playerCount);
                reusablePlayers = preallocatedPlayers;
            }
        }

        private sealed class PreallocatedPlayerList : IReadOnlyList<SimulationPlayerInput>
        {
            private SimulationPlayerInput[] buffer = Array.Empty<SimulationPlayerInput>();

            public int Count { get; private set; }

            public SimulationPlayerInput this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    return buffer[index];
                }
            }

            internal void Reset(SimulationPlayerInput[] nextBuffer, int count)
            {
                buffer = nextBuffer ?? Array.Empty<SimulationPlayerInput>();
                Count = count;
            }

            public IEnumerator<SimulationPlayerInput> GetEnumerator()
            {
                for (int index = 0; index < Count; index++)
                    yield return buffer[index];
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
