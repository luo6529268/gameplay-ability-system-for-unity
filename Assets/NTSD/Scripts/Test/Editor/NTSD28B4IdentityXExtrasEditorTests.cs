#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System.Collections.Generic;

using NTSD.Animation;
using NTSD.Animation.LF2Objects;
using NTSD.Simulation;
using NUnit.Framework;

namespace NTSD.Test.Editor
{
    public sealed class NTSD28B4IdentityXExtrasEditorTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void RegisteredType3WeaponHitJPreciseZUsesViewRatio(
            bool configuredView)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            ProbeWeapon weapon = CreateWeapon((int)LF2ObjectType.SpecialAttack,
                900, 0);
            weapon.Frame.D.hit_j = 60;
            weapon.SetRequiredRuntimeSlot(20);
            world.Register(weapon);
            weapon.Runtime.Z = 250;
            weapon.Runtime.ZInt = 250;
            weapon.Runtime.Type3VisualZOffset = 0;
            weapon.Runtime.SetSourceRulePosition(100.75, 30.25);
            weapon.Runtime.SyncSourceRuleIntegerPosition();

            weapon.InvokeFlightPhysics();

            double expectedZ = 250 + 10.0 *
                (configuredView ? 1152.0 / 730.0 : 1.0);
            Assert.That(weapon.Runtime.Z, Is.EqualTo(expectedZ).Within(0.000001));
            Assert.That(weapon.Runtime.ZInt, Is.EqualTo(250));
            Assert.That(weapon.Runtime.Type3VisualZOffset, Is.EqualTo(10.0));
            Assert.That(weapon.Runtime.SourceRuleZ, Is.EqualTo(40.25));
            Assert.That(weapon.Runtime.SourceRuleZInt, Is.EqualTo(30));
        }

        [TestCase(false, 120, 2.0)]
        [TestCase(true, 120, 2.0)]
        [TestCase(false, 101, -2.0)]
        [TestCase(true, 101, -2.0)]
        public void RegisteredWeaponIdentityExtraUsesViewRatio(
            bool configuredView,
            int objectId,
            double rawExtra)
        {
            var world = new SimulationWorld();
            if (configuredView)
                world.ConfigureFixedViewRunDistance(2048, 1152);
            ProbeWeapon weapon = CreateWeapon((int)LF2ObjectType.LightWeapon,
                objectId, 0);
            weapon.SetRequiredRuntimeSlot(20);
            world.Register(weapon);
            weapon.Runtime.X = 200;
            weapon.Runtime.XInt = 200;
            weapon.Runtime.Vx = 10.0;
            weapon.Runtime.SetSourceRulePosition(50.75, 20.25);
            weapon.Runtime.SyncSourceRuleIntegerPosition();

            weapon.InvokeFlightPhysics();

            double expectedX = 200 + rawExtra *
                (configuredView ? 2048.0 / 1333.0 : 1.0);
            Assert.That(weapon.Runtime.X, Is.EqualTo(expectedX).Within(0.000001));
            Assert.That(weapon.Runtime.XInt, Is.EqualTo(200));
            Assert.That(weapon.Runtime.Vx, Is.EqualTo(10.0));
            Assert.That(weapon.Runtime.SourceRuleX,
                Is.EqualTo(50.75 + rawExtra).Within(0.000001));
            Assert.That(weapon.Runtime.SourceRuleXInt, Is.EqualTo(50));
        }

        [TestCase((int)LF2ObjectType.ThrowWeapon, 900, 101, 0.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 120, 0, 2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 101, 0, -2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 120, 2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 101, -2.0)]
        [TestCase((int)LF2ObjectType.LightWeapon, 900, 0, 0.0)]
        public void DerivedWeaponUsesIndependentRealAndAliasIdentityOperations(
            int objectType,
            int objectId,
            int alias,
            double expectedExtraX)
        {
            ProbeWeapon weapon = CreateWeapon(objectType, objectId, alias);
            weapon.Runtime.X = 0.0;
            weapon.Runtime.Vx = 10.0;

            weapon.InvokeFlightPhysics();

            Assert.That(weapon.Runtime.X, Is.EqualTo(expectedExtraX));
        }

        private static ProbeWeapon CreateWeapon(
            int objectType,
            int objectId,
            int alias)
        {
            var frame = new LF2FrameData
            {
                frameId = 0,
                state = LF2States.WeaponInSky,
                wait = 100,
                next = 0,
            };
            var data = new LF2CharacterData
            {
                name = "B4IdentityX",
                type_sub = alias,
                frames = new List<LF2FrameData> { frame },
            };
            var weapon = new ProbeWeapon { ObjectId = objectId };
            weapon.SetType(objectType);
            var wrapper = new LF2CharacterDataWrapper(objectId, data);
            weapon.FrameCache.Load(wrapper);
            weapon.SetRuntimeCharacterConfigResolverForSelfCheck(
                new RuntimeCharacterConfigResolver(id =>
                    id == objectId ? wrapper : null));
            weapon.ImmediateFrame(0);
            return weapon;
        }

        private sealed class ProbeWeapon : LF2Weapon
        {
            internal void SetType(int objectType)
            {
                SetWeaponType(objectType);
            }

            internal void InvokeFlightPhysics()
            {
                base.WeaponFlightPhysics();
            }
        }
    }
}
#endif
