using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Lockstep
{
    public enum InProcessLockstepChecksumDomain : byte
    {
        None = 0,
        Input = 1,
        Metadata = 2,
        Rng = 3,
        World = 4,
        Slots = 5,
        ARest = 6,
        VRest = 7,
        Stats = 8,
        Events = 9,
        Overall = 10,
    }

    /// <summary>
    /// Mismatch-only S0 diagnostic projection. It must never be constructed on a
    /// matching tick because BattleLockstepChecksumSnapshot is allocation-heavy.
    /// </summary>
    public readonly struct InProcessLockstepChecksumWitness
    {
        private InProcessLockstepChecksumWitness(
            InProcessLockstepChecksumDomain firstDifferingDomain,
            string serverDomainChecksum,
            string clientDomainChecksum,
            int firstDifferingRuntimeSlot,
            uint serverSlotGeneration,
            uint clientSlotGeneration,
            uint serverRngState,
            uint clientRngState,
            ulong serverRngCallCount,
            ulong clientRngCallCount,
            BattleLockstepChecksumSnapshot serverSnapshot,
            BattleLockstepChecksumSnapshot clientSnapshot)
        {
            FirstDifferingDomain = firstDifferingDomain;
            ServerDomainChecksum = serverDomainChecksum ?? string.Empty;
            ClientDomainChecksum = clientDomainChecksum ?? string.Empty;
            FirstDifferingRuntimeSlot = firstDifferingRuntimeSlot;
            ServerSlotGeneration = serverSlotGeneration;
            ClientSlotGeneration = clientSlotGeneration;
            ServerRngState = serverRngState;
            ClientRngState = clientRngState;
            ServerRngCallCount = serverRngCallCount;
            ClientRngCallCount = clientRngCallCount;
            ServerSnapshot = serverSnapshot;
            ClientSnapshot = clientSnapshot;
        }

        public InProcessLockstepChecksumDomain FirstDifferingDomain { get; }
        public string ServerDomainChecksum { get; }
        public string ClientDomainChecksum { get; }
        public int FirstDifferingRuntimeSlot { get; }
        public uint ServerSlotGeneration { get; }
        public uint ClientSlotGeneration { get; }
        public uint ServerRngState { get; }
        public uint ClientRngState { get; }
        public ulong ServerRngCallCount { get; }
        public ulong ClientRngCallCount { get; }
        public BattleLockstepChecksumSnapshot ServerSnapshot { get; }
        public BattleLockstepChecksumSnapshot ClientSnapshot { get; }
        public bool HasSnapshots => ServerSnapshot != null && ClientSnapshot != null;
        public bool HasSlotDifference => FirstDifferingRuntimeSlot >= 0;

        internal static InProcessLockstepChecksumWitness Capture(
            BattleLockstepChecksumSnapshot serverSnapshot,
            BattleLockstepChecksumSnapshot clientSnapshot,
            InProcessLockstepChecksumDomain fallbackDomain)
        {
            if (serverSnapshot == null)
                throw new ArgumentNullException(nameof(serverSnapshot));
            if (clientSnapshot == null)
                throw new ArgumentNullException(nameof(clientSnapshot));

            InProcessLockstepChecksumDomain firstDifferingDomain = FindFirstDifferingDomain(
                serverSnapshot.Hashes,
                clientSnapshot.Hashes,
                out string serverDomainChecksum,
                out string clientDomainChecksum);
            if (firstDifferingDomain == InProcessLockstepChecksumDomain.None)
            {
                firstDifferingDomain = fallbackDomain;
            }

            ReadRng(serverSnapshot, out uint serverRngState, out ulong serverRngCallCount);
            ReadRng(clientSnapshot, out uint clientRngState, out ulong clientRngCallCount);
            int firstDifferingRuntimeSlot = -1;
            uint serverSlotGeneration = 0;
            uint clientSlotGeneration = 0;
            if (firstDifferingDomain == InProcessLockstepChecksumDomain.Slots)
            {
                FindFirstDifferingSlot(
                    serverSnapshot,
                    clientSnapshot,
                    out firstDifferingRuntimeSlot,
                    out serverSlotGeneration,
                    out clientSlotGeneration);
            }

            return new InProcessLockstepChecksumWitness(
                firstDifferingDomain,
                serverDomainChecksum,
                clientDomainChecksum,
                firstDifferingRuntimeSlot,
                serverSlotGeneration,
                clientSlotGeneration,
                serverRngState,
                clientRngState,
                serverRngCallCount,
                clientRngCallCount,
                serverSnapshot,
                clientSnapshot);
        }

        private static InProcessLockstepChecksumDomain FindFirstDifferingDomain(
            BattleLockstepChecksumHashes serverHashes,
            BattleLockstepChecksumHashes clientHashes,
            out string serverDomainChecksum,
            out string clientDomainChecksum)
        {
            if (serverHashes == null || clientHashes == null)
            {
                serverDomainChecksum = string.Empty;
                clientDomainChecksum = string.Empty;
                return InProcessLockstepChecksumDomain.Overall;
            }

            if (TryDiffer(serverHashes.Input, clientHashes.Input,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Input;
            }
            if (TryDiffer(serverHashes.Metadata, clientHashes.Metadata,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Metadata;
            }
            if (TryDiffer(serverHashes.Rng, clientHashes.Rng,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Rng;
            }
            if (TryDiffer(serverHashes.World, clientHashes.World,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.World;
            }
            if (TryDiffer(serverHashes.Slots, clientHashes.Slots,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Slots;
            }
            if (TryDiffer(serverHashes.ARest, clientHashes.ARest,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.ARest;
            }
            if (TryDiffer(serverHashes.VRest, clientHashes.VRest,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.VRest;
            }
            if (TryDiffer(serverHashes.Stats, clientHashes.Stats,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Stats;
            }
            if (TryDiffer(serverHashes.Events, clientHashes.Events,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Events;
            }
            if (TryDiffer(serverHashes.Overall, clientHashes.Overall,
                    out serverDomainChecksum, out clientDomainChecksum))
            {
                return InProcessLockstepChecksumDomain.Overall;
            }

            serverDomainChecksum = string.Empty;
            clientDomainChecksum = string.Empty;
            return InProcessLockstepChecksumDomain.None;
        }

        private static bool TryDiffer(
            string serverValue,
            string clientValue,
            out string serverDomainChecksum,
            out string clientDomainChecksum)
        {
            serverDomainChecksum = serverValue ?? string.Empty;
            clientDomainChecksum = clientValue ?? string.Empty;
            return !string.Equals(
                serverDomainChecksum,
                clientDomainChecksum,
                StringComparison.Ordinal);
        }

        private static void ReadRng(
            BattleLockstepChecksumSnapshot snapshot,
            out uint state,
            out ulong callCount)
        {
            state = 0;
            callCount = 0;
            if (!(snapshot.RngDomain is SortedDictionary<string, object> domain))
                return;

            TryReadUInt32(domain, "seed", out state);
            TryReadUInt64(domain, "callCount", out callCount);
        }

        private static void FindFirstDifferingSlot(
            BattleLockstepChecksumSnapshot serverSnapshot,
            BattleLockstepChecksumSnapshot clientSnapshot,
            out int runtimeSlot,
            out uint serverGeneration,
            out uint clientGeneration)
        {
            runtimeSlot = -1;
            serverGeneration = 0;
            clientGeneration = 0;
            if (!TryGetSlots(serverSnapshot, out object[] serverSlots) ||
                !TryGetSlots(clientSnapshot, out object[] clientSlots))
            {
                return;
            }

            int sharedCount = Math.Min(serverSlots.Length, clientSlots.Length);
            for (int index = 0; index < sharedCount; index++)
            {
                if (string.Equals(
                        BattleCanonicalJson.Sha256(serverSlots[index]),
                        BattleCanonicalJson.Sha256(clientSlots[index]),
                        StringComparison.Ordinal))
                {
                    continue;
                }

                runtimeSlot = index;
                serverGeneration = ReadSlotGeneration(serverSlots[index]);
                clientGeneration = ReadSlotGeneration(clientSlots[index]);
                return;
            }

            if (serverSlots.Length == clientSlots.Length)
                return;

            runtimeSlot = sharedCount;
            if (sharedCount < serverSlots.Length)
                serverGeneration = ReadSlotGeneration(serverSlots[sharedCount]);
            if (sharedCount < clientSlots.Length)
                clientGeneration = ReadSlotGeneration(clientSlots[sharedCount]);
        }

        private static bool TryGetSlots(
            BattleLockstepChecksumSnapshot snapshot,
            out object[] slots)
        {
            slots = null;
            if (!(snapshot.SlotsDomain is SortedDictionary<string, object> domain) ||
                !domain.TryGetValue("slots", out object value) ||
                !(value is object[] projectedSlots))
            {
                return false;
            }

            slots = projectedSlots;
            return true;
        }

        private static uint ReadSlotGeneration(object slotProjection)
        {
            if (!(slotProjection is SortedDictionary<string, object> slot) ||
                !TryReadUInt32(slot, "generation", out uint generation))
            {
                return 0;
            }

            return generation;
        }

        private static bool TryReadUInt32(
            SortedDictionary<string, object> domain,
            string key,
            out uint value)
        {
            value = 0;
            if (domain == null || !domain.TryGetValue(key, out object raw))
                return false;

            switch (raw)
            {
                case uint unsignedValue:
                    value = unsignedValue;
                    return true;
                case int signedValue when signedValue >= 0:
                    value = unchecked((uint)signedValue);
                    return true;
                default:
                    return false;
            }
        }

        private static bool TryReadUInt64(
            SortedDictionary<string, object> domain,
            string key,
            out ulong value)
        {
            value = 0;
            if (domain == null || !domain.TryGetValue(key, out object raw))
                return false;

            switch (raw)
            {
                case ulong unsignedValue:
                    value = unsignedValue;
                    return true;
                case uint unsignedValue:
                    value = unsignedValue;
                    return true;
                case int signedValue when signedValue >= 0:
                    value = unchecked((ulong)signedValue);
                    return true;
                default:
                    return false;
            }
        }
    }
}
