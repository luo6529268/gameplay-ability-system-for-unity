using System;

namespace NTSD.Simulation.Lockstep
{
    public enum LockstepProtocolReason : byte
    {
        None = 0,
        DuplicateIdentical = 1,
        InvalidConfiguration = 2,
        SchemaVersionMismatch = 3,
        SessionIdMismatch = 4,
        SeedMismatch = 5,
        CatalogFingerprintMismatch = 6,
        StageFingerprintMismatch = 7,
        PlayerSetMismatch = 8,
        UnknownPlayerSlot = 9,
        LateOrConsumedTick = 10,
        FutureWindowExceeded = 11,
        BufferCapacityExceeded = 12,
        ConflictingDuplicate = 13,
        FrameNotReady = 14,
        WrongFrameTick = 15,
        NonCanonicalPlayerOrder = 16,
        FrameAlreadyJournaled = 17,
        JournalCapacityExceeded = 18,
        BootstrapRequired = 19,
        DriverTickMismatch = 20,
        DriverRejectedFrame = 21,
        SnapshotRecoveryPendingL1 = 22,
    }

    public enum LockstepSessionStatus : byte
    {
        WaitingForInput = 0,
        Ready = 1,
        Advanced = 2,
        ProtocolError = 3,
    }

    [Serializable]
    public readonly struct LockstepChecksumMessage
    {
        public LockstepChecksumMessage(ulong identityFingerprint, int tick, ulong checksum)
        {
            IdentityFingerprint = identityFingerprint;
            Tick = tick;
            Checksum = checksum;
        }

        public ulong IdentityFingerprint { get; }
        public int Tick { get; }
        public ulong Checksum { get; }
    }

    [Serializable]
    public readonly struct LockstepSyncStatusMessage
    {
        public LockstepSyncStatusMessage(
            ulong identityFingerprint,
            int tick,
            LockstepSessionStatus status,
            LockstepProtocolReason reason)
        {
            IdentityFingerprint = identityFingerprint;
            Tick = tick;
            Status = status;
            Reason = reason;
        }

        public ulong IdentityFingerprint { get; }
        public int Tick { get; }
        public LockstepSessionStatus Status { get; }
        public LockstepProtocolReason Reason { get; }
    }
}
