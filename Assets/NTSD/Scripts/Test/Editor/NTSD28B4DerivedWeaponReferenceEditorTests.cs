#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4DerivedWeaponReferenceEditorTests
    {
        [Test]
        public void PooledType1SettlesAtNegativeReference()
        {
            ProbeWeapon weapon = CreateWeapon(
                LF2ObjectType.LightWeapon,
                LF2States.WeaponThrowing,
                3);
            PrepareImpact(weapon, -2, -3.0, 8.0, 3.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(70));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(97));
        }

        [Test]
        public void PooledType2ThresholdEqualityCopiesNegativeReferenceIntoVy()
        {
            ProbeWeapon weapon = CreateWeapon(
                LF2ObjectType.HeavyWeapon,
                LF2States.HeavyWeaponInSky,
                10);
            PrepareImpact(weapon, -2, -3.0, 6.0, 9.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(20));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(3.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(89));
        }

        [Test]
        public void PooledType4ExactReferenceContactDoesNotApplyGravity()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.ThrowWeapon, 0, 35);
            PrepareImpact(weapon, -2, -4.0, 4.0, 2.0);
            weapon.AttackingCounter = 7;

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.Zero);
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(100));
            Assert.That(weapon.AttackingCounter, Is.EqualTo(7));
        }

        [Test]
        public void PooledDeadType6SettlesAtNegativeReference()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.Drink, 0, 35);
            weapon.Health.HP = 0;
            PrepareImpact(weapon, -2, -3.0, 0.0, 2.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Frame.N, Is.EqualTo(60));
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-2.0));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(-1));
        }

        [Test]
        public void PooledWeaponFrameCpointKind2SuppressesPhysics()
        {
            ProbeWeapon weapon = CreateWeapon(LF2ObjectType.LightWeapon, 0, 3, cpointKind: 2);
            PrepareImpact(weapon, 0, -10.0, 4.0, 2.0);

            Assert.That(weapon.RunNativePhysicsForWorldPass(1), Is.True);

            Assert.That(weapon.Runtime.X, Is.Zero);
            Assert.That(weapon.Runtime.Y, Is.EqualTo(-10.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(4.0));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(2.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(100));
        }

        private static void PrepareImpact(
            ProbeWeapon weapon,
            int collisionYReference,
            double y,
            double vx,
            double vy)
        {
            weapon.Runtime.CollisionYReference = collisionYReference;
            weapon.Runtime.SetPosition(0.0, y, 0.0);
            weapon.Runtime.SetVelocity(vx, vy, 0.0);
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.Runtime.SyncIntegerPosition();
        }

        private static ProbeWeapon CreateWeapon(
            LF2ObjectType dataType,
            int state,
            int dropHurt,
            int cpointKind = 0)
        {
            LF2FrameData current = Frame(0, state);
            if (cpointKind != 0)
                current.cpoint = new CatchPoint { kind = cpointKind };

            var data = new LF2CharacterData
            {
                name = "B4DerivedWeaponReference",
                weapon_drop_hurt = dropHurt,
                frames = new List<LF2FrameData>
                {
                    current,
                    Frame(20, LF2States.HeavyWeaponOnGround),
                    Frame(40, state),
                    Frame(60, LF2States.WeaponOnGround),
                    Frame(70, LF2States.WeaponOnGround),
                },
            };
            var weapon = new ProbeWeapon { ObjectId = 8303 };
            weapon.ConfigureType((int)dataType);
            weapon.FrameCache.Load(
                new LF2CharacterDataWrapper(weapon.ObjectId, data));
            weapon.ImmediateFrame(0);
            weapon.SwitchDir("right");
            return weapon;
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

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void ConfigureType(int dataType)
            {
                SetWeaponType(dataType);
            }
        }
    }
}
#endif
