#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5ReducedState2000AwayDampingEditorTests
    {
        [TestCase(20.0, 10.0, 4.0, true)]
        [TestCase(20.0, 10.0, 0.0, true)]
        [TestCase(20.0, 10.0, -4.0, false)]
        [TestCase(10.0, 20.0, -4.0, true)]
        [TestCase(10.0, 20.0, 0.0, true)]
        [TestCase(10.0, 20.0, 4.0, false)]
        [TestCase(10.0, 10.0, 4.0, false)]
        public void Predicate_UsesNativeStrictSideAndAwayDirection(
            double attackerX,
            double targetX,
            double attackerVx,
            bool expected)
        {
            MethodInfo method = typeof(BattleDamageWriter).GetMethod(
                "ShouldDampenNativeReducedState2000",
                BindingFlags.Static | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            bool actual = (bool)method.Invoke(
                null,
                new object[] { attackerX, targetX, attackerVx });

            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Actual_EqualLogicalXDoesNotDampen()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 1, 0, true);
            TypedCharacter target = CreateCharacter(world, 2, 1, false, Armor());
            SetMotion(attacker, 10.5, 4.0, 5.0);
            target.Runtime.SetPosition(10.5, 0.0, 0.0);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(5.0));
        }

        [Test]
        public void Actual_FractionalSameIntegerBucketMovingTowardDoesNotDampen()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 3, 0, true);
            TypedCharacter target = CreateCharacter(world, 4, 1, false, Armor());
            SetMotion(attacker, 10.75, -4.0, 5.0);
            target.Runtime.SetPosition(10.25, 0.0, 0.0);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(-4.0));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(5.0));
        }

        [Test]
        public void Actual_FractionalSameIntegerBucketMovingAwayDampens()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 5, 0, true);
            TypedCharacter target = CreateCharacter(world, 6, 1, false, Armor());
            SetMotion(attacker, 10.75, 4.0, 5.0);
            target.Runtime.SetPosition(10.25, 0.0, 0.0);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(1.6));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(2.0));
        }

        [Test]
        public void Actual_SeparatedMovingTowardDoesNotDampen()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 13, 0, true);
            TypedCharacter target = CreateCharacter(world, 14, 1, false, Armor());
            SetMotion(attacker, 20.5, -4.0, 5.0);
            target.Runtime.SetPosition(10.5, 0.0, 0.0);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit());

            Assert.That(applied, Is.True);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(-4.0));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(5.0));
        }

        [Test]
        public void HitPlan_MovingAwayDampens()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 7, 0, true);
            TypedCharacter target = CreateCharacter(world, 8, 1, false, Armor());
            SetMotion(attacker, 20.5, 4.0, 5.0);
            target.Runtime.SetPosition(10.5, 0.0, 0.0);

            object projection = ProjectWriterEffect(
                world, attacker, target, Hit());

            Assert.That(ReadDouble(projection, "AttackerVx"), Is.EqualTo(1.6));
            Assert.That(ReadDouble(projection, "AttackerVz"), Is.EqualTo(2.0));
        }

        [Test]
        public void HitPlan_MovingTowardDoesNotDampen()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 9, 0, true);
            TypedCharacter target = CreateCharacter(world, 10, 1, false, Armor());
            SetMotion(attacker, 20.5, -4.0, 5.0);
            target.Runtime.SetPosition(10.5, 0.0, 0.0);

            object projection = ProjectWriterEffect(
                world, attacker, target, Hit());

            Assert.That(ReadDouble(projection, "AttackerVx"), Is.EqualTo(-4.0));
            Assert.That(ReadDouble(projection, "AttackerVz"), Is.EqualTo(5.0));
        }

        [Test]
        public void HitPlan_EqualLogicalXDoesNotDampen()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 11, 0, true);
            TypedCharacter target = CreateCharacter(world, 12, 1, false, Armor());
            SetMotion(attacker, 10.5, 4.0, 5.0);
            target.Runtime.SetPosition(10.5, 0.0, 0.0);

            object projection = ProjectWriterEffect(
                world, attacker, target, Hit());

            Assert.That(ReadDouble(projection, "AttackerVx"), Is.EqualTo(4.0));
            Assert.That(ReadDouble(projection, "AttackerVz"), Is.EqualTo(5.0));
        }

        private static void SetMotion(
            LF2Entity entity,
            double x,
            double vx,
            double vz)
        {
            entity.Runtime.SetPosition(x, 0.0, 0.0);
            entity.Runtime.Vx = vx;
            entity.Runtime.Vz = vz;
        }

        private static object ProjectWriterEffect(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            FieldInfo planField = typeof(SimulationWorld).GetField(
                "battleEcsHitExecutionPlan",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(planField, Is.Not.Null);
            object plan = planField.GetValue(world);
            MethodInfo capture = plan.GetType().GetMethod(
                "CaptureWriterEffectSnapshot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(capture, Is.Not.Null);
            object projection = capture.Invoke(
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
                BattleHitCandidateDisposition.Damage,
                projection,
            };
            bool projected = (bool)project.Invoke(null, args);
            Assert.That(projected, Is.True);
            return args[4];
        }

        private static double ReadDouble(object value, string memberName)
        {
            FieldInfo field = value.GetType().GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.Public |
                BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, memberName);
            return (double)field.GetValue(value);
        }

        private static InteractionArea Hit()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 20,
                fall = 0,
                dvx = 0,
                dvy = 0,
                arest = 10,
                vrest = 0,
                effect = 0,
                bdefend = 0,
            };
        }

        private static LF2ArmorData Armor()
        {
            return new LF2ArmorData
            {
                type = 1,
                ratio = 0,
                decrease = 50,
                mp = 0,
                fall = -1,
                bdefend = -1,
                injury = -1,
                hp = 100,
                delay = -1,
            };
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot,
            bool state2000,
            params LF2ArmorData[] armors)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = id == 0 && state2000
                        ? LF2States.HeavyWeaponInSky
                        : LF2States.Standing,
                    wait = 100,
                    next = id,
                });
            }

            var data = new LF2CharacterData
            {
                name = "B5ReducedState2000AwayDamping",
                type_sub = objectId,
                armors = new List<LF2ArmorData>(armors),
                frames = frames,
            };
            var entity = new TypedCharacter
            {
                ObjectId = objectId,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            if (state2000)
                entity.Frame.D.state = LF2States.HeavyWeaponInSky;
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
            if (armors.Length == 1)
                entity.Runtime.RuntimeArmorHp118 = armors[0].hp;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
            return entity;
        }

        private sealed class TypedCharacter : LF2Character
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }
    }
}
#endif
