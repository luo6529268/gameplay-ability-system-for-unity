using System;
using System.Runtime.CompilerServices;

namespace NTSD.Simulation.Ecs
{
    internal readonly struct BattleFrameMotionAiProjection
    {
        internal BattleFrameMotionAiProjection(
            int x,
            int y,
            int z,
            double vx,
            int facing,
            int frame,
            int state,
            int hitStop)
        {
            X = x;
            Y = y;
            Z = z;
            Vx = vx;
            Facing = facing;
            Frame = frame;
            State = state;
            HitStop = hitStop;
        }

        internal int X { get; }
        internal int Y { get; }
        internal int Z { get; }
        internal double Vx { get; }
        internal int Facing { get; }
        internal int Frame { get; }
        internal int State { get; }
        internal int HitStop { get; }
    }

    public readonly struct BattleFrameMotionStateView
    {
        internal BattleFrameMotionStateView(
            RuntimeEntityHandle handle,
            int xInt,
            int yInt,
            int zInt,
            double vx,
            int facing,
            int frame,
            int state,
            int hitStop)
        {
            Handle = handle;
            XInt = xInt;
            YInt = yInt;
            ZInt = zInt;
            Vx = vx;
            Facing = facing;
            Frame = frame;
            State = state;
            HitStop = hitStop;
        }

        public RuntimeEntityHandle Handle { get; }
        public int XInt { get; }
        public int YInt { get; }
        public int ZInt { get; }
        public double Vx { get; }
        public int Facing { get; }
        public int Frame { get; }
        public int State { get; }
        public int HitStop { get; }
    }

    /// <summary>
    /// Persistent Direct-SoA storage for the frame/motion fields consumed by the
    /// same-tick AI projection. Runtime objects remain compatibility mirrors during
    /// U6; generation ownership prevents a released handle from mutating a reused slot.
    /// </summary>
    internal sealed class BattleFrameMotionStore
    {
        private readonly BattleAiUnifiedRowPublisher unifiedRowPublisher;
        private NTSDEntityRuntime[] owners;
        private uint[] generations;
        private int[] xInt;
        private int[] yInt;
        private int[] zInt;
        private double[] vx;
        private byte[] facing;
        private int[] frame;
        private int[] state;
        private int[] hitStop;

        internal BattleFrameMotionStore(
            int capacity,
            BattleAiUnifiedRowPublisher unifiedRowPublisher)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            this.unifiedRowPublisher = unifiedRowPublisher ??
                throw new ArgumentNullException(nameof(unifiedRowPublisher));
            owners = new NTSDEntityRuntime[capacity];
            generations = new uint[capacity];
            xInt = new int[capacity];
            yInt = new int[capacity];
            zInt = new int[capacity];
            vx = new double[capacity];
            facing = new byte[capacity];
            frame = new int[capacity];
            state = new int[capacity];
            hitStop = new int[capacity];
        }

        internal void Bind(NTSDEntityRuntime runtime, RuntimeEntityHandle handle)
        {
            if (runtime == null ||
                !handle.IsValid ||
                handle.Slot >= owners.Length ||
                runtime.SlotIndex != handle.Slot)
            {
                throw new InvalidOperationException(
                    "Frame/motion store requires a current runtime handle.");
            }

            int slot = handle.Slot;
            owners[slot] = runtime;
            generations[slot] = handle.Generation;
            CaptureAll(slot, runtime);
            runtime.BindFrameMotionStore(this, slot);
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
            owners[slot]?.UnbindFrameMotionStore(this, slot);
            owners[slot] = null;
            generations[slot] = 0;
            ClearSlot(slot);
        }

        internal void Reset()
        {
            for (int slot = 0; slot < owners.Length; slot++)
            {
                owners[slot]?.UnbindFrameMotionStore(this, slot);
            }

            Array.Clear(owners, 0, owners.Length);
            Array.Clear(generations, 0, generations.Length);
            for (int slot = 0; slot < owners.Length; slot++)
                ClearSlot(slot);
        }

        internal void GrowTo(int capacity)
        {
            if (capacity <= owners.Length)
                return;

            Array.Resize(ref owners, capacity);
            Array.Resize(ref generations, capacity);
            Array.Resize(ref xInt, capacity);
            Array.Resize(ref yInt, capacity);
            Array.Resize(ref zInt, capacity);
            Array.Resize(ref vx, capacity);
            Array.Resize(ref facing, capacity);
            Array.Resize(ref frame, capacity);
            Array.Resize(ref state, capacity);
            Array.Resize(ref hitStop, capacity);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureChangedField(
            int slot,
            RuntimeFrameMotionField field,
            double value)
        {
            if (field != RuntimeFrameMotionField.Vx)
                return;

            switch (field)
            {
                case RuntimeFrameMotionField.Vx: vx[slot] = value; break;
            }
            unifiedRowPublisher.PublishFrameMotion(
                slot,
                generations[slot],
                field,
                value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureChangedField(
            int slot,
            RuntimeFrameMotionField field,
            int value)
        {
            switch (field)
            {
                case RuntimeFrameMotionField.XInt:
                    xInt[slot] = value;
                    break;
                case RuntimeFrameMotionField.YInt:
                    yInt[slot] = value;
                    break;
                case RuntimeFrameMotionField.ZInt:
                    zInt[slot] = value;
                    break;
                case RuntimeFrameMotionField.Frame:
                    frame[slot] = value;
                    break;
                case RuntimeFrameMotionField.State:
                    state[slot] = value;
                    break;
                case RuntimeFrameMotionField.HitStop:
                    hitStop[slot] = value;
                    break;
                default:
                    return;
            }

            unifiedRowPublisher.PublishFrameMotion(
                slot,
                generations[slot],
                field,
                value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureFacing(int slot, bool isFacingLeft)
        {
            facing[slot] = isFacingLeft ? (byte)1 : (byte)0;
            unifiedRowPublisher.PublishFacing(
                slot,
                generations[slot],
                facing[slot]);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void CaptureIntegerPosition(
            int slot,
            int nextX,
            int nextY,
            int nextZ)
        {
            xInt[slot] = nextX;
            yInt[slot] = nextY;
            zInt[slot] = nextZ;
            unifiedRowPublisher.PublishIntegerPosition(
                slot,
                generations[slot],
                nextX,
                nextY,
                nextZ);
        }

        internal bool TryCaptureAiProjection(
            NTSDEntityRuntime runtime,
            out BattleFrameMotionAiProjection projection)
        {
            if (!TryResolve(runtime, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleFrameMotionAiProjection(
                xInt[slot],
                yInt[slot],
                zInt[slot],
                vx[slot],
                facing[slot],
                frame[slot],
                state[slot],
                hitStop[slot]);
            return true;
        }

        internal bool TryCaptureAiProjection(
            RuntimeEntityHandle handle,
            out BattleFrameMotionAiProjection projection)
        {
            if (!TryResolve(handle, out int slot))
            {
                projection = default;
                return false;
            }

            projection = new BattleFrameMotionAiProjection(
                xInt[slot],
                yInt[slot],
                zInt[slot],
                vx[slot],
                facing[slot],
                frame[slot],
                state[slot],
                hitStop[slot]);
            return true;
        }

        internal bool TryGetState(
            NTSDEntityRuntime runtime,
            out BattleFrameMotionStateView view)
        {
            if (!TryResolve(runtime, out int slot))
            {
                view = default;
                return false;
            }

            view = new BattleFrameMotionStateView(
                new RuntimeEntityHandle(slot, generations[slot]),
                xInt[slot],
                yInt[slot],
                zInt[slot],
                vx[slot],
                facing[slot],
                frame[slot],
                state[slot],
                hitStop[slot]);
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
            xInt[slot] = runtime.XInt;
            yInt[slot] = runtime.YInt;
            zInt[slot] = runtime.ZInt;
            vx[slot] = runtime.Vx;
            facing[slot] = runtime.IsFacingLeft ? (byte)1 : (byte)0;
            frame[slot] = runtime.Frame;
            state[slot] = runtime.FrameState;
            hitStop[slot] = runtime.HitStop;
        }

        private void ClearSlot(int slot)
        {
            xInt[slot] = yInt[slot] = zInt[slot] = 0;
            vx[slot] = 0.0;
            facing[slot] = 0;
            frame[slot] = 0;
            state[slot] = 0;
            hitStop[slot] = 0;
        }
    }
}
