#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5NonType0EncodedJoinEditorTests
    {
        [TestCase((int)LF2ObjectType.LightWeapon)]
        [TestCase((int)LF2ObjectType.HeavyWeapon)]
        [TestCase((int)LF2ObjectType.ThrowWeapon)]
        public void WeaponTypes124_ApplyEncodedStatusAndJoin(int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8360, 0, 0, 7);
            TypedCharacter target = CreateEntity(world, 8361, 1, targetType, 9);

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                CreateStatusInteraction());

            Assert.That(applied, Is.True);
            AssertEncodedJoinApplied(target);
        }

        [TestCase((int)LF2ObjectType.SpecialAttack)]
        [TestCase((int)LF2ObjectType.Other)]
        public void SpecialAndOtherTypes35_ApplyEncodedStatusAndJoin(
            int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8362, 0, 0, 7);
            TypedCharacter target = CreateEntity(world, 8363, 1, targetType, 9);

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                CreateStatusInteraction());

            Assert.That(applied, Is.True);
            AssertEncodedJoinApplied(target);
        }

        [Test]
        public void Type6_SkipsStatusRngAndJoinSideEffect()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8364, 0, 0, 7);
            TypedCharacter target = CreateEntity(
                world,
                8365,
                1,
                (int)LF2ObjectType.Drink,
                9);
            target.Runtime.DelayTimer134 = 91;
            target.Runtime.JoinTimer148 = 92;
            ulong before = world.NativeRandom.CaptureScalarState()
                .SynchronizedCalls;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                CreateStatusInteraction());

            Assert.That(applied, Is.True);
            Assert.That(world.NativeRandom.CaptureScalarState()
                .SynchronizedCalls, Is.EqualTo(before));
            Assert.That(target.Runtime.DelayTimer134, Is.EqualTo(91));
            Assert.That(target.Runtime.JoinTimer148, Is.EqualTo(92));
            Assert.That(target.Runtime.JoinOverrideActive170, Is.Zero);
        }

        [TestCase((int)LF2ObjectType.LightWeapon)]
        [TestCase((int)LF2ObjectType.HeavyWeapon)]
        [TestCase((int)LF2ObjectType.SpecialAttack)]
        [TestCase((int)LF2ObjectType.ThrowWeapon)]
        [TestCase((int)LF2ObjectType.Other)]
        public void NonTypeZeroTarget_DoesNotEnableMimic(int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8366, 0, 0, 7);
            TypedCharacter target = CreateEntity(world, 8367, 1, targetType, 9);
            InteractionArea interaction = CreateStatusInteraction();

            bool applied = targetType == (int)LF2ObjectType.SpecialAttack ||
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

            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.InputProxyCounter14C, Is.EqualTo(44));
            Assert.That(target.Runtime.InputProxySourceSlot178, Is.EqualTo(-1));
            Assert.That(target.Runtime.InputProxyEnabled17C, Is.Zero);
        }

        private static void AssertEncodedJoinApplied(TypedCharacter target)
        {
            Assert.That(target.Runtime.DelayTimer134, Is.EqualTo(42));
            Assert.That(target.Runtime.JoinTimer148, Is.EqualTo(43));
            Assert.That(target.Runtime.InputProxyCounter14C, Is.EqualTo(44));
            Assert.That(target.Runtime.JoinOverrideActive170, Is.EqualTo(1));
            Assert.That(target.Runtime.JoinOriginalBattleGroup174, Is.EqualTo(9));
            Assert.That(target.RelationTeam, Is.EqualTo(7));
            Assert.That(target.Runtime.InputProxyEnabled17C, Is.Zero);
        }

        private static InteractionArea CreateStatusInteraction()
        {
            return new InteractionArea
            {
                kind = 0,
                fall = 1,
                delay = 42,
                join = 43,
                mimic = 44,
            };
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            int objectType,
            int relationTeam)
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
                    name = "B5NonType0EncodedJoin",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            world.Register(entity);
            entity.RelationTeam = relationTeam;
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
