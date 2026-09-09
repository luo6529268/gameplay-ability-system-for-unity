#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4HardMotionConsumerEditorTests
    {
        [TestCase(2.0, 5, 0, 5.0)]
        [TestCase(7.0, 5, 0, 7.0)]
        [TestCase(-2.0, 5, 1, -5.0)]
        [TestCase(-7.0, 5, 1, -7.0)]
        [TestCase(2.0, -5, 0, -5.0)]
        [TestCase(-7.0, -5, 0, -7.0)]
        [TestCase(-2.0, -5, 1, 5.0)]
        public void DxClamp_UsesHitFacingSpaceAndOnlyStrengthensTowardThreshold(
            double initialVx,
            int statusDx,
            int hitFacing,
            double expectedVx)
        {
            var runtime = CreateRuntime();
            runtime.Vx = initialVx;
            runtime.StatusDx1C0 = statusDx;
            runtime.StatusHitFacing1D0 = hitFacing;

            BattleNativeHardLandingMotionKernel.Consume(runtime);

            Assert.That(runtime.Vx, Is.EqualTo(expectedVx));
            AssertConsumed(runtime);
        }

        [TestCase(500, 500.0)]
        [TestCase(501, -49.0)]
        [TestCase(550, 0.0)]
        [TestCase(560, 10.0)]
        public void DxSentinel_IsStrictlyGreaterThan500(
            int statusDx,
            double expectedVx)
        {
            var runtime = CreateRuntime();
            runtime.Vx = 0.0;
            runtime.StatusDx1C0 = statusDx;

            BattleNativeHardLandingMotionKernel.Consume(runtime);

            Assert.That(runtime.Vx, Is.EqualTo(expectedVx));
            AssertConsumed(runtime);
        }

        [Test]
        public void DyDz_AddAt500AndOverrideAbove500()
        {
            var additive = CreateRuntime();
            additive.Vy = 2.0;
            additive.Vz = -3.0;
            additive.StatusDy1C4 = 500;
            additive.StatusDz1C8 = -5;

            BattleNativeHardLandingMotionKernel.Consume(additive);

            Assert.That(additive.Vy, Is.EqualTo(502.0));
            Assert.That(additive.Vz, Is.EqualTo(-8.0));
            AssertConsumed(additive);

            var overriding = CreateRuntime();
            overriding.Vy = 20.0;
            overriding.Vz = 30.0;
            overriding.StatusDy1C4 = 501;
            overriding.StatusDz1C8 = 560;

            BattleNativeHardLandingMotionKernel.Consume(overriding);

            Assert.That(overriding.Vy, Is.EqualTo(-49.0));
            Assert.That(overriding.Vz, Is.EqualTo(10.0));
            AssertConsumed(overriding);
        }

        [Test]
        public void ZeroMotionValues_PreserveVelocityButStillClearGain()
        {
            var runtime = CreateRuntime();
            runtime.Vx = 3.0;
            runtime.Vy = 4.0;
            runtime.Vz = 5.0;

            BattleNativeHardLandingMotionKernel.Consume(runtime);

            Assert.That(runtime.Vx, Is.EqualTo(3.0));
            Assert.That(runtime.Vy, Is.EqualTo(4.0));
            Assert.That(runtime.Vz, Is.EqualTo(5.0));
            AssertConsumed(runtime);
        }

        [Test]
        public void Consume_PreservesFacingAndPickedActionCarriers()
        {
            var runtime = CreateRuntime();
            runtime.StatusHitFacing1D0 = 1;
            runtime.StatusPickedAction1D4 = 300;
            runtime.StatusPickingAction1D8 = 301;
            runtime.StatusDx1C0 = 6;
            runtime.StatusDy1C4 = 7;
            runtime.StatusDz1C8 = 8;

            BattleNativeHardLandingMotionKernel.Consume(runtime);

            Assert.That(runtime.StatusHitFacing1D0, Is.EqualTo(1));
            Assert.That(runtime.StatusPickedAction1D4, Is.EqualTo(300));
            Assert.That(runtime.StatusPickingAction1D8, Is.EqualTo(301));
            AssertConsumed(runtime);
        }

        [Test]
        public void NullRuntime_IsANoOp()
        {
            Assert.DoesNotThrow(() =>
                BattleNativeHardLandingMotionKernel.Consume(null));
        }

        private static NTSDEntityRuntime CreateRuntime()
        {
            return new NTSDEntityRuntime
            {
                StatusGain1CC = 1,
            };
        }

        private static void AssertConsumed(NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.StatusDx1C0, Is.Zero);
            Assert.That(runtime.StatusDy1C4, Is.Zero);
            Assert.That(runtime.StatusDz1C8, Is.Zero);
            Assert.That(runtime.StatusGain1CC, Is.Zero);
        }
    }
}
#endif
