#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NonType0HitMotionArmEditorTests
    {
        [TestCase((int)LF2ObjectType.LightWeapon, 6.5)]
        [TestCase((int)LF2ObjectType.HeavyWeapon, 6.5)]
        [TestCase((int)LF2ObjectType.SpecialAttack, 0.0)]
        [TestCase((int)LF2ObjectType.ThrowWeapon, 6.5)]
        [TestCase((int)LF2ObjectType.Other, 6.5)]
        [TestCase((int)LF2ObjectType.Drink, 6.5)]
        public void Type1Through6_ArmAtProductionReactionSeam(
            int targetType,
            double expectedFinalPendingZ)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8370, 0, 0);
            TypedCharacter target = CreateEntity(world, 8371, 1, targetType);
            attacker.Runtime.Dir = "left";
            target.KnockbackVz = 2.5;

            bool applied = ApplyByTargetType(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    dvx = 1,
                    dvz = 4,
                    dx = 123,
                    dy = 124,
                    dz = 125,
                    gain = 222,
                    pickedact = 311,
                    pickingact = 312,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.KnockbackVz, Is.EqualTo(expectedFinalPendingZ));
            Assert.That(target.Runtime.StatusHitFacing1D0, Is.EqualTo(1));
            Assert.That(target.Runtime.StatusDx1C0, Is.EqualTo(123));
            Assert.That(target.Runtime.StatusDy1C4, Is.EqualTo(124));
            Assert.That(target.Runtime.StatusDz1C8, Is.EqualTo(125));
            Assert.That(target.Runtime.StatusGain1CC, Is.EqualTo(1));
            Assert.That(target.Runtime.StatusPickedAction1D4, Is.EqualTo(311));
            Assert.That(target.Runtime.StatusPickingAction1D8, Is.EqualTo(312));
        }

        [Test]
        public void Type6_StatusTailStillSkipsWhileArmRuns()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8372, 0, 0);
            TypedCharacter target = CreateEntity(
                world,
                8373,
                1,
                (int)LF2ObjectType.Drink);
            target.Runtime.DelayTimer134 = 91;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    dvx = 1,
                    delay = 42,
                    gain = 222,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.DelayTimer134, Is.EqualTo(91));
            Assert.That(target.Runtime.StatusGain1CC, Is.EqualTo(1));
        }

        [Test]
        public void Type6_ProductionDeathStabilizationBypassesArm()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8374, 0, 0);
            TypedCharacter target = CreateEntity(
                world,
                8375,
                1,
                (int)LF2ObjectType.Drink);
            target.Runtime.Vx = 0.0;
            target.KnockbackVz = 2.5;
            target.Runtime.StatusHitFacing1D0 = 7;
            target.Runtime.StatusGain1CC = 14;
            target.Runtime.StatusPickedAction1D4 = 15;
            target.Runtime.StatusPickingAction1D8 = 16;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    dvx = 0,
                    dvz = 4,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.KnockbackVz, Is.EqualTo(2.5));
            Assert.That(target.Runtime.StatusHitFacing1D0, Is.EqualTo(7));
            Assert.That(target.Runtime.StatusGain1CC, Is.EqualTo(14));
            Assert.That(target.Runtime.StatusPickedAction1D4, Is.EqualTo(15));
            Assert.That(target.Runtime.StatusPickingAction1D8, Is.EqualTo(16));
        }

        private static bool ApplyByTargetType(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            return targetType == (int)LF2ObjectType.SpecialAttack ||
                   targetType == (int)LF2ObjectType.Other
                ? world.DamageWriter.ApplySpecialAttackDamage(
                    world,
                    attacker,
                    target,
                    interaction)
                : world.DamageWriter.ApplyWeaponDamage(
                    world,
                    attacker,
                    target,
                    interaction);
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            int objectType)
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
            var entity = new TypedCharacter(objectType)
            {
                ObjectId = objectId,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5NonType0HitMotionArm",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
            return entity;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly int objectType;

            internal TypedCharacter(int objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return objectType;
            }
        }
    }
}
#endif
