using System;

namespace NTSD.Simulation.Lockstep
{
    public sealed class StrictDelayedInputBuffer
    {
        private readonly LockstepSessionIdentity identity;
        private readonly int frameCapacity;
        private readonly int playerCount;
        private readonly int[] tickTags;
        private readonly int[] receivedCounts;
        private readonly bool[] received;
        private readonly LockstepFramePacket[] packets;
        private readonly SimulationPlayerInput[][] canonicalInputs;
        private readonly FrameInputSet[] canonicalFrames;

        private int currentTick;
        private int bufferedFrameCount;

        public StrictDelayedInputBuffer(LockstepSessionIdentity identity, int frameCapacity)
        {
            this.identity = identity ?? throw new ArgumentNullException(nameof(identity));
            if (frameCapacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(frameCapacity));

            this.frameCapacity = frameCapacity;
            playerCount = identity.PlayerCount;
            tickTags = new int[frameCapacity];
            receivedCounts = new int[frameCapacity];
            received = new bool[frameCapacity * playerCount];
            packets = new LockstepFramePacket[frameCapacity * playerCount];
            canonicalInputs = new SimulationPlayerInput[frameCapacity][];
            canonicalFrames = new FrameInputSet[frameCapacity];
            for (int frameIndex = 0; frameIndex < frameCapacity; frameIndex++)
            {
                canonicalInputs[frameIndex] = new SimulationPlayerInput[playerCount];
                canonicalFrames[frameIndex] = FrameInputSetPreallocation.CreateReusable();
            }

            Reset(0);
        }

        public int CurrentTick => currentTick;
        public int FrameCapacity => frameCapacity;
        public int BufferedFrameCount => bufferedFrameCount;

        public LockstepProtocolReason TrySubmit(in LockstepFramePacket packet)
        {
            LockstepProtocolReason validation = ValidatePacket(packet);
            if (validation != LockstepProtocolReason.None)
                return validation;

            int playerIndex = identity.FindPlayerIndex(packet.PlayerSlot);
            if (playerIndex < 0)
                return LockstepProtocolReason.UnknownPlayerSlot;

            int frameIndex = packet.Tick % frameCapacity;
            if (tickTags[frameIndex] != packet.Tick)
            {
                if (receivedCounts[frameIndex] != 0)
                    return LockstepProtocolReason.BufferCapacityExceeded;

                tickTags[frameIndex] = packet.Tick;
                bufferedFrameCount++;
            }

            int packetIndex = frameIndex * playerCount + playerIndex;
            if (received[packetIndex])
            {
                return packets[packetIndex].CanonicalEquals(packet)
                    ? LockstepProtocolReason.DuplicateIdentical
                    : LockstepProtocolReason.ConflictingDuplicate;
            }

            packets[packetIndex] = packet;
            received[packetIndex] = true;
            receivedCounts[frameIndex]++;
            return LockstepProtocolReason.None;
        }

        public bool IsFrameReady(int tick)
        {
            if (tick <= currentTick || tick > currentTick + frameCapacity)
                return false;
            int frameIndex = tick % frameCapacity;
            return tickTags[frameIndex] == tick && receivedCounts[frameIndex] == playerCount;
        }

        public bool TryConsumeFrame(
            int tick,
            out FrameInputSet frame,
            out LockstepProtocolReason reason)
        {
            frame = null;
            if (tick <= currentTick)
            {
                reason = LockstepProtocolReason.LateOrConsumedTick;
                return false;
            }
            if (tick != currentTick + 1)
            {
                reason = LockstepProtocolReason.WrongFrameTick;
                return false;
            }
            if (!IsFrameReady(tick))
            {
                reason = LockstepProtocolReason.FrameNotReady;
                return false;
            }

            int frameIndex = tick % frameCapacity;
            int packetBase = frameIndex * playerCount;
            SimulationPlayerInput[] inputs = canonicalInputs[frameIndex];
            for (int playerIndex = 0; playerIndex < playerCount; playerIndex++)
            {
                LockstepFramePacket packet = packets[packetBase + playerIndex];
                inputs[playerIndex] = new SimulationPlayerInput(
                    identity.CanonicalPlayerSlots[playerIndex],
                    packet.Buttons,
                    packet.PressedButtons,
                    packet.ReleasedButtons);
                received[packetBase + playerIndex] = false;
                packets[packetBase + playerIndex] = default;
            }

            frame = canonicalFrames[frameIndex];
            frame.ResetPreallocated(tick, inputs);
            tickTags[frameIndex] = -1;
            receivedCounts[frameIndex] = 0;
            bufferedFrameCount--;
            currentTick = tick;
            reason = LockstepProtocolReason.None;
            return true;
        }

        public void Reset(int consumedTick)
        {
            if (consumedTick < 0)
                throw new ArgumentOutOfRangeException(nameof(consumedTick));

            currentTick = consumedTick;
            bufferedFrameCount = 0;
            for (int i = 0; i < frameCapacity; i++)
            {
                tickTags[i] = -1;
                receivedCounts[i] = 0;
            }
            Array.Clear(received, 0, received.Length);
            Array.Clear(packets, 0, packets.Length);
        }

        private LockstepProtocolReason ValidatePacket(in LockstepFramePacket packet)
        {
            if (packet.SchemaVersion != identity.SchemaVersion)
                return LockstepProtocolReason.SchemaVersionMismatch;
            if (packet.SessionId != identity.SessionId)
                return LockstepProtocolReason.SessionIdMismatch;
            if (packet.Seed != identity.Seed)
                return LockstepProtocolReason.SeedMismatch;
            if (packet.CatalogFingerprint != identity.CatalogFingerprint)
                return LockstepProtocolReason.CatalogFingerprintMismatch;
            if (packet.StageFingerprint != identity.StageFingerprint)
                return LockstepProtocolReason.StageFingerprintMismatch;
            if (packet.PlayerSetFingerprint != identity.PlayerSetFingerprint)
                return LockstepProtocolReason.PlayerSetMismatch;
            if (packet.Tick <= currentTick)
                return LockstepProtocolReason.LateOrConsumedTick;
            if (packet.Tick > currentTick + frameCapacity)
                return LockstepProtocolReason.FutureWindowExceeded;
            return LockstepProtocolReason.None;
        }
    }
}
