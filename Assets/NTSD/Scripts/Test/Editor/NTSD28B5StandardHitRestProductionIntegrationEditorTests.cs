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

        [TestCase(false, -11)]
        [TestCase(true, -11)]
        [TestCase(false, -10)]
        [TestCase(true, -10)]
        [TestCase(false, -9)]
        [TestCase(true, -9)]
        public void AirborneVerticalBoundaryPreservesCommittedDamageAcrossViewScale(
            bool configuredView,
            int targetSourceY)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            TypedCharacter attacker = CreateEntity(
                world, 9120, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 9121, 1, LF2ObjectType.Character);
            InteractionArea interaction = StandardInteraction();
            interaction.injury = 10;
            interaction.arest = 4;
            interaction.vrest = 1;
            interaction.x = 40;
            interaction.y = -20;
            interaction.w = 25;
            interaction.h = 40;
            interaction.zwidth = 15;
            attacker.Frame.D.centerx = 39;
            attacker.Frame.D.itrs.Add(interaction);
            target.Frame.D.centerx = 39;
            target.Frame.D.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 21,
                y = -10,
                w = 43,
                h = 20,
            });
            SetProjectedAirbornePosition(world, attacker, -40);
            SetProjectedAirbornePosition(world, target, targetSourceY);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            query.FormalCollectorMode = CollisionFormalCollectorMode.ForceRoleAware;
            query.ForceRoleAwareDirectForDiagnostics = true;
            ulong crtCallsBefore = world.NativeRandom.CaptureScalarState().CrtCalls;

            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker, out List<SceneQueryHit> candidates), Is.True);
            int expectedHits = targetSourceY == -11 ? 1 : 0;
            Assert.That(candidates.Count, Is.EqualTo(expectedHits));

            world.PostInteractionTickAll(1);
            world.EndCollisionCandidateConsumption();

            Assert.That(target.Health.HP, Is.EqualTo(500 - 10 * expectedHits));
            Assert.That(target.HitCount, Is.EqualTo(expectedHits));
            Assert.That(attacker.FrameDelay, Is.EqualTo(3 * expectedHits));
            Assert.That(target.FrameDelay, Is.EqualTo(-3 * expectedHits));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(4 * expectedHits));
            Assert.That(target.ItrRest.GetVrest(0), Is.EqualTo(expectedHits));
            Assert.That(target.Runtime.YInt, Is.EqualTo(targetSourceY));
            Assert.That(attacker.Runtime.YInt, Is.EqualTo(-40));
            Assert.That(world.NativeRandom.CaptureScalarState().CrtCalls - crtCallsBefore,
                Is.EqualTo((ulong)(2 * expectedHits)));
        }

        private static void SetProjectedAirbornePosition(
            SimulationWorld world,
            TypedCharacter entity,
            int sourceY)
        {
            entity.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(535),
                sourceY,
                world.SpatialProjection.SourceToViewZ(650));
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetSourceRulePosition(535, 650);
            entity.Runtime.SyncSourceRuleIntegerPosition();
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
