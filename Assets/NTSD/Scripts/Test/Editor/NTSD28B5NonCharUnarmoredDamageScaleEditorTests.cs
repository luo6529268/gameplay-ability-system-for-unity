#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NonCharUnarmoredDamageScaleEditorTests
    {
        [TestCase((int)LF2ObjectType.LightWeapon)]
        [TestCase((int)LF2ObjectType.HeavyWeapon)]
        [TestCase((int)LF2ObjectType.ThrowWeapon)]
        public void WeaponType1_2_4_UsesPlus340ThenWeakAndKeepsRawConsumers(
            int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8420, 0, 0);
            TypedCharacter target = CreateEntity(world, 8421, 1, targetType);
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.FallDamageDiv = 200;
            target.Runtime.WeaponFlightCounter = 100;
            target.ComboCountVic = 31;
            world.DamageStats[1] = 41;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 11,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(478));
            Assert.That(target.Health.HPBound, Is.EqualTo(493));
            Assert.That(target.ComboCountVic, Is.EqualTo(31));
            Assert.That(world.DamageStats[1], Is.EqualTo(41));
            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(89),
                "Weapon durability must continue consuming raw ITR injury.");
            Assert.That(target.Runtime.DisplayScoreStep1F4, Is.EqualTo(1),
                "Display steps must continue consuming raw ITR injury.");
        }

        [TestCase((int)LF2ObjectType.SpecialAttack)]
        [TestCase((int)LF2ObjectType.Other)]
        public void Type3And5_UsePlus340ThenWeakWithoutFallDamageDivFallback(
            int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8422, 0, 0);
            TypedCharacter target = CreateEntity(world, 8423, 1, targetType);
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.FallDamageDiv = 200;
            target.ComboCountVic = 31;
            world.DamageStats[1] = 41;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 11,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(478));
            Assert.That(target.Health.HPBound, Is.EqualTo(493));
            Assert.That(target.ComboCountVic, Is.EqualTo(31));
            Assert.That(world.DamageStats[1], Is.EqualTo(41));
            Assert.That(target.Runtime.DisplayScoreStep1F4, Is.EqualTo(1));
        }

        [Test]
        public void Type6_SkipsDamageAndDisplayButKeepsRawDurabilityTail()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8424, 0, 0);
            TypedCharacter target = CreateEntity(
                world,
                8425,
                1,
                (int)LF2ObjectType.Drink);
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.FallDamageDiv = 200;
            target.Runtime.WeaponFlightCounter = 100;
            target.Runtime.DisplayScoreStep1F4 = 17;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 11,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(500));
            Assert.That(target.Health.HPBound, Is.EqualTo(500));
            Assert.That(target.ComboCountVic, Is.Zero);
            Assert.That(world.DamageStats[1], Is.Zero);
            Assert.That(target.Runtime.WeaponFlightCounter, Is.EqualTo(89));
            Assert.That(target.Runtime.DisplayScoreStep1F4, Is.EqualTo(17));
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
                    name = "B5NonCharDamageScale",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.ComboCountVic = 0;
            entity.Unk344 = 1;
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
