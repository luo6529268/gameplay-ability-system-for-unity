#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Simulation;
using NTSD.Simulation.Presentation;
using NUnit.Framework;

namespace NTSD.Test
{
    public sealed class BattlePresentationMotionSamplerEditorTests
    {
        [Test]
        public void Sample_UsesNativeRoundingBeforeApprovedViewScale()
        {
            BattlePresentationMotionState previous = State(3, 1, 2, 100, -2, -2);
            BattlePresentationMotionState current = State(3, 1, 2, 101, -1, -1);

            BattlePresentationMotionSampleStatus status =
                BattlePresentationMotionSampler.Sample(
                    previous, current, 10, 11, 0.5,
                    2048.0 / 1333.0, 1152.0 / 730.0,
                    out BattlePresentationMotionDelta delta);

            Assert.That(status, Is.EqualTo(BattlePresentationMotionSampleStatus.Sampled));
            Assert.That(delta.ViewX, Is.Zero);
            Assert.That(delta.Y, Is.EqualTo(-1));
            Assert.That(delta.ViewZ, Is.EqualTo(-1152.0 / 730.0).Within(1e-12));
            Assert.That(previous.PreciseX, Is.EqualTo(100));
            Assert.That(current.PreciseX, Is.EqualTo(101));

            status = BattlePresentationMotionSampler.Sample(
                previous, current, 10, 11, -4.0,
                2048.0 / 1333.0, 1152.0 / 730.0, out delta);
            Assert.That(status, Is.EqualTo(BattlePresentationMotionSampleStatus.Sampled));
            Assert.That(delta.ViewX, Is.EqualTo(-2048.0 / 1333.0).Within(1e-12));
            Assert.That(delta.Y, Is.EqualTo(-1));
            status = BattlePresentationMotionSampler.Sample(
                previous, current, 10, 11, 4.0,
                2048.0 / 1333.0, 1152.0 / 730.0, out delta);
            Assert.That(status, Is.EqualTo(BattlePresentationMotionSampleStatus.Sampled));
            Assert.That(delta.ViewX, Is.Zero);
            Assert.That(delta.Y, Is.Zero);
            Assert.That(delta.ViewZ, Is.Zero);
        }

        [Test]
        public void Sample_RejectsTickIdentityAndEveryRelationChange()
        {
            var priorRuntime = Runtime(20, 2, 1);
            var currentRuntime = Runtime(21, 2, 1);
            priorRuntime.OwnerSlotIndex = currentRuntime.OwnerSlotIndex = 0;
            priorRuntime.HolderStableId = currentRuntime.HolderStableId = 1;
            priorRuntime.TargetSlotIndex = currentRuntime.TargetSlotIndex = 2;
            priorRuntime.CaughtSlotIndex = currentRuntime.CaughtSlotIndex = 3;
            priorRuntime.CatchSourceSlot90 = currentRuntime.CatchSourceSlot90 = 0x2004;
            priorRuntime.LinkState = currentRuntime.LinkState = -1;
            BattlePresentationMotionState previous = new BattlePresentationMotionState(
                new RuntimeEntityHandle(3, 1), 2, priorRuntime);
            BattlePresentationMotionState current = new BattlePresentationMotionState(
                new RuntimeEntityHandle(3, 1), 2, currentRuntime);

            AssertStatus(previous, current, 10, 12,
                BattlePresentationMotionSampleStatus.NonAdjacentTicks);
            AssertStatus(previous, current, -1, 0,
                BattlePresentationMotionSampleStatus.NonAdjacentTicks);
            AssertStatus(previous, current, 10, 11,
                BattlePresentationMotionSampleStatus.Sampled);
            AssertStatus(previous, new BattlePresentationMotionState(
                    new RuntimeEntityHandle(3, 2), 2, currentRuntime), 10, 11,
                BattlePresentationMotionSampleStatus.IdentityChanged);
            AssertStatus(previous, new BattlePresentationMotionState(
                    new RuntimeEntityHandle(3, 1), 9, currentRuntime), 10, 11,
                BattlePresentationMotionSampleStatus.IdentityChanged);

            currentRuntime.OwnerSlotIndex++;
            AssertRelationChanged(previous, currentRuntime);
            currentRuntime.OwnerSlotIndex--;
            currentRuntime.HolderStableId++;
            AssertRelationChanged(previous, currentRuntime);
            currentRuntime.HolderStableId--;
            currentRuntime.TargetSlotIndex++;
            AssertRelationChanged(previous, currentRuntime);
            currentRuntime.TargetSlotIndex--;
            currentRuntime.CaughtSlotIndex++;
            AssertRelationChanged(previous, currentRuntime);
            currentRuntime.CaughtSlotIndex--;
            currentRuntime.CatchSourceSlot90++;
            AssertRelationChanged(previous, currentRuntime);
            currentRuntime.CatchSourceSlot90--;
            currentRuntime.LinkState++;
            AssertRelationChanged(previous, currentRuntime);
        }

        [Test]
        public void Sample_RejectsTeleportButAcceptsMotionSupportedDash()
        {
            BattlePresentationMotionState still = State(3, 1, 2, 0, 0, 0);
            BattlePresentationMotionState jumped = State(3, 1, 2, 65, 0, 0);
            AssertStatus(still, jumped, 10, 11,
                BattlePresentationMotionSampleStatus.MotionDiscontinuity);

            NTSDEntityRuntime dashRuntime = Runtime(65, 0, 0);
            dashRuntime.Vx = 20;
            BattlePresentationMotionState dash = new BattlePresentationMotionState(
                new RuntimeEntityHandle(3, 1), 2, dashRuntime);
            AssertStatus(still, dash, 10, 11,
                BattlePresentationMotionSampleStatus.Sampled);

            dashRuntime.SourceRulePositionInitialized = false;
            AssertStatus(still, new BattlePresentationMotionState(
                    new RuntimeEntityHandle(3, 1), 2, dashRuntime), 10, 11,
                BattlePresentationMotionSampleStatus.SourcePositionUnavailable);
        }

        private static void AssertRelationChanged(
            BattlePresentationMotionState previous,
            NTSDEntityRuntime currentRuntime)
        {
            AssertStatus(previous, new BattlePresentationMotionState(
                    new RuntimeEntityHandle(3, 1), 2, currentRuntime), 10, 11,
                BattlePresentationMotionSampleStatus.RelationChanged);
        }

        private static void AssertStatus(
            BattlePresentationMotionState previous,
            BattlePresentationMotionState current,
            int previousTick,
            int currentTick,
            BattlePresentationMotionSampleStatus expected)
        {
            BattlePresentationMotionSampleStatus actual =
                BattlePresentationMotionSampler.Sample(
                    previous, current, previousTick, currentTick, 0.5,
                    2048.0 / 1333.0, 1152.0 / 730.0,
                    out BattlePresentationMotionDelta delta);
            Assert.That(actual, Is.EqualTo(expected));
            if (expected != BattlePresentationMotionSampleStatus.Sampled)
            {
                Assert.That(delta.ViewX, Is.Zero);
                Assert.That(delta.Y, Is.Zero);
                Assert.That(delta.ViewZ, Is.Zero);
            }
        }

        private static BattlePresentationMotionState State(
            int slot, uint generation, int objectId,
            double x, double y, double z)
        {
            return new BattlePresentationMotionState(
                new RuntimeEntityHandle(slot, generation), objectId,
                Runtime(x, y, z));
        }

        private static NTSDEntityRuntime Runtime(double x, double y, double z)
        {
            var runtime = new NTSDEntityRuntime();
            runtime.Reset();
            runtime.SetSourceRulePosition(x, z);
            runtime.X = x * 2048.0 / 1333.0;
            runtime.Y = y;
            runtime.Z = z * 1152.0 / 730.0;
            return runtime;
        }
    }
}
#endif
