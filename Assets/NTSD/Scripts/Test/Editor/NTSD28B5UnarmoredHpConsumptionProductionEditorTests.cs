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
    public sealed class NTSD28B5UnarmoredHpConsumptionProductionEditorTests
    {
        [Test]
        public void Type3ChildToType0Target_ResourceRewardGoesToRootOwner()
        {
            var world = new SimulationWorld();
            TypedCharacter root = CreateEntity(world, 8670, 0, 0);
            TypedCharacter target = CreateEntity(world, 8671, 1, 0);
            TypedCharacter child = CreateEntity(
                world, 8672, 2, (int)LF2ObjectType.SpecialAttack);
            child.Runtime.OwnerSlotIndex = root.Runtime.SlotIndex;
            root.Health.PP = 400;
            target.Health.PP = 300;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                child,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 35,
                    fall = 1,
                    dvx = 1,
                    arest = 10,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(465));
            Assert.That(root.Health.PP, Is.EqualTo(426));
            Assert.That(target.Health.PP, Is.EqualTo(326));
            Assert.That(child.Health.PP, Is.EqualTo(500));
        }

        [Test]
        public void Type3ChildToType0ArmorTarget_ReducedHitTransfersCurrentMp()
        {
            var world = new SimulationWorld();
            TypedCharacter root = CreateEntity(world, 8673, 0, 0);
            LF2ArmorData armor = Armor();
            TypedCharacter target = CreateEntity(world, 8674, 1, 0, armor);
            TypedCharacter child = CreateEntity(
                world, 8675, 2, (int)LF2ObjectType.SpecialAttack);
            child.Runtime.OwnerSlotIndex = root.Runtime.SlotIndex;
            root.Health.PP = 400;
            target.Health.PP = 300;

            world.DamageWriter.ApplyAlternateDamage(
                world,
                child,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 35,
                    fall = 1,
                    dvx = 1,
                    arest = 10,
                },
                armor);

            Assert.That(target.Health.HP, Is.LessThan(500));
            Assert.That(root.Health.PP, Is.EqualTo(426));
            Assert.That(target.Health.PP, Is.EqualTo(326));
            Assert.That(child.Health.PP, Is.EqualTo(500));
        }

        [Test]
        public void Type3ChildToType0ArmorTarget_OddGroundDvxKeepsFraction()
        {
            var world = new SimulationWorld();
            TypedCharacter root = CreateEntity(world, 8676, 0, 0);
            LF2ArmorData armor = Armor();
            TypedCharacter target = CreateEntity(world, 8677, 1, 0, armor);
            TypedCharacter child = CreateEntity(
                world, 8678, 2, (int)LF2ObjectType.SpecialAttack);
            child.Runtime.OwnerSlotIndex = root.Runtime.SlotIndex;
            double pendingXBefore = target.KnockbackVx;

            world.DamageWriter.ApplyAlternateDamage(
                world,
                child,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 35,
                    fall = 1,
                    dvx = 7,
                    arest = 10,
                },
                armor);

            Assert.That(target.Health.HP, Is.LessThan(500));
            Assert.That(target.KnockbackVx,
                Is.EqualTo(pendingXBefore + 3.5).Within(0.0000001),
                "Ground reduced hit retains the half-unit before impulse averaging.");
        }

        [Test]
        public void Actual_CharacterAccumulatesEffectiveHpDamage()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8600, 0, 0);
            TypedCharacter target = CreateEntity(world, 8601, 1, 0);
            ConfigureScaledInjury(attacker, target);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                Hit());

            Assert.That(applied, Is.True);
            AssertEffectiveConsumption(target);
        }

        [TestCase((int)LF2ObjectType.LightWeapon)]
        [TestCase((int)LF2ObjectType.HeavyWeapon)]
        [TestCase((int)LF2ObjectType.ThrowWeapon)]
        public void Actual_Type1_2_4AccumulateEffectiveHpDamage(int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8610, 0, 0);
            TypedCharacter target = CreateEntity(world, 8611, 1, targetType);
            ConfigureScaledInjury(attacker, target);
            target.Runtime.WeaponFlightCounter = 100;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                Hit());

            Assert.That(applied, Is.True);
            AssertEffectiveConsumption(target);
        }

        [TestCase((int)LF2ObjectType.SpecialAttack)]
        [TestCase((int)LF2ObjectType.Other)]
        public void Actual_Type3_5AccumulateEffectiveHpDamage(int targetType)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8620, 0, 0);
            TypedCharacter target = CreateEntity(world, 8621, 1, targetType);
            ConfigureScaledInjury(attacker, target);

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                Hit());

            Assert.That(applied, Is.True);
            AssertEffectiveConsumption(target);
        }

        [Test]
        public void Actual_Type6SkipsHpDamageAndConsumption()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8630, 0, 0);
            TypedCharacter target = CreateEntity(
                world,
                8631,
                1,
                (int)LF2ObjectType.Drink);
            ConfigureScaledInjury(attacker, target);
            target.Runtime.WeaponFlightCounter = 100;

            bool applied = world.DamageWriter.ApplyWeaponDamage(
                world,
                attacker,
                target,
                Hit());

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(500));
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.EqualTo(100));
        }

        [Test]
        public void Actual_BrokenType1ArmorFallbackAccumulatesOnce()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8640, 0, 0);
            LF2ArmorData armor = Armor();
            TypedCharacter target = CreateEntity(world, 8641, 1, 0, armor);
            target.Runtime.RuntimeArmorHp118 = 10;
            target.Runtime.InputHpConsumedTotal34C = 100;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 20,
                    arest = 10,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.EqualTo(120));
        }

        [Test]
        public void HitPlan_CharacterProjectionAccumulatesEffectiveHpDamage()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8650, 0, 0);
            TypedCharacter target = CreateEntity(world, 8651, 1, 0);
            ConfigureScaledInjury(attacker, target);

            object projection = ProjectWriterEffect(
                world,
                attacker,
                target,
                Hit());

            Assert.That(
                ReadInt(projection, "TargetInputHpConsumedTotal"),
                Is.EqualTo(122));
        }

        [TestCase("ProjectWeaponNormalVitalAndStatWrites", (int)LF2ObjectType.LightWeapon, 122)]
        [TestCase("ProjectWeaponNormalVitalAndStatWrites", (int)LF2ObjectType.HeavyWeapon, 122)]
        [TestCase("ProjectWeaponNormalVitalAndStatWrites", (int)LF2ObjectType.ThrowWeapon, 122)]
        [TestCase("ProjectWeaponNormalVitalAndStatWrites", (int)LF2ObjectType.Drink, 100)]
        [TestCase("ProjectType3NormalVitalAndStatWrites", (int)LF2ObjectType.SpecialAttack, 122)]
        [TestCase("ProjectType3NormalVitalAndStatWrites", (int)LF2ObjectType.Other, 122)]
        public void HitPlan_NormalVitalProjectionHelpersMirrorAuthorityScope(
            string helperName,
            int targetType,
            int expectedConsumption)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8660, 0, 0);
            TypedCharacter target = CreateEntity(world, 8661, 1, targetType);
            ConfigureScaledInjury(attacker, target);
            object projection = CaptureWriterEffect(world, attacker, target);

            MethodInfo helper = GetHitPlanType().GetMethod(
                helperName,
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(helper, Is.Not.Null, helperName);
            object[] args = { attacker, target, 11, projection };
            helper.Invoke(null, args);

            Assert.That(
                ReadInt(args[3], "TargetInputHpConsumedTotal"),
                Is.EqualTo(expectedConsumption));
        }

        private static void ConfigureScaledInjury(
            TypedCharacter attacker,
            TypedCharacter target)
        {
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.Runtime.InputHpConsumedTotal34C = 100;
        }

        private static void AssertEffectiveConsumption(TypedCharacter target)
        {
            Assert.That(target.Health.HP, Is.EqualTo(478));
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.EqualTo(122),
                "Authority accumulates effective HP damage, not raw ITR injury.");
        }

        private static object ProjectWriterEffect(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            object plan = GetHitPlan(world);
            object projection = CaptureWriterEffect(world, attacker, target);
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

        private static object CaptureWriterEffect(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target)
        {
            object plan = GetHitPlan(world);
            MethodInfo capture = plan.GetType().GetMethod(
                "CaptureWriterEffectSnapshot",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(capture, Is.Not.Null);
            return capture.Invoke(plan, new object[] { attacker, target, -1 });
        }

        private static object GetHitPlan(SimulationWorld world)
        {
            FieldInfo field = typeof(SimulationWorld).GetField(
                "battleEcsHitExecutionPlan",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            return field.GetValue(world);
        }

        private static Type GetHitPlanType()
        {
            return Type.GetType(
                "NTSD.Simulation.Ecs.BattleEcsHitExecutionPlan, Assembly-CSharp");
        }

        private static int ReadInt(object value, string memberName)
        {
            FieldInfo field = value.GetType().GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, memberName);
            return (int)field.GetValue(value);
        }

        private static InteractionArea Hit()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 11,
                fall = 1,
                dvx = 1,
                arest = 10,
            };
        }

        private static LF2ArmorData Armor()
        {
            return new LF2ArmorData
            {
                type = 1,
                decrease = 50,
                hp = 100,
                action = 90,
                fall = -1,
                bdefend = -1,
                injury = -1,
                delay = -1,
            };
        }

        private static TypedCharacter CreateEntity(
            SimulationWorld world,
            int objectId,
            int slot,
            int objectType,
            params LF2ArmorData[] armors)
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

            var data = new LF2CharacterData
            {
                name = "B5UnarmoredHpConsumption",
                type_sub = objectType,
                armors = new List<LF2ArmorData>(armors),
                frames = frames,
            };
            var entity = new TypedCharacter(objectType) { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(objectId, data));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
            entity.Unk344 = 1;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
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
