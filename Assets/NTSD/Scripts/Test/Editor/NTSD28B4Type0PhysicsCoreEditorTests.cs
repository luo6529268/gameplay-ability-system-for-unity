#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using NTSD.Animation;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4Type0PhysicsCoreEditorTests
    {
        [Test]
        public void NegativeReferenceGroundedStepAppliesFrictionWithoutGravity()
        {
            NTSDEntityRuntime runtime = Runtime(-10.0, 3.0, 0.0, -2.0, -10);

            BattleMechanicsStepResult result = Step(runtime);

            Assert.That(result.Landed, Is.False);
            Assert.That((runtime.X, runtime.Z), Is.EqualTo((3.0, -2.0)));
            Assert.That((runtime.Vx, runtime.Vz), Is.EqualTo((2.0, -1.0)));
            Assert.That(runtime.Y, Is.EqualTo(-10.0));
            Assert.That(runtime.Vy, Is.Zero);
        }

        [Test]
        public void NegativeReferenceStrictCrossingClampsToEffectiveFloor()
        {
            NTSDEntityRuntime runtime = Runtime(-12.0, 0.0, 3.0, 0.0, -10);

            BattleMechanicsStepResult result = Step(runtime);

            Assert.That(result.Landed, Is.True);
            Assert.That(result.VerticalVelocityBeforeLanding, Is.EqualTo(3.0));
            Assert.That(runtime.Y, Is.EqualTo(-10.0));
            Assert.That(runtime.Vy, Is.EqualTo(3.0));
        }

        [Test]
        public void BelowNegativeReferenceRemainsAirborneAndReceivesGravity()
        {
            NTSDEntityRuntime runtime = Runtime(-21.0, 0.0, 0.0, 0.0, -20);

            BattleMechanicsStepResult result = Step(runtime);

            Assert.That(result.Landed, Is.False);
            Assert.That(runtime.Y, Is.EqualTo(-21.0));
            Assert.That(runtime.Vy, Is.EqualTo(1.7));
        }

        [Test]
        public void AlreadyOnFloorPositiveMotionDoesNotRetriggerLanding()
        {
            NTSDEntityRuntime runtime = Runtime(0.0, 0.0, 2.0, 0.0, 0);

            BattleMechanicsStepResult result = Step(runtime);

            Assert.That(result.Landed, Is.False);
            Assert.That(runtime.Y, Is.Zero);
            Assert.That(runtime.Vy, Is.EqualTo(2.0));
        }

        [Test]
        public void DefaultZeroReferenceStillDetectsStrictAirToFloorCrossing()
        {
            NTSDEntityRuntime runtime = Runtime(-1.0, 0.0, 2.0, 0.0, 0);

            BattleMechanicsStepResult result = Step(runtime);

            Assert.That(result.Landed, Is.True);
            Assert.That(runtime.Y, Is.Zero);
            Assert.That(result.VerticalVelocityBeforeLanding, Is.EqualTo(2.0));
        }

        private static BattleMechanicsStepResult Step(NTSDEntityRuntime runtime)
        {
            var context = new CharacterMechanicsContext(
                runtime,
                null,
                0f,
                0f,
                1.7);
            return new CharacterMechanics().StepBattleLogic(context);
        }

        private static NTSDEntityRuntime Runtime(
            double y,
            double vx,
            double vy,
            double vz,
            int collisionYReference)
        {
            var runtime = new NTSDEntityRuntime
            {
                CollisionYReference = collisionYReference,
            };
            runtime.SetPosition(0.0, y, 0.0);
            runtime.SetVelocity(vx, vy, vz);
            runtime.SyncIntegerPosition();
            return runtime;
        }
    }
}
#endif
