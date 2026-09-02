using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Lockstep
{
    public enum InProcessLockstepAuthorityStatus : byte
    {
        Ready = 0,
        Advanced = 1,
        Faulted = 2,
    }

    public enum InProcessAuthorityFailureReason : byte
    {
        None = 0,
        SessionAlreadyFaulted = 1,
        WrongFrameTick = 2,
        NonCanonicalPlayerOrder = 3,
        JournalCapacityExceeded = 4,
        KernelRejectedFrame = 5,
        InputHashMismatch = 6,
        StateChecksumMismatch = 7,
    }

    public readonly struct InProcessAuthorityDifference
    {
        internal InProcessAuthorityDifference(
            int tickIndex,
            int clientReplicaIndex,
            ulong serverInputHash,
            ulong clientInputHash,
            ulong serverStateChecksum,
            ulong clientStateChecksum,
            InProcessLockstepChecksumWitness structuredWitness)
        {
            TickIndex = tickIndex;
            ClientReplicaIndex = clientReplicaIndex;
            ServerInputHash = serverInputHash;
            ClientInputHash = clientInputHash;
            ServerStateChecksum = serverStateChecksum;
            ClientStateChecksum = clientStateChecksum;
            StructuredWitness = structuredWitness;
        }

        public int TickIndex { get; }
        public int ClientReplicaIndex { get; }
        public ulong ServerInputHash { get; }
        public ulong ClientInputHash { get; }
        public ulong ServerStateChecksum { get; }
        public ulong ClientStateChecksum { get; }
        public InProcessLockstepChecksumWitness StructuredWitness { get; }
        public bool HasStructuredWitness => StructuredWitness.HasSnapshots;
        public bool HasDifference => TickIndex > 0;
    }

    /// <summary>
    /// S0 in-memory authority owner. Frames are copied into a fixed journal before any
    /// world consumes them, then server and clients advance in deterministic order.
    /// </summary>
    public sealed class InProcessLockstepAuthoritySession
    {
        private readonly LockstepStartBarrier barrier;
        private readonly InProcessBattleKernelHost server;
        private readonly InProcessBattleKernelHost[] clients;
        private int currentTick;

        public InProcessLockstepAuthoritySession(
            LockstepStartBarrier barrier,
            InProcessBattleKernelHost server,
            IReadOnlyList<InProcessBattleKernelHost> clients,
            int authorityJournalCapacity)
        {
            this.barrier = barrier ?? throw new ArgumentNullException(nameof(barrier));
            this.server = server ?? throw new ArgumentNullException(nameof(server));
            if (clients == null || clients.Count < 2)
            {
                throw new ArgumentException(
                    "S0 requires at least two client worlds.",
                    nameof(clients));
            }
            if (authorityJournalCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(authorityJournalCapacity));
            if (!barrier.Matches(server.Barrier))
            {
                throw new ArgumentException(
                    "The server start barrier does not match the authority session.",
                    nameof(server));
            }
            if (server.CurrentTick != 0)
            {
                throw new ArgumentException(
                    "The S0 server world must be at tick zero.",
                    nameof(server));
            }

            this.clients = new InProcessBattleKernelHost[clients.Count];
            for (int index = 0; index < clients.Count; index++)
            {
                InProcessBattleKernelHost client = clients[index];
                if (client == null)
                    throw new ArgumentException("A client world is null.", nameof(clients));
                if (ReferenceEquals(client, server))
                {
                    throw new ArgumentException(
                        "The authority server cannot also be a client replica.",
                        nameof(clients));
                }
                if (!barrier.Matches(client.Barrier))
                {
                    throw new ArgumentException(
                        $"Client replica {client.ReplicaIndex} has a different start barrier.",
                        nameof(clients));
                }
                if (client.CurrentTick != 0)
                {
                    throw new ArgumentException(
                        $"Client replica {client.ReplicaIndex} is not at tick zero.",
                        nameof(clients));
                }
                for (int prior = 0; prior < index; prior++)
                {
                    if (ReferenceEquals(client, this.clients[prior]))
                    {
                        throw new ArgumentException(
                            "The same client world was registered more than once.",
                            nameof(clients));
                    }
                }

                this.clients[index] = client;
            }

            AuthorityJournal = new LockstepReplayJournal(
                barrier.Identity,
                authorityJournalCapacity);
            Status = InProcessLockstepAuthorityStatus.Ready;
            LastProtocolReason = LockstepProtocolReason.None;
        }

        public LockstepStartBarrier Barrier => barrier;
        public InProcessBattleKernelHost Server => server;
        public IReadOnlyList<InProcessBattleKernelHost> Clients => clients;
        public LockstepReplayJournal AuthorityJournal { get; }
        public int CurrentTick => currentTick;
        public InProcessLockstepAuthorityStatus Status { get; private set; }
        public InProcessAuthorityFailureReason LastFailureReason { get; private set; }
        public LockstepProtocolReason LastProtocolReason { get; private set; }
        public InProcessAuthorityDifference FirstDifference { get; private set; }

        public bool TryAdvance(FrameInputSet sourceFrame)
        {
            if (Status == InProcessLockstepAuthorityStatus.Faulted)
            {
                LastFailureReason = InProcessAuthorityFailureReason.SessionAlreadyFaulted;
                return false;
            }
            if (sourceFrame == null || sourceFrame.TickIndex != currentTick + 1)
            {
                return LatchFault(
                    InProcessAuthorityFailureReason.WrongFrameTick,
                    LockstepProtocolReason.WrongFrameTick);
            }
            if (!barrier.IsCanonicalFrame(sourceFrame))
            {
                return LatchFault(
                    InProcessAuthorityFailureReason.NonCanonicalPlayerOrder,
                    LockstepProtocolReason.NonCanonicalPlayerOrder);
            }
            if (!AuthorityJournal.HasCapacity)
            {
                return LatchFault(
                    InProcessAuthorityFailureReason.JournalCapacityExceeded,
                    LockstepProtocolReason.JournalCapacityExceeded);
            }
            if (!PreflightHost(server, sourceFrame))
                return false;
            for (int index = 0; index < clients.Length; index++)
            {
                if (!PreflightHost(clients[index], sourceFrame))
                    return false;
            }

            if (!AuthorityJournal.TryRecordConsumed(
                    sourceFrame,
                    out LockstepProtocolReason reason))
            {
                return LatchFault(MapProtocolFailure(reason), reason);
            }

            FrameInputSet authorityFrame =
                AuthorityJournal[AuthorityJournal.Count - 1];
            if (!server.TryStepOneTick(authorityFrame))
            {
                return LatchFault(
                    InProcessAuthorityFailureReason.KernelRejectedFrame,
                    server.LastReason);
            }

            for (int index = 0; index < clients.Length; index++)
            {
                InProcessBattleKernelHost client = clients[index];
                if (!client.TryStepOneTick(authorityFrame))
                {
                    return LatchFault(
                        InProcessAuthorityFailureReason.KernelRejectedFrame,
                        client.LastReason);
                }
                if (client.LastInputHash != server.LastInputHash)
                {
                    FirstDifference = BuildDifference(
                        client,
                        authorityFrame,
                        InProcessLockstepChecksumDomain.Input);
                    return LatchFault(
                        InProcessAuthorityFailureReason.InputHashMismatch,
                        LockstepProtocolReason.ReplayInputMismatch);
                }
                if (client.LastStateChecksum != server.LastStateChecksum)
                {
                    FirstDifference = BuildDifference(
                        client,
                        authorityFrame,
                        InProcessLockstepChecksumDomain.None);
                    return LatchFault(
                        InProcessAuthorityFailureReason.StateChecksumMismatch,
                        LockstepProtocolReason.ReplayChecksumMismatch);
                }
            }

            currentTick = authorityFrame.TickIndex;
            Status = InProcessLockstepAuthorityStatus.Advanced;
            LastFailureReason = InProcessAuthorityFailureReason.None;
            LastProtocolReason = LockstepProtocolReason.None;
            return true;
        }

        private bool PreflightHost(
            InProcessBattleKernelHost host,
            FrameInputSet frame)
        {
            if (host.CanStep(frame, out LockstepProtocolReason reason))
                return true;

            return LatchFault(
                InProcessAuthorityFailureReason.KernelRejectedFrame,
                reason);
        }

        private InProcessAuthorityDifference BuildDifference(
            InProcessBattleKernelHost client,
            FrameInputSet authorityFrame,
            InProcessLockstepChecksumDomain fallbackDomain)
        {
            BattleLockstepChecksumSnapshot serverSnapshot =
                server.CaptureDiagnosticSnapshot(authorityFrame);
            BattleLockstepChecksumSnapshot clientSnapshot =
                client.CaptureDiagnosticSnapshot(authorityFrame);
            InProcessLockstepChecksumWitness structuredWitness =
                InProcessLockstepChecksumWitness.Capture(
                    serverSnapshot,
                    clientSnapshot,
                    fallbackDomain);
            return new InProcessAuthorityDifference(
                authorityFrame.TickIndex,
                client.ReplicaIndex,
                server.LastInputHash,
                client.LastInputHash,
                server.LastStateChecksum,
                client.LastStateChecksum,
                structuredWitness);
        }

        private bool LatchFault(
            InProcessAuthorityFailureReason failureReason,
            LockstepProtocolReason protocolReason)
        {
            LastFailureReason = failureReason;
            LastProtocolReason = protocolReason;
            Status = InProcessLockstepAuthorityStatus.Faulted;
            return false;
        }

        private InProcessAuthorityFailureReason MapProtocolFailure(
            LockstepProtocolReason reason)
        {
            switch (reason)
            {
                case LockstepProtocolReason.WrongFrameTick:
                case LockstepProtocolReason.FrameAlreadyJournaled:
                    return InProcessAuthorityFailureReason.WrongFrameTick;
                case LockstepProtocolReason.NonCanonicalPlayerOrder:
                    return InProcessAuthorityFailureReason.NonCanonicalPlayerOrder;
                case LockstepProtocolReason.JournalCapacityExceeded:
                    return InProcessAuthorityFailureReason.JournalCapacityExceeded;
                default:
                    return InProcessAuthorityFailureReason.KernelRejectedFrame;
            }
        }
    }
}
