#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NTSD.Simulation.Ecs;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type0JoinMimicSideEffectsEditorTests
    {
        [Test]
        public void PositiveJoin_StoresOriginalGroupAndCopiesAttackerGroup()
        {
            var attacker = new NTSDEntityRuntime { RelationTeam = 7 };
            var target = new NTSDEntityRuntime
            {
                RelationTeam = 9,
                JoinTimer148 = 30,
            };

            BattleDamageWriter.ApplyNativeJoinAndMimicSideEffects(
                attacker,
                target);

            Assert.That(target.JoinOverrideActive170, Is.EqualTo(1));
            Assert.That(target.JoinOriginalBattleGroup174, Is.EqualTo(9));
            Assert.That(target.RelationTeam, Is.EqualTo(7));
        }

        [Test]
        public void ActiveJoin_DoesNotOverwriteOriginalOrCurrentGroup()
        {
            var attacker = new NTSDEntityRuntime { RelationTeam = 7 };
            var target = new NTSDEntityRuntime
            {
                RelationTeam = 8,
                JoinTimer148 = 30,
                JoinOverrideActive170 = 1,
                JoinOriginalBattleGroup174 = 9,
            };

            BattleDamageWriter.ApplyNativeJoinAndMimicSideEffects(
                attacker,
                target);

            Assert.That(target.JoinOverrideActive170, Is.EqualTo(1));
            Assert.That(target.JoinOriginalBattleGroup174, Is.EqualTo(9));
            Assert.That(target.RelationTeam, Is.EqualTo(8));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void NonPositiveJoin_DoesNotActivate(int counter)
        {
            var attacker = new NTSDEntityRuntime { RelationTeam = 7 };
            var target = new NTSDEntityRuntime
            {
                RelationTeam = 9,
                JoinTimer148 = counter,
            };

            BattleDamageWriter.ApplyNativeJoinAndMimicSideEffects(
                attacker,
                target);

            Assert.That(target.JoinOverrideActive170, Is.Zero);
            Assert.That(target.JoinOriginalBattleGroup174, Is.Zero);
            Assert.That(target.RelationTeam, Is.EqualTo(9));
        }

        [Test]
        public void PositiveTypeZeroMimic_RecordsSourceSlotAndEnablesProxy()
        {
            var attacker = new NTSDEntityRuntime
            {
                ObjType = 0,
                SlotIndex = 17,
            };
            var target = new NTSDEntityRuntime
            {
                ObjType = 0,
                InputProxyCounter14C = 40,
            };

            BattleDamageWriter.ApplyNativeJoinAndMimicSideEffects(
                attacker,
                target);

            Assert.That(target.InputProxySourceSlot178, Is.EqualTo(17));
            Assert.That(target.InputProxyEnabled17C, Is.EqualTo(1));
        }

        [TestCase(3, 0, 40, 0)]
        [TestCase(0, 3, 40, 0)]
        [TestCase(0, 0, 0, 0)]
        [TestCase(0, 0, 40, 1)]
        public void FailedMimicGate_DoesNotRearm(
            int attackerType,
            int targetType,
            int counter,
            int enabled)
        {
            var attacker = new NTSDEntityRuntime
            {
                ObjType = attackerType,
                SlotIndex = 17,
            };
            var target = new NTSDEntityRuntime
            {
                ObjType = targetType,
                InputProxyCounter14C = counter,
                InputProxySourceSlot178 = 23,
                InputProxyEnabled17C = enabled,
            };

            BattleDamageWriter.ApplyNativeJoinAndMimicSideEffects(
                attacker,
                target);

            Assert.That(target.InputProxySourceSlot178, Is.EqualTo(23));
            Assert.That(target.InputProxyEnabled17C, Is.EqualTo(enabled));
        }

        [Test]
        public void ProductionStandardDamage_ActivatesFreshEncodedJoinAndMimic()
        {
            var world = new SimulationWorld();
            LF2Character attacker = CreateCharacter(8350, 0);
            LF2Character victim = CreateCharacter(8351, 1);
            attacker.RelationTeam = 7;
            victim.RelationTeam = 9;
            victim.Health.HP = 500;
            victim.Health.HPBound = 500;
            world.Register(attacker);
            world.Register(victim);

            bool applied = world.DamageWriter.ApplyStandardCharacterDamage(
                world,
                attacker,
                victim,
                victim.HitCounters,
                new InteractionArea
                {
                    kind = 9,
                    fall = 1,
                    join = 33,
                    mimic = 44,
                });

            Assert.That(applied, Is.True);
            Assert.That(victim.Runtime.JoinTimer148, Is.EqualTo(33));
            Assert.That(victim.Runtime.JoinOverrideActive170, Is.EqualTo(1));
            Assert.That(victim.Runtime.JoinOriginalBattleGroup174, Is.EqualTo(9));
            Assert.That(victim.RelationTeam, Is.EqualTo(7));
            Assert.That(victim.Runtime.InputProxyCounter14C, Is.EqualTo(44));
            Assert.That(victim.Runtime.InputProxySourceSlot178, Is.EqualTo(0));
            Assert.That(victim.Runtime.InputProxyEnabled17C, Is.EqualTo(1));
        }

        private static LF2Character CreateCharacter(int objectId, int slot)
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
            var entity = new LF2Character { ObjectId = objectId };
            entity.SetRequiredRuntimeSlot(slot);
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                objectId,
                new LF2CharacterData
                {
                    name = "B5Type0JoinMimic",
                    frames = frames,
                }));
            entity.ImmediateFrame(0);
            return entity;
        }
    }
}
#endif
