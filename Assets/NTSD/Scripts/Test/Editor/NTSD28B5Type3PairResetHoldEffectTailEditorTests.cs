#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type3PairResetHoldEffectTailEditorTests
    {
        [Test]
        public void MatchingPair_UsesLatchedHitUjAndPreservesRuntimeMotion()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 210, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 211, 1, LF2ObjectType.SpecialAttack);
            attacker.GetFrameDataById(0).state = LF2States.ObjectFlying;
            target.GetFrameDataById(0).state = LF2States.ObjectFlying;
            attacker.GetFrameDataById(5).hit_Uj = 71;
            target.GetFrameDataById(6).hit_Uj = 72;
            attacker.ImmediateFrame(0);
            target.ImmediateFrame(0);
            attacker.Trans.SyncDirectFrameData(
                attacker.Frame.D.wait,
                attacker.Frame.D.next,
                5);
            target.Trans.SyncDirectFrameData(
                target.Frame.D.wait,
                target.Frame.D.next,
                6);
            attacker.Runtime.SetVelocity(1.0, 2.0, 3.0);
            target.Runtime.SetVelocity(4.0, 5.0, 6.0);
            attacker.KnockbackVx = 7.0;
            attacker.KnockbackVy = 8.0;
            attacker.KnockbackVz = 9.0;
            target.KnockbackVx = 10.0;
            target.KnockbackVy = 11.0;
            target.KnockbackVz = 12.0;

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(attacker.Frame.N, Is.EqualTo(71));
            Assert.That(target.Frame.N, Is.EqualTo(72));
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(attacker.KnockbackVx, Is.Zero);
            Assert.That(attacker.KnockbackVy, Is.Zero);
            Assert.That(attacker.KnockbackVz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(1.0));
            Assert.That(attacker.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(3.0));
            Assert.That(target.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(target.Runtime.Vy, Is.EqualTo(5.0));
            Assert.That(target.Runtime.Vz, Is.EqualTo(6.0));
        }

        [Test]
        public void NonMatchingPair_StillNegatesOrdinaryAttackerPositiveHold()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 300, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 211, 1, LF2ObjectType.SpecialAttack);
            target.Frame.D.hit_Fj = 77;
            attacker.FrameDelay = 6;

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.EqualTo(77));
            Assert.That(attacker.FrameDelay, Is.EqualTo(-6));
        }

        [Test]
        public void NonMatchingPair_MirrorsAndNegatesNegativeLinkParentHold()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 210, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 211, 1, LF2ObjectType.SpecialAttack);
            TypedCharacter parent = CreateEntity(
                world, 9000, 2, LF2ObjectType.Character);
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = parent.Runtime.SlotIndex;
            attacker.FrameDelay = 6;
            parent.FrameDelay = 11;
            target.Frame.D.hit_Fj = 77;

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.EqualTo(77));
            Assert.That(attacker.FrameDelay, Is.EqualTo(6));
            Assert.That(parent.FrameDelay, Is.EqualTo(-6));
        }

        [Test]
        public void MissingNegativeLinkParent_DoesNotNegateAttackerHold()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 210, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 211, 1, LF2ObjectType.SpecialAttack);
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = 399;
            attacker.FrameDelay = 6;
            target.Frame.D.hit_Fj = 77;

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(attacker.FrameDelay, Is.EqualTo(6));
        }

        [TestCase(5005)]
        [TestCase(6033)]
        [TestCase(23)]
        public void Type3Target_DoesNotRunLegacyEffectTail(int effect)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 210, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 211, 1, LF2ObjectType.SpecialAttack);
            target.Frame.D.hit_Uj = 77;
            target.Health.PP = 100;

            InvokeTail(world, attacker, target, Effect(effect));

            Assert.That(target.Frame.N, Is.EqualTo(77));
            Assert.That(target.Health.PP, Is.EqualTo(100));
            Assert.That(world.PendingSounds.Count, Is.Zero);
        }

        private static InteractionArea Effect(int effect)
        {
            return new InteractionArea { kind = 0, effect = effect };
        }

        private static void InvokeTail(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            MethodInfo method = typeof(BattleDamageWriter).GetMethod(
                "ApplyKind0Type3Tail",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { world, attacker, target, interaction });
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
                    name = "B5Type3PairResetHoldEffectTail",
                    type_sub = objectId,
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            world.Register(entity);
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
