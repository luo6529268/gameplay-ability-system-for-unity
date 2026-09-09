#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5MultiBodyCandidateProductionEditorTests
    {
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware)]
        public void MultiplePath_EmitsEveryOverlappingBodyInSourceOrder(
            CollisionFormalCollectorMode mode)
        {
            CreateScenario(
                vrest: 1,
                bodyCount: 2,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker,
                out _);

            List<SceneQueryHit> candidates = RunCollection(world, query, mode, attacker);

            Assert.That(candidates.Count, Is.EqualTo(2));
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(2));
            Assert.That(candidates[0].BodyX, Is.EqualTo(0));
            Assert.That(candidates[1].BodyX, Is.EqualTo(1));
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware)]
        public void MultiplePath_AppliesTwentyCandidateCapacityAfterBodyExpansion(
            CollisionFormalCollectorMode mode)
        {
            CreateScenario(
                vrest: 1,
                bodyCount: 22,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker,
                out _);

            List<SceneQueryHit> candidates = RunCollection(world, query, mode, attacker);

            Assert.That(candidates.Count, Is.EqualTo(20));
            Assert.That(attacker.Runtime.HitCandidateCount, Is.EqualTo(20));
            for (int index = 0; index < candidates.Count; index++)
                Assert.That(candidates[index].BodyX, Is.EqualTo(index));
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware)]
        public void NearestPath_VisitsSecondOverlappingBodyAndConsumesTieRng(
            CollisionFormalCollectorMode mode)
        {
            CreateScenario(
                vrest: 0,
                bodyCount: 2,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker,
                out _);
            world.Rng.Seed(0x2847u);

            List<SceneQueryHit> candidates = RunCollection(
                world,
                query,
                mode,
                attacker,
                seedBeforeCollection: false);

            Assert.That(candidates.Count, Is.EqualTo(1));
            Assert.That(world.Rng.CallCount, Is.EqualTo(1));
        }

        [Test]
        public void DirectQuery_RemainsOneHitPerTargetCompatibilitySurface()
        {
            CreateScenario(
                vrest: 1,
                bodyCount: 2,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker,
                out InteractionArea itr);

            List<SceneQueryHit> hits = query.QueryBodyHits(
                attacker,
                attacker.GetCollisionFrameData(),
                itr);

            Assert.That(hits.Count, Is.EqualTo(1));
            Assert.That(hits[0].BodyX, Is.EqualTo(0));
            _ = world;
        }

        private static List<SceneQueryHit> RunCollection(
            SimulationWorld world,
            BruteForceSceneQuery query,
            CollisionFormalCollectorMode mode,
            LF2Character attacker,
            bool seedBeforeCollection = true)
        {
            query.FormalCollectorMode = mode;
            query.ForceRoleAwareDirectForDiagnostics = true;
            if (seedBeforeCollection)
                world.Rng.Seed(0x2847u);
            world.CaptureCollisionFrameSnapshotsAll();
            world.CollectCollisionCandidatesAll();
            Assert.That(
                query.TryGetCollisionCandidateSequence(
                    attacker,
                    out List<SceneQueryHit> sequence),
                Is.True);
            var copy = new List<SceneQueryHit>(sequence);
            world.EndCollisionCandidateConsumption();
            return copy;
        }

        private static void CreateScenario(
            int vrest,
            int bodyCount,
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character attacker,
            out InteractionArea itr)
        {
            itr = new InteractionArea
            {
                kind = 0,
                effect = 0,
                vrest = vrest,
                x = -100,
                y = -20,
                w = 240,
                h = 40,
                zwidth = 15,
            };
            LF2FrameData attackerFrame = Frame();
            attackerFrame.itrs.Add(itr);
            LF2FrameData targetFrame = Frame();
            for (int bodyIndex = 0; bodyIndex < bodyCount; bodyIndex++)
            {
                targetFrame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = bodyIndex,
                    y = -10,
                    w = 10,
                    h = 20,
                });
            }

            world = new SimulationWorld();
            attacker = Character("MultiBodyAttacker", 8900, attackerFrame);
            LF2Character target = Character("MultiBodyTarget", 8901, targetFrame);
            Register(world, attacker, 0, 1);
            Register(world, target, 1, 2);
            query = (BruteForceSceneQuery)world.SceneQuery;
        }

        private static LF2FrameData Frame()
        {
            return new LF2FrameData
            {
                frameId = 0,
                state = LF2States.Standing,
                wait = 100,
                next = 0,
                centerx = 0,
                centery = 0,
            };
        }

        private static LF2Character Character(
            string name,
            int objectId,
            LF2FrameData frame)
        {
            var data = new LF2CharacterData
            {
                name = name,
                type_sub = 0,
                frames = new List<LF2FrameData> { frame },
            };
            var character = new TestCharacter
            {
                Name = name,
                ObjectId = objectId,
            };
            character.ModuleInitialize();
            character.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            character.Frame.D = character.FrameCache.GetFrameDataById(0);
            character.Frame.N = 0;
            character.Frame.PN = 0;
            character.Initialize(500, 500);
            character.FrameDelay = 0;
            return character;
        }

        private static void Register(
            SimulationWorld world,
            LF2Character entity,
            int slot,
            int team)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
            entity.Team = team;
            entity.RelationTeam = team;
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
            public override int GetCurrentDataObjectTypeForSimulation() =>
                (int)LF2ObjectType.Character;
        }

    }
}
#endif
