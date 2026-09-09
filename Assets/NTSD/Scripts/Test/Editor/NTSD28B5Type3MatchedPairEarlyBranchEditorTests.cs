#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B5Type3MatchedPairEarlyBranchEditorTests
    {
        [TestCase(LF2States.ObjectFlying, 71, 72)]
        [TestCase(LF2States.ObjectExpanding, 73, 74)]
        public void InitialMatchingPair_ReturnsBeforeDamageAndOnlyCommitsRestResetHold(
            int matchingState,
            int attackerAction,
            int targetAction)
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 310, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 311, 1, LF2ObjectType.SpecialAttack);
            PrepareMatchingPair(
                attacker,
                target,
                matchingState,
                attackerAction,
                targetAction);
            target.Health.PP = 88;
            target.ComboCountVic = 12;
            target.HitCount = 5;
            target.FallCounter = 6;
            target.HitStateCount = 7;
            target.Runtime.SetVelocity(1.0, 2.0, 3.0);
            attacker.Runtime.SetVelocity(4.0, 5.0, 6.0);
            target.KnockbackVx = 7.0;
            target.KnockbackVy = 8.0;
            target.KnockbackVz = 9.0;
            attacker.KnockbackVx = 10.0;
            attacker.KnockbackVy = 11.0;
            attacker.KnockbackVz = 12.0;
            attacker.FrameDelay = 6;
            world.DamageStats[1] = 15;
            world.Rng.Seed(0x12345678u);
            uint rngState = world.Rng.State;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(500));
            Assert.That(target.Health.HPBound, Is.EqualTo(500));
            Assert.That(target.Health.PP, Is.EqualTo(88));
            Assert.That(target.ComboCountVic, Is.EqualTo(12));
            Assert.That(world.DamageStats[1], Is.EqualTo(15));
            Assert.That(target.HitCount, Is.EqualTo(5));
            Assert.That(target.FallCounter, Is.EqualTo(6));
            Assert.That(target.HitStateCount, Is.EqualTo(7));
            Assert.That(target.HitRecordCount, Is.Zero);
            Assert.That(world.PendingSounds.Count, Is.Zero);
            Assert.That(world.Rng.State, Is.EqualTo(rngState));
            Assert.That(world.Rng.CallCount, Is.Zero);
            Assert.That(attacker.AttackExempt, Is.EqualTo(2));
            Assert.That(attacker.ItrRest.Arest, Is.EqualTo(2));
            Assert.That(
                target.ItrRest.GetVrest(attacker.Runtime.SlotIndex),
                Is.EqualTo(9));
            Assert.That(attacker.Frame.N, Is.EqualTo(attackerAction));
            Assert.That(target.Frame.N, Is.EqualTo(targetAction));
            Assert.That(attacker.AttackingCounter, Is.Zero);
            Assert.That(target.AttackingCounter, Is.Zero);
            Assert.That(attacker.KnockbackVx, Is.Zero);
            Assert.That(attacker.KnockbackVy, Is.Zero);
            Assert.That(attacker.KnockbackVz, Is.Zero);
            Assert.That(target.KnockbackVx, Is.Zero);
            Assert.That(target.KnockbackVy, Is.Zero);
            Assert.That(target.KnockbackVz, Is.Zero);
            Assert.That(attacker.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(target.Runtime.Vx, Is.EqualTo(1.0));
            Assert.That(attacker.FrameDelay, Is.EqualTo(-3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
        }

        [Test]
        public void InitialMatchingNegativeLinkPair_ReleasesMirroredParentHold()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 312, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 313, 1, LF2ObjectType.SpecialAttack);
            TypedCharacter parent = CreateEntity(
                world, 9000, 2, LF2ObjectType.Character);
            PrepareMatchingPair(
                attacker,
                target,
                LF2States.ObjectFlying,
                71,
                72);
            attacker.Runtime.LinkState = -1;
            attacker.Runtime.HolderStableId = parent.Runtime.SlotIndex;
            attacker.FrameDelay = 6;
            parent.FrameDelay = 11;

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(attacker.FrameDelay, Is.EqualTo(3));
            Assert.That(parent.FrameDelay, Is.EqualTo(-3));
            Assert.That(target.FrameDelay, Is.EqualTo(-3));
            Assert.That(world.PendingSounds.Count, Is.Zero);
            Assert.That(target.HitRecordCount, Is.Zero);
        }

        [Test]
        public void DifferentPairStates_ContinueOrdinaryDamagePath()
        {
            var world = new SimulationWorld();
            TypedCharacter attacker = CreateEntity(
                world, 314, 0, LF2ObjectType.SpecialAttack);
            TypedCharacter target = CreateEntity(
                world, 315, 1, LF2ObjectType.SpecialAttack);
            attacker.GetFrameDataById(0).state = LF2States.ObjectExpanding;
            target.GetFrameDataById(0).state = LF2States.ObjectFlying;
            attacker.ImmediateFrame(0);
            target.ImmediateFrame(0);

            bool applied = world.DamageWriter.ApplySpecialAttackDamage(
                world,
                attacker,
                target,
                DamageInteraction());

            Assert.That(applied, Is.True);
            Assert.That(target.Health.HP, Is.EqualTo(490));
            Assert.That(target.HitCount, Is.EqualTo(1));
            Assert.That(target.HitRecordCount, Is.EqualTo(1));
            Assert.That(world.PendingSounds.Count, Is.EqualTo(1));
        }

        private static InteractionArea DamageInteraction()
        {
            return new InteractionArea
            {
                kind = 0,
                injury = 10,
                fall = 10,
                arest = 2,
                vrest = 9,
                effect = 23,
                poison = 12345,
                join = 4,
            };
        }

        private static void PrepareMatchingPair(
            LF2Entity attacker,
            LF2Entity target,
            int state,
            int attackerAction,
            int targetAction)
        {
            attacker.GetFrameDataById(0).state = state;
            target.GetFrameDataById(0).state = state;
            attacker.GetFrameDataById(5).hit_Uj = attackerAction;
            target.GetFrameDataById(6).hit_Uj = targetAction;
            attacker.ImmediateFrame(0);
            target.ImmediateFrame(0);
            attacker.Trans.SyncDirectFrameData(
                attacker.Frame.D.wait,
                attacker.Frame.D.next,
                5);
            target.Trans.SyncDirectFrameData(
                target.Frame.D.wait,
                target.Frame.D.next,
                6);
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
                    name = "B5Type3MatchedPairEarlyBranch",
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
