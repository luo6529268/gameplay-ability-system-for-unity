#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5StandardHitRestProductionIntegrationEditorTests
    {
        [TestCase(0, 1)]
        [TestCase(1, 1)]
        [TestCase(2, -1)]
        [TestCase(3, 1)]
        public void StandardActualOwners_UseSharedReducedRest(
            int route,
            int expectedFinalAttackerHold)
        {
            var world = new SimulationWorld();
            world.Runtime.NativeStandardHitRest.SetTimingReduction4A9FF4(2);
            TypedCharacter attacker = CreateEntity(
                world, 9100 + route * 2, 0, LF2ObjectType.Character);
            LF2ObjectType targetType = route switch
            {
                0 => LF2ObjectType.Character,
                1 => LF2ObjectType.LightWeapon,
                2 => LF2ObjectType.SpecialAttack,
                _ => LF2ObjectType.Other,
            };
            TypedCharacter target = CreateEntity(
                world, 9101 + route * 2, 1, targetType);
            attacker.FrameDelay = 7;
            target.FrameDelay = -7;

            bool applied = ApplyRoute(
                route,
                world,
                attacker,
                target,
                StandardInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.FrameDelay,
                Is.EqualTo(expectedFinalAttackerHold));
            Assert.That(target.FrameDelay, Is.EqualTo(-1));
            Assert.That(attacker.AttackExempt, Is.EqualTo(8));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(8));
            Assert.That(target.ItrRest.GetVrest(0), Is.EqualTo(1));
        }

        [Test]
        public void RecoverAndDefinitionEffects_SuppressOnlyTheirOwnedHoldWrites()
        {
            var world = new SimulationWorld();
            world.Runtime.NativeStandardHitRest.SetTimingReduction4A9FF4(2);
            TypedCharacter attacker = CreateEntity(
                world, 9110, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 9111, 1, LF2ObjectType.Other);
            attacker.FrameCache.Wrapper.characterData.definition_effect = 3;
            target.FrameCache.Wrapper.characterData.definition_effect = 4;
            attacker.FrameDelay = 7;
            target.FrameDelay = -7;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                StandardInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.FrameDelay, Is.EqualTo(7));
            Assert.That(target.FrameDelay, Is.EqualTo(-7));

            attacker.FrameCache.Wrapper.characterData.definition_effect = 0;
            target.FrameCache.Wrapper.characterData.definition_effect = 0;
            attacker.FrameDelay = 9;
            target.FrameDelay = -9;
            InteractionArea recoverBoth = StandardInteraction();
            recoverBoth.recover = 3;
            world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                recoverBoth);

            Assert.That(attacker.FrameDelay, Is.EqualTo(9));
            Assert.That(target.FrameDelay, Is.EqualTo(-9));
        }

        [Test]
        public void InitialMatchingPair_UsesSharedRestBeforeResetAndHoldRelease()
        {
            var world = new SimulationWorld();
            world.Runtime.NativeStandardHitRest.SetTimingReduction4A9FF4(2);
            TypedCharacter attacker = CreateEntity(
                world, 9112, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 9113, 1, LF2ObjectType.SpecialAttack);
            attacker.Frame.D.state = LF2States.ObjectFlying;
            target.Frame.D.state = LF2States.ObjectFlying;
            attacker.Frame.D.hit_Uj = 20;
            target.Frame.D.hit_Uj = 20;
            attacker.FrameDelay = 7;
            target.FrameDelay = -7;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                StandardInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.FrameDelay, Is.EqualTo(-1));
            Assert.That(target.FrameDelay, Is.EqualTo(-1));
            Assert.That(attacker.AttackExempt, Is.EqualTo(8));
            Assert.That(target.ItrRest.GetVrest(0), Is.EqualTo(1));
            Assert.That(target.Health.HP, Is.EqualTo(500));
            Assert.That(target.HitRecordCount, Is.Zero);
            Assert.That(world.PendingSounds.Count, Is.Zero);
        }

        private static bool ApplyRoute(
            int route,
            SimulationWorld world,
            TypedCharacter attacker,
            TypedCharacter target,
            InteractionArea interaction)
        {
            return route switch
            {
                0 => world.DamageWriter.ApplyStandardCharacterDamage(
                    world,
                    attacker,
                    target,
                    target.HitCounters,
                    interaction),
                1 => world.DamageWriter.ApplyWeaponDamage(
                    world,
                    attacker,
                    target,
                    interaction),
                _ => world.DamageWriter.ApplySpecialAttackDamage(
                    world,
                    attacker,
                    target,
                    interaction),
            };
        }

        private static InteractionArea StandardInteraction()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 1,
                fall = 1,
                dvx = 1,
                arest = 10,
                vrest = 258,
                recover = 0,
            };
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
                    state = LF2States.Standing,
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
                    name = "B5StandardHitRestProductionIntegration",
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
