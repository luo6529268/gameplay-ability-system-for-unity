#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4State1218AirborneEditorTests
    {
        [TestCase(12, 170, -8.0001, 0, 0, 180)]
        [TestCase(12, 170, -8.0, 0, 0, 181)]
        [TestCase(12, 170, 1.0, 0, 0, 182)]
        [TestCase(12, 170, 8.0, 0, 0, 183)]
        [TestCase(12, 187, 1.0, 0, 0, 188)]
        [TestCase(12, 185, 2.0, 0, 0, -1)]
        [TestCase(12, 170, 11.999, -1, 6, 182)]
        [TestCase(12, 170, 11.999, -1, 5, 181)]
        [TestCase(12, 170, 12.0, -1, 6, 181)]
        [TestCase(18, 204, 1.0, 0, 0, -1)]
        [TestCase(18, 204, 1.0001, 0, 0, 205)]
        [TestCase(18, 205, 2.0, 0, 0, -1)]
        public void KernelUsesStrictNativeBands(
            int state,
            int action,
            double postGravityVy,
            int environmentState,
            int upcomingPhase12,
            int expected)
        {
            Assert.That(
                BattleNativeType0AirborneActionKernel.Resolve(
                    state,
                    action,
                    postGravityVy,
                    environmentState,
                    upcomingPhase12),
                Is.EqualTo(expected));
        }

        [Test]
        public void ExactCharacterUsesEnvironmentStateAndUpcomingWorldPhase()
        {
            LF2Character entity = CreateExact(12, 170);
            SimulationWorld world = RegisterAtPhase(entity, 5);
            PrepareAirborne(entity, 0, -20.0, 0.0);
            entity.Runtime.EnvironmentState320 = -1;
            entity.WeaponCount = 0;
            entity.AttackingCounter = 7;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(world.NativeResourcePhase12, Is.EqualTo(5));
            Assert.That(entity.Frame.N, Is.EqualTo(182));
            Assert.That(entity.AttackingCounter, Is.EqualTo(7));
        }

        [Test]
        public void WeaponCountDoesNotActivateNegativeEnvironmentOverride()
        {
            LF2Character entity = CreateExact(12, 170);
            RegisterAtPhase(entity, 5);
            PrepareAirborne(entity, 0, -20.0, -5.0);
            entity.Runtime.EnvironmentState320 = 0;
            entity.WeaponCount = -1;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(181));
        }

        [Test]
        public void NegativeReferenceGroundContactIsNotAirborne()
        {
            LF2Character entity = CreateExact(12, 170);
            RegisterAtPhase(entity, 5);
            PrepareAirborne(entity, -20, -20.0, 0.0);
            entity.Runtime.EnvironmentState320 = -1;
            entity.AttackingCounter = 7;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(230));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-20.0));
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void SharedCharacterUsesTheSameUpcomingPhaseSelector()
        {
            ProbeOther entity = CreateShared(12, 170);
            RegisterAtPhase(entity, 5);
            PrepareAirborne(entity, 0, -20.0, 0.0);
            entity.Runtime.EnvironmentState320 = -1;
            entity.AttackingCounter = 8;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(182));
            Assert.That(entity.AttackingCounter, Is.EqualTo(8));
        }

        [Test]
        public void State18UsesPostGravityStrictGreaterThanOne()
        {
            LF2Character entity = CreateExact(18, 204);
            RegisterAtPhase(entity, 0);
            PrepareAirborne(entity, -20, -25.0, 1.0);
            entity.AttackingCounter = 9;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(205));
            Assert.That(entity.AttackingCounter, Is.EqualTo(9));
        }

        private static SimulationWorld RegisterAtPhase(LF2Entity entity, int phase12)
        {
            var world = new SimulationWorld();
            world.Runtime.NativeWorldClock.ResourcePhase12 = phase12;
            world.Register(entity);
            return world;
        }

        private static void PrepareAirborne(
            LF2Entity entity,
            int reference,
            double y,
            double vy)
        {
            entity.Runtime.CollisionYReference = reference;
            entity.Runtime.SetPosition(0.0, y, 0.0);
            entity.Runtime.SetVelocity(0.0, vy, 0.0);
            entity.Runtime.SyncIntegerPosition();
        }

        private static LF2Character CreateExact(int state, int action)
        {
            var entity = new LF2Character { ObjectId = 8309 };
            Load(entity, state, action);
            return entity;
        }

        private static ProbeOther CreateShared(int state, int action)
        {
            var entity = new ProbeOther { ObjectId = 8310 };
            Load(entity, state, action);
            return entity;
        }

        private static void Load(LF2Entity entity, int state, int action)
        {
            var frames = new List<LF2FrameData>
            {
                Frame(action, state),
            };
            int[] targetActions = { 180, 181, 182, 183, 186, 187, 188, 189, 205, 230, 231 };
            for (int index = 0; index < targetActions.Length; index++)
            {
                int target = targetActions[index];
                if (target != action)
                    frames.Add(Frame(target, state));
            }

            entity.FrameCache.Load(new LF2CharacterDataWrapper(
                entity.ObjectId,
                new LF2CharacterData
                {
                    name = "B4State1218Airborne",
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
