using System;
using System.Threading;

namespace NTSD.Simulation
{
    [Flags]
    public enum BattleHudChanges
    {
        None = 0, Binding = 1, Hp = 2, HpBound = 4, HpMax = 8,
        Mp = 16, MpMax = 32, All = Binding | Hp | HpBound | HpMax | Mp | MpMax
    }

    public readonly struct BattleHudValues
    {
        public readonly long Session;
        public readonly long Version;
        public readonly int PlayerIndex;
        public readonly RuntimeEntityHandle Handle;
        public readonly int StableId;
        public readonly int ObjectId;
        public readonly int Hp, HpBound, HpMax, Mp, MpMax;
        public readonly BattleHudChanges Changes;
        public bool IsVisible => PlayerIndex >= 0;

        internal BattleHudValues(long session, long version, int playerIndex,
            RuntimeEntityHandle handle, int stableId, int objectId,
            int hp, int hpBound, int hpMax, int mp, int mpMax, BattleHudChanges changes)
        {
            Session = session; Version = version; PlayerIndex = playerIndex;
            Handle = handle; StableId = stableId; ObjectId = objectId;
            Hp = hp; HpBound = hpBound; HpMax = hpMax; Mp = mp; MpMax = mpMax;
            Changes = changes;
        }
    }

    // Single simulation owner; consumed only while the worker awaits publication acknowledgement.
    internal sealed class BattleHudChangeTracker
    {
        private sealed class Participant
        {
            internal NTSDEntityRuntime Runtime;
            internal RuntimeEntityHandle Handle;
            internal int StableId, ObjectId, Hp, HpBound, HpMax, Mp, MpMax;
        }

        private static long nextSession;
        private readonly long session = Interlocked.Increment(ref nextSession);
        private readonly Participant[] participants = new Participant[8];
        private int selected = -1;
        private long version;
        private BattleHudChanges pending;
        private bool accepting = true;

        internal BattleHudChangeTracker()
        {
            for (int i = 0; i < participants.Length; i++)
                participants[i] = new Participant();
        }

        internal void Bind(int playerIndex, NTSDEntityRuntime runtime, RuntimeEntityHandle handle)
        {
            if (!accepting || (uint)playerIndex >= participants.Length || runtime == null || !handle.IsValid)
                return;
            Participant entry = participants[playerIndex];
            if (ReferenceEquals(entry.Runtime, runtime) && entry.Handle.Equals(handle) &&
                entry.StableId == runtime.StableId && entry.ObjectId == runtime.ObjectId)
                return;
            for (int i = 0; i < participants.Length; i++)
                if (i != playerIndex && ReferenceEquals(participants[i].Runtime, runtime))
                    UnbindPlayer(i);
            entry.Runtime?.UnbindHudChanges(this, playerIndex, entry.Handle);
            entry.Runtime = runtime;
            entry.Handle = handle;
            entry.StableId = runtime.StableId;
            entry.ObjectId = runtime.ObjectId;
            entry.Hp = runtime.HP; entry.HpBound = runtime.HPBound; entry.HpMax = runtime.HP3;
            // Native skill costs and resource writers use PP; MP is a separate legacy bank.
            entry.Mp = runtime.PP; entry.MpMax = runtime.MPMax;
            runtime.BindHudChanges(this, playerIndex, handle);
            if (selected < 0 || playerIndex <= selected)
            {
                selected = playerIndex;
                pending = BattleHudChanges.All;
            }
        }

        internal void Capture(int playerIndex, RuntimeEntityHandle handle, BattleHudChanges field, int value)
        {
            if (!accepting || (uint)playerIndex >= participants.Length)
                return;
            Participant entry = participants[playerIndex];
            if (entry.Runtime == null || !entry.Handle.Equals(handle))
                return;
            switch (field)
            {
                case BattleHudChanges.Hp: entry.Hp = value; break;
                case BattleHudChanges.HpBound: entry.HpBound = value; break;
                case BattleHudChanges.HpMax: entry.HpMax = value; break;
                case BattleHudChanges.Mp: entry.Mp = value; break;
                case BattleHudChanges.MpMax: entry.MpMax = value; break;
                default: return;
            }
            if (selected == playerIndex)
                pending |= field;
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            for (int i = 0; i < participants.Length; i++)
                if (participants[i].Runtime != null && participants[i].Handle.Equals(handle))
                    UnbindPlayer(i);
        }

        internal void UnbindPlayer(int playerIndex)
        {
            if ((uint)playerIndex >= participants.Length)
                return;
            Participant entry = participants[playerIndex];
            if (entry.Runtime == null)
                return;
            entry.Runtime.UnbindHudChanges(this, playerIndex, entry.Handle);
            entry.Runtime = null;
            if (selected != playerIndex)
                return;
            selected = -1;
            for (int i = 0; i < participants.Length; i++)
            {
                if (participants[i].Runtime == null) continue;
                selected = i;
                break;
            }
            pending = BattleHudChanges.All;
        }

        internal void Reset(bool stop = false)
        {
            for (int i = 0; i < participants.Length; i++)
            {
                Participant entry = participants[i];
                entry.Runtime?.UnbindHudChanges(this, i, entry.Handle);
                entry.Runtime = null;
            }
            selected = -1;
            pending = BattleHudChanges.All;
            accepting = !stop;
        }

        internal bool TryConsume(out BattleHudValues values)
        {
            if (pending == BattleHudChanges.None)
            {
                values = default;
                return false;
            }
            Participant entry = selected >= 0 ? participants[selected] : null;
            values = new BattleHudValues(session, ++version, selected,
                entry?.Handle ?? RuntimeEntityHandle.Invalid, entry?.StableId ?? -1, entry?.ObjectId ?? 0,
                entry?.Hp ?? 0, entry?.HpBound ?? 0, entry?.HpMax ?? 0,
                entry?.Mp ?? 0, entry?.MpMax ?? 0, pending);
            pending = BattleHudChanges.None;
            return true;
        }
    }
}
