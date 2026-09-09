#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5HitDisplayStepProducerEditorTests
    {
        [Test]
        public void PositiveInjury_WritesExactIntegerDisplaySteps()
        {
            var target = new NTSDEntityRuntime();

            BattleDamageWriter.ApplyNativeHitDisplaySteps(target, 25);

            Assert.That(target.DisplayScoreStep1F4, Is.EqualTo(2));
            Assert.That(target.DisplayDamageStep1FC, Is.EqualTo(2));
            Assert.That(target.DisplayCurrentHpStep204, Is.EqualTo(2));
            Assert.That(target.DisplayEffectiveMaxHpStep20C, Is.EqualTo(1));
        }

        [TestCase(0)]
        [TestCase(-25)]
        public void NonPositiveInjury_PreservesExistingSteps(int injury)
        {
            var target = CreatePrimedRuntime();

            BattleDamageWriter.ApplyNativeHitDisplaySteps(target, injury);

            AssertPrimedRuntimeIsUnchanged(target);
        }

        [TestCase((int)LF2ObjectType.Character)]
        [TestCase((int)LF2ObjectType.LightWeapon)]
        [TestCase((int)LF2ObjectType.HeavyWeapon)]
        [TestCase((int)LF2ObjectType.SpecialAttack)]
        [TestCase((int)LF2ObjectType.ThrowWeapon)]
        [TestCase((int)LF2ObjectType.Other)]
        public void Type0Through5_ProductionWritesDisplaySteps(int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8380, 0, 0);
            TypedCharacter target = CreateEntity(world, 8381, 1, targetType);

            bool applied = ApplyByTargetType(
                world,
                attacker,
                target,
                targetType,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 25,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Runtime.DisplayScoreStep1F4, Is.EqualTo(2));
            Assert.That(target.Runtime.DisplayDamageStep1FC, Is.EqualTo(2));
            Assert.That(target.Runtime.DisplayCurrentHpStep204, Is.EqualTo(2));
            Assert.That(target.Runtime.DisplayEffectiveMaxHpStep20C, Is.EqualTo(1));
        }

        [Test]
        public void Type6_ProductionSkipsDisplayStepTail()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8382, 0, 0);
            TypedCharacter target = CreateEntity(
                world,
                8383,
                1,
                (int)LF2ObjectType.Drink);
            PrimeRuntime(target.Runtime);

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 25,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            AssertPrimedRuntimeIsUnchanged(target.Runtime);
        }

        private static bool ApplyByTargetType(
            SimulationWorld world,
            LF2Entity attacker,
            TypedCharacter target,
            int targetType,
            InteractionArea interaction)
        {
            if (targetType == (int)LF2ObjectType.Character)
            {
                return world.DamageWriter.ApplyStandardCharacterDamage(
                    world,
                    attacker,
                    target,
                    target.HitCounters,
                    interaction);
            }
            if (targetType == (int)LF2ObjectType.SpecialAttack ||
                targetType == (int)LF2ObjectType.Other)
            {
                return world.DamageWriter.ApplySpecialAttackDamage(
                    world,
                    attacker,
                    target,
                    interaction);
            }
            return world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                interaction);
        }

        private static NTSDEntityRuntime CreatePrimedRuntime()
        {
            var runtime = new NTSDEntityRuntime();
            PrimeRuntime(runtime);
            return runtime;
        }

        private static void PrimeRuntime(NTSDEntityRuntime runtime)
        {
            runtime.DisplayScoreStep1F4 = 11;
            runtime.DisplayDamageStep1FC = 12;
            runtime.DisplayCurrentHpStep204 = 13;
            runtime.DisplayEffectiveMaxHpStep20C = 14;
        }

        private static void AssertPrimedRuntimeIsUnchanged(
            NTSDEntityRuntime runtime)
        {
            Assert.That(runtime.DisplayScoreStep1F4, Is.EqualTo(11));
            Assert.That(runtime.DisplayDamageStep1FC, Is.EqualTo(12));
            Assert.That(runtime.DisplayCurrentHpStep204, Is.EqualTo(13));
            Assert.That(runtime.DisplayEffectiveMaxHpStep20C, Is.EqualTo(14));
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
                    name = "B5HitDisplaySteps",
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
