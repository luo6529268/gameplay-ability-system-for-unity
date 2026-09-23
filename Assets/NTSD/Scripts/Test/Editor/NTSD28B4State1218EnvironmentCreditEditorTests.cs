#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4State1218EnvironmentCreditEditorTests
    {
        [Test]
        public void ExactNonlethalContact_AppliesAbsoluteDamageBeforeSoftAction()
        {
            var world = new SimulationWorld();
            LF2Character victim = CreateExact(8313, 170, LF2States.Falling);
            LF2Character credit = CreateExact(8314, 0, LF2States.Standing);
            Register(world, victim, 0);
            Register(world, credit, 1);
            PrepareContact(victim);
            victim.Health.HP = 100;
            victim.Health.HPBound = 80;
            victim.Runtime.EnvironmentState320 = -10;
            victim.Runtime.CatchSourceSlot90 = 1;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(victim.Frame.N, Is.EqualTo(230));
            Assert.That(victim.Health.HP, Is.EqualTo(90));
            Assert.That(victim.Health.HPBound, Is.EqualTo(70));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(10));
            Assert.That(victim.Runtime.EnvironmentState320, Is.EqualTo(1));
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(credit.Runtime.KnockoutCount358, Is.Zero);
        }

        [Test]
        public void SharedEncodedSource_FollowsExactlyTwoOwnersAndUsesScale()
        {
            var world = new SimulationWorld();
            ProbeOther victim = CreateShared(8315, 170, LF2States.Falling);
            LF2Character source = CreateExact(8316, 0, LF2States.Standing);
            LF2Character owner1 = CreateExact(8317, 0, LF2States.Standing);
            LF2Character owner2 = CreateExact(8318, 0, LF2States.Standing);
            LF2Character owner3 = CreateExact(8319, 0, LF2States.Standing);
            Register(world, victim, 0);
            Register(world, source, 1);
            Register(world, owner1, 2);
            Register(world, owner2, 3);
            Register(world, owner3, 4);
            source.Runtime.OwnerSlotIndex = 2;
            owner1.Runtime.OwnerSlotIndex = 3;
            owner2.Runtime.OwnerSlotIndex = 4;
            PrepareContact(victim);
            victim.Health.HP = 100;
            victim.Health.HPBound = 90;
            victim.Runtime.EnvironmentState320 = 20;
            victim.Runtime.IncomingDamageScale340 = 50;
            victim.Runtime.CatchSourceSlot90 = 0x2000 + 1;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(victim.Health.HP, Is.EqualTo(60));
            Assert.That(victim.Health.HPBound, Is.EqualTo(50));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(40));
            Assert.That(source.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(owner1.Runtime.InputScoreTotal348, Is.Zero);
            Assert.That(owner2.Runtime.InputScoreTotal348, Is.EqualTo(40));
            Assert.That(owner3.Runtime.InputScoreTotal348, Is.Zero);
        }

        [Test]
        public void MissingNextOwner_RetainsCurrentCreditEntity()
        {
            var world = new SimulationWorld();
            LF2Character victim = CreateExact(8320, 170, LF2States.Falling);
            LF2Character source = CreateExact(8321, 0, LF2States.Standing);
            Register(world, victim, 0);
            Register(world, source, 1);
            source.Runtime.OwnerSlotIndex = 99;
            PrepareContact(victim);
            victim.Runtime.EnvironmentState320 = 7;
            victim.Runtime.CatchSourceSlot90 = 1;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(source.Runtime.InputScoreTotal348, Is.EqualTo(7));
        }

        [Test]
        public void MissingCredit_DoesNotBlockVictimDamage()
        {
            var world = new SimulationWorld();
            LF2Character victim = CreateExact(8322, 170, LF2States.Falling);
            Register(world, victim, 0);
            PrepareContact(victim);
            victim.Health.HP = 20;
            victim.Health.HPBound = 18;
            victim.Runtime.EnvironmentState320 = 6;
            victim.Runtime.CatchSourceSlot90 = 77;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(victim.Health.HP, Is.EqualTo(14));
            Assert.That(victim.Health.HPBound, Is.EqualTo(12));
            Assert.That(victim.Runtime.InputHpConsumedTotal34C, Is.EqualTo(6));
            Assert.That(victim.Runtime.EnvironmentState320, Is.EqualTo(1));
        }

        [Test]
        public void LethalCrossing_IncrementsResolvedCreditKnockoutBeforeSubtraction()
        {
            var world = new SimulationWorld();
            LF2Character victim = CreateExact(8323, 170, LF2States.Falling);
            LF2Character credit = CreateExact(8324, 0, LF2States.Standing);
            Register(world, victim, 0);
            Register(world, credit, 1);
            PrepareContact(victim);
            victim.Health.HP = 5;
            victim.Health.HPBound = 4;
            victim.Runtime.EnvironmentState320 = 10;
            victim.Runtime.CatchSourceSlot90 = 1;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(victim.Health.HP, Is.EqualTo(-5));
            Assert.That(victim.Health.HPBound, Is.EqualTo(-6));
            Assert.That(credit.Runtime.KnockoutCount358, Is.EqualTo(1));
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(world.NativeKnockoutEvents.Count, Is.EqualTo(1));
            NativeKnockoutEvent knockout = world.NativeKnockoutEvents[0];
            Assert.That(knockout.VictimSlot, Is.EqualTo(0));
            Assert.That(knockout.SourceSlot, Is.EqualTo(0));
            Assert.That(knockout.CreditSlot, Is.EqualTo(1));
            Assert.That(knockout.FourOwnerSlot, Is.EqualTo(0));
            Assert.That(knockout.SourceObjectType, Is.EqualTo(0));
        }

        [Test]
        public void AlreadyDeadVictim_StillConsumesDamageButDoesNotIncrementKnockout()
        {
            var world = new SimulationWorld();
            LF2Character victim = CreateExact(8325, 170, LF2States.Falling);
            LF2Character credit = CreateExact(8326, 0, LF2States.Standing);
            Register(world, victim, 0);
            Register(world, credit, 1);
            PrepareContact(victim);
            victim.Health.HP = 0;
            victim.Health.HPBound = 0;
            victim.Runtime.EnvironmentState320 = 10;
            victim.Runtime.CatchSourceSlot90 = 1;

            Assert.That(victim.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(victim.Health.HP, Is.EqualTo(-10));
            Assert.That(victim.Health.HPBound, Is.EqualTo(-10));
            Assert.That(credit.Runtime.KnockoutCount358, Is.Zero);
            Assert.That(credit.Runtime.InputScoreTotal348, Is.EqualTo(10));
            Assert.That(world.NativeKnockoutEvents, Is.Empty);
        }

        [Test]
        public void AirborneOrNonState1218_DoesNotConsumeEnvironmentState()
        {
            var world = new SimulationWorld();
            LF2Character airborne = CreateExact(8327, 170, LF2States.Falling);
            Register(world, airborne, 0);
            airborne.Health.HP = 20;
            airborne.Runtime.EnvironmentState320 = 8;
            airborne.Runtime.SetPosition(0.0, -20.0, 0.0);
            airborne.Runtime.SetVelocity(0.0, 0.0, 0.0);
            airborne.Runtime.SyncIntegerPosition();

            Assert.That(airborne.RunNativePhysicsForWorldPass(1), Is.True);
            Assert.That(airborne.Health.HP, Is.EqualTo(20));
            Assert.That(airborne.Runtime.EnvironmentState320, Is.EqualTo(8));

            LF2Character ordinary = CreateExact(8328, 0, LF2States.Standing);
            Register(world, ordinary, 1);
            PrepareContact(ordinary);
            ordinary.Health.HP = 20;
            ordinary.Runtime.EnvironmentState320 = 8;

            Assert.That(ordinary.RunNativePhysicsForWorldPass(1), Is.True);
            Assert.That(ordinary.Health.HP, Is.EqualTo(20));
            Assert.That(ordinary.Runtime.EnvironmentState320, Is.EqualTo(8));
        }

        private static void Register(
            SimulationWorld world,
            LF2Entity entity,
            int slot)
        {
            entity.SetRequiredRuntimeSlot(slot);
            world.Register(entity);
        }

        private static void PrepareContact(LF2Entity entity)
        {
            entity.Runtime.CollisionYReference = 0;
            entity.Runtime.SetPosition(0.0, -1.0, 0.0);
            entity.Runtime.SetVelocity(0.0, 1.0, 0.0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static LF2Character CreateExact(
            int objectId,
            int action,
            int state)
        {
            var entity = new LF2Character { ObjectId = objectId };
            Load(entity, action, state);
            return entity;
        }

        private static ProbeOther CreateShared(
            int objectId,
            int action,
            int state)
        {
            var entity = new ProbeOther { ObjectId = objectId };
            Load(entity, action, state);
            return entity;
        }

        private static void Load(LF2Entity entity, int action, int state)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(action, state),
                Frame(185, 0),
                Frame(191, 0),
                Frame(219, 0),
                Frame(230, 0),
                Frame(231, 0),
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                entity.ObjectId,
                new LF2CharacterData
                {
                    name = "B4State1218EnvironmentCredit",
                    frames = frames,
                }));
            entity.ImmediateFrame(action);
        }

        private static LF2FrameData Frame(int id, int state)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                wait = 100,
                next = id,
            };
        }

        private sealed class ProbeOther : LF2OtherObject
        {
            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return (int)LF2ObjectType.Character;
            }
        }
    }
}
#endif
