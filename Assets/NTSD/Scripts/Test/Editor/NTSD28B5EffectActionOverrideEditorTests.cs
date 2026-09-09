#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;
using System.Reflection;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.DatParser;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5EffectActionOverrideEditorTests
    {
        [Test]
        public void DefinitionConverter_PreservesNativePropertyCarrier()
        {
            var datFile = new Lf2DatFile { Bmp = new Lf2BmpSection() };
            datFile.Bmp.AddProperty(new Lf2DatProperty("property", "3"));
            var data = new LF2CharacterData();

            Lf2DatConverter.ApplyNativeInputDefinitionData(datFile, data);

            FieldInfo propertyField = typeof(LF2CharacterData).GetField("property");
            Assert.That(propertyField, Is.Not.Null);
            Assert.That(propertyField.GetValue(data), Is.EqualTo(3));
        }

        [Test]
        public void FrameConverter_PreservesOnlyFirstBodyKindForEffectSuppression()
        {
            var frame = new Lf2FrameBlock { FrameIndex = 10 };
            var first = new Lf2DatSubBlock { Name = "bdy" };
            first.AddProperty(new Lf2DatProperty("kind", "50"));
            first.AddProperty(new Lf2DatProperty("x", "1"));
            var second = new Lf2DatSubBlock { Name = "bdy" };
            second.AddProperty(new Lf2DatProperty("kind", "0"));
            second.AddProperty(new Lf2DatProperty("x", "2"));
            frame.SubBlocks.Add(first);
            frame.SubBlocks.Add(second);

            LF2FrameData converted = Lf2DatConverter.ConvertToFrameData(frame);

            Assert.That(converted.primaryBodyKindForEffectSuppression, Is.EqualTo(50));
            Assert.That(converted.bodies.Count, Is.EqualTo(2));
            Assert.That(converted.bodies[0].X, Is.EqualTo(1));
            Assert.That(converted.bodies[1].X, Is.EqualTo(2));
        }

        [TestCase(8, LF2ObjectType.Character, true)]
        [TestCase(8, LF2ObjectType.SpecialAttack, false)]
        [TestCase(9, LF2ObjectType.SpecialAttack, true)]
        [TestCase(9, LF2ObjectType.Character, false)]
        [TestCase(10, LF2ObjectType.Character, true)]
        [TestCase(10, LF2ObjectType.SpecialAttack, true)]
        [TestCase(11, LF2ObjectType.LightWeapon, true)]
        [TestCase(11, LF2ObjectType.Character, false)]
        [TestCase(12, LF2ObjectType.Other, true)]
        [TestCase(13, LF2ObjectType.Character, true)]
        [TestCase(14, LF2ObjectType.SpecialAttack, true)]
        [TestCase(15, LF2ObjectType.Character, true)]
        [TestCase(16, LF2ObjectType.HeavyWeapon, true)]
        public void UnarmoredWriter_UsesNativeEffectTargetTypeMatrix(
            int effect,
            LF2ObjectType targetType,
            bool shouldApply)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8470, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8471, 1, targetType);

            bool resolved = ApplyUnarmoredHit(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    effect = effect,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                    caughtact = new[] { 232 },
                });

            Assert.That(resolved, Is.True);
            Assert.That(target.Frame.N == 232, Is.EqualTo(shouldApply));
        }

        [Test]
        public void Override_WritesBothPositiveActionsAndPreservesFrameCounters()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8472, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8473, 1, LF2ObjectType.Character);
            attacker.AttackingCounter = 7;
            target.AttackingCounter = 9;

            ApplyUnarmoredHit(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    effect = 8,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                    catchingact = new[] { 230 },
                    caughtact = new[] { 232 },
                });

            Assert.That(attacker.Frame.N, Is.EqualTo(230));
            Assert.That(target.Frame.N, Is.EqualTo(232));
            Assert.That(attacker.AttackingCounter, Is.EqualTo(7));
            Assert.That(target.AttackingCounter, Is.EqualTo(9));
        }

        [Test]
        public void Override_RequiresPickedActPreviousStateMatch()
        {
            var acceptedWorld = new SimulationWorld();
            TypedCharacter acceptedAttacker = CreateEntity(
                acceptedWorld, 8474, 0, LF2ObjectType.Character);
            TypedCharacter acceptedTarget = CreateEntity(
                acceptedWorld, 8475, 1, LF2ObjectType.Character);
            acceptedTarget.Frame.Prev = 5;

            ApplyUnarmoredHit(
                acceptedWorld,
                acceptedAttacker,
                acceptedTarget,
                Effect8(caughtAction: 232, pickedState: 7));

            Assert.That(acceptedTarget.Frame.N, Is.EqualTo(232));

            var rejectedWorld = new SimulationWorld();
            TypedCharacter rejectedAttacker = CreateEntity(
                rejectedWorld, 8476, 0, LF2ObjectType.Character);
            TypedCharacter rejectedTarget = CreateEntity(
                rejectedWorld, 8477, 1, LF2ObjectType.Character);
            rejectedTarget.Frame.Prev = 5;

            ApplyUnarmoredHit(
                rejectedWorld,
                rejectedAttacker,
                rejectedTarget,
                Effect8(caughtAction: 232, pickedState: 8));

            Assert.That(rejectedTarget.Frame.N, Is.Not.EqualTo(232));
        }

        [TestCase(50, 0)]
        [TestCase(52, 0)]
        [TestCase(0, 602)]
        [TestCase(0, 603)]
        public void Override_ActionLatchBodyOrStateSuppressesBothActions(
            int firstBodyKind,
            int latchState)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8478, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8479, 1, LF2ObjectType.Character);
            LF2FrameData latch = target.GetFrameDataById(10);
            latch.state = latchState;
            if (firstBodyKind != 0)
            {
                latch.primaryBodyKindForEffectSuppression = firstBodyKind;
                latch.bodies.Add(new BodyBox { kind = firstBodyKind });
            }
            target.Trans.SyncWaitCounterFrame(10);

            ApplyUnarmoredHit(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    effect = 8,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                    catchingact = new[] { 230 },
                    caughtact = new[] { 232 },
                });

            Assert.That(attacker.Frame.N, Is.Not.EqualTo(230));
            Assert.That(target.Frame.N, Is.Not.EqualTo(232));
        }

        [Test]
        public void Override_DefinitionPropertyAndNegativeCaughtActionSuppressBothActions()
        {
            var propertyWorld = new SimulationWorld();
            TypedCharacter propertyAttacker = CreateEntity(
                propertyWorld, 8480, 0, LF2ObjectType.Character);
            TypedCharacter propertyTarget = CreateEntity(
                propertyWorld, 8481, 1, LF2ObjectType.Character);
            FieldInfo propertyField = typeof(LF2CharacterData).GetField("property");
            Assert.That(propertyField, Is.Not.Null);
            propertyField.SetValue(
                propertyTarget.FrameCache.Wrapper.characterData,
                2);

            ApplyUnarmoredHit(
                propertyWorld,
                propertyAttacker,
                propertyTarget,
                new InteractionArea
                {
                    kind = 0,
                    effect = 8,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                    catchingact = new[] { 230 },
                    caughtact = new[] { 232 },
                });

            Assert.That(propertyAttacker.Frame.N, Is.Not.EqualTo(230));
            Assert.That(propertyTarget.Frame.N, Is.Not.EqualTo(232));

            var negativeWorld = new SimulationWorld();
            TypedCharacter negativeAttacker = CreateEntity(
                negativeWorld, 8482, 0, LF2ObjectType.Character);
            TypedCharacter negativeTarget = CreateEntity(
                negativeWorld, 8483, 1, LF2ObjectType.Character);
            ApplyUnarmoredHit(
                negativeWorld,
                negativeAttacker,
                negativeTarget,
                new InteractionArea
                {
                    kind = 0,
                    effect = 8,
                    injury = 1,
                    fall = 1,
                    dvx = 1,
                    catchingact = new[] { 230 },
                    caughtact = new[] { -2 },
                });

            Assert.That(negativeAttacker.Frame.N, Is.Not.EqualTo(230));
        }

        [Test]
        public void Override_DoesNotReplaceTerminalTargetReaction()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(world, 8484, 0, LF2ObjectType.Character);
            TypedCharacter target = CreateEntity(world, 8485, 1, LF2ObjectType.Character);
            target.Health.HP = 1;

            ApplyUnarmoredHit(
                world,
                attacker,
                target,
                new InteractionArea
                {
                    kind = 0,
                    effect = 8,
                    injury = 10,
                    fall = 1,
                    dvx = 1,
                    catchingact = new[] { 230 },
                    caughtact = new[] { 232 },
                });

            Assert.That(target.Health.HP, Is.LessThanOrEqualTo(0));
            Assert.That(target.Frame.N, Is.Not.EqualTo(232));
            Assert.That(attacker.Frame.N, Is.EqualTo(230));
        }

        private static InteractionArea Effect8(int caughtAction, int pickedState)
        {
            return new InteractionArea
            {
                kind = 0,
                effect = 8,
                injury = 1,
                fall = 1,
                dvx = 1,
                pickedact = pickedState,
                caughtact = new[] { caughtAction },
            };
        }

        private static bool ApplyUnarmoredHit(
            SimulationWorld world,
            LF2Entity attacker,
            LF2Entity target,
            InteractionArea itr)
        {
            int targetType = target.GetCurrentDataObjectTypeForSimulation();
            if (targetType == (int)LF2ObjectType.Character)
            {
                return world.DamageWriter.ApplyStandardCharacterDamage(
                    world,
                    attacker,
                    target,
                    ((LF2Character)target).HitCounters,
                    itr);
            }
            if (targetType == (int)LF2ObjectType.LightWeapon ||
                targetType == (int)LF2ObjectType.HeavyWeapon ||
                targetType == (int)LF2ObjectType.ThrowWeapon ||
                targetType == (int)LF2ObjectType.Drink)
            {
                return world.DamageWriter.ApplyWeaponDamage(
                    world,
                    attacker,
                    target,
                    itr);
            }

            return world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                itr);
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
                    state = id == 5 ? 7 : 0,
                    wait = 100,
                    next = id,
                });
            }

            var entity = new TypedCharacter(objectType)
            {
                ObjectId = objectId,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5EffectActionOverride",
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
