using System;
using System.Collections.Generic;

namespace NTSD.Simulation.Lockstep
{
    public sealed class LockstepSessionIdentity
    {
        public const int CurrentSchemaVersion = 1;
        public const int MaxPlayerSlots = 64;

        private readonly int[] canonicalPlayerSlots;
        private readonly IReadOnlyList<int> canonicalPlayerSlotView;

        public LockstepSessionIdentity(
            int schemaVersion,
            ulong sessionId,
            uint seed,
            ulong catalogFingerprint,
            ulong stageFingerprint,
            IReadOnlyList<int> playerSlots)
        {
            if (schemaVersion != CurrentSchemaVersion)
                throw new ArgumentOutOfRangeException(nameof(schemaVersion));
            if (sessionId == 0)
                throw new ArgumentOutOfRangeException(nameof(sessionId));
            if (playerSlots == null || playerSlots.Count == 0 ||
                playerSlots.Count > MaxPlayerSlots)
            {
                throw new ArgumentException("A lockstep session requires 1-64 human player slots.",
                    nameof(playerSlots));
            }

            canonicalPlayerSlots = new int[playerSlots.Count];
            for (int i = 0; i < playerSlots.Count; i++)
            {
                if (playerSlots[i] < 0)
                    throw new ArgumentOutOfRangeException(nameof(playerSlots));
                canonicalPlayerSlots[i] = playerSlots[i];
            }

            Array.Sort(canonicalPlayerSlots);
            for (int i = 1; i < canonicalPlayerSlots.Length; i++)
            {
                if (canonicalPlayerSlots[i] == canonicalPlayerSlots[i - 1])
                    throw new ArgumentException("Human player slots must be unique.", nameof(playerSlots));
            }

            SchemaVersion = schemaVersion;
            canonicalPlayerSlotView = Array.AsReadOnly(canonicalPlayerSlots);
            SessionId = sessionId;
            Seed = seed;
            CatalogFingerprint = catalogFingerprint;
            StageFingerprint = stageFingerprint;
            PlayerSetFingerprint = ComputePlayerSetFingerprint(canonicalPlayerSlots);
            IdentityFingerprint = ComputeIdentityFingerprint();
        }

        public int SchemaVersion { get; }
        public ulong SessionId { get; }
        public uint Seed { get; }
        public ulong CatalogFingerprint { get; }
        public ulong StageFingerprint { get; }
        public ulong PlayerSetFingerprint { get; }
        public ulong IdentityFingerprint { get; }
        public IReadOnlyList<int> CanonicalPlayerSlots => canonicalPlayerSlotView;
        public int PlayerCount => canonicalPlayerSlots.Length;

        public int FindPlayerIndex(int playerSlot)
        {
            int low = 0;
            int high = canonicalPlayerSlots.Length - 1;
            while (low <= high)
            {
                int middle = low + ((high - low) >> 1);
                int value = canonicalPlayerSlots[middle];
                if (value == playerSlot)
                    return middle;
                if (value < playerSlot)
                    low = middle + 1;
                else
                    high = middle - 1;
            }

            return -1;
        }

        private ulong ComputeIdentityFingerprint()
        {
            ulong hash = CanonicalHash.Offset;
            CanonicalHash.AddInt(ref hash, SchemaVersion);
            CanonicalHash.AddUlong(ref hash, SessionId);
            CanonicalHash.AddUint(ref hash, Seed);
            CanonicalHash.AddUlong(ref hash, CatalogFingerprint);
            CanonicalHash.AddUlong(ref hash, StageFingerprint);
            CanonicalHash.AddUlong(ref hash, PlayerSetFingerprint);
            return hash;
        }

        private static ulong ComputePlayerSetFingerprint(IReadOnlyList<int> slots)
        {
            ulong hash = CanonicalHash.Offset;
            CanonicalHash.AddInt(ref hash, slots.Count);
            for (int i = 0; i < slots.Count; i++)
                CanonicalHash.AddInt(ref hash, slots[i]);
            return hash;
        }
    }

    internal static class CanonicalHash
    {
        internal const ulong Offset = 14695981039346656037UL;
        private const ulong Prime = 1099511628211UL;

        internal static void AddInt(ref ulong hash, int value)
        {
            AddUint(ref hash, unchecked((uint)value));
        }

        internal static void AddUint(ref ulong hash, uint value)
        {
            hash = (hash ^ (byte)value) * Prime;
            hash = (hash ^ (byte)(value >> 8)) * Prime;
            hash = (hash ^ (byte)(value >> 16)) * Prime;
            hash = (hash ^ (byte)(value >> 24)) * Prime;
        }

        internal static void AddUlong(ref ulong hash, ulong value)
        {
            AddUint(ref hash, (uint)value);
            AddUint(ref hash, (uint)(value >> 32));
        }

        internal static void AddByte(ref ulong hash, byte value)
        {
            hash = (hash ^ value) * Prime;
        }
    }
}
