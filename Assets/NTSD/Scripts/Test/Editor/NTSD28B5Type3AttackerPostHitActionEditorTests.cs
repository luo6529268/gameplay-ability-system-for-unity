#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type3AttackerPostHitActionEditorTests
    {
        [Test]
        public void FrameConverter_PreservesFrameLevelCoverSeparately()
        {
            var block = new Lf2FrameBlock { FrameIndex = 7 };
            block.AddProperty(new Lf2DatProperty("cover", "3"));

            LF2FrameData frame = Lf2DatConverter.ConvertToFrameData(block);
            FieldInfo field = typeof(LF2FrameData).GetField("cover");

            Assert.That(field, Is.Not.Null);
            Assert.That(field.GetValue(frame), Is.EqualTo(3));
        }

        [TestCase("standard")]
        [TestCase("alternate")]
        [TestCase("special")]
        [TestCase("weapon")]
        public void State3000_UsesCurrentHitFjAndSelectedFrameDvx(string route)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world,
                8500,
                0,
                LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world,
                8501,
                1,
                route == "special"
                    ? LF2ObjectType.SpecialAttack
                    : route == "weapon"
                        ? LF2ObjectType.LightWeapon
                    : LF2ObjectType.Character);
            LF2FrameData current = attacker.GetFrameDataById(0);
            current.state = LF2States.ProjectileFlying;
            current.hit_Fj = 77;
            attacker.GetFrameDataById(77).dvx = 123;
            attacker.GetFrameDataById(77).dvz = -456;
            attacker.GetFrameDataById(10).dvx = 10;
            attacker.GetFrameDataById(10).dvz = 7;
            attacker.ImmediateFrame(0);
            attacker.Runtime.Vx = 9.0;
            attacker.Runtime.Vz = 11.0;
            attacker.AttackingCounter = 8;

            Apply(route, world, attacker, target);

            Assert.That(attacker.Frame.N, Is.EqualTo(77));
            Assert.That(attacker.Runtime.Frame, Is.EqualTo(77));
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(attacker.Runtime.Vx, Is.Zero);
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(123.0));
        }

        [Test]
        public void State3000_ZeroHitFjFallsBackToTenAndUsesDvx()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8502, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 8503, 1, LF2ObjectType.Character);
            attacker.GetFrameDataById(0).state = LF2States.ProjectileFlying;
            attacker.GetFrameDataById(0).hit_Fj = 0;
            attacker.GetFrameDataById(10).dvx = 550;
            attacker.GetFrameDataById(10).dvz = 7;
            attacker.ImmediateFrame(0);

            Apply("standard", world, attacker, target);

            Assert.That(attacker.Frame.N, Is.EqualTo(10));
            Assert.That(attacker.Runtime.Vz, Is.EqualTo(550.0));
        }

        [Test]
        public void Resolver_TreatsOnlyZeroHitFjAsFallback()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8506, 0, LF2ObjectType.SpecialAttack);
            attacker.GetFrameDataById(0).state = LF2States.ProjectileFlying;
            attacker.GetFrameDataById(0).hit_Fj = -1;
            attacker.ImmediateFrame(0);

            BattleDamageWriter.NativeType3AttackerPostHitActionDecision decision =
                BattleDamageWriter.ResolveNativeType3AttackerPostHitAction(attacker);

            Assert.That(decision.Applies, Is.True);
            Assert.That(decision.Action, Is.EqualTo(-1));
            Assert.That(decision.HasSelectedFrame, Is.False);
        }

        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(1, false)]
        [TestCase(0, false)]
        public void State3007_RequiresFrameLevelCoverTwoOrThree(
            int cover,
            bool expectedApplied)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 8504, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 8505, 1, LF2ObjectType.Character);
            LF2FrameData current = attacker.GetFrameDataById(0);
            current.state = 3007;
            current.hit_Fj = 78;
            SetFrameCover(current, cover);
            attacker.GetFrameDataById(78).dvx = 321;
            attacker.ImmediateFrame(0);
            attacker.Runtime.Vx = 9.0;
            attacker.Runtime.Vz = 11.0;
            attacker.AttackingCounter = 8;

            Apply("standard", world, attacker, target);

            if (expectedApplied)
            {
                Assert.That(attacker.Frame.N, Is.EqualTo(78));
                Assert.That(attacker.AttackingCounter, Is.Zero);
                Assert.That(attacker.Runtime.Vx, Is.Zero);
                Assert.That(attacker.Runtime.Vz, Is.EqualTo(321.0));
            }
            else
            {
                Assert.That(attacker.Frame.N, Is.EqualTo(0));
                Assert.That(attacker.AttackingCounter, Is.EqualTo(8));
                Assert.That(attacker.Runtime.Vx, Is.EqualTo(9.0));
                Assert.That(attacker.Runtime.Vz, Is.EqualTo(11.0));
            }
        }

        private static void Apply(
            string route,
            SimulationWorld world,
            TypedCharacter attacker,
            TypedCharacter target)
        {
            var itr = new InteractionArea
            {
                kind = 0,
                effect = 0,
                injury = 1,
                fall = 1,
                dvx = 1,
                arest = 4,
            };

            if (route == "weapon")
            {
                Assert.That(
                    world.DamageWriter.ApplyWeaponDamage(
                        world,
                        attacker,
                        target,
                        itr),
                    Is.True);
            }
            else if (route == "alternate")
            {
                world.DamageWriter.ApplyAlternateDamage(
                    world,
                    attacker,
                    target,
                    target.HitCounters,
                    itr);
            }
            else if (route == "special")
            {
                Assert.That(
                    world.DamageWriter.ApplySpecialAttackDamage(
                        world,
                        attacker,
                        target,
                        itr),
                    Is.True);
            }
            else
            {
                Assert.That(
                    world.DamageWriter.ApplyStandardCharacterDamage(
                        world,
                        attacker,
                        target,
                        target.HitCounters,
                        itr),
                    Is.True);
            }
        }

        private static void SetFrameCover(LF2FrameData frame, int value)
        {
            FieldInfo field = typeof(LF2FrameData).GetField("cover");
            Assert.That(field, Is.Not.Null);
            field.SetValue(frame, value);
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
                    name = "B5Type3AttackerPostHitAction",
                    type_sub = objectId,
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            entity.Health.HP = 500;
            entity.Health.HPBound = 500;
            entity.Unk344 = 1;
            world.Register(entity);
            entity.RelationTeam = slot + 1;
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
