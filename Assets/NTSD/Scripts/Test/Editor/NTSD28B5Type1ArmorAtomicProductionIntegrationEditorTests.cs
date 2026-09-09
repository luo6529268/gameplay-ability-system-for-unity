#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;
using UnityEngine;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type1ArmorAtomicProductionIntegrationEditorTests
    {
        [Test]
        public void Route_OrdinaryDefensePrecedesType1Armor()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 1, 0);
            TypedCharacter target = CreateCharacter(world, 2, 1, Armor());
            attacker.SwitchDir("right");
            target.SwitchDir("left");
            target.ImmediateFrame(7);

            object route = ResolveRoute(world, attacker, target, Hit(20));

            AssertRoute(route, "ReducedDefense");
        }

        [Test]
        public void Route_FirstType1RequiresExactlyOneArmorRecord()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 3, 0);
            TypedCharacter target = CreateCharacter(
                world, 4, 1, Armor(), Armor());

            object route = ResolveRoute(world, attacker, target, Hit(20));

            AssertRoute(route, "Unsupported");
        }

        [Test]
        public void Route_Type1BypassAndMpFailureUseDistinctUnarmoredFallbacks()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 5, 0);
            LF2ArmorData armor = Armor();
            armor.effects.Add(20);
            TypedCharacter target = CreateCharacter(world, 6, 1, armor);

            object bypass = ResolveRoute(
                world,
                attacker,
                target,
                new InteractionArea { kind = 0, injury = 20, effect = 20 });
            AssertRoute(bypass, "UnarmoredType1Bypass");

            armor.effects.Clear();
            armor.mp = -7;
            target.Health.PP = 6;
            object unavailable = ResolveRoute(world, attacker, target, Hit(20));
            AssertRoute(unavailable, "UnarmoredType1ResourceFallback");
        }

        [Test]
        public void Route_EffectiveInjuryUsesDefinitionThenActiveModeWithTruncation()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 7, 0);
            TypedCharacter target = CreateCharacter(world, 8, 1, Armor());
            attacker.FrameCache.Wrapper.characterData.definition_attacking = 33;
            world.Runtime.NativeHitResourceRules.ActiveModeAttackingPercent1C = 250;

            object definitionRoute = ResolveRoute(
                world, attacker, target, Hit(10));
            Assert.That(ReadInt(definitionRoute, "EffectiveInjury"), Is.EqualTo(3));

            attacker.FrameCache.Wrapper.characterData.definition_attacking = 0;
            object modeRoute = ResolveRoute(world, attacker, target, Hit(10));
            Assert.That(ReadInt(modeRoute, "EffectiveInjury"), Is.EqualTo(25));
        }

        [Test]
        public void Actual_SelectedHpArmorWritesReducedHpArmorAndHpConsumption()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 9, 0);
            LF2ArmorData armor = Armor();
            armor.hp = 100;
            TypedCharacter target = CreateCharacter(world, 10, 1, armor);
            target.Runtime.RuntimeArmorHp118 = 100;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit(20));

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(490));
            Assert.That(target.Health.HPBound, Is.EqualTo(497));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.EqualTo(80));
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.EqualTo(10));
            Assert.That(target.Runtime.InputMpConsumedTotal350, Is.Zero);
            Assert.That(target.HitCounters.HitStateCount, Is.Zero);
        }

        [Test]
        public void Actual_SelectedMpArmorWritesPpAndMpConsumptionOnly()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 11, 0);
            LF2ArmorData armor = Armor();
            armor.mp = -7;
            TypedCharacter target = CreateCharacter(world, 12, 1, armor);
            target.Health.PP = 7;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit(20));

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(500));
            Assert.That(target.Health.PP, Is.Zero);
            Assert.That(target.Runtime.InputHpConsumedTotal34C, Is.Zero);
            Assert.That(target.Runtime.InputMpConsumedTotal350, Is.EqualTo(7));
        }

        [Test]
        public void Actual_BrokenArmorUsesUnarmoredDamageThenActionAndZero()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 13, 0);
            LF2ArmorData armor = Armor();
            armor.hp = 100;
            armor.action = 90;
            TypedCharacter target = CreateCharacter(world, 14, 1, armor);
            target.Runtime.RuntimeArmorHp118 = 20;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit(20));

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.Zero);
            Assert.That(target.Frame.N, Is.EqualTo(90));
        }

        [Test]
        public void Actual_InsufficientMpFallsBackWithoutConsumingMp()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 15, 0);
            LF2ArmorData armor = Armor();
            armor.mp = -7;
            TypedCharacter target = CreateCharacter(world, 16, 1, armor);
            target.Health.PP = 6;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world, attacker, target, target.HitCounters, Hit(20));

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(480));
            Assert.That(target.Health.PP, Is.EqualTo(6));
            Assert.That(target.Runtime.InputMpConsumedTotal350, Is.Zero);
        }

        [Test]
        public void ProductionCharacterEntryUsesSameType1Route()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 17, 0);
            LF2ArmorData armor = Armor();
            armor.hp = 100;
            TypedCharacter target = CreateCharacter(world, 18, 1, armor);
            target.Runtime.RuntimeArmorHp118 = 100;

            bool applied = world.DamageWriter.TryApplyCurrentDatTargetHit(
                world, attacker, target, Hit(20), Vector3.zero);

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(490));
            Assert.That(target.Runtime.RuntimeArmorHp118, Is.EqualTo(80));
        }

        [Test]
        public void HitPlanProjection_ProjectsSelectedArmorRuntimeWrites()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 19, 0);
            LF2ArmorData armor = Armor();
            armor.hp = 100;
            TypedCharacter target = CreateCharacter(world, 20, 1, armor);
            target.Runtime.RuntimeArmorHp118 = 100;
            InteractionArea interaction = Hit(20);

            object projection = ProjectWriterEffect(
                world, attacker, target, interaction);

            Assert.That(ReadInt(projection, "TargetHp"), Is.EqualTo(490));
            Assert.That(ReadInt(projection, "TargetRuntimeArmorHp"), Is.EqualTo(80));
            Assert.That(ReadInt(projection, "TargetInputHpConsumedTotal"),
                Is.EqualTo(10));
            Assert.That(ReadInt(projection, "TargetInputMpConsumedTotal"), Is.Zero);
        }

        private static object ResolveRoute(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea interaction)
        {
            Type resolver = Type.GetType(
                "NTSD.Simulation.Ecs.BattleOrdinaryCharacterDamageRouteResolver, Assembly-CSharp");
            Assert.That(resolver, Is.Not.Null);
            MethodInfo method = resolver.GetMethod(
                "Resolve",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            return method.Invoke(
                null,
                new object[] { world, attacker, target, interaction });
        }

        private static void AssertRoute(object route, string expectedKind)
        {
            PropertyInfo property = route.GetType().GetProperty("Kind");
            Assert.That(property, Is.Not.Null);
            Assert.That(property.GetValue(route).ToString(), Is.EqualTo(expectedKind));
        }

        private static int ReadInt(object value, string memberName)
        {
            Type type = value.GetType();
            PropertyInfo property = type.GetProperty(memberName);
            if (property != null)
                return (int)property.GetValue(value);
            FieldInfo field = type.GetField(
                memberName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, memberName);
            return (int)field.GetValue(value);
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

        private static InteractionArea Hit(int injury)
        {
            return new InteractionArea
            {
                kind = 0,
                injury = injury,
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
                hp = 0,
                delay = -1,
            };
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot,
            params LF2ArmorData[] armors)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = id == 7 ? LF2States.Defending : LF2States.Standing,
                    wait = 100,
                    next = id,
                });
            }

            var data = new LF2CharacterData
            {
                name = "B5Type1ArmorAtomicProduction",
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
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Health.PP = 500;
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
