using System;
using NTSD.Animation;
using UnityEngine;

namespace NTSD.Simulation.Presentation
{
    /// <summary>Reusable, presentation-only lookup of accepted adjacent-tick motion.</summary>
    public sealed class BattlePresentationDisplayMotion
    {
        private int generation;
        private bool capacitySealed;
        private int sealedRuntimeSlotCapacity;
        private int[] previousIndexBySlot = new int[16];
        private int[] previousGenerationBySlot = new int[16];
        private int[] sampledGenerationBySlot = new int[16];
        private RuntimeEntityHandle[] sampledHandleBySlot = new RuntimeEntityHandle[16];
        private BattlePresentationMotionDelta[] sampledDeltaBySlot =
            new BattlePresentationMotionDelta[16];

        public int SampledCount { get; private set; }

        public void PrepareCapacity(int runtimeSlotCapacity)
        {
            if (runtimeSlotCapacity < 0)
                throw new ArgumentOutOfRangeException(nameof(runtimeSlotCapacity));
            if (capacitySealed)
                throw new InvalidOperationException("Cannot resize sealed presentation motion storage.");
            if (runtimeSlotCapacity > 0)
                EnsureCapacity(runtimeSlotCapacity);
        }

        internal void SealCapacity(int runtimeSlotCapacity)
        {
            if (capacitySealed || runtimeSlotCapacity < 0 || runtimeSlotCapacity > previousIndexBySlot.Length)
                throw new InvalidOperationException("Presentation motion capacity must be prepared before sealing.");
            sealedRuntimeSlotCapacity = runtimeSlotCapacity;
            capacitySealed = true;
        }

        internal void UnsealCapacity()
        {
            capacitySealed = false;
        }

        internal bool CanPrepare(BattlePresentationFrame frame)
        {
            if (!capacitySealed || frame == null)
                return true;
            if ((uint)frame.PreviousMotionStateCount > (uint)sealedRuntimeSlotCapacity ||
                (uint)frame.MotionStateCount > (uint)sealedRuntimeSlotCapacity)
                return false;
            for (int index = 0; index < frame.PreviousMotionStateCount; index++)
            {
                if (frame.GetPreviousMotionState(index).Handle.Slot >= sealedRuntimeSlotCapacity)
                    return false;
            }
            for (int index = 0; index < frame.MotionStateCount; index++)
            {
                if (frame.GetMotionState(index).Handle.Slot >= sealedRuntimeSlotCapacity)
                    return false;
            }
            return true;
        }

        public void Prepare(
            BattlePresentationFrame frame,
            double alpha,
            double viewScaleX,
            double viewScaleZ)
        {
            Prepare(frame, alpha, viewScaleX, 1.0, viewScaleZ);
        }

        public void Prepare(
            BattlePresentationFrame frame,
            double alpha,
            double viewScaleX,
            double viewScaleY,
            double viewScaleZ)
        {
            if (!CanPrepare(frame))
                throw new InvalidOperationException("Presentation motion slot capacity exceeded; whole frame rejected.");
            if (generation == int.MaxValue)
            {
                Array.Clear(previousGenerationBySlot, 0,
                    previousGenerationBySlot.Length);
                Array.Clear(sampledGenerationBySlot, 0,
                    sampledGenerationBySlot.Length);
                generation = 0;
            }
            generation++;
            SampledCount = 0;
            if (frame == null || frame.PreviousMotionTickIndex < 0 ||
                (long)frame.PreviousMotionTickIndex + 1 != frame.TickIndex ||
                alpha >= 1.0)
            {
                return;
            }

            for (int index = 0; index < frame.PreviousMotionStateCount; index++)
            {
                BattlePresentationMotionState prior =
                    frame.GetPreviousMotionState(index);
                int slot = prior.Handle.Slot;
                if (slot < 0)
                    continue;
                EnsureCapacity(slot + 1);
                previousIndexBySlot[slot] = index;
                previousGenerationBySlot[slot] = generation;
            }

            for (int index = 0; index < frame.MotionStateCount; index++)
            {
                BattlePresentationMotionState current = frame.GetMotionState(index);
                int slot = current.Handle.Slot;
                if (slot < 0 || slot >= previousGenerationBySlot.Length ||
                    previousGenerationBySlot[slot] != generation)
                {
                    continue;
                }

                BattlePresentationMotionState previous =
                    frame.GetPreviousMotionState(previousIndexBySlot[slot]);
                if (BattlePresentationMotionSampler.Sample(
                        previous, current,
                        frame.PreviousMotionTickIndex, frame.TickIndex,
                        alpha, viewScaleX, viewScaleY, viewScaleZ,
                        out BattlePresentationMotionDelta delta) !=
                    BattlePresentationMotionSampleStatus.Sampled)
                {
                    continue;
                }

                sampledHandleBySlot[slot] = current.Handle;
                sampledDeltaBySlot[slot] = delta;
                sampledGenerationBySlot[slot] = generation;
                SampledCount++;
            }
        }

        public bool TryGet(
            RuntimeEntityHandle handle,
            out BattlePresentationMotionDelta delta)
        {
            int slot = handle.Slot;
            if (slot >= 0 && slot < sampledGenerationBySlot.Length &&
                sampledGenerationBySlot[slot] == generation &&
                sampledHandleBySlot[slot].Equals(handle))
            {
                delta = sampledDeltaBySlot[slot];
                return true;
            }

            delta = default;
            return false;
        }

        public void ApplyToCapturedCommands(BattlePresentationFrame capturedFrame)
        {
            if (capturedFrame == null || SampledCount == 0)
                return;

            for (int index = 0; index < capturedFrame.CommandCount; index++)
            {
                BattleRenderCommand command = capturedFrame.GetCommand(index);
                if (command.Type == BattleRenderCommandType.HitRecord ||
                    !TryGet(command.Handle, out BattlePresentationMotionDelta delta))
                {
                    continue;
                }

                Vector3 bodyOffset = ToWorldBody(delta);
                Vector3 groundOffset = ToWorldGround(delta);
                Vector3 positionOffset = command.Type == BattleRenderCommandType.Shadow ||
                                         (command.Type == BattleRenderCommandType.OverlayGlyph &&
                                          command.MotionAnchor == BattlePresentationMotionAnchor.Ground)
                    ? groundOffset
                    : bodyOffset;
                capturedFrame.ReplaceCommand(index,
                    command.WithPresentationOffsets(
                        positionOffset,
                        new Vector2(bodyOffset.x, bodyOffset.y),
                        new Vector2(groundOffset.x, groundOffset.y)));
            }
        }

        public static Vector3 ToWorldBody(in BattlePresentationMotionDelta delta)
        {
            return new Vector3(
                (float)delta.ViewX * NTSDRenderSpace.UnitsPerPixelX,
                -(float)(delta.ViewY + delta.ViewZ) * NTSDRenderSpace.UnitsPerPixelY,
                0f);
        }

        public static Vector3 ToWorldGround(in BattlePresentationMotionDelta delta)
        {
            return new Vector3(
                (float)delta.ViewX * NTSDRenderSpace.UnitsPerPixelX,
                -(float)delta.ViewZ * NTSDRenderSpace.UnitsPerPixelY,
                0f);
        }

        private void EnsureCapacity(int required)
        {
            if (required <= previousIndexBySlot.Length)
                return;
            int capacity = previousIndexBySlot.Length;
            while (capacity < required)
                capacity *= 2;
            Array.Resize(ref previousIndexBySlot, capacity);
            Array.Resize(ref previousGenerationBySlot, capacity);
            Array.Resize(ref sampledGenerationBySlot, capacity);
            Array.Resize(ref sampledHandleBySlot, capacity);
            Array.Resize(ref sampledDeltaBySlot, capacity);
        }
    }
}
