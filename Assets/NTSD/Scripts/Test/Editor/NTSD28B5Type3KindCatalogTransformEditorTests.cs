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
    public sealed class NTSD28B5Type3KindCatalogTransformEditorTests
    {
        private static readonly int[] BoundIds = { 8, 209, 213 };
        private static readonly int[] RespondIds = { 200, 203, 205, 206, 207, 215, 216 };

        [Test]
        public void LockedRecord_MatchesExactBoundRespondMatrix()
        {
            foreach (int boundId in BoundIds)
            {
                foreach (int respondId in RespondIds)
                {
                    var world = new SimulationWorld();
                    TypedCharacter attacker = CreateEntity(
                        world, boundId, 0, LF2ObjectType.SpecialAttack);
                    TypedCharacter target = CreateEntity(
                        world, respondId, 1, LF2ObjectType.SpecialAttack);
                    Assert.That(
                        BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                            attacker,
                            target),
                        Is.True,
                        $"bound={boundId}, respond={respondId}");
                }
            }

            var negativeWorld = new SimulationWorld();
            TypedCharacter characterAttacker = CreateEntity(
                negativeWorld, 209, 0, LF2ObjectType.Character);
            TypedCharacter nonBoundAttacker = CreateEntity(
                negativeWorld, 210, 1, LF2ObjectType.SpecialAttack);
            TypedCharacter respondTarget = CreateEntity(
                negativeWorld, 200, 2, LF2ObjectType.SpecialAttack);
            TypedCharacter nonRespondTarget = CreateEntity(
                negativeWorld, 201, 3, LF2ObjectType.SpecialAttack);
            Assert.That(
                BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    characterAttacker,
                    respondTarget),
                Is.False);
            Assert.That(
                BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    nonBoundAttacker,
                    respondTarget),
                Is.False);
            Assert.That(
                BattleDamageWriter.IsNativeLockedKindTransformCandidate(
                    nonBoundAttacker,
                    nonRespondTarget),
                Is.False);
        }

        [Test]
        public void CandidateGate_UsesEffectRespondDirectionAndKind9Exception()
        {
            MethodInfo method = typeof(BruteForceSceneQuery).GetMethod(
                "IsBlockedReleaseOidInteraction",
                BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(method, Is.Not.Null);

            foreach (int respondId in RespondIds)
            {
                Assert.That(
                    InvokeCandidateGate(method, respondId, 209, 0),
                    Is.True,
                    $"respond={respondId}");
                Assert.That(
                    InvokeCandidateGate(method, respondId, 209, 9),
                    Is.False,
                    $"kind9 respond={respondId}");
            }

            Assert.That(InvokeCandidateGate(method, 209, 200, 0), Is.False);
            Assert.That(InvokeCandidateGate(method, 210, 209, 0), Is.False);
        }

        [TestCase(8)]
        [TestCase(209)]
        [TestCase(213)]
        public void Transform_CopiesAttackerIdentityAndPreservesExcludedFields(
            int boundId)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, boundId, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 200, 1, LF2ObjectType.SpecialAttack);
            TypedCharacter parent = CreateEntity(
                world, 9000, 2, LF2ObjectType.Character);
            attacker.RelationTeam = 7;
            attacker.Runtime.OwnerSlotIndex = 11;
            attacker.HolderCopySlot = 55;
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = parent.Runtime.SlotIndex;
            parent.RelationTeam = 8;
            parent.Runtime.OwnerSlotIndex = 13;
            parent.HolderCopySlot = 88;

            PrepareTarget(target);
            LF2CharacterDataWrapper sourceWrapper = attacker.FrameCache.Wrapper;

            InvokeTail(world, attacker, target, Effect());

            Assert.That(target.ObjectId, Is.EqualTo(boundId));
            Assert.That(target.FrameCache.Wrapper, Is.SameAs(sourceWrapper));
            Assert.That(target.RelationTeam, Is.EqualTo(7));
            Assert.That(target.Runtime.OwnerSlotIndex, Is.EqualTo(11));
            Assert.That(target.Frame.N, Is.EqualTo(40));
            Assert.That(target.Runtime.Frame, Is.EqualTo(40));
            Assert.That(target.Trans.WaitCounter, Is.EqualTo(40));
            Assert.That(target.Frame.Prev, Is.EqualTo(40));
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(target.HitConfirm2, Is.EqualTo(7));
            Assert.That(target.Runtime.SpecialHitLatch0EB, Is.True);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(target.HitCount, Is.EqualTo(3));
            Assert.That(target.Runtime.Vx, Is.EqualTo(9.0));
            Assert.That(target.Runtime.Vy, Is.EqualTo(10.0));
            Assert.That(target.Runtime.Vz, Is.EqualTo(11.0));
            Assert.That(target.HolderCopySlot, Is.EqualTo(66));
            Assert.That(target.Runtime.AnimCounter, Is.EqualTo(99));
            Assert.That(target.WeaponCount, Is.EqualTo(73));
            Assert.That(target.Frame.PN, Is.EqualTo(18));
            Assert.That(target.Frame.Prev2, Is.EqualTo(17));
            Assert.That(target.Runtime.PrevFrame2, Is.EqualTo(17));
        }

        [Test]
        public void State3005_SkipsLockedTransform()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 209, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 200, 1, LF2ObjectType.SpecialAttack);
            target.GetFrameDataById(0).state = LF2States.ObjectFlying;
            target.ImmediateFrame(0);
            LF2CharacterDataWrapper originalWrapper = target.FrameCache.Wrapper;

            InvokeTail(world, attacker, target, Effect());

            Assert.That(target.ObjectId, Is.EqualTo(200));
            Assert.That(target.FrameCache.Wrapper, Is.SameAs(originalWrapper));
            Assert.That(target.Frame.N, Is.Zero);
        }

        [Test]
        public void Transform_PrecedesMatchingStatePairReset()
        {
            // The catalog action/history write is observable even when the
            // following matching-state transaction selects frame 20.
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 209, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 200, 1, LF2ObjectType.SpecialAttack);
            attacker.GetFrameDataById(0).state = LF2States.ObjectExpanding;
            attacker.GetFrameDataById(40).state = LF2States.ObjectExpanding;
            attacker.ImmediateFrame(0);
            PrepareTarget(target);

            InvokeTail(world, attacker, target, Effect());

            Assert.That(target.ObjectId, Is.EqualTo(209));
            Assert.That(target.Frame.N, Is.EqualTo(20));
            Assert.That(target.Frame.Prev, Is.EqualTo(40));
            Assert.That(target.Trans.WaitCounter, Is.EqualTo(40));
            Assert.That(target.Runtime.Vx, Is.EqualTo(9.0));
            Assert.That(target.Runtime.Vy, Is.EqualTo(10.0));
            Assert.That(target.Runtime.Vz, Is.EqualTo(11.0));
            Assert.That(attacker.Frame.N, Is.EqualTo(20));
        }

        private static bool InvokeCandidateGate(
            MethodInfo method,
            int attackerOid,
            int targetOid,
            int kind)
        {
            return (bool)method.Invoke(
                null,
                new object[] { attackerOid, targetOid, kind });
        }

        private static void PrepareTarget(TypedCharacter target)
        {
            target.RelationTeam = 2;
            target.Runtime.OwnerSlotIndex = 44;
            target.Runtime.AnimCounter = 99;
            target.HolderCopySlot = 66;
            target.WeaponCount = 73;
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
            target.Frame.PN = 18;
            target.Frame.Prev = 19;
            target.Frame.Prev2 = 17;
            target.Runtime.PrevFrame2 = 17;
            target.Trans.SyncDirectFrameData(
                target.Frame.D.wait,
                target.Frame.D.next,
                9);
        }

        private static InteractionArea Effect()
        {
            return new InteractionArea { kind = 0, effect = 0 };
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
                    name = "B5Type3KindCatalogTransform",
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
