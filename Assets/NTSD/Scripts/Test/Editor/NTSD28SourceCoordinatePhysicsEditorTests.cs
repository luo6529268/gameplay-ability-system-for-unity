using NUnit.Framework;

using NTSD.Animation;
using NTSD.Simulation;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SourceCoordinatePhysicsEditorTests
    {
        [TestCase(false, 1.0, 1.0)]
        [TestCase(false, 2048.0 / 1333.0, 1152.0 / 730.0)]
        [TestCase(true, 2048.0 / 1333.0, 1152.0 / 730.0)]
        public void CharacterMotion_TracksRawSourceIndependentlyOfViewAndFriction(
            bool blockPositiveX, double scaleX, double scaleZ)
        {
            var runtime = new NTSDEntityRuntime
            {
                Vx = 18.5,
                Vz = -3.25,
                SourceRuleXBoundPositive = blockPositiveX,
                SourceRuleZBoundPositive = true,
            };
            runtime.SetPosition(100, 0, 200);
            runtime.SyncIntegerPosition();
            runtime.SetSourceRulePosition(20.75, -10.5);
            runtime.SyncSourceRuleIntegerPosition();

            var context = new CharacterMechanicsContext(runtime, null, 0f, 0f,
                0.0, scaleX, scaleZ);
            new CharacterMechanics().StepBattleLogic(context);

            Assert.That(runtime.X, Is.EqualTo(100 + 18.5 * scaleX).Within(1e-9));
            Assert.That(runtime.Z, Is.EqualTo(200 - 3.25 * scaleZ).Within(1e-9));
            Assert.That(runtime.SourceRuleX,
                Is.EqualTo(blockPositiveX ? 20.75 : 39.25).Within(1e-9));
            Assert.That(runtime.SourceRuleZ, Is.EqualTo(-13.75).Within(1e-9));
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo(blockPositiveX ? 20 : 39));
            Assert.That(runtime.SourceRuleZInt, Is.EqualTo(-13));
            Assert.That(runtime.Vx, Is.EqualTo(17.5));
            Assert.That(runtime.Vz, Is.EqualTo(-2.25));
            Assert.That(runtime.SourceRuleXBoundPositive, Is.False);
            Assert.That(runtime.SourceRuleZBoundPositive, Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NonCharacterMotion_UsesRawSignedVelocityAndIndependentBlockers(
            bool blockNegativeZ)
        {
            var runtime = new NTSDEntityRuntime
            {
                Vx = -12.5,
                Vz = -4.25,
                SourceRuleXBoundPositive = true,
                SourceRuleZBoundNegative = blockNegativeZ,
            };
            runtime.SetSourceRulePosition(-30.75, 50.5);
            runtime.SyncSourceRuleIntegerPosition();

            CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.0,
                2048.0 / 1333.0, 1152.0 / 730.0);

            Assert.That(runtime.SourceRuleX, Is.EqualTo(-43.25).Within(1e-9));
            Assert.That(runtime.SourceRuleZ,
                Is.EqualTo(blockNegativeZ ? 50.5 : 46.25).Within(1e-9));
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo(-43));
            Assert.That(runtime.SourceRuleZInt,
                Is.EqualTo(blockNegativeZ ? 50 : 46));
            Assert.That(runtime.SourceRuleXBoundPositive, Is.False);
            Assert.That(runtime.SourceRuleZBoundNegative, Is.False);
        }

        [Test]
        public void WeaponDynamics_UsesSameRawSourcePhysicsBoundary()
        {
            var runtime = new NTSDEntityRuntime { Vx = 6.5, Vz = 1.25 };
            runtime.SetSourceRulePosition(-5.5, 7.75);
            runtime.SyncSourceRuleIntegerPosition();

            CharacterMechanics.WeaponDynamics(runtime, 0.0, out _,
                2048.0 / 1333.0, 1152.0 / 730.0);

            Assert.That(runtime.SourceRuleX, Is.EqualTo(1.0));
            Assert.That(runtime.SourceRuleZ, Is.EqualTo(9.0));
            Assert.That(runtime.SourceRuleXInt, Is.EqualTo(1));
            Assert.That(runtime.SourceRuleZInt, Is.EqualTo(9));
        }

        [Test]
        public void MissingSourceCarrier_DoesNotStartImplicitCoordinateHistory()
        {
            var runtime = new NTSDEntityRuntime { Vx = 9.0, Vz = 2.0 };
            CharacterMechanics.StepNonCharacterBattleLogic(runtime, 0.0,
                2048.0 / 1333.0, 1152.0 / 730.0);
            Assert.That(runtime.SourceRulePositionInitialized, Is.False);
            Assert.That(runtime.SourceRuleX, Is.Zero);
            Assert.That(runtime.SourceRuleZ, Is.Zero);
        }
    }
}
