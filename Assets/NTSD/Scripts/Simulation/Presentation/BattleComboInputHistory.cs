namespace NTSD.Simulation
{
    public readonly struct BattleComboInputChangedEvent
    {
        public const int Capacity = 6;
        public readonly BattleHudValues Binding;
        public readonly long Version;
        public readonly int Count;
        private readonly ulong keys;

        internal BattleComboInputChangedEvent(BattleHudValues binding, long version, int count, ulong keys)
        {
            Binding = binding;
            Version = version;
            Count = count;
            this.keys = keys;
        }

        public SimulationInputButtons GetKey(int index)
        {
            return (uint)index < (uint)Count
                ? (SimulationInputButtons)((keys >> ((Count - 1 - index) * 8)) & 255)
                : SimulationInputButtons.None;
        }
    }

    // Main-thread presentation history of completed input frames, never a skill recognizer.
    internal sealed class BattleComboInputHistory
    {
        private static readonly SimulationInputButtons[] Order =
        {
            SimulationInputButtons.Right, SimulationInputButtons.Left,
            SimulationInputButtons.Up, SimulationInputButtons.Down,
            SimulationInputButtons.Attack, SimulationInputButtons.Jump, SimulationInputButtons.Defend,
        };
        private BattleHudValues binding;
        private bool initialized;
        private int lastTick = int.MinValue;
        private int count;
        private ulong keys;
        private long version;
        internal BattleComboInputChangedEvent Current { get; private set; }

        internal bool Capture(BattleHudValues next, FrameInputSet frame)
        {
            bool changed = !initialized || binding.Session != next.Session ||
                binding.PlayerIndex != next.PlayerIndex || !binding.Handle.Equals(next.Handle) ||
                binding.StableId != next.StableId || binding.ObjectId != next.ObjectId;
            if (changed)
            {
                initialized = true;
                binding = next;
                count = 0;
                keys = 0;
                lastTick = int.MinValue;
            }
            if (next.IsVisible && frame != null && frame.TickIndex > lastTick)
            {
                lastTick = frame.TickIndex;
                for (int i = 0; i < frame.Players.Count; i++)
                {
                    SimulationPlayerInput input = frame.Players[i];
                    if (input.PlayerSlot != next.PlayerIndex)
                        continue;
                    for (int key = 0; key < Order.Length; key++)
                    {
                        if ((input.PressedButtons & Order[key]) == 0)
                            continue;
                        keys = ((keys << 8) | (byte)Order[key]) & 0xFFFFFFFFFFFFUL;
                        if (count < BattleComboInputChangedEvent.Capacity)
                            count++;
                        changed = true;
                    }
                    break;
                }
            }
            if (changed)
                Current = new BattleComboInputChangedEvent(next, ++version, count, keys);
            return changed;
        }
    }
}
