using System;

namespace NTSD.Simulation.Presentation
{
    public enum BattlePresentationMotionSampleStatus : byte
    {
        Sampled,
        NonAdjacentTicks,
        IdentityChanged,
        RelationChanged,
        MotionDiscontinuity,
        SourcePositionUnavailable,
    }

    public readonly struct BattlePresentationMotionDelta
    {
        public BattlePresentationMotionDelta(double viewX, int y, double viewZ)
            : this(viewX, y, y, viewZ)
        {
        }

        public BattlePresentationMotionDelta(
            double viewX, int y, double viewY, double viewZ)
        {
            ViewX = viewX;
            Y = y;
            ViewY = viewY;
            ViewZ = viewZ;
        }

        public double ViewX { get; }
        public int Y { get; }
        public double ViewY { get; }
        public double ViewZ { get; }
    }

    public static class BattlePresentationMotionSampler
    {
        public static BattlePresentationMotionSampleStatus Sample(
            in BattlePresentationMotionState previous,
            in BattlePresentationMotionState current,
            int previousTick,
            int currentTick,
            double alpha,
            double viewScaleX,
            double viewScaleZ,
            out BattlePresentationMotionDelta delta)
        {
            return Sample(previous, current, previousTick, currentTick,
                alpha, viewScaleX, 1.0, viewScaleZ, out delta);
        }

        public static BattlePresentationMotionSampleStatus Sample(
            in BattlePresentationMotionState previous,
            in BattlePresentationMotionState current,
            int previousTick,
            int currentTick,
            double alpha,
            double viewScaleX,
            double viewScaleY,
            double viewScaleZ,
            out BattlePresentationMotionDelta delta)
        {
            delta = default;
            if (previousTick < 0 || (long)previousTick + 1 != currentTick)
                return BattlePresentationMotionSampleStatus.NonAdjacentTicks;
            if (!previous.Handle.Equals(current.Handle) ||
                previous.ObjectId != current.ObjectId)
                return BattlePresentationMotionSampleStatus.IdentityChanged;
            if (previous.OwnerSlot != current.OwnerSlot ||
                previous.LinkedParentSlot != current.LinkedParentSlot ||
                previous.LinkedChildSlot != current.LinkedChildSlot ||
                previous.CatchTargetSlot != current.CatchTargetSlot ||
                previous.CatchSourceSlot != current.CatchSourceSlot ||
                previous.InteractionState != current.InteractionState)
            {
                return BattlePresentationMotionSampleStatus.RelationChanged;
            }
            if (!previous.HasSourceRulePosition || !current.HasSourceRulePosition)
                return BattlePresentationMotionSampleStatus.SourcePositionUnavailable;
            if (!ContinuousAxis(previous.PreciseX, current.PreciseX,
                    previous.MotionX, current.MotionX) ||
                !ContinuousAxis(previous.PreciseY, current.PreciseY,
                    previous.MotionY, current.MotionY) ||
                !ContinuousAxis(previous.PreciseZ, current.PreciseZ,
                    previous.MotionZ, current.MotionZ))
            {
                return BattlePresentationMotionSampleStatus.MotionDiscontinuity;
            }

            // Alignment contract: NTSD28-Q09-P02-MOTION-SAMPLE-KERNEL-001.
            // Native continuity and lround happen in source space; D-024 scales only the display delta.
            alpha = Math.Max(0.0, Math.Min(1.0, alpha));
            int sourceDeltaX = RoundedDelta(previous.PreciseX, current.PreciseX, alpha);
            int sourceDeltaY = RoundedDelta(previous.PreciseY, current.PreciseY, alpha);
            int sourceDeltaZ = RoundedDelta(previous.PreciseZ, current.PreciseZ, alpha);
            delta = new BattlePresentationMotionDelta(
                sourceDeltaX * viewScaleX,
                sourceDeltaY,
                sourceDeltaY * viewScaleY,
                sourceDeltaZ * viewScaleZ);
            return BattlePresentationMotionSampleStatus.Sampled;
        }

        private static bool ContinuousAxis(
            double previousPosition,
            double currentPosition,
            double previousMotion,
            double currentMotion)
        {
            double motion = Math.Max(Math.Abs(previousMotion), Math.Abs(currentMotion));
            double maximumDelta = Math.Max(64.0, motion * 4.0 + 4.0);
            return Math.Abs(currentPosition - previousPosition) <= maximumDelta;
        }

        private static int RoundedDelta(double previous, double current, double alpha)
        {
            double interpolated = previous + (current - previous) * alpha;
            return (int)Math.Round(interpolated, MidpointRounding.AwayFromZero) -
                   (int)Math.Round(current, MidpointRounding.AwayFromZero);
        }
    }
}
