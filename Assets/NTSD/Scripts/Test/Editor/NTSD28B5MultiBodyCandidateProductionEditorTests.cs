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

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, false)]
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, true)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, false)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, true)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, false)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, true)]
        public void SourceGeometryKeepsNearAndFarCandidatesAcrossViewScale(
            CollisionFormalCollectorMode mode,
            bool configuredView)
        {
            foreach (int targetSourceX in new[] { 519, 520, 580 })
            {
                CreateProjectedGeometryScenario(
                    configuredView, 535, 650, targetSourceX, 650, false,
                    out SimulationWorld world,
                    out BruteForceSceneQuery query,
                    out LF2Character attacker);
                List<SceneQueryHit> candidates = RunCollection(world, query, mode, attacker);

                Assert.That(candidates.Count, Is.EqualTo(targetSourceX == 580 ? 0 : 1),
                    $"mode={mode} configuredView={configuredView} targetSourceX={targetSourceX}");
            }
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, false)]
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, true)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, false)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, true)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, false)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, true)]
        public void SourceDepthRadiusKeepsNearZCandidateAcrossViewScale(
            CollisionFormalCollectorMode mode,
            bool configuredView)
        {
            CreateProjectedGeometryScenario(
                configuredView, 535, 650, 535, 664, false,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            List<SceneQueryHit> candidates = RunCollection(world, query, mode, attacker);
            Assert.That(candidates.Count, Is.EqualTo(1),
                $"mode={mode} configuredView={configuredView} sourceDeltaZ=14");
        }

        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, false)]
        [TestCase(CollisionFormalCollectorMode.ForceBruteForce, true)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, false)]
        [TestCase(CollisionFormalCollectorMode.ForceLegacyUnionAabb, true)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, false)]
        [TestCase(CollisionFormalCollectorMode.ForceRoleAware, true)]
        public void MirroredSourceGeometryKeepsNearCandidateAcrossViewScale(
            CollisionFormalCollectorMode mode,
            bool configuredView)
        {
            CreateProjectedGeometryScenario(
                configuredView, 535, 650, 550, 650, true,
                out SimulationWorld world,
                out BruteForceSceneQuery query,
                out LF2Character attacker);
            List<SceneQueryHit> candidates = RunCollection(world, query, mode, attacker);
            Assert.That(candidates.Count, Is.EqualTo(1),
                $"mode={mode} configuredView={configuredView} facing=left");
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ImmediateBodyQuerySharesProjectedNearFarGeometry(bool configuredView)
        {
            foreach (int targetSourceX in new[] { 519, 580 })
            {
                CreateProjectedGeometryScenario(
                    configuredView, 535, 650, targetSourceX, 650, false,
                    out _,
                    out BruteForceSceneQuery query,
                    out LF2Character attacker);
                LF2FrameData frame = attacker.GetCollisionFrameData();
                List<SceneQueryHit> hits = query.QueryBodyHits(
                    attacker, frame, frame.itrs[0]);
                Assert.That(hits.Count, Is.EqualTo(targetSourceX == 519 ? 1 : 0),
                    $"configuredView={configuredView} targetSourceX={targetSourceX}");
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void NonCharacterBodyUsesSameProjectedGeometry(bool configuredView)
        {
            foreach (int targetSourceX in new[] { 519, 580 })
            {
                CreateProjectedGeometryScenario(
                    configuredView, 535, 650, targetSourceX, 650, false,
                    out SimulationWorld world,
                    out BruteForceSceneQuery query,
                    out LF2Character attacker);
                LF2FrameData targetFrame = Frame();
                targetFrame.centerx = 39;
                targetFrame.bodies.Add(new BodyBox
                {
                    kind = 0,
                    x = 21,
                    y = -10,
                    w = 43,
                    h = 20,
                });
                var data = new LF2CharacterData
                {
                    name = "ProjectedSpecialBody",
                    type_sub = 0,
                    frames = new List<LF2FrameData> { targetFrame },
                };
                var special = new LF2SpecialAttack
                {
                    Name = data.name,
                    ObjectId = 8904,
                };
                special.FrameCache.Load(new LF2CharacterDataWrapper(8904, data));
                special.Frame.D = special.FrameCache.GetFrameDataById(0);
                special.Frame.N = 0;
                special.SetRequiredRuntimeSlot(2);
                world.Register(special);
                special.RelationTeam = 2;
                special.Health.HP = 500;
                special.Health.HPBound = 500;
                SetProjectedPosition(world, special, targetSourceX, 650);

                List<SceneQueryHit> hits = query.QueryBodyHits(
                    attacker, attacker.GetCollisionFrameData(),
                    attacker.GetCollisionFrameData().itrs[0]);
                Assert.That(hits.Exists(hit => ReferenceEquals(hit.Target, special)),
                    Is.EqualTo(targetSourceX == 519),
                    $"configuredView={configuredView} targetSourceX={targetSourceX}");
            }
        }

        private static void CreateProjectedGeometryScenario(
            bool configuredView,
            int attackerSourceX,
            int attackerSourceZ,
            int targetSourceX,
            int targetSourceZ,
            bool facingLeft,
            out SimulationWorld world,
            out BruteForceSceneQuery query,
            out LF2Character attacker)
        {
            world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);

            LF2FrameData attackerFrame = Frame();
            attackerFrame.centerx = 39;
            attackerFrame.itrs.Add(new InteractionArea
            {
                kind = 0,
                effect = 0,
                vrest = 1,
                x = 40,
                y = -20,
                w = 25,
                h = 40,
                zwidth = 15,
            });
            LF2FrameData targetFrame = Frame();
            targetFrame.centerx = 39;
            targetFrame.bodies.Add(new BodyBox
            {
                kind = 0,
                x = 21,
                y = -10,
                w = 43,
                h = 20,
            });

            attacker = Character("ProjectedItr", 8902, attackerFrame);
            LF2Character target = Character("ProjectedBdy", 8903, targetFrame);
            Register(world, attacker, 0, 1);
            Register(world, target, 1, 2);
            SetProjectedPosition(world, attacker, attackerSourceX, attackerSourceZ);
            SetProjectedPosition(world, target, targetSourceX, targetSourceZ);
            if (facingLeft)
            {
                attacker.Runtime.Dir = "left";
                attacker.PS.dir = "left";
            }
            query = (BruteForceSceneQuery)world.SceneQuery;
        }

        private static void SetProjectedPosition(
            SimulationWorld world,
            LF2Entity entity,
            int sourceX,
            int sourceZ)
        {
            entity.Runtime.SetPosition(
                world.SpatialProjection.SourceToViewX(sourceX, 0.0),
                0.0,
                world.SpatialProjection.SourceToViewZ(sourceZ, 0.0));
            entity.Runtime.SyncIntegerPosition();
            entity.Runtime.SetSourceRulePosition(sourceX, sourceZ);
            entity.Runtime.SyncSourceRuleIntegerPosition();
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
