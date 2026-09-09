#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4Type0OrdinaryLandingEditorTests
    {
        [Test]
        public void ExactCharacterNegativeFloorLandingUsesFrameHitG()
        {
            LF2Character entity = CreateExact(300, 4, 630);
            Prepare(entity, -10, -12.0, 6.0, 3.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(630));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void SharedCharacterNegativeFloorLandingUsesFrameHitG()
        {
            ProbeOther entity = CreateShared(300, 4, 630);
            Prepare(entity, -10, -12.0, 6.0, 3.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(630));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void Action212HasPriorityOverFrameHitG()
        {
            LF2Character entity = CreateExact(212, 4, 630);
            Prepare(entity, 0, -1.0, 9.0, 2.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(215));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(3.0));
        }

        [Test]
        public void State6HasPriorityOverFrameHitG()
        {
            LF2Character entity = CreateExact(300, LF2States.Rowing, 630);
            Prepare(entity, 0, -1.0, 6.0, 2.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(215));
        }

        [Test]
        public void State100HasHighestOrdinaryLandingPriority()
        {
            LF2Character entity = CreateExact(300, LF2States.CustomSkill1, 630);
            Prepare(entity, 0, -1.0, 6.0, 2.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(94));
        }

        [Test]
        public void AlreadyGroundedAtNegativeReferenceDoesNotRetriggerLanding()
        {
            LF2Character entity = CreateExact(300, 4, 630);
            Prepare(entity, -10, -10.0, 5.0, 0.0);
            entity.AttackingCounter = 7;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(300));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(entity.AttackingCounter, Is.EqualTo(7));
        }

        private static LF2Character CreateExact(int frameId, int state, int hitG)
        {
            var entity = new LF2Character { ObjectId = 8307 };
            Load(entity, frameId, state, hitG);
            return entity;
        }

        private static ProbeOther CreateShared(int frameId, int state, int hitG)
        {
            var entity = new ProbeOther { ObjectId = 8308 };
            Load(entity, frameId, state, hitG);
            return entity;
        }

        private static void Load(LF2Entity entity, int frameId, int state, int hitG)
        {
            var data = new LF2CharacterData
            {
                name = "B4Type0OrdinaryLanding",
                frames = new List<LF2FrameData>
                {
                    Frame(frameId, state, hitG),
                    Frame(94, 0, 0),
                    Frame(215, 0, 0),
                    Frame(219, 0, 0),
                    Frame(630, 0, 0),
                },
            };
            entity.FrameCache.Load(new LF2CharacterDataWrapper(entity.ObjectId, data));
            entity.ImmediateFrame(frameId);
        }

        private static LF2FrameData Frame(int id, int state, int hitG)
        {
            return new LF2FrameData
            {
                frameId = id,
                state = state,
                hit_g = hitG,
                wait = 100,
                next = id,
            };
        }

        private static void Prepare(
            LF2Entity entity,
            int reference,
            double y,
            double vx,
            double vy)
        {
            entity.Runtime.CollisionYReference = reference;
            entity.Runtime.SetPosition(0.0, y, 0.0);
            entity.Runtime.SetVelocity(vx, vy, 0.0);
            entity.Runtime.SyncIntegerPosition();
            entity.AttackingCounter = 5;
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
