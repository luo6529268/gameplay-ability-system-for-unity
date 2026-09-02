using System;

namespace NTSD.Simulation.Lockstep
{
    public readonly struct LockstepChecksumHistoryEntry
    {
        internal LockstepChecksumHistoryEntry(
            int protocolSchemaVersion,
            ulong identityFingerprint,
            int checksumSchemaVersion,
            int tickIndex,
            ulong inputHash,
            ulong stateChecksum)
        {
            ProtocolSchemaVersion = protocolSchemaVersion;
            IdentityFingerprint = identityFingerprint;
            ChecksumSchemaVersion = checksumSchemaVersion;
            TickIndex = tickIndex;
            InputHash = inputHash;
            StateChecksum = stateChecksum;
        }

        public int ProtocolSchemaVersion { get; }
        public ulong IdentityFingerprint { get; }
        public int ChecksumSchemaVersion { get; }
        public int TickIndex { get; }
        public ulong InputHash { get; }
        public ulong StateChecksum { get; }
        public bool HasStateChecksum => ChecksumSchemaVersion > 0;
    }

    /// <summary>
    /// Fixed-capacity history aligned with consumed canonical input frames. A tick is
    /// retained even when runtime checksum capture is disabled, so history windows do
    /// not silently diverge before checksum capture is enabled.
    /// </summary>
    public sealed class LockstepChecksumHistoryRing
    {
        private readonly LockstepSessionIdentity identity;
        private readonly int[] ticks;
        private readonly int[] checksumSchemaVersions;
        private readonly ulong[] inputHashes;
        private readonly ulong[] stateChecksums;
        private int count;
        private int nextWriteIndex;
        private int lastRecordedTick;

        public LockstepChecksumHistoryRing(
            LockstepSessionIdentity identity,
            int capacity)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            ticks = new int[capacity];
            checksumSchemaVersions = new int[capacity];
            inputHashes = new ulong[capacity];
            stateChecksums = new ulong[capacity];
        }

        public int Count => count;
        public int Capacity => ticks.Length;
        public int ProtocolSchemaVersion => identity.SchemaVersion;
        public ulong IdentityFingerprint => identity.IdentityFingerprint;
        public int LatestTick => count > 0 ? lastRecordedTick : 0;
        public int EarliestTick => count > 0 ? lastRecordedTick - count + 1 : 0;

        public bool TryRecordConsumed(
            int tickIndex,
            ulong inputHash,
            int checksumSchemaVersion,
            ulong stateChecksum,
            out LockstepProtocolReason reason)
        {
            if (tickIndex <= lastRecordedTick)
            {
                reason = LockstepProtocolReason.FrameAlreadyJournaled;
                return false;
            }
            if (tickIndex != lastRecordedTick + 1)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (checksumSchemaVersion < 0)
            {
                reason = LockstepProtocolReason.InvalidConfiguration;
                return false;
            }

            int writeIndex = nextWriteIndex;
            ticks[writeIndex] = tickIndex;
            checksumSchemaVersions[writeIndex] = checksumSchemaVersion;
            inputHashes[writeIndex] = inputHash;
            stateChecksums[writeIndex] = checksumSchemaVersion > 0
                ? stateChecksum
                : 0UL;

            nextWriteIndex++;
            if (nextWriteIndex == ticks.Length)
                nextWriteIndex = 0;
            if (count < ticks.Length)
                count++;
            lastRecordedTick = tickIndex;
            reason = LockstepProtocolReason.None;
            return true;
        }

        public bool TryGet(int tickIndex, out LockstepChecksumHistoryEntry entry)
        {
            if (count == 0 || tickIndex < EarliestTick || tickIndex > lastRecordedTick)
            {
                entry = default;
                return false;
            }

            int distanceFromLatest = lastRecordedTick - tickIndex;
            int latestIndex = nextWriteIndex == 0 ? ticks.Length - 1 : nextWriteIndex - 1;
            int physicalIndex = latestIndex - distanceFromLatest;
            if (physicalIndex < 0)
                physicalIndex += ticks.Length;
            if (ticks[physicalIndex] != tickIndex)
            {
                entry = default;
                return false;
            }

            entry = new LockstepChecksumHistoryEntry(
                identity.SchemaVersion,
                identity.IdentityFingerprint,
                checksumSchemaVersions[physicalIndex],
                tickIndex,
                inputHashes[physicalIndex],
                stateChecksums[physicalIndex]);
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
