#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5OrdinaryDefenseReducedHitProductionIntegrationEditorTests
    {
        [Test]
        public void ProductionSelector_UsesCurrentNativeDefenseTruthTable()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 1, 0);
            TypedCharacter target = CreateCharacter(world, 37, 1);
            var itr = new InteractionArea
            {
                kind = 0,
                effect = 0,
                dvx = 5,
            };
            attacker.SwitchDir("right");
            target.SwitchDir("right");

            target.ImmediateFrame(0);
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.False,
                "legacy OID37 standing heuristic must be retired");

            target.ImmediateFrame(7);
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.False);
            target.SwitchDir("left");
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.True);

            target.ImmediateFrame(0);
            target.Runtime.PrevFrame2 = 7;
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.False,
                "Prev2 defense must not replace current state");

            target.ImmediateFrame(70);
            target.SwitchDir("right");
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.True);
            target.ImmediateFrame(7);
            itr.spark = 1;
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.True);
            itr.spark = 0;
            itr.dbdefend = 1;
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.True);
            itr.dbdefend = 0;
            attacker.FrameCache.Wrapper.characterData.type_sub = 822;
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.True);
            itr.effect = 61;
            Assert.That(
                LF2AlternateDamageResolver.ShouldUseAlternateHurt(
                    attacker, target, itr),
                Is.False);
        }

        [Test]
        public void ProductionReducedDamage_UsesRawDivideThenTargetScaleWithoutWeakness()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 2, 0);
            TypedCharacter target = CreateCharacter(world, 3, 1);
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.FallDamageDiv = 200;
            target.Health.HP = 500;
            target.Health.HPBound = 500;
            target.KillCount = 0;

            world.DamageWriter.ApplyAlternateDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 49,
                    dvx = 5,
                    arest = 10,
                });

            Assert.That(target.Health.HP, Is.EqualTo(484));
            Assert.That(target.Health.HPBound, Is.EqualTo(495));
            Assert.That(target.ComboCountVic, Is.EqualTo(16));
        }

        [Test]
        public void ProductionReducedRest_UsesDefinitionEffectsAndWorldReduction()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 4, 0);
            TypedCharacter target = CreateCharacter(world, 5, 1);
            world.Runtime.NativeStandardHitRest.SetTimingReduction4A9FF4(2);
            attacker.FrameDelay = -9;
            target.FrameDelay = 7;

            world.DamageWriter.ApplyAlternateDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 10,
                    dvx = 5,
                    arest = 10,
                });

            Assert.That(attacker.FrameDelay, Is.EqualTo(1));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));

            attacker.FrameCache.Wrapper.characterData.definition_effect = 3;
            target.FrameCache.Wrapper.characterData.definition_effect = 4;
            attacker.FrameDelay = -9;
            target.FrameDelay = 7;
            world.DamageWriter.ApplyAlternateDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    injury = 10,
                    dvx = 5,
                    arest = 10,
                });

            Assert.That(attacker.FrameDelay, Is.EqualTo(-9));
            Assert.That(target.FrameDelay, Is.EqualTo(7));
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                int state = id == 7
                    ? LF2States.Defending
                    : id == 70 ? 70 : id == 75 ? 75 : LF2States.Standing;
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = state,
                    wait = 100,
                    next = id,
                });
            }

            var data = new LF2CharacterData
            {
                name = "B5OrdinaryDefenseReducedIntegration",
                type_sub = objectId,
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
