using System;
using System.Runtime.CompilerServices;

namespace NTSD.Simulation.Ecs
{
    internal enum RuntimeVitalField : byte
    {
        Hp,
        HpBound,
        Hp3,
        Pp,
    }

    internal readonly struct BattleVitalAiProjection
    {
        internal BattleVitalAiProjection(int hp, int hpBound, int hp3, int pp)
        {
            Hp = hp;
            HpBound = hpBound;
            Hp3 = hp3;
            Pp = pp;
        }

        internal int Hp { get; }
        internal int HpBound { get; }
        internal int Hp3 { get; }
        internal int Pp { get; }
    }

    public readonly struct BattleVitalStateView
    {
        internal BattleVitalStateView(
            RuntimeEntityHandle handle,
            int hp,
            int hpBound,
            int hp3,
            int pp)
        {
            Handle = handle;
            Hp = hp;
            HpBound = hpBound;
            Hp3 = hp3;
            Pp = pp;
        }

        public RuntimeEntityHandle Handle { get; }
        public int Hp { get; }
        public int HpBound { get; }
        public int Hp3 { get; }
        public int Pp { get; }
    }

    /// <summary>
    /// Persistent Direct-SoA storage for the vitality fields consumed by
    /// same-tick AI. Existing health wrappers continue to write Runtime during
    /// U6, while generation ownership keeps reused slots isolated.
    /// </summary>
    internal sealed class BattleVitalStore
    {
        private readonly BattleAiUnifiedRowPublisher unifiedRowPublisher;
        private NTSDEntityRuntime[] owners;
        private uint[] generations;
        private int[] hp;
        private int[] hpBound;
        private int[] hp3;
        private int[] pp;

        internal BattleVitalStore(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.unifiedRowPublisher = unifiedRowPublisher ??
                throw new ArgumentNullException(nameof(unifiedRowPublisher));
            owners = new NTSDEntityRuntime[capacity];
            generations = new uint[capacity];
            hp = new int[capacity];
            hpBound = new int[capacity];
            hp3 = new int[capacity];
            pp = new int[capacity];
            Array.Fill(hp, 500);
            Array.Fill(hpBound, 500);
            Array.Fill(hp3, 500);
            Array.Fill(pp, 500);
        }

        internal void Bind(NTSDEntityRuntime runtime, RuntimeEntityHandle handle)
        {
            if (runtime == null ||
                !handle.IsValid ||
                handle.Slot >= owners.Length ||
                runtime.SlotIndex != handle.Slot)
            {
                throw new InvalidOperationException(
                    "Vital store requires a current runtime handle.");
            }

            int slot = handle.Slot;
            owners[slot] = runtime;
            generations[slot] = handle.Generation;
            CaptureAll(slot, runtime);
            runtime.BindVitalStore(this, slot);
        }

        internal void Release(RuntimeEntityHandle handle)
        {
            if (!handle.IsValid ||
                handle.Slot >= owners.Length ||
                generations[handle.Slot] != handle.Generation)
            {
                return;
            }

            int slot = handle.Slot;
            owners[slot]?.UnbindVitalStore(this, slot);
            owners[slot] = null;
            generations[slot] = 0;
            ClearSlot(slot);
        }

        internal void Reset()
        {
            for (int slot = 0; slot < owners.Length; slot++)
                owners[slot]?.UnbindVitalStore(this, slot);

            Array.Clear(owners, 0, owners.Length);
            Array.Clear(generations, 0, generations.Length);
            for (int slot = 0; slot < owners.Length; slot++)
                ClearSlot(slot);
        }

        internal void GrowTo(int capacity)
        {
            if (capacity <= owners.Length)
                return;

            int previousCapacity = owners.Length;
            Array.Resize(ref owners, capacity);
            Array.Resize(ref generations, capacity);
            Array.Resize(ref hp, capacity);
            Array.Resize(ref hpBound, capacity);
            Array.Resize(ref hp3, capacity);
            Array.Resize(ref pp, capacity);
            Array.Fill(hp, 500, previousCapacity, capacity - previousCapacity);
            Array.Fill(hpBound, 500, previousCapacity, capacity - previousCapacity);
            Array.Fill(hp3, 500, previousCapacity, capacity - previousCapacity);
            Array.Fill(pp, 500, previousCapacity, capacity - previousCapacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureChangedField(
            int slot,
            RuntimeVitalField field,
            int value)
        {
            switch (field)
            {
                case RuntimeVitalField.Hp:
                    hp[slot] = value;
                    break;
                case RuntimeVitalField.HpBound:
                    hpBound[slot] = value;
                    break;
                case RuntimeVitalField.Hp3:
                    hp3[slot] = value;
                    break;
                case RuntimeVitalField.Pp:
                    pp[slot] = value;
                    break;
            }
            unifiedRowPublisher.PublishVital(
                slot,
                generations[slot],
                field,
                value);
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleVitalAiProjection projection)
        {
            if (!TryResolve(runtime, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleVitalAiProjection(
                hp[slot],
                hpBound[slot],
                hp3[slot],
                pp[slot]);
            return true;
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleVitalAiProjection projection)
        {
            if (!TryResolve(handle, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleVitalAiProjection(
                hp[slot],
                hpBound[slot],
                hp3[slot],
                pp[slot]);
            return true;
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleVitalStateView view)
        {
            if (!TryResolve(runtime, out int slot))
            {
                view = default;
                return false;
            }

            view = new BattleVitalStateView(
                new RuntimeEntityHandle(slot, generations[slot]),
                hp[slot],
                hpBound[slot],
                hp3[slot],
                pp[slot]);
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool TryResolve(NTSDEntityRuntime runtime, out int slot)
        {
            slot = runtime?.SlotIndex ?? -1;
            return (uint)slot < (uint)owners.Length &&
                   generations[slot] != 0 &&
                   ReferenceEquals(owners[slot], runtime);
        }

        private bool TryResolve(RuntimeEntityHandle handle, out int slot)
        {
            slot = handle.Slot;
            return handle.IsValid &&
                   (uint)slot < (uint)owners.Length &&
                   generations[slot] == handle.Generation;
        }

        private void CaptureAll(int slot, NTSDEntityRuntime runtime)
        {
            hp[slot] = runtime.HP;
            hpBound[slot] = runtime.HPBound;
            hp3[slot] = runtime.HP3;
            pp[slot] = runtime.PP;
        }

        private void ClearSlot(int slot)
        {
            hp[slot] = 500;
            hpBound[slot] = 500;
            hp3[slot] = 500;
            pp[slot] = 500;
        }
    }
}
