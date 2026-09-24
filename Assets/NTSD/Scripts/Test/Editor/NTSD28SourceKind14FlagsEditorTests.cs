using NUnit.Framework;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28SourceKind14FlagsEditorTests
    {
        [TestCase(5, 2, 1.0, 1.0, false, false)]
        [TestCase(6, 3, 1.0, 1.0, true, true)]
        [TestCase(-5, -2, -1.0, -1.0, false, false)]
        [TestCase(-6, -3, -1.0, -1.0, true, true)]
        public void SourceIntegerThresholds_StayIndependentOfPhysicalView(
            int attackerSourceX,
            int attackerSourceZ,
            double velocityX,
            double velocityZ,
            bool expectX,
            bool expectZ)
        {
            var attacker = new LF2Character();
            var victim = new LF2Character();
            attacker.Runtime.SetPosition(100, 0, 100);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(100, 0, 100);
            victim.Runtime.SyncIntegerPosition();
            attacker.Runtime.SetSourceRulePosition(attackerSourceX, attackerSourceZ);
            attacker.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.SetSourceRulePosition(0, 0);
            victim.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.Vx = velocityX;
            victim.Runtime.Vz = velocityZ;

            BattleBoundaryWriter.ApplySourceRuleKind14DirectionalBlock(
                attacker, victim);

            Assert.That(victim.Runtime.XBoundPositive, Is.False);
            Assert.That(victim.Runtime.XBoundNegative, Is.False);
            Assert.That(victim.Runtime.ZBoundPositive, Is.False);
            Assert.That(victim.Runtime.ZBoundNegative, Is.False);
            Assert.That(velocityX > 0
                ? victim.Runtime.SourceRuleXBoundPositive
                : victim.Runtime.SourceRuleXBoundNegative, Is.EqualTo(expectX));
            Assert.That(velocityZ > 0
                ? victim.Runtime.SourceRuleZBoundPositive
                : victim.Runtime.SourceRuleZBoundNegative, Is.EqualTo(expectZ));

            new CharacterMechanics().StepBattleLogic(new CharacterMechanicsContext(
                victim.Runtime, null, 0f, 0f, 0.0,
                2048.0 / 1333.0, 1152.0 / 730.0));
            Assert.That(victim.Runtime.SourceRuleX,
                Is.EqualTo(expectX ? 0.0 : velocityX).Within(1e-12));
            Assert.That(victim.Runtime.SourceRuleZ,
                Is.EqualTo(expectZ ? 0.0 : velocityZ).Within(1e-12));
            Assert.That(victim.Runtime.SourceRuleXBoundPositive, Is.False);
            Assert.That(victim.Runtime.SourceRuleXBoundNegative, Is.False);
            Assert.That(victim.Runtime.SourceRuleZBoundPositive, Is.False);
            Assert.That(victim.Runtime.SourceRuleZBoundNegative, Is.False);
        }

        [Test]
        public void CentralWriter_UsesSourceGapEvenWhenPhysicalGapIsBelowThreshold()
        {
            var world = new SimulationWorld();
            var attacker = new LF2Character();
            var victim = new LF2Character();
            attacker.Runtime.SetPosition(100, 0, 100);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(100, 0, 100);
            victim.Runtime.SyncIntegerPosition();
            attacker.Runtime.SetSourceRulePosition(6, 3);
            attacker.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.SetSourceRulePosition(0, 0);
            victim.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.Vx = 1;
            victim.Runtime.Vz = 1;

            Assert.That(world.BoundaryWriter.TryApplyKind14DirectionalBlock(
                attacker, victim), Is.True);
            Assert.That(victim.Runtime.XBoundPositive, Is.False);
            Assert.That(victim.Runtime.ZBoundPositive, Is.False);
            Assert.That(victim.Runtime.SourceRuleXBoundPositive, Is.True);
            Assert.That(victim.Runtime.SourceRuleZBoundPositive, Is.True);
        }

        [Test]
        public void MissingEitherSourcePosition_DoesNotPublishSourceFlags()
        {
            var attacker = new LF2Character();
            var victim = new LF2Character();
            victim.Runtime.SetSourceRulePosition(0, 0);
            victim.Runtime.Vx = 1;
            BattleBoundaryWriter.ApplySourceRuleKind14DirectionalBlock(
                attacker, victim);
            Assert.That(victim.Runtime.SourceRuleXBoundPositive, Is.False);
            attacker.Runtime.SetSourceRulePosition(6, 0);
            victim.Runtime.SourceRulePositionInitialized = false;
            BattleBoundaryWriter.ApplySourceRuleKind14DirectionalBlock(
                attacker, victim);
            Assert.That(victim.Runtime.SourceRuleXBoundPositive, Is.False);
        }

        [Test]
        public void UnregisteredEntityFallback_PublishesIndependentSourceFlags()
        {
            var attacker = new LF2Character();
            var victim = new ProbeSpecialAttack();
            attacker.Runtime.SetPosition(100, 0, 100);
            attacker.Runtime.SyncIntegerPosition();
            victim.Runtime.SetPosition(100, 0, 100);
            victim.Runtime.SyncIntegerPosition();
            attacker.Runtime.SetSourceRulePosition(6, 3);
            attacker.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.SetSourceRulePosition(0, 0);
            victim.Runtime.SyncSourceRuleIntegerPosition();
            victim.Runtime.Vx = 1.0;
            victim.Runtime.Vz = 1.0;

            victim.ApplyFallback(attacker);

            Assert.That(victim.Runtime.XBoundPositive, Is.False);
            Assert.That(victim.Runtime.ZBoundPositive, Is.False);
            Assert.That(victim.Runtime.SourceRuleXBoundPositive, Is.True);
            Assert.That(victim.Runtime.SourceRuleZBoundPositive, Is.True);
        }

        private sealed class ProbeSpecialAttack : LF2SpecialAttack
        {
            internal void ApplyFallback(LF2Entity attacker)
            {
                ApplyKind14DirectionalBlockFrom(attacker);
            }
        }
    }
}
