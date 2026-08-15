using System;

namespace NTSD.Simulation.Lockstep
{
    public readonly struct LockstepFrameHistoryEntry
    {
        internal LockstepFrameHistoryEntry(
            int schemaVersion,
            ulong identityFingerprint,
            ulong inputHash,
            FrameInputSet frame)
        {
            SchemaVersion = schemaVersion;
            IdentityFingerprint = identityFingerprint;
            InputHash = inputHash;
            Frame = frame;
        }

        public int SchemaVersion { get; }
        public ulong IdentityFingerprint { get; }
        public ulong InputHash { get; }
        public FrameInputSet Frame { get; }
        public int TickIndex => Frame?.TickIndex ?? 0;
    }

    /// <summary>
    /// Fixed-capacity history of consumed canonical input frames. Each ring cell owns
    /// its player-input storage, so future buffer reuse cannot mutate recorded history.
    /// </summary>
    public sealed class LockstepFrameHistoryRing
    {
        private readonly LockstepSessionIdentity identity;
        private readonly SimulationPlayerInput[][] inputStorage;
        private readonly FrameInputSet[] frames;
        private readonly int[] ticks;
        private readonly ulong[] inputHashes;
        private int count;
        private int nextWriteIndex;
        private int lastRecordedTick;

        public LockstepFrameHistoryRing(
            LockstepSessionIdentity identity,
            int capacity)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            inputStorage = new SimulationPlayerInput[capacity][];
            frames = new FrameInputSet[capacity];
            ticks = new int[capacity];
            inputHashes = new ulong[capacity];
            for (int index = 0; index < capacity; index++)
            {
                inputStorage[index] = new SimulationPlayerInput[identity.PlayerCount];
                frames[index] = new FrameInputSet(0, inputStorage[index]);
            }
        }

        public int Count => count;
        public int Capacity => frames.Length;
        public int SchemaVersion => identity.SchemaVersion;
        public ulong IdentityFingerprint => identity.IdentityFingerprint;
        public int LatestTick => count > 0 ? lastRecordedTick : 0;
        public int EarliestTick => count > 0 ? lastRecordedTick - count + 1 : 0;

        public bool TryRecordConsumed(
            FrameInputSet frame,
            out LockstepProtocolReason reason)
        {
            if (frame == null)
            {
                reason = LockstepProtocolReason.NonCanonicalPlayerOrder;
                return false;
            }
            if (frame.TickIndex <= lastRecordedTick)
            {
                reason = LockstepProtocolReason.FrameAlreadyJournaled;
                return false;
            }
            if (frame.TickIndex != lastRecordedTick + 1)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (!frame.IsCanonicalFor(frame.TickIndex, identity.CanonicalPlayerSlots))
            {
                reason = LockstepProtocolReason.NonCanonicalPlayerOrder;
                return false;
            }

            int writeIndex = nextWriteIndex;
            SimulationPlayerInput[] destination = inputStorage[writeIndex];
            for (int playerIndex = 0; playerIndex < destination.Length; playerIndex++)
                destination[playerIndex] = frame.Players[playerIndex];

            FrameInputSet storedFrame = frames[writeIndex];
            storedFrame.ResetPreallocated(frame.TickIndex, destination);
            ticks[writeIndex] = frame.TickIndex;
            inputHashes[writeIndex] = storedFrame.GetCanonicalHash64();

            nextWriteIndex++;
            if (nextWriteIndex == frames.Length)
                nextWriteIndex = 0;
            if (count < frames.Length)
                count++;
            lastRecordedTick = frame.TickIndex;
            reason = LockstepProtocolReason.None;
            return true;
        }

        public bool TryGet(int tickIndex, out LockstepFrameHistoryEntry entry)
        {
            if (count == 0 || tickIndex < EarliestTick || tickIndex > lastRecordedTick)
            {
                entry = default;
                return false;
            }

            int distanceFromLatest = lastRecordedTick - tickIndex;
            int latestIndex = nextWriteIndex == 0 ? frames.Length - 1 : nextWriteIndex - 1;
            int physicalIndex = latestIndex - distanceFromLatest;
            if (physicalIndex < 0)
                physicalIndex += frames.Length;
            if (ticks[physicalIndex] != tickIndex)
            {
                entry = default;
                return false;
            }

            entry = new LockstepFrameHistoryEntry(
                identity.SchemaVersion,
                identity.IdentityFingerprint,
                inputHashes[physicalIndex],
                frames[physicalIndex]);
            return true;
        }

        public void Reset(int consumedTick = 0)
        {
            if (consumedTick < 0)
                throw new ArgumentOutOfRangeException(nameof(consumedTick));

            count = 0;
            nextWriteIndex = 0;
            lastRecordedTick = consumedTick;
        }
    }
}
