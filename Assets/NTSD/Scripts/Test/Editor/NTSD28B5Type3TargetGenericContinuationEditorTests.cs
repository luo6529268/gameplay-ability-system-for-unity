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
    public sealed class NTSD28B5Type3TargetGenericContinuationEditorTests
    {
        [Test]
        public void DirectSource_CopiesExactOwnershipAndPreservesNonPendingMotion()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8510, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 8511, 1, LF2ObjectType.SpecialAttack);
            attacker.RelationTeam = 7;
            attacker.Runtime.OwnerSlotIndex = 11;
            PrepareTarget(target, hitFj: 77, hitUj: 78);

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.EqualTo(77));
            Assert.That(target.Runtime.Frame, Is.EqualTo(77));
            Assert.That(target.RelationTeam, Is.EqualTo(7));
            Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(11));
            Assert.That(target.Runtime.AnimCounter, Is.EqualTo(0));
            Assert.That(typeof(NTSD.Simulation.NTSDEntityRuntime).GetMember("HolderCopySlotIndex").Length == 0, Is.True);
            Assert.That(target.HitConfirm2, Is.EqualTo(7));
            Assert.That(target.Runtime.SpecialHitLatch0EB, Is.True);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.HitCount, Is.EqualTo(3));
            Assert.That(target.Runtime.Vx, Is.EqualTo(9.0));
            Assert.That(target.Runtime.Vy, Is.EqualTo(10.0));
            Assert.That(target.Runtime.Vz, Is.EqualTo(11.0));
            Assert.That(target.AttackingCounter, Is.Zero);
        }

        [TestCase(LF2ObjectType.SpecialAttack, 0, 0, 78)]
        [TestCase(LF2ObjectType.Character, 0, 2, 78)]
        [TestCase(LF2ObjectType.Character, 0, 20, 78)]
        [TestCase(LF2ObjectType.Character, 0, 0, 77)]
        public void ResponseField_UsesAttackerTypeLinkAndEffect(
            LF2ObjectType attackerType,
            int linkState,
            int effect,
            int expectedAction)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8512, 0, attackerType);
            TypedCharacter target = CreateEntity(
                world, 8513, 1, LF2ObjectType.SpecialAttack);
            attacker.Runtime.LinkState = linkState;
            PrepareTarget(target, hitFj: 77, hitUj: 78);

            InvokeTail(world, attacker, target, Effect(effect));

            Assert.That(target.Frame.N, Is.EqualTo(expectedAction));
        }

        [TestCase(LF2ObjectType.Character, 0, 30)]
        [TestCase(LF2ObjectType.SpecialAttack, 0, 20)]
        [TestCase(LF2ObjectType.Character, 2, 20)]
        public void ZeroResponseField_UsesNativeFallback(
            LF2ObjectType attackerType,
            int effect,
            int expectedAction)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8514, 0, attackerType);
            TypedCharacter target = CreateEntity(
                world, 8515, 1, LF2ObjectType.SpecialAttack);
            PrepareTarget(target, hitFj: 0, hitUj: 0);

            InvokeTail(world, attacker, target, Effect(effect));

            Assert.That(target.Frame.N, Is.EqualTo(expectedAction));
        }

        [Test]
        public void NegativeLink_UsesActiveParentOwnershipAndFjPath()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8516, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 8517, 1, LF2ObjectType.SpecialAttack);
            TypedCharacter parent = CreateEntity(
                world, 8518, 2, LF2ObjectType.Character);
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = 2;
            parent.RelationTeam = 8;
            parent.Runtime.OwnerSlotIndex = 13;
            PrepareTarget(target, hitFj: 77, hitUj: 78);

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.EqualTo(77));
            Assert.That(target.RelationTeam, Is.EqualTo(8));
            Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(13));
            Assert.That(target.Runtime.AnimCounter, Is.EqualTo(2));
        }

        [Test]
        public void State3005_SkipsGenericTransaction()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8519, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 8520, 1, LF2ObjectType.SpecialAttack);
            PrepareTarget(target, hitFj: 77, hitUj: 78);
            target.GetFrameDataById(0).state = LF2States.ObjectFlying;
            target.ImmediateFrame(0);
            target.RelationTeam = 3;
            target.Runtime.OwnerSlotIndex = 4;
            target.Runtime.AnimCounter = 5;

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.Zero);
            Assert.That(target.RelationTeam, Is.EqualTo(3));
            Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(4));
            Assert.That(target.Runtime.AnimCounter, Is.EqualTo(5));
            Assert.That(target.KnockbackVx, Is.EqualTo(5.0));
            Assert.That(target.HitCount, Is.EqualTo(3));
        }

        [Test]
        public void State3006_DoesNotSkipWhenAttackerIsState3005()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8521, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(
                world, 8522, 1, LF2ObjectType.SpecialAttack);
            attacker.GetFrameDataById(0).state = LF2States.ObjectFlying;
            attacker.ImmediateFrame(0);
            PrepareTarget(target, hitFj: 77, hitUj: 78);
            target.GetFrameDataById(0).state = LF2States.ObjectExpanding;
            target.ImmediateFrame(0);

            InvokeTail(world, attacker, target, Effect(0));

            Assert.That(target.Frame.N, Is.EqualTo(77));
        }

        private static void PrepareTarget(
            TypedCharacter target,
            int hitFj,
            int hitUj)
        {
            LF2FrameData current = target.GetFrameDataById(0);
            current.state = LF2States.Standing;
            current.hit_Fj = hitFj;
            current.hit_Uj = hitUj;
            target.ImmediateFrame(0);
            target.RelationTeam = 2;
            target.Runtime.OwnerSlotIndex = 44;
            target.Runtime.AnimCounter = 99;
            target.HitConfirm2 = 7;
            target.Runtime.SpecialHitLatch0EB = false;
            target.KnockbackVx = 5.0;
            target.KnockbackVy = 6.0;
            target.KnockbackVz = 7.0;
            target.HitCount = 3;
            target.Runtime.Vx = 9.0;
            target.Runtime.Vy = 10.0;
            target.Runtime.Vz = 11.0;
            target.AttackingCounter = 4;
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
                    name = "B5Type3TargetGenericContinuation",
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
