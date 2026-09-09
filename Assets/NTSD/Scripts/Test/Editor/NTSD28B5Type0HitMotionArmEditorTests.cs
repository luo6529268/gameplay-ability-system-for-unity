#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type0HitMotionArmEditorTests
    {
        [Test]
        public void StrictStabilizationBypass_PreservesEveryArmDestination()
        {
            var attacker = new NTSDEntityRuntime { Dir = "left" };
            var target = CreatePrimedTarget();
            target.Fall = 80;
            target.Vx = 4.999;

            bool armed = BattleDamageWriter.ArmNativeUnarmoredHitMotion(
                target,
                attacker,
                CreateMotionInteraction(dvx: 0));

            Assert.That(armed, Is.False);
            AssertPrimedTargetIsUnchanged(target);
        }

        [TestCase(-5.0)]
        [TestCase(5.0)]
        public void StabilizationBoundary_IsNotBypassed(double velocityX)
        {
            var attacker = new NTSDEntityRuntime();
            var target = CreatePrimedTarget();
            target.Fall = 80;
            target.Vx = velocityX;

            bool armed = BattleDamageWriter.ArmNativeUnarmoredHitMotion(
                target,
                attacker,
                CreateMotionInteraction(dvx: 0));

            Assert.That(armed, Is.True);
            Assert.That(target.StatusGain1CC, Is.EqualTo(1));
        }

        [TestCase(79, 0.0, 0)]
        [TestCase(80, -5.001, 0)]
        [TestCase(80, 0.0, 1)]
        public void AnyFailedBypassGate_ArmsMotion(
            int reactionTimer,
            double velocityX,
            int interactionDvx)
        {
            var target = CreatePrimedTarget();
            target.Fall = reactionTimer;
            target.Vx = velocityX;

            bool armed = BattleDamageWriter.ArmNativeUnarmoredHitMotion(
                target,
                new NTSDEntityRuntime(),
                CreateMotionInteraction(interactionDvx));

            Assert.That(armed, Is.True);
        }

        [Test]
        public void ArmedMotion_AddsPendingZAndWritesFacingMotionAndDefaultActions()
        {
            var attacker = new NTSDEntityRuntime { Dir = "left" };
            var target = CreatePrimedTarget();
            target.Fall = 20;

            bool armed = BattleDamageWriter.ArmNativeUnarmoredHitMotion(
                target,
                attacker,
                CreateMotionInteraction(dvx: 0));

            Assert.That(armed, Is.True);
            Assert.That(target.KnockbackVz, Is.EqualTo(6.5));
            Assert.That(target.StatusHitFacing1D0, Is.EqualTo(1));
            Assert.That(target.StatusDx1C0, Is.EqualTo(123));
            Assert.That(target.StatusDy1C4, Is.EqualTo(124));
            Assert.That(target.StatusDz1C8, Is.EqualTo(125));
            Assert.That(target.StatusGain1CC, Is.EqualTo(1));
            Assert.That(target.StatusPickedAction1D4, Is.EqualTo(191));
            Assert.That(target.StatusPickingAction1D8, Is.EqualTo(185));
        }

        [Test]
        public void ArmedMotion_UsesExplicitActionsAndRightFacing()
        {
            var interaction = CreateMotionInteraction(dvx: 0);
            interaction.pickedact = 311;
            interaction.pickingact = 312;
            var target = CreatePrimedTarget();
            target.Fall = 20;

            bool armed = BattleDamageWriter.ArmNativeUnarmoredHitMotion(
                target,
                new NTSDEntityRuntime { Dir = "right" },
                interaction);

            Assert.That(armed, Is.True);
            Assert.That(target.StatusHitFacing1D0, Is.Zero);
            Assert.That(target.StatusPickedAction1D4, Is.EqualTo(311));
            Assert.That(target.StatusPickingAction1D8, Is.EqualTo(312));
        }

        [Test]
        public void ProductionStandardDamage_ArmsAfterEncodedStatusAndReaction()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(8340, 0);
            LF2Character victim = CreateCharacter(8341, 1);
            attacker.Runtime.Dir = "left";
            victim.Health.HP = 500;
            victim.Health.HPBound = 500;
            victim.KnockbackVz = 2.5;
            world.Register(attacker);
            world.Register(victim);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                new InteractionArea
                {
                    kind = 9,
                    fall = 1,
                    dvz = 4,
                    dx = 123,
                    dy = 124,
                    dz = 125,
                    gain = 222,
                });

            Assert.That(applied, Is.True);
            Assert.That(victim.HitCounters.Fall, Is.EqualTo(20));
            Assert.That(victim.KnockbackVz, Is.EqualTo(6.5));
            Assert.That(victim.Runtime.StatusHitFacing1D0, Is.EqualTo(1));
            Assert.That(victim.Runtime.StatusGain1CC, Is.EqualTo(1));
            Assert.That(victim.Runtime.StatusPickedAction1D4, Is.EqualTo(191));
            Assert.That(victim.Runtime.StatusPickingAction1D8, Is.EqualTo(185));
        }

        [Test]
        public void ProductionDeathStabilization_PreservesEncodedAndOldArmValues()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(8342, 0);
            LF2Character victim = CreateCharacter(8343, 1);
            victim.Health.HP = 5;
            victim.Health.HPBound = 5;
            victim.Runtime.Vx = 0.0;
            victim.KnockbackVz = 2.5;
            victim.Runtime.StatusPickedAction1D4 = 901;
            victim.Runtime.StatusPickingAction1D8 = 902;
            world.Register(attacker);
            world.Register(victim);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                new InteractionArea
                {
                    kind = 9,
                    injury = 10,
                    dvx = 0,
                    dvz = 4,
                    gain = 222,
                });

            Assert.That(applied, Is.True);
            Assert.That(victim.Health.HP, Is.LessThanOrEqualTo(0));
            Assert.That(victim.KnockbackVz, Is.EqualTo(2.5));
            Assert.That(victim.Runtime.StatusGain1CC, Is.EqualTo(222));
            Assert.That(victim.Runtime.StatusPickedAction1D4, Is.EqualTo(901));
            Assert.That(victim.Runtime.StatusPickingAction1D8, Is.EqualTo(902));
        }

        private static NTSDEntityRuntime CreatePrimedTarget()
        {
            return new NTSDEntityRuntime
            {
                KnockbackVz = 2.5,
                StatusHitFacing1D0 = 7,
                StatusDx1C0 = 11,
                StatusDy1C4 = 12,
                StatusDz1C8 = 13,
                StatusGain1CC = 14,
                StatusPickedAction1D4 = 15,
                StatusPickingAction1D8 = 16,
            };
        }

        private static void AssertPrimedTargetIsUnchanged(
            NTSDEntityRuntime target)
        {
            Assert.That(target.KnockbackVz, Is.EqualTo(2.5));
            Assert.That(target.StatusHitFacing1D0, Is.EqualTo(7));
            Assert.That(target.StatusDx1C0, Is.EqualTo(11));
            Assert.That(target.StatusDy1C4, Is.EqualTo(12));
            Assert.That(target.StatusDz1C8, Is.EqualTo(13));
            Assert.That(target.StatusGain1CC, Is.EqualTo(14));
            Assert.That(target.StatusPickedAction1D4, Is.EqualTo(15));
            Assert.That(target.StatusPickingAction1D8, Is.EqualTo(16));
        }

        private static InteractionArea CreateMotionInteraction(int dvx)
        {
            return new InteractionArea
            {
                dvx = dvx,
                dvz = 4,
                dx = 123,
                dy = 124,
                dz = 125,
            };
        }

        private static LF2Character CreateCharacter(int objectId, int slot)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = 0,
                    wait = 100,
                    next = id,
                });
            }
            var entity = new LF2Character { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5Type0HitMotionArm",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            return entity;
        }
    }
}
#endif
