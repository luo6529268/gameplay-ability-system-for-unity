#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Kind0DirectPostEffectActionEditorTests
    {
        [TestCase(3, 0, 200)]
        [TestCase(30, 0, 200)]
        [TestCase(3, 13, -1)]
        [TestCase(2, 0, 203)]
        [TestCase(21, 0, 203)]
        [TestCase(22, 0, 203)]
        [TestCase(20, 0, 203)]
        [TestCase(20, 18, -1)]
        public void Type0Unarmored_UsesPreviousStateEffectActionTable(
            int effect,
            int previousState,
            int expectedAction)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8490, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8491, 1, LF2ObjectType.Character);
            target.GetFrameDataById(5).state = previousState;
            target.Frame.Prev = 5;
            target.AttackingCounter = 9;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    effect = effect,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            if (expectedAction > 0)
            {
                Assert.That(target.Frame.N, Is.EqualTo(expectedAction));
                Assert.That(target.AttackingCounter, Is.Zero);
            }
            else
            {
                Assert.That(target.Frame.N, Is.Not.EqualTo(200));
                Assert.That(target.Frame.N, Is.Not.EqualTo(203));
            }
        }

        [TestCase("right", 2, true)]
        [TestCase("left", 2, false)]
        [TestCase("right", 0, true)]
        public void Action203_FacingUsesFinalPendingHorizontalImpulse(
            string attackerDirection,
            int dvx,
            bool expectFacingLeft)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8492, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8493, 1, LF2ObjectType.Character);
            attacker.SwitchDir(attackerDirection);

            world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    effect = 2,
                    injury = 1,
                    fall = 1,
                    dvx = dvx,
                });

            Assert.That(target.Frame.N, Is.EqualTo(203));
            Assert.That(target.Runtime.IsFacingLeft, Is.EqualTo(expectFacingLeft));
        }

        [TestCase(LF2ObjectType.LightWeapon)]
        [TestCase(LF2ObjectType.SpecialAttack)]
        [TestCase(LF2ObjectType.Other)]
        public void NonType0Targets_DoNotInheritDirectPostEffectAction(
            LF2ObjectType targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8494, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8495, 1, targetType);
            var itr = new InteractionArea
            {
                kind = 0,
                effect = 2,
                injury = 1,
                fall = 1,
                dvx = 1,
            };

            if (targetType == LF2ObjectType.LightWeapon)
                world.DamageWriter.ApplyWeaponDamage(world, attacker, target, itr);
            else
                world.DamageWriter.ApplySpecialAttackDamage(world, attacker, target, itr);

            Assert.That(target.Frame.N, Is.Not.EqualTo(203));
        }

        [Test]
        public void Kind9AndReducedHit_DoNotInheritDirectPostEffectAction()
        {
            var kind9World = new SimulationWorld();
            TypedCharacter kind9Attacker = CreateEntity(
                kind9World, 8496, 0, LF2ObjectType.Character);
            TypedCharacter kind9Target = CreateEntity(
                kind9World, 8497, 1, LF2ObjectType.Character);
            kind9World.DamageWriter.ApplyStandardCharacterDamage(
                kind9World,
                kind9Attacker,
                kind9Target,
                kind9Target.HitCounters,
                new InteractionArea
                {
                    kind = 9,
                    effect = 2,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                });
            Assert.That(kind9Target.Frame.N, Is.Not.EqualTo(203));

            var reducedWorld = new SimulationWorld();
            TypedCharacter reducedAttacker = CreateEntity(
                reducedWorld, 8498, 0, LF2ObjectType.Character);
            TypedCharacter reducedTarget = CreateEntity(
                reducedWorld, 8499, 1, LF2ObjectType.Character);
            reducedWorld.DamageWriter.ApplyAlternateDamage(
                reducedWorld,
                reducedAttacker,
                reducedTarget,
                reducedTarget.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    effect = 2,
                    injury = 10,
                    fall = 1,
                    dvx = 1,
                });
            Assert.That(reducedTarget.Frame.N, Is.Not.EqualTo(203));
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            LF2ObjectType objectType)
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

            var entity = new TypedCharacter(objectType) { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5Kind0DirectPostEffectAction",
                    type_sub = objectId,
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Unk344 = 1;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
            return entity;
        }

        private sealed class TypedCharacter : LF2Character
        {
            private readonly LF2ObjectType objectType;

            internal TypedCharacter(LF2ObjectType objectType)
            {
                this.objectType = objectType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)objectType;
            }
        }
    }
}
#endif
