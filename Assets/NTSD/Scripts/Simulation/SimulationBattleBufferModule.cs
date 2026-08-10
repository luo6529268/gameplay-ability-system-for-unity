using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    /// <summary>
    /// Match-owned reusable managed buffers. All growth belongs to loading;
    /// after sealing, bounded presentation events are rejected instead of
    /// expanding a List inside the production tick.
    /// </summary>
    public sealed class SimulationBattleBufferModule
    {
        private bool isSealed;
        private int soundEventLimit;

        public SimulationBattleBufferModule(int runtimeCapacity)
        {
            PendingSounds = new List<PendingSoundEvent>();
            PendingUnregister = new List<ISimObject>();
            PendingSlotReleasedDestroy = new List<LF2Entity>();
            EntityScratch = new List<LF2Entity>();
            RegisteredObjectResetSet = new HashSet<ISimObject>(
                Math.Max(128, runtimeCapacity));
            DefaultHeldObjectWeaponPoint = new WeaponPoint();
            Prepare(runtimeCapacity, runtimeCapacity);
        }

        public List<PendingSoundEvent> PendingSounds { get; }
        public List<ISimObject> PendingUnregister { get; }
        public List<LF2Entity> PendingSlotReleasedDestroy { get; }
        public List<LF2Entity> EntityScratch { get; }
        internal HashSet<ISimObject> RegisteredObjectResetSet { get; }
        internal WeaponPoint DefaultHeldObjectWeaponPoint { get; }
        public bool IsSealed => isSealed;
        public long RejectedSoundEventCount { get; private set; }

        public void Prepare(int runtimeCapacity, int registeredObjectCount)
        {
            if (isSealed)
                return;

            int entityCapacity = Math.Max(128, runtimeCapacity);
            int objectCapacity = Math.Max(entityCapacity, registeredObjectCount + 128);
            long desiredSoundCapacity = Math.Max(256L, (long)runtimeCapacity * 16L);
            soundEventLimit = (int)Math.Min(1_048_576L, desiredSoundCapacity);

            EnsureCapacity(PendingSounds, soundEventLimit);
            EnsureCapacity(PendingUnregister, objectCapacity);
            EnsureCapacity(PendingSlotReleasedDestroy, entityCapacity);
            EnsureCapacity(EntityScratch, entityCapacity);
            RegisteredObjectResetSet.EnsureCapacity(objectCapacity);
        }

        public void Seal()
        {
            isSealed = true;
        }

        public void Unseal()
        {
            isSealed = false;
        }

        public bool TryQueueSound(PendingSoundEvent sound)
        {
            if (isSealed && PendingSounds.Count >= soundEventLimit)
            {
                RejectedSoundEventCount++;
                return false;
            }

            PendingSounds.Add(sound);
            return true;
        }

        private static void EnsureCapacity<T>(List<T> values, int capacity)
        {
            if (values.Capacity < capacity)
                values.Capacity = capacity;
        }
    }
}
