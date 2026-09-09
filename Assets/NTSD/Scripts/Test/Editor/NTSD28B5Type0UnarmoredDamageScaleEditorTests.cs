#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type0UnarmoredDamageScaleEditorTests
    {
        [TestCase(11, 0, 0, 11)]
        [TestCase(11, -50, 0, 11)]
        [TestCase(11, 50, 0, 22)]
        [TestCase(11, 25, 1, 22)]
        [TestCase(-11, 25, 1, -22)]
        public void PureResolver_AppliesTargetScaleThenAttackerWeakness(
            int injury,
            int targetScale,
            int attackerWeakTimer,
            int expected)
        {
            Assert.That(
                BattleDamageWriter.ResolveNativeUnarmoredHpInjury(
                    injury,
                    targetScale,
                    attackerWeakTimer),
                Is.EqualTo(expected));
        }

        [Test]
        public void PureResolver_PreservesNativeLow32BitMultiplyAndSignedDivision()
        {
            Assert.That(
                BattleDamageWriter.ResolveNativeUnarmoredHpInjury(
                    int.MaxValue,
                    3,
                    1),
                Is.EqualTo(-16));
        }

        [Test]
        public void Type0Production_UsesEffectiveInjuryButRawDisplaySteps()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 8400, 0);
            TypedCharacter target = CreateCharacter(world, 8401, 1);
            attacker.Runtime.WeakTimer12C = 1;
            target.Runtime.IncomingDamageScale340 = 25;
            target.Health.HP = 500;
            target.Health.HPBound = 500;
            target.ComboCountVic = 0;
            target.Unk344 = 1;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 11,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(478));
            Assert.That(target.Health.HPBound, Is.EqualTo(493));
            Assert.That(target.ComboCountVic, Is.EqualTo(22));
            Assert.That(world.DamageStats[1], Is.EqualTo(22));
            Assert.That(target.Runtime.DisplayScoreStep1F4, Is.EqualTo(1));
            Assert.That(target.Runtime.DisplayDamageStep1FC, Is.EqualTo(1));
            Assert.That(target.Runtime.DisplayCurrentHpStep204, Is.EqualTo(1));
            Assert.That(target.Runtime.DisplayEffectiveMaxHpStep20C, Is.Zero);
        }

        [Test]
        public void Type0Production_EffectiveInjuryControlsLethalAndCreditCounts()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateCharacter(world, 8402, 0);
            TypedCharacter target = CreateCharacter(world, 8403, 1);
            target.Runtime.IncomingDamageScale340 = 50;
            target.Health.HP = 15;
            target.Health.HPBound = 30;
            target.KillCount = -1;
            target.Unk344 = 1;

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                target,
                target.HitCounters,
                new InteractionArea
                {
                    kind = 0,
                    fall = 1,
                    injury = 10,
                    dvx = 1,
                });

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(-5));
            Assert.That(world.KillStats[1], Is.EqualTo(1));
        }

        [Test]
        public void WarmPureResolver_DoesNotAllocate()
        {
            _ = BattleDamageWriter.ResolveNativeUnarmoredHpInjury(11, 25, 1);
            _ = GC.GetAllocatedBytesForCurrentThread();
            long before = GC.GetAllocatedBytesForCurrentThread();
            int checksum = 17;
            for (int index = 0; index < 4096; index++)
            {
                checksum = unchecked(checksum * 31 +
                    BattleDamageWriter.ResolveNativeUnarmoredHpInjury(
                        index,
                        25,
                        index & 1));
            }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

            Assert.That(checksum, Is.Not.Zero);
            Assert.That(allocated, Is.Zero);
        }

        private static TypedCharacter CreateCharacter(
            SimulationWorld world,
            int objectId,
            int slot)
        {
            var frames = new List<LF2FrameData>();
            for (int id = 0; id <= 240; id++)
            {
                frames.Add(new LF2FrameData
                {
                    frameId = id,
                    state = 0,
                    wait = 100,
                    next = id,
                });
            }

            var entity = new TypedCharacter
            {
                ObjectId = objectId,
            };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5Type0DamageScale",
                    frames = frames,
                }));
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
