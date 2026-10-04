namespace NTSD.Simulation
{
    public enum BattleInputHistoryKey { None = -1, Jump = 0, Down = 2, Left = 4, Attack = 5, Right = 6, Up = 8, Defend = 9 }
    public enum BattleComboInputChangeKind { Reset, Changed, Consumed }

    internal readonly struct NativeInputHistorySnapshot
    {
        internal readonly int Count;
        internal readonly ulong Keys;
        internal readonly BattleComboInputChangeKind Kind;

        private NativeInputHistorySnapshot(int count, ulong keys, BattleComboInputChangeKind kind)
        {
            Count = count; Keys = keys; Kind = kind;
        }

        internal static NativeInputHistorySnapshot Capture(int[] history, BattleComboInputChangeKind kind)
        {
            int count = 0;
            ulong keys = 0;
            if (kind != BattleComboInputChangeKind.Reset && history != null)
            {
                for (int i = 1; i < history.Length && i <= BattleComboInputChangedEvent.Capacity; i++)
                {
                    int key = history[i];
                    if (key != 0 && key != 2 && key != 4 && key != 5 && key != 6 && key != 8 && key != 9)
                        continue;
                    keys = (keys << 8) | (byte)key;
                    count++;
                }
            }
            return new NativeInputHistorySnapshot(count, keys, kind);
        }
    }

    public readonly struct BattleComboInputChangedEvent
    {
        public const int Capacity = 5;
        public readonly BattleHudValues Binding;
        public readonly long Version;
        public readonly int Count;
        public readonly BattleComboInputChangeKind Kind;
        public readonly double PublishedAt;
        private readonly ulong keys;

        internal BattleComboInputChangedEvent(BattleHudValues binding, long version,
            NativeInputHistorySnapshot snapshot, double publishedAt)
        {
            Binding = binding; Version = version; Count = snapshot.Count;
            Kind = snapshot.Kind; keys = snapshot.Keys; PublishedAt = publishedAt;
        }

        public BattleInputHistoryKey GetKey(int index)
        {
            return (uint)index < (uint)Count
                ? (BattleInputHistoryKey)((keys >> ((Count - 1 - index) * 8)) & 255)
                : BattleInputHistoryKey.None;
        }
    }

    // Publication cache only; the native runtime owns the sequence and its lifecycle.
    internal sealed class BattleComboInputPublication
    {
        private bool initialized;
        private long version;
        internal BattleComboInputChangedEvent Current { get; private set; }

        internal bool Capture(BattleHudValues next, bool changed,
            NativeInputHistorySnapshot snapshot, double publishedAt)
        {
            BattleHudValues previous = Current.Binding;
            bool rebound = !initialized || previous.Session != next.Session ||
                previous.PlayerIndex != next.PlayerIndex || !previous.Handle.Equals(next.Handle) ||
                previous.StableId != next.StableId || previous.ObjectId != next.ObjectId;
            if (!rebound && !changed) return false;
            initialized = true;
            if (!next.IsVisible || !changed) snapshot = default;
            Current = new BattleComboInputChangedEvent(next, ++version, snapshot, publishedAt);
            return true;
        }
    }
}
