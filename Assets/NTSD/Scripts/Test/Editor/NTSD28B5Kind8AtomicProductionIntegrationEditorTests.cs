#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Kind8AtomicProductionIntegrationEditorTests
    {
        [Test]
        public void Candidate_NonCharacterExactSelectorIsAccepted()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8700, 0, 0);
            TypedCharacter target = CreateEntity(world, 8701, 1, 3);
            attacker.RelationTeam = 1;
            target.RelationTeam = 2;

            bool accepted = BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                attacker,
                Kind8(targetSelector: 3, relationSelector: 0),
                target);

            Assert.That(accepted, Is.True);
        }

        [Test]
        public void Candidate_SelectorAndRelationRejectionsAreApplied()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8710, 0, 0);
            TypedCharacter target = CreateEntity(world, 8711, 1, 0);
            attacker.RelationTeam = 1;
            target.RelationTeam = 2;

            Assert.That(
                BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                    attacker,
                    Kind8(targetSelector: 3, relationSelector: 0),
                    target),
                Is.False,
                "Exact target selector mismatch must reject.");
            Assert.That(
                BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                    attacker,
                    Kind8(targetSelector: 8, relationSelector: 1),
                    target),
                Is.False,
                "respond=1 requires the same battle group.");
        }

        [Test]
        public void Candidate_Respond4UsesBattleGameModeContext()
        {
            var world = new SimulationWorld();
            world.Runtime.Match.BattleGameModeId = 6;
            TypedCharacter attacker = CreateEntity(world, 8720, 0, 0);
            TypedCharacter target = CreateEntity(world, 8721, 1, 0);
            attacker.RelationTeam = 9;
            target.RelationTeam = 9;
            attacker.Runtime.OwnerSlotIndex = 6;
            target.Runtime.OwnerSlotIndex = 6;

            Assert.That(
                BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                    attacker,
                    Kind8(targetSelector: 0, relationSelector: 4),
                    target),
                Is.True);

            world.Runtime.Match.BattleGameModeId = 2;
            Assert.That(
                BruteForceSceneQuery.RuntimeConsumeItrAllowed(
                    attacker,
                    Kind8(targetSelector: 0, relationSelector: 4),
                    target),
                Is.False);
        }

        [Test]
        public void Actual_RejectedSelectorHasNoMutation()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8730, 0, 0);
            TypedCharacter target = CreateEntity(world, 8731, 1, 0);
            ConfigurePositions(attacker, target);

            bool applied = ApplyActual(
                world,
                attacker,
                target,
                Kind8(targetSelector: 3, relationSelector: 0));

            Assert.That(applied, Is.False);
            AssertUnchanged(attacker, target);
        }

        [Test]
        public void Actual_ZeroHealCaughtactAndDvx999UseIndependentGates()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8740, 0, 0);
            TypedCharacter target = CreateEntity(world, 8741, 1, 5);
            ConfigurePositions(attacker, target);
            InteractionArea interaction = Kind8(8, 0);
            interaction.injury = 0;
            interaction.caughtact = new[] { 25 };
            interaction.dvx = 999;
            interaction.dvy = -1;

            bool applied = ApplyActual(world, attacker, target, interaction);

            Assert.That(applied, Is.True);
            Assert.That(target.HealTimer, Is.EqualTo(77));
            Assert.That(target.Health.PP, Is.EqualTo(125));
            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            Assert.That(attacker.Runtime.Frame, Is.EqualTo(10));
            AssertAttackerPosition(attacker, 10.5, 20.5, 30.5, 10, 20, 30);
        }

        [TestCase(-1, 10.5, 20.5, 30.5)]
        [TestCase(0, 100.25, 20.5, 301.25)]
        [TestCase(1, 10.5, 200.25, 301.25)]
        [TestCase(2, 100.25, 200.25, 301.25)]
        [TestCase(3, 100.25, 20.5, 301.25)]
        [TestCase(-2, 100.25, 20.5, 301.25)]
        public void Actual_DvySelectsPreciseAxesAndNeverWritesIntegerMirrors(
            int dvy,
            double expectedX,
            double expectedY,
            double expectedZ)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8750, 0, 0);
            TypedCharacter target = CreateEntity(world, 8751, 1, 3);
            ConfigurePositions(attacker, target);
            InteractionArea interaction = Kind8(3, 0);
            interaction.injury = 5;
            interaction.caughtact = new[] { 7 };
            interaction.dvx = 30;
            interaction.dvy = dvy;

            bool applied = ApplyActual(world, attacker, target, interaction);

            Assert.That(applied, Is.True);
            Assert.That(target.HealTimer, Is.EqualTo(1005));
            Assert.That(target.Health.PP, Is.EqualTo(107));
            Assert.That(attacker.Frame.N, Is.EqualTo(30));
            AssertAttackerPosition(
                attacker,
                expectedX,
                expectedY,
                expectedZ,
                10,
                20,
                30);
        }

        [Test]
        public void HitPlan_ProjectsConditionalTransactionWithoutIntegerWrites()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8760, 0, 0);
            TypedCharacter target = CreateEntity(world, 8761, 1, 5);
            ConfigurePositions(attacker, target);
            InteractionArea interaction = Kind8(8, 0);
            interaction.injury = 0;
            interaction.caughtact = new[] { 25 };
            interaction.dvx = 999;
            interaction.dvy = 1;

            bool projected = TryProject(
                world,
                attacker,
                target,
                interaction,
                out object projection);

            Assert.That(projected, Is.True);
            Assert.That(ReadInt(projection, "TargetHealTimer"), Is.EqualTo(77));
            Assert.That(ReadInt(projection, "TargetPp"), Is.EqualTo(125));
            Assert.That(ReadInt(projection, "AttackerFrame"), Is.EqualTo(10));
            Assert.That(ReadDouble(projection, "AttackerX"), Is.EqualTo(10.5));
            Assert.That(ReadDouble(projection, "AttackerY"), Is.EqualTo(200.25));
            Assert.That(ReadDouble(projection, "AttackerZ"), Is.EqualTo(301.25));
            Assert.That(ReadInt(projection, "AttackerXInt"), Is.EqualTo(10));
            Assert.That(ReadInt(projection, "AttackerYInt"), Is.EqualTo(20));
            Assert.That(ReadInt(projection, "AttackerZInt"), Is.EqualTo(30));
        }

        [Test]
        public void HitPlan_RejectedSelectorReturnsFalseWithoutProjectionMutation()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8770, 0, 0);
            TypedCharacter target = CreateEntity(world, 8771, 1, 0);
            ConfigurePositions(attacker, target);

            bool projected = TryProject(
                world,
                attacker,
                target,
                Kind8(3, 0),
                out object projection);

            Assert.That(projected, Is.False);
            Assert.That(ReadInt(projection, "TargetHealTimer"), Is.EqualTo(77));
            Assert.That(ReadInt(projection, "TargetPp"), Is.EqualTo(100));
            Assert.That(ReadInt(projection, "AttackerFrame"), Is.EqualTo(10));
        }

        [Test]
        public void SharedRunnerSourceRoutesKind8ThroughSingleWriter()
        {
            string source = File.ReadAllText(Path.Combine(
                Application.dataPath,
                "NTSD/Scripts/Animation/LF2Objects/BattleHitCandidateSequenceRunner.cs"));
            StringAssert.Contains(
                "disposition == BattleHitCandidateDisposition.Kind8",
                source);
            StringAssert.Contains(
                "BattleKind8ControlRelationWriter.TryApply",
                source);
        }

        private static bool ApplyActual(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            Type type = Type.GetType(
                "NTSD.Simulation.Ecs.BattleKind8ControlRelationWriter, Assembly-CSharp");
            Assert.That(type, Is.Not.Null);
            MethodInfo method = type.GetMethod(
                "TryApply",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return (bool)method.Invoke(
                null,
                new object[] { world, attacker, target, interaction });
        }

        private static bool TryProject(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction,
            out object projection)
        {
            FieldInfo field = typeof(SimulationWorld).GetField(
                "battleEcsHitExecutionPlan",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            object plan = field.GetValue(world);
            MethodInfo capture = plan.GetType().GetMethod(
                "CaptureWriterEffectSnapshot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(capture, Is.Not.Null);
            projection = capture.Invoke(
                plan,
                new object[] { attacker, target, -1 });
            MethodInfo project = plan.GetType().GetMethod(
                "ProjectWriterEffect",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(project, Is.Not.Null);
            object[] args =
            {
                attacker,
                target,
                interaction,
                BattleHitCandidateDisposition.Kind8,
                projection,
            };
            bool result = (bool)project.Invoke(null, args);
            projection = args[4];
            return result;
        }

        private static int ReadInt(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return (int)field.GetValue(value);
        }

        private static double ReadDouble(object value, string name)
        {
            FieldInfo field = value.GetType().GetField(
                name,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name);
            return (double)field.GetValue(value);
        }

        private static InteractionArea Kind8(
            int targetSelector,
            int relationSelector)
        {
            return new InteractionArea
            {
                kind = 8,
                x = -20,
                y = -20,
                w = 40,
                h = 40,
                zwidth = 20,
                bdefend = targetSelector,
                respond = relationSelector,
                dvx = 999,
                dvy = -1,
            };
        }

        private static void ConfigurePositions(
            TypedCharacter attacker,
            TypedCharacter target)
        {
            attacker.ImmediateFrame(10);
            attacker.Runtime.X = 10.5;
            attacker.Runtime.Y = 20.5;
            attacker.Runtime.Z = 30.5;
            attacker.Runtime.XInt = 10;
            attacker.Runtime.YInt = 20;
            attacker.Runtime.ZInt = 30;
            target.Runtime.X = 100.25;
            target.Runtime.Y = 200.25;
            target.Runtime.Z = 300.25;
            target.Runtime.XInt = 100;
            target.Runtime.YInt = 200;
            target.Runtime.ZInt = 300;
            target.HealTimer = 77;
            target.Health.PP = 100;
        }

        private static void AssertUnchanged(
            TypedCharacter attacker,
            TypedCharacter target)
        {
            Assert.That(target.HealTimer, Is.EqualTo(77));
            Assert.That(target.Health.PP, Is.EqualTo(100));
            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            AssertAttackerPosition(attacker, 10.5, 20.5, 30.5, 10, 20, 30);
        }

        private static void AssertAttackerPosition(
            TypedCharacter attacker,
            double x,
            double y,
            double z,
            int xInt,
            int yInt,
            int zInt)
        {
            Assert.That(attacker.Runtime.X, Is.EqualTo(x).Within(0.000000001));
            Assert.That(attacker.Runtime.Y, Is.EqualTo(y).Within(0.000000001));
            Assert.That(attacker.Runtime.Z, Is.EqualTo(z).Within(0.000000001));
            Assert.That(attacker.Runtime.XInt, Is.EqualTo(xInt));
            Assert.That(attacker.Runtime.YInt, Is.EqualTo(yInt));
            Assert.That(attacker.Runtime.ZInt, Is.EqualTo(zInt));
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            int objectType)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 1000; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = LF2States.Standing,
                    wait = 100,
                    next = id,
                    bodies = new List<BattleBodyBoxValue>
                    {
                        new BattleBodyBoxValue(-20, -20, 40, 40),
                    },
                });
            }
            var data = new LF2CharacterData
            {
                name = "B5Kind8AtomicProduction",
                type_sub = objectType,
                frames = frames,
            };
            var entity = new TypedCharacter(objectType) { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 100;
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
