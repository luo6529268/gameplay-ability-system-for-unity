#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4SharedType246ReferenceEditorTests
    {
        [Test]
        public void Type2NegativeReferenceHardImpactBouncesAtReference()
        {
            ProbeOther entity = CreateShell(
                LF2ObjectType.HeavyWeapon,
                LF2States.HeavyWeaponInSky,
                10);
            PrepareImpact(entity, -2, -3.0, 10.0, 10.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-5.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(5.0));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(99));
            Assert.That(entity.Runtime.Dir, Is.EqualTo("left"));
        }

        [Test]
        public void Type2ThresholdEqualitySettlesAndCopiesReferenceIntoVerticalMotion()
        {
            ProbeOther entity = CreateShell(
                LF2ObjectType.HeavyWeapon,
                LF2States.HeavyWeaponInSky,
                10);
            PrepareImpact(entity, -2, -3.0, 6.0, 9.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(20));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(89));
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void Type4NegativeReferenceHardImpactUsesNativeBounce()
        {
            ProbeOther entity = CreateShell(
                LF2ObjectType.ThrowWeapon,
                LF2States.WeaponInSky,
                35);
            PrepareImpact(entity, -2, -3.0, 12.0, 12.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(-8.4).Within(1e-12));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(8.4).Within(1e-12));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(65));
        }

        [Test]
        public void Type4NegativeReferenceSoftImpactSettlesAtReference()
        {
            ProbeOther entity = CreateShell(LF2ObjectType.ThrowWeapon, 0, 35);
            PrepareImpact(entity, -2, -3.0, 4.0, 2.0);
            entity.AttackingCounter = 7;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(60));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.Vx, Is.EqualTo(2.8).Within(1e-12));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(65));
            Assert.That(entity.AttackingCounter, Is.Zero);
        }

        [Test]
        public void DeadType6NegativeReferenceLandingForcesFlightCounterMinusOne()
        {
            ProbeOther entity = CreateShell(LF2ObjectType.Drink, 0, 35);
            entity.Health.HP = 0;
            PrepareImpact(entity, -2, -3.0, 0.0, 2.0);

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.EqualTo(60));
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.Zero);
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(-1));
        }

        [Test]
        public void Type4ExactReferenceContactIsNativeNoopWithoutGravity()
        {
            ProbeOther entity = CreateShell(LF2ObjectType.ThrowWeapon, 0, 35);
            PrepareImpact(entity, -2, -4.0, 4.0, 2.0);
            entity.AttackingCounter = 7;

            Assert.That(entity.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(entity.Frame.N, Is.Zero);
            Assert.That(entity.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(entity.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(entity.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(entity.Runtime.WeaponFlightCounter, Is.EqualTo(100));
            Assert.That(entity.AttackingCounter, Is.EqualTo(7));
        }

        private static void PrepareImpact(
            ProbeOther entity,
            int collisionYReference,
            double y,
            double vx,
            double vy)
        {
            entity.Runtime.CollisionYReference = collisionYReference;
            entity.Runtime.SetPosition(0.0, y, 0.0);
            entity.Runtime.SetVelocity(vx, vy, 0.0);
            entity.Runtime.WeaponFlightCounter = 100;
            entity.Runtime.SyncIntegerPosition();
        }

        private static ProbeOther CreateShell(
            LF2ObjectType dataType,
            int state,
            int dropHurt)
        {
            var data = new LF2CharacterData
            {
                name = "B4SharedType246Reference",
                type_sub = (int)dataType,
                weapon_drop_hurt = dropHurt,
                frames = new List<LF2FrameData>
                {
                    Frame(0, state),
                    Frame(20, LF2States.HeavyWeaponOnGround),
                    Frame(40, state),
                    Frame(60, LF2States.WeaponOnGround),
                    Frame(70, LF2States.WeaponOnGround),
                },
            };
            var entity = new ProbeOther((int)dataType) { ObjectId = 8302 };
            entity.FrameCache.Load(
                new LF2CharacterDataWrapper(entity.ObjectId, data));
            entity.ImmediateFrame(0);
            entity.SwitchDir("right");
            return entity;
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
            private readonly int dataType;

            internal ProbeOther(int dataType)
            {
                this.dataType = dataType;
            }

            public override int GetCurrentDataObjectTypeForSimulation()
            {
                return dataType;
            }
        }
    }
}
#endif
