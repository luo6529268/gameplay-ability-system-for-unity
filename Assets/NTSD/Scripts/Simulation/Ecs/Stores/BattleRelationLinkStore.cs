using System;
using System.Runtime.CompilerServices;

namespace NTSD.Simulation.Ecs
{
    internal enum RuntimeRelationLinkField : byte
    {
        RelationTeam,
        LinkState,
        KillCount,
        TargetSlot,
    }

    internal readonly struct BattleRelationLinkAiProjection
    {
        internal BattleRelationLinkAiProjection(
            int relationTeam,
            int linkState,
            int killCount,
            int targetSlot)
        {
            RelationTeam = relationTeam;
            LinkState = linkState;
            KillCount = killCount;
            TargetSlot = targetSlot;
        }

        internal int RelationTeam { get; }
        internal int LinkState { get; }
        internal int KillCount { get; }
        internal int TargetSlot { get; }
    }

    public readonly struct BattleRelationLinkStateView
    {
        internal BattleRelationLinkStateView(
            RuntimeEntityHandle handle,
            int relationTeam,
            int linkState,
            int killCount,
            int targetSlot)
        {
            Handle = handle;
            RelationTeam = relationTeam;
            LinkState = linkState;
            KillCount = killCount;
            TargetSlot = targetSlot;
        }

        public RuntimeEntityHandle Handle { get; }
        public int RelationTeam { get; }
        public int LinkState { get; }
        public int KillCount { get; }
        public int TargetSlot { get; }
    }

    /// <summary>
    /// Persistent Direct-SoA storage for the low-frequency relation and link
    /// fields consumed by same-tick AI. Runtime objects remain compatibility
    /// mirrors during U6; slot generation owns every published row.
    /// </summary>
    internal sealed class BattleRelationLinkStore
    {
        private readonly BattleAiUnifiedRowPublisher unifiedRowPublisher;
        private NTSDEntityRuntime[] owners;
        private uint[] generations;
        private int[] relationTeam;
        private int[] linkState;
        private int[] killCount;
        private int[] targetSlot;
        private ulong[] positiveLinkWords;
        private int positiveLinkCount;

        internal BattleRelationLinkStore(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.unifiedRowPublisher = unifiedRowPublisher ??
                throw new ArgumentNullException(nameof(unifiedRowPublisher));
            owners = new NTSDEntityRuntime[capacity];
            generations = new uint[capacity];
            relationTeam = new int[capacity];
            linkState = new int[capacity];
            killCount = new int[capacity];
            targetSlot = new int[capacity];
            positiveLinkWords = new ulong[(capacity + 63) >> 6];
            Array.Fill(killCount, -1);
            Array.Fill(targetSlot, -1);
        }

        internal void Bind(NTSDEntityRuntime runtime, RuntimeEntityHandle handle)
        {
            if (runtime == null ||
                !handle.IsValid ||
                handle.Slot >= owners.Length ||
                runtime.SlotIndex != handle.Slot)
            {
                throw new InvalidOperationException(
                    "Relation/link store requires a current runtime handle.");
            }

            int slot = handle.Slot;
            owners[slot] = runtime;
            generations[slot] = handle.Generation;
            CaptureAll(slot, runtime);
            runtime.BindRelationLinkStore(this, slot);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            if (!handle.IsValid ||
                handle.Slot >= owners.Length ||
                generations[handle.Slot] != handle.Generation)
            {
                return;
            }

            int slot = handle.Slot;
            owners[slot]?.UnbindRelationLinkStore(this, slot);
            ClearPositiveLinkSlot(slot);
            owners[slot] = null;
            generations[slot] = 0;
            ClearSlot(slot);
        }

        internal void Reset()
        {
            for (int slot = 0; slot < owners.Length; slot++)
                owners[slot]?.UnbindRelationLinkStore(this, slot);

            Array.Clear(owners, 0, owners.Length);
            Array.Clear(generations, 0, generations.Length);
            Array.Clear(positiveLinkWords, 0, positiveLinkWords.Length);
            positiveLinkCount = 0;
            for (int slot = 0; slot < owners.Length; slot++)
                ClearSlot(slot);
        }

        internal void GrowTo(int capacity)
        {
            if (capacity <= owners.Length)
                return;

            int previousCapacity = owners.Length;
            Array.Resize(ref owners, capacity);
            Array.Resize(ref generations, capacity);
            Array.Resize(ref relationTeam, capacity);
            Array.Resize(ref linkState, capacity);
            Array.Resize(ref killCount, capacity);
            Array.Resize(ref targetSlot, capacity);
            Array.Resize(ref positiveLinkWords, (capacity + 63) >> 6);
            Array.Fill(killCount, -1, previousCapacity, capacity - previousCapacity);
            Array.Fill(targetSlot, -1, previousCapacity, capacity - previousCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureChangedField(
            int slot,
            RuntimeRelationLinkField field,
            int value)
        {
            switch (field)
            {
                case RuntimeRelationLinkField.RelationTeam:
                    relationTeam[slot] = value;
                    break;
                case RuntimeRelationLinkField.LinkState:
                    linkState[slot] = value;
                    SetPositiveLinkMembership(slot, value > 0);
                    break;
                case RuntimeRelationLinkField.KillCount:
                    killCount[slot] = value;
                    break;
                case RuntimeRelationLinkField.TargetSlot:
                    targetSlot[slot] = value;
                    break;
            }
            unifiedRowPublisher.PublishRelationLink(
                slot,
                generations[slot],
                field,
                value);
        }

        internal int PositiveLinkCount => positiveLinkCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal int FindNextPositiveLinkSlot(int startSlot)
        {
            if (startSlot < 0)
                startSlot = 0;
            if (startSlot >= owners.Length || positiveLinkCount == 0)
                return -1;

            int wordIndex = startSlot >> 6;
            int bitOffset = startSlot & 63;
            ulong word = positiveLinkWords[wordIndex] &
                         (ulong.MaxValue << bitOffset);
            while (true)
            {
                if (word != 0)
                {
                    int firstBit = 0;
                    while ((word & 1UL) == 0)
                    {
                        word >>= 1;
                        firstBit++;
                    }

                    int slot = (wordIndex << 6) + firstBit;
                    return slot < owners.Length ? slot : -1;
                }

                wordIndex++;
                if (wordIndex >= positiveLinkWords.Length)
                    return -1;
                word = positiveLinkWords[wordIndex];
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal bool TryGetPositiveLinkHandle(
            int slot,
            out RuntimeEntityHandle handle)
        {
            if ((uint)slot >= (uint)owners.Length ||
                linkState[slot] <= 0 ||
                generations[slot] == 0 ||
                owners[slot] == null ||
                !ContainsPositiveLinkSlot(slot))
            {
                handle = RuntimeEntityHandle.Invalid;
                return false;
            }

            handle = new RuntimeEntityHandle(slot, generations[slot]);
            return true;
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleRelationLinkAiProjection projection)
        {
            if (!TryResolve(runtime, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleRelationLinkAiProjection(
                relationTeam[slot],
                linkState[slot],
                killCount[slot],
                targetSlot[slot]);
            return true;
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleRelationLinkAiProjection projection)
        {
            if (!TryResolve(handle, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleRelationLinkAiProjection(
                relationTeam[slot],
                linkState[slot],
                killCount[slot],
                targetSlot[slot]);
            return true;
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleRelationLinkStateView view)
        {
            if (!TryResolve(runtime, out int slot))
            {
                view = default;
                return false;
            }

            view = new BattleRelationLinkStateView(
                new RuntimeEntityHandle(slot, generations[slot]),
                relationTeam[slot],
                linkState[slot],
                killCount[slot],
                targetSlot[slot]);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolve(NTSDEntityRuntime runtime, out int slot)
        {
            slot = runtime?.SlotIndex ?? -1;
            return (uint)slot < (uint)owners.Length &&
                   generations[slot] != 0 &&
                   ReferenceEquals(owners[slot], runtime);
        }

        private bool TryResolve(RuntimeEntityHandle handle, out int slot)
        {
            slot = handle.Slot;
            return handle.IsValid &&
                   (uint)slot < (uint)owners.Length &&
                   generations[slot] == handle.Generation;
        }

        private void CaptureAll(int slot, NTSDEntityRuntime runtime)
        {
            relationTeam[slot] = runtime.RelationTeam;
            linkState[slot] = runtime.LinkState;
            killCount[slot] = runtime.KillCount;
            targetSlot[slot] = runtime.TargetSlotIndex;
            SetPositiveLinkMembership(slot, linkState[slot] > 0);
        }

        private void ClearSlot(int slot)
        {
            relationTeam[slot] = 0;
            linkState[slot] = 0;
            killCount[slot] = -1;
            targetSlot[slot] = -1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ContainsPositiveLinkSlot(int slot)
        {
            return (positiveLinkWords[slot >> 6] &
                    (1UL << (slot & 63))) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetPositiveLinkMembership(int slot, bool included)
        {
            if (included)
            {
                int wordIndex = slot >> 6;
                ulong mask = 1UL << (slot & 63);
                if ((positiveLinkWords[wordIndex] & mask) != 0)
                    return;

                positiveLinkWords[wordIndex] |= mask;
                positiveLinkCount++;
                return;
            }

            ClearPositiveLinkSlot(slot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ClearPositiveLinkSlot(int slot)
        {
            int wordIndex = slot >> 6;
            ulong mask = 1UL << (slot & 63);
            if ((positiveLinkWords[wordIndex] & mask) == 0)
                return;

            positiveLinkWords[wordIndex] &= ~mask;
            positiveLinkCount--;
        }
    }
}
