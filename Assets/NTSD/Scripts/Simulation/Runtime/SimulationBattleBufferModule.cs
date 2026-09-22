using System;
using System.Collections.Generic;
using NTSD.Animation;
using NTSD.Animation.LF2Objects;

namespace NTSD.Simulation
{
    public readonly struct NativeKnockoutEvent
    {
        public NativeKnockoutEvent(
            int battleTimeTick,
            int sourceObjectType,
            int fourOwnerSlot,
            int victimSlot,
            int sourceSlot,
            int creditSlot)
        {
            BattleTimeTick = battleTimeTick;
            SourceObjectType = sourceObjectType;
            FourOwnerSlot = fourOwnerSlot;
            VictimSlot = victimSlot;
            SourceSlot = sourceSlot;
            CreditSlot = creditSlot;
        }

        public int BattleTimeTick { get; }
        public int SourceObjectType { get; }
        public int FourOwnerSlot { get; }
        public int VictimSlot { get; }
        public int SourceSlot { get; }
        public int CreditSlot { get; }
    }

    /// <summary>
    /// Match-owned reusable managed buffers. Bounded sound events reject
    /// overflow after sealing. Native KO records retain the formal unbounded
    /// newest-tail behavior; excess growth is measured by the memory boundary.
    /// </summary>
    public sealed class SimulationBattleBufferModule
    {
        private bool isSealed;
        private int soundEventLimit;

        public SimulationBattleBufferModule(int runtimeCapacity)
        {
            PendingSounds = new List<PendingSoundEvent>();
            NativeKnockoutEvents = new List<NativeKnockoutEvent>();
            PendingUnregister = new List<ISimObject>();
            PendingSlotReleasedDestroy = new List<LF2Entity>();
            EntityScratch = new List<LF2Entity>();
            RegisteredObjectResetSet = new HashSet<ISimObject>(
                Math.Max(128, runtimeCapacity));
            DefaultHeldObjectWeaponPoint = default;
            Prepare(runtimeCapacity, runtimeCapacity);
        }

        public List<PendingSoundEvent> PendingSounds { get; }
        public List<NativeKnockoutEvent> NativeKnockoutEvents { get; }
        public List<ISimObject> PendingUnregister { get; }
        public List<LF2Entity> PendingSlotReleasedDestroy { get; }
        public List<LF2Entity> EntityScratch { get; }
        internal HashSet<ISimObject> RegisteredObjectResetSet { get; }
        internal BattleWeaponPointValue DefaultHeldObjectWeaponPoint { get; }
        public bool IsSealed => isSealed;
        public long RejectedSoundEventCount { get; private set; }
        internal bool CanQueueSoundWithoutRejection =>
            !isSealed || PendingSounds.Count < soundEventLimit;

        internal bool CanQueueSoundsWithoutRejection(int count)
        {
            if (count <= 0 || !isSealed)
                return true;

            return PendingSounds.Count <= soundEventLimit - count;
        }

        public void Prepare(int runtimeCapacity, int registeredObjectCount)
        {
            if (isSealed)
                return;

            int entityCapacity = Math.Max(128, runtimeCapacity);
            int objectCapacity = Math.Max(entityCapacity, registeredObjectCount + 128);
            long desiredSoundCapacity = Math.Max(256L, (long)runtimeCapacity * 16L);
            soundEventLimit = (int)Math.Min(1_048_576L, desiredSoundCapacity);

            EnsureCapacity(PendingSounds, soundEventLimit);
            EnsureCapacity(NativeKnockoutEvents, entityCapacity);
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

        internal void RecordNativeKnockout(in NativeKnockoutEvent value)
        {
            // The formal newest-tail expiry can retain old records indefinitely
            // while new KOs keep arriving. Do not impose a gameplay count cap.
            NativeKnockoutEvents.Add(value);
        }

        internal int PruneNativeKnockoutTail(int currentTick, int lifetimeTicks)
        {
            if (lifetimeTicks < 0)
                return 0;
            int removed = 0;
            while (NativeKnockoutEvents.Count > 0)
            {
                NativeKnockoutEvent tail =
                    NativeKnockoutEvents[NativeKnockoutEvents.Count - 1];
                if (tail.BattleTimeTick == 0 || currentTick <= tail.BattleTimeTick ||
                    (long)currentTick - tail.BattleTimeTick <= lifetimeTicks)
                    break;
                NativeKnockoutEvents.RemoveAt(NativeKnockoutEvents.Count - 1);
                removed++;
            }
            return removed;
        }

        private static void EnsureCapacity<T>(List<T> values, int capacity)
        {
            if (values.Capacity < capacity)
                values.Capacity = capacity;
        }
    }
}
