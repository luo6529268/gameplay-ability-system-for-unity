using System;
using NTSD.Simulation.Lockstep;

namespace NTSD.Simulation
{
    /// <summary>
    /// Captures checksum-visible pending events at the formal tick boundary.
    /// Deferred unregister/destroy queues must already be drained there; a nonempty
    /// lifecycle queue is rejected instead of serializing released CLR objects.
    /// </summary>
    public sealed class BattleWorldPendingEventSnapshotBuffer
    {
        public const int CurrentSchemaVersion = 1;

        private readonly PendingSoundEvent[] sounds;

        public BattleWorldPendingEventSnapshotBuffer(int soundCapacity)
        {
            if (soundCapacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(soundCapacity));
            }

            SoundCapacity = soundCapacity;
            sounds = new PendingSoundEvent[soundCapacity];
        }

        public int SoundCapacity { get; }
        public int SoundCount { get; private set; }
        public int PendingUnregisterCount { get; private set; }
        public int PendingSlotReleasedDestroyCount { get; private set; }
        public int SchemaVersion { get; private set; }
        public int ProtocolSchemaVersion { get; private set; }
        public ulong IdentityFingerprint { get; private set; }
        public int CapturedTick { get; private set; }

        public PendingSoundEvent GetSound(int index)
        {
            if ((uint)index >= (uint)SoundCount)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            return sounds[index];
        }

        internal bool TryCapture(
            SimulationBattleBufferModule source,
            LockstepSessionIdentity identity,
            int tick)
        {
            if (source == null ||
                identity == null ||
                source.PendingSounds.Count > SoundCapacity ||
                source.PendingUnregister.Count != 0 ||
                source.PendingSlotReleasedDestroy.Count != 0)
            {
                return false;
            }

            for (int index = 0; index < source.PendingSounds.Count; index++)
            {
                PendingSoundEvent sound = source.PendingSounds[index];
                if (string.IsNullOrWhiteSpace(sound.Cue))
                {
                    return false;
                }
            }

            int soundCount = source.PendingSounds.Count;
            for (int index = 0; index < soundCount; index++)
            {
                sounds[index] = source.PendingSounds[index];
            }
            for (int index = soundCount; index < SoundCount; index++)
            {
                sounds[index] = default;
            }

            SoundCount = soundCount;
            PendingUnregisterCount = 0;
            PendingSlotReleasedDestroyCount = 0;
            SchemaVersion = CurrentSchemaVersion;
            ProtocolSchemaVersion = identity.SchemaVersion;
            IdentityFingerprint = identity.IdentityFingerprint;
            CapturedTick = tick;
            return true;
        }

        internal bool TryRestoreTo(SimulationBattleBufferModule destination)
        {
            if (SchemaVersion != CurrentSchemaVersion ||
                destination == null ||
                PendingUnregisterCount != 0 ||
                PendingSlotReleasedDestroyCount != 0 ||
                destination.PendingUnregister.Count != 0 ||
                destination.PendingSlotReleasedDestroy.Count != 0 ||
                destination.PendingSounds.Capacity < SoundCount)
            {
                return false;
            }

            destination.PendingSounds.Clear();
            for (int index = 0; index < SoundCount; index++)
                destination.PendingSounds.Add(sounds[index]);
            return true;
        }
    }

    internal sealed class BattleWorldPendingEventSnapshotModule
    {
        private readonly SimulationWorld world;
        private readonly SimulationBattleBufferModule source;

        internal BattleWorldPendingEventSnapshotModule(
            SimulationWorld world,
            SimulationBattleBufferModule source)
        {
            this.world = world ?? throw new ArgumentNullException(nameof(world));
            this.source = source ?? throw new ArgumentNullException(nameof(source));
        }

        internal BattleWorldPendingEventSnapshotBuffer CreateBufferForBootstrap()
        {
            int capacity = Math.Max(1, source.PendingSounds.Capacity);
            return new BattleWorldPendingEventSnapshotBuffer(capacity);
        }

        internal bool TryCapture(
            LockstepSessionIdentity identity,
            int tick,
            BattleWorldPendingEventSnapshotBuffer destination)
        {
            if (identity == null)
            {
                throw new ArgumentNullException(nameof(identity));
            }
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }
            if (!ReferenceEquals(source, world.BattleBuffersForServices))
            {
                return false;
            }

            return destination.TryCapture(source, identity, tick);
        }
    }
}
