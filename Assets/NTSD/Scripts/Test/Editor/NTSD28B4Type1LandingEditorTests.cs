#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4Type1LandingEditorTests
    {
        [Test]
        public void NativePredicateUsesStrictState1002Threshold()
        {
            Assert.That(BattleNativeType1LandingKernel.ShouldBounce(
                LF2States.WeaponThrowing,
                BattleNativeType1LandingKernel.State1002BounceThreshold), Is.False);
            Assert.That(BattleNativeType1LandingKernel.ShouldBounce(
                LF2States.WeaponThrowing,
                5.257102245049804e120), Is.True);
            Assert.That(BattleNativeType1LandingKernel.ShouldBounce(
                LF2States.WeaponInSky,
                double.MaxValue), Is.False);
        }

        [Test]
        public void State1002NormalFiniteImpactSettlesToAction70()
        {
            ProbeWeapon weapon = CreateWeapon(LF2States.WeaponThrowing);
            weapon.Runtime.SetPosition(0.0, 1.0, 0.0);
            weapon.Runtime.SetVelocity(10.0, 12.0, 0.0);
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.AttackingCounter = 4;

            bool handled = weapon.InvokeLanding(12.0, crossedGround: true);

            Assert.That(handled, Is.True);
            Assert.That(weapon.Frame.N, Is.EqualTo(70));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(5.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(97));
            Assert.That(weapon.AttackingCounter, Is.Zero);
            Assert.That(weapon.Runtime.Dir, Is.EqualTo("right"));
        }

        [Test]
        public void State1002ImpactStrictlyAboveNativeThresholdBouncesToAction7()
        {
            ProbeWeapon weapon = CreateWeapon(LF2States.WeaponThrowing);
            weapon.Runtime.SetPosition(0.0, 1.0, 0.0);
            weapon.Runtime.SetVelocity(
                10.0,
                5.257102245049804e120,
                0.0);
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.AttackingCounter = 4;

            bool handled = weapon.InvokeLanding(
                5.257102245049804e120,
                crossedGround: true);

            Assert.That(handled, Is.True);
            Assert.That(weapon.Frame.N, Is.EqualTo(7));
            Assert.That(weapon.Runtime.Vy, Is.EqualTo(-8.0));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(5.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(97));
            Assert.That(weapon.AttackingCounter, Is.EqualTo(4));
            Assert.That(weapon.Runtime.Dir, Is.EqualTo("left"));
        }

        [Test]
        public void NonState1002HighImpactSettlesToAction60()
        {
            ProbeWeapon weapon = CreateWeapon(LF2States.WeaponInSky);
            weapon.Runtime.SetPosition(0.0, 1.0, 0.0);
            weapon.Runtime.SetVelocity(10.0, 12.0, 0.0);
            weapon.Runtime.WeaponFlightCounter = 100;
            weapon.AttackingCounter = 4;

            bool handled = weapon.InvokeLanding(12.0, crossedGround: true);

            Assert.That(handled, Is.True);
            Assert.That(weapon.Frame.N, Is.EqualTo(60));
            Assert.That(weapon.Runtime.Vy, Is.Zero);
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(5.0));
            Assert.That(weapon.Runtime.WeaponFlightCounter, Is.EqualTo(97));
            Assert.That(weapon.AttackingCounter, Is.Zero);
            Assert.That(weapon.Runtime.Dir, Is.EqualTo("right"));
        }

        private static ProbeWeapon CreateWeapon(int state)
        {
            var current = new LF2FrameData
            {
                frameId = 0,
                state = state,
                wait = 100,
                next = 0,
            };
            var data = new LF2CharacterData
            {
                name = "B4Type1Landing",
                type_sub = (int)LF2ObjectType.LightWeapon,
                weapon_drop_hurt = 3,
                weapon_drop_sound = "B4_TYPE1_DROP",
                frames = new List<LF2FrameData>
                {
                    current,
                    new LF2FrameData { frameId = 7, state = LF2States.WeaponInSky },
                    new LF2FrameData { frameId = 60, state = LF2States.WeaponOnGround },
                    new LF2FrameData { frameId = 70, state = LF2States.WeaponOnGround },
                },
            };
            var weapon = new ProbeWeapon();
            weapon.ObjectId = 8101;
            weapon.ConfigureType1();
            weapon.FrameCache.Load(
                new LF2CharacterDataWrapper(weapon.ObjectId, data));
            weapon.ImmediateFrame(0);
            weapon.SwitchDir("right");
            return weapon;
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void ConfigureType1()
            {
                SetWeaponType((int)LF2ObjectType.LightWeapon);
            }

            internal bool InvokeLanding(double landingVy, bool crossedGround)
            {
                return ApplyCurrentDatNonCharacterLanding(
                    (int)LF2ObjectType.LightWeapon,
                    Frame.D,
                    landingVy,
                    crossedGround);
            }
        }
    }
}
#endif
