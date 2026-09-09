#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5HitGroupEligibilityAtomicProductionEditorTests
    {
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware)]
        public void FormalProducer_FreezesCompleteValidPair(
            CollisionFormalCollectorMode mode)
        {
            CreateScenario(
                attackerState: 0,
                attackerType: LF2ObjectType.Character,
                targetType: LF2ObjectType.Character,
                attackerGroup: 7,
                targetGroup: 8,
                vrest: 1,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out TestCharacter attacker,
                out TestCharacter target,
                out _);

            List<SceneQueryHit> candidates = Collect(world, query, mode, attacker);

            Assert.That(candidates.Count, Is.EqualTo(1));
            BattleHitCandidatePairSnapshot pair = candidates[0].PairSnapshot;
            Assert.That(pair.Valid, Is.True);
            Assert.That(pair.AttackerObjectId, Is.EqualTo(9100));
            Assert.That(pair.TargetObjectId, Is.EqualTo(9101));
            Assert.That(pair.AttackerObjectType, Is.EqualTo(0));
            Assert.That(pair.TargetObjectType, Is.EqualTo(0));
            Assert.That(pair.AttackerBattleGroup, Is.EqualTo(7));
            Assert.That(pair.TargetBattleGroup, Is.EqualTo(8));
            Assert.That(pair.AttackerCurrentState, Is.Zero);
            Assert.That(pair.TargetCurrentState, Is.Zero);
            Assert.That(pair.AttackerTickState, Is.Zero);
            Assert.That(pair.TargetTickState, Is.Zero);
            Assert.That(pair.AttackerFacing, Is.False);
            Assert.That(pair.TargetFacing, Is.False);
            Assert.That(
                pair.LinkedHolderPresent,
                Is.True,
                "Native unlinked storage projects implicit linked-parent slot zero.");
            Assert.That(pair.LinkedHolderBattleGroup, Is.EqualTo(7));
            _ = target;
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, 190)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, 190)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, 190)]
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, 180)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, 180)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, 180)]
        public void FormalProducer_StateExceptionAcceptsSameGroup(
            CollisionFormalCollectorMode mode,
            int attackerState)
        {
            CreateScenario(
                attackerState,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                9,
                9,
                1,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out TestCharacter attacker,
                out _,
                out _);

            List<SceneQueryHit> candidates = Collect(world, query, mode, attacker);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].PairSnapshot.AttackerCurrentState,
                Is.EqualTo(attackerState));
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware)]
        public void FormalProducer_Type0ToType3OpposingFacingIsAccepted(
            CollisionFormalCollectorMode mode)
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.SpecialAttack,
                11,
                11,
                1,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out TestCharacter attacker,
                out TestCharacter target,
                out _);
            attacker.SwitchDir("right");
            target.SwitchDir("left");

            List<SceneQueryHit> candidates = Collect(world, query, mode, attacker);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].PairSnapshot.AttackerFacing, Is.False);
            Assert.That(candidates[0].PairSnapshot.TargetFacing, Is.True);
        }

        [Test]
        public void FormalProducer_RejectedState190DifferentGroupCannotAdvanceTieRng()
        {
            InteractionArea itr = Interaction(vrest: 0);
            var world = new SimulationWorld();
            TestCharacter attacker = Entity("GroupAttacker", 9110, 190,
                LF2ObjectType.Character, itr, hasBody: false);
            TestCharacter accepted = Entity("Accepted", 9111, 0,
                LF2ObjectType.Character, null, hasBody: true);
            TestCharacter rejected = Entity("Rejected", 9112, 0,
                LF2ObjectType.Character, null, hasBody: true);
            Register(world, attacker, 0, 3);
            Register(world, accepted, 1, 3);
            Register(world, rejected, 2, 4);
            var query = (BruteForceSceneQuery)world.SceneQuery;
            world.Rng.Seed(0x28B5u);

            List<SceneQueryHit> candidates = Collect(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker,
                seedBeforeCollection: false);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(candidates[0].Target, Is.SameAs(accepted));
            Assert.That(world.Rng.CallCount, Is.Zero);
        }

        [TestCase(1)]
        [TestCase(3)]
        public void FormalProducer_ModeGateAcceptsSameGroup(int modeGate)
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                12,
                12,
                1,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out TestCharacter attacker,
                out _,
                out _);
            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 =
                modeGate;

            List<SceneQueryHit> candidates = Collect(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);

            Assert.That(candidates.Count, Is.EqualTo(1));
        }

        [Test]
        public void FormalProducer_ModeGateDoesNotBypassEffect21()
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                12,
                12,
                1,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out TestCharacter attacker,
                out _,
                out InteractionArea itr);
            itr.effect = 21;
            world.Runtime.NativeHitResourceRules.ActiveModeHitGroupGate18 = 1;

            List<SceneQueryHit> candidates = Collect(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);

            Assert.That(candidates, Is.Empty);
        }

        [Test]
        public void FormalProducer_RejectedPairDoesNotConsumeTwentySlotCapacity()
        {
            InteractionArea itr = Interaction(vrest: 1);
            var world = new SimulationWorld();
            TestCharacter attacker = Entity("CapacityAttacker", 9120, 0,
                LF2ObjectType.Character, itr, hasBody: false);
            Register(world, attacker, 0, 1);
            TestCharacter rejected = Entity("RejectedFirst", 9121, 0,
                LF2ObjectType.Character, null, hasBody: true);
            Register(world, rejected, 1, 1);
            for (int index = 0; index < 20; index++)
            {
                TestCharacter accepted = Entity(
                    $"Accepted{index}",
                    9130 + index,
                    0,
                    LF2ObjectType.Character,
                    null,
                    hasBody: true);
                Register(world, accepted, index + 2, 2);
            }
            var query = (BruteForceSceneQuery)world.SceneQuery;

            List<SceneQueryHit> candidates = Collect(
                world,
                query,
                CollisionFormalCollectorMode.ForceBruteForce,
                attacker);

            Assert.That(candidates.Count, Is.EqualTo(20));
            for (int index = 0; index < candidates.Count; index++)
                Assert.That(candidates[index].PairSnapshot.TargetBattleGroup,
                    Is.EqualTo(2));
        }

        [Test]
        public void SharedConsumer_UsesFrozenState190AfterLiveStateChanges()
        {
            CreateScenario(
                190,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                4,
                4,
                1,
                out SimulationWorld world,
                out _,
                out TestCharacter attacker,
                out TestCharacter target,
                out _);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(1));
            attacker.Frame.D.state = 0;

            world.PostInteractionTickAll(3204);

            Assert.That(target.Health.HP, Is.EqualTo(490));
        }

        [Test]
        public void RuntimeConsume_ValidSnapshotDoesNotRereadChangedLiveGroup()
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                1,
                2,
                1,
                out _,
                out _,
                out TestCharacter attacker,
                out TestCharacter target,
                out InteractionArea itr);
            BattleHitCandidatePairSnapshot pair =
                BattleHitCandidatePairSnapshotFactory.Capture(attacker, target);
            target.RelationTeam = 1;

            bool accepted = BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                attacker,
                itr,
                target,
                in pair);

            Assert.That(pair.Valid, Is.True);
            Assert.That(accepted, Is.True);
        }

        [Test]
        public void RuntimeConsume_ValidSnapshotDoesNotAcceptChangedLiveGroup()
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                5,
                5,
                1,
                out _,
                out _,
                out TestCharacter attacker,
                out TestCharacter target,
                out InteractionArea itr);
            BattleHitCandidatePairSnapshot pair =
                BattleHitCandidatePairSnapshotFactory.Capture(attacker, target);
            target.RelationTeam = 6;

            bool accepted = BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                attacker,
                itr,
                target,
                in pair);

            Assert.That(pair.Valid, Is.True);
            Assert.That(accepted, Is.False);
        }

        [TestCase(5, 5, false)]
        [TestCase(0, 5, true)]
        [TestCase(5, 6, true)]
        public void SubstitutedKind5CharacterGate_UsesFrozenHolderGroup(
            int holderGroup,
            int targetGroup,
            bool expected)
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                targetObjectId: 123,
                attackerObjectId: 122,
                targetGroup,
                linkedHolderPresent: true,
                linkedHolderGroup: holderGroup);

            bool accepted =
                BattleHitGroupEligibilityResolver.AcceptsSubstitutedKind5Character(
                    in pair);

            Assert.That(accepted, Is.EqualTo(expected));
        }

        [Test]
        public void SubstitutedKind5CharacterGate_FreezeColumnUsesFrozenActions()
        {
            BattleHitCandidatePairSnapshot pair = Pair(
                targetObjectId: 212,
                attackerObjectId: 212,
                targetGroup: 5,
                linkedHolderPresent: true,
                linkedHolderGroup: 5,
                attackerAction: 10,
                targetAction: 15);

            Assert.That(
                BattleHitGroupEligibilityResolver
                    .AcceptsSubstitutedKind5Character(in pair),
                Is.True);
        }

        [Test]
        public void WarmLiveSnapshotAndResolver_AllocateZeroManagedBytes()
        {
            CreateScenario(
                0,
                LF2ObjectType.Character,
                LF2ObjectType.Character,
                1,
                2,
                1,
                out _,
                out _,
                out TestCharacter attacker,
                out TestCharacter target,
                out InteractionArea itr);
            BattleHitCandidatePairSnapshot warm =
                BattleHitCandidatePairSnapshotFactory.Capture(attacker, target);
            _ = BattleHitGroupEligibilityResolver.Resolve(itr.kind, itr.effect,
                in warm, 0);
            long before = System.GC.GetAllocatedBytesForCurrentThread();

            for (int index = 0; index < 128; index++)
            {
                BattleHitCandidatePairSnapshot pair =
                    BattleHitCandidatePairSnapshotFactory.Capture(attacker, target);
                _ = BattleHitGroupEligibilityResolver.Resolve(
                    itr.kind,
                    itr.effect,
                    in pair,
                    0);
            }

            long after = System.GC.GetAllocatedBytesForCurrentThread();
            Assert.That(after - before, Is.Zero);
        }

        private static BattleHitCandidatePairSnapshot Pair(
            int targetObjectId,
            int attackerObjectId,
            int targetGroup,
            bool linkedHolderPresent,
            int linkedHolderGroup,
            int attackerAction = 0,
            int targetAction = 0)
        {
            return new BattleHitCandidatePairSnapshot(
                true,
                attackerObjectId,
                targetObjectId,
                2,
                0,
                99,
                targetGroup,
                attackerAction,
                targetAction,
                0,
                0,
                0,
                0,
                0,
                0,
                false,
                false,
                linkedHolderPresent,
                linkedHolderGroup);
        }

        private static List<SceneQueryHit> Collect(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            TestCharacter attacker,
            bool seedBeforeCollection = true)
        {
            query.FormalCollectorMode = mode;
            query.ForceRoleAwareDirectForDiagnostics = true;
            if (seedBeforeCollection)
                world.Rng.Seed(0x28B5u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(query.TryGetCollisionCandidateSequence(
                attacker,
                out List<SceneQueryHit> sequence), Is.True);
            var copy = new List<SceneQueryHit>(sequence);
            world.EndCollisionCandidateConsumption();
            return copy;
        }

        private static void CreateScenario(
            int attackerState,
            LF2ObjectType attackerType,
            LF2ObjectType targetType,
            int attackerGroup,
            int targetGroup,
            int vrest,
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out TestCharacter attacker,
            out TestCharacter target,
            out InteractionArea itr)
        {
            itr = Interaction(vrest);
            world = new SimulationWorld();
            attacker = Entity("GroupAttacker", 9100, attackerState,
                attackerType, itr, hasBody: false);
            target = Entity("GroupTarget", 9101, 0,
                targetType, null, hasBody: true);
            Register(world, attacker, 0, attackerGroup);
            Register(world, target, 1, targetGroup);
            query = (BruteForceSceneQuery)world.SceneQuery;
        }

        private static InteractionArea Interaction(int vrest)
        {
            return new InteractionArea
            {
                kind = 0,
                effect = 0,
                injury = 10,
                fall = 1,
                arest = 10,
                vrest = vrest,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
            };
        }

        private static TestCharacter Entity(
            string name,
            int objectId,
            int state,
            LF2ObjectType type,
            InteractionArea itr,
            bool hasBody)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
            if (itr != null)
                frame.itrs.Add(itr);
            if (hasBody)
            {
                frame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = 0,
                    y = -10,
                    w = 10,
                    h = 20,
                });
            }

            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 0,
                frames = new List<LF2FrameData> { frame },
            };
            var entity = new TestCharacter(type)
            {
                Name = name,
                ObjectId = objectId,
            };
            entity.ModuleInitialize();
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.Frame.D = entity.FrameCache.GetFrameDataById(0);
            entity.Frame.N = 0;
            entity.Frame.PN = 0;
            entity.Frame.Prev = 0;
            entity.Frame.Prev2 = 0;
            entity.Frame.Prev2D = entity.Frame.D;
            entity.Initialize(500, 500);
            entity.FrameDelay = 0;
            return entity;
        }

        private static void Register(
            SimulationWorld world,
            TestCharacter entity,
            int slot,
            int relationGroup)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = relationGroup;
            entity.RelationTeam = relationGroup;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.AttackExempt = 0;
            entity.HitStun = 0;
            entity.Runtime.LinkState = 0;
            entity.ItrRest.Reset();
            entity.Runtime.SetPosition(0, 0, 0);
            entity.Runtime.SetVelocity(0, 0, 0);
            entity.Runtime.SyncIntegerPosition();
        }

        private sealed class TestCharacter : LF2Character
        {
            private readonly LF2ObjectType type;

            internal TestCharacter(LF2ObjectType type)
            {
                this.type = type;
            }

            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)type;
        }
    }
}
#endif
