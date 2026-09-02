using System;

namespace NTSD.Simulation.Lockstep
{
    [Serializable]
    public readonly struct LockstepFramePacket : IEquatable<LockstepFramePacket>
    {
        public LockstepFramePacket(
            LockstepSessionIdentity identity,
            int tick,
            int playerSlot,
            SimulationInputButtons buttons,
            SimulationInputButtons pressedButtons = SimulationInputButtons.None,
            SimulationInputButtons releasedButtons = SimulationInputButtons.None)
            : this(
                identity?.SchemaVersion ?? 0,
                identity?.SessionId ?? 0,
                identity?.Seed ?? 0,
                identity?.CatalogFingerprint ?? 0,
                identity?.StageFingerprint ?? 0,
                identity?.PlayerSetFingerprint ?? 0,
                tick,
                playerSlot,
                buttons,
                pressedButtons,
                releasedButtons)
        {
        }

        public LockstepFramePacket(
            int schemaVersion,
            ulong sessionId,
            uint seed,
            ulong catalogFingerprint,
            ulong stageFingerprint,
            ulong playerSetFingerprint,
            int tick,
            int playerSlot,
            SimulationInputButtons buttons,
            SimulationInputButtons pressedButtons,
            SimulationInputButtons releasedButtons)
        {
            SchemaVersion = schemaVersion;
            SessionId = sessionId;
            Seed = seed;
            CatalogFingerprint = catalogFingerprint;
            StageFingerprint = stageFingerprint;
            PlayerSetFingerprint = playerSetFingerprint;
            Tick = tick;
            PlayerSlot = playerSlot;
            Buttons = buttons;
            PressedButtons = pressedButtons;
            ReleasedButtons = releasedButtons;
        }

        public int SchemaVersion { get; }
        public ulong SessionId { get; }
        public uint Seed { get; }
        public ulong CatalogFingerprint { get; }
        public ulong StageFingerprint { get; }
        public ulong PlayerSetFingerprint { get; }
        public int Tick { get; }
        public int PlayerSlot { get; }
        public SimulationInputButtons Buttons { get; }
        public SimulationInputButtons PressedButtons { get; }
        public SimulationInputButtons ReleasedButtons { get; }

        public bool CanonicalEquals(in LockstepFramePacket other)
        {
            return SchemaVersion == other.SchemaVersion &&
                   SessionId == other.SessionId &&
                   Seed == other.Seed &&
                   CatalogFingerprint == other.CatalogFingerprint &&
                   StageFingerprint == other.StageFingerprint &&
                   PlayerSetFingerprint == other.PlayerSetFingerprint &&
                   Tick == other.Tick &&
                   PlayerSlot == other.PlayerSlot &&
                   Buttons == other.Buttons &&
                   PressedButtons == other.PressedButtons &&
                   ReleasedButtons == other.ReleasedButtons;
        }

        public ulong GetCanonicalHash64()
        {
            ulong hash = CanonicalHash.Offset;
            CanonicalHash.AddInt(ref hash, SchemaVersion);
            CanonicalHash.AddUlong(ref hash, SessionId);
            CanonicalHash.AddUint(ref hash, Seed);
            CanonicalHash.AddUlong(ref hash, CatalogFingerprint);
            CanonicalHash.AddUlong(ref hash, StageFingerprint);
            CanonicalHash.AddUlong(ref hash, PlayerSetFingerprint);
            CanonicalHash.AddInt(ref hash, Tick);
            CanonicalHash.AddInt(ref hash, PlayerSlot);
            CanonicalHash.AddByte(ref hash, (byte)Buttons);
            CanonicalHash.AddByte(ref hash, (byte)PressedButtons);
            CanonicalHash.AddByte(ref hash, (byte)ReleasedButtons);
            return hash;
        }

        public bool Equals(LockstepFramePacket other)
        {
            return CanonicalEquals(other);
        }

        public override bool Equals(object obj)
        {
            return obj is LockstepFramePacket other && CanonicalEquals(other);
        }

        public override int GetHashCode()
        {
            ulong hash = GetCanonicalHash64();
            return unchecked((int)(hash ^ (hash >> 32)));
        }
    }
}
