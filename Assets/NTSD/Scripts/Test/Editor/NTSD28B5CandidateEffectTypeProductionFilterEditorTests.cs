#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.IO;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5CandidateEffectTypeProductionFilterEditorTests
    {
        [TestCase(13, 0, true)]
        [TestCase(13, 3, false)]
        [TestCase(14, 3, true)]
        [TestCase(14, 0, false)]
        [TestCase(15, 0, true)]
        [TestCase(15, 3, true)]
        [TestCase(15, 5, false)]
        [TestCase(16, 1, true)]
        [TestCase(16, 3, true)]
        [TestCase(16, 6, true)]
        [TestCase(16, 0, false)]
        [TestCase(16, 5, false)]
        [TestCase(12, 5, true)]
        [TestCase(17, 0, true)]
        public void RuntimeDefensiveFilter_UsesCandidateEffectTypeMatrix(
            int effect,
            int targetType,
            bool accepted)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8800, 0, 0);
            TypedCharacter target = CreateEntity(world, 8801, 1, targetType);
            attacker.RelationTeam = 1;
            target.RelationTeam = 2;

            bool actual = BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                attacker,
                Interaction(effect),
                target);

            Assert.That(actual, Is.EqualTo(accepted));
        }

        [Test]
        public void SharedRunnerSourceDefensivelyFiltersResolvedItrBeforeDisposition()
        {
            string source = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs"));
            int resolvedItrIndex = source.IndexOf(
                "InteractionArea runtimeItr = BruteForceSceneQuery.ResolveRuntimeItrForPair(");
            int filterIndex = source.IndexOf(
                "BattleHitCandidateEffectTypeResolver.Accepts(",
                resolvedItrIndex);
            int dispositionIndex = source.IndexOf(
                "BattleHitCandidateDisposition disposition =",
                resolvedItrIndex);

            Assert.That(resolvedItrIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(filterIndex, Is.GreaterThan(resolvedItrIndex));
            Assert.That(dispositionIndex, Is.GreaterThan(filterIndex));
        }

        private static InteractionArea Interaction(int effect)
        {
            return new InteractionArea
            {
                kind = 6,
                effect = effect,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 20,
            };
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            int objectType)
        {
            var frames = new List<LF2FrameData>
            {
                new LF2FrameData
                {
                    frameId = 0,
                    state = LF2States.Standing,
                    wait = 100,
                    next = 0,
                    bodies = new List<BattleBodyBoxValue>
                    {
                        new BattleBodyBoxValue(-20, -20, 40, 40),
                    },
                },
            };
            var data = new LF2CharacterData
            {
                type_sub = objectType,
                frames = frames,
            };
            var entity = new TypedCharacter(objectType) { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            world.Register(entity);
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

